using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	public class ProductValueObjectDataAdapter : ValueObjectDataAdapter<OrgSupplierPart, Xsd.Product>
	{
		protected delegate ProductValueObjectDataAdapter NewDelegate();
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		public static ProductValueObjectDataAdapter New()
		{
			ProductValueObjectDataAdapter result;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden();
			}
			else
			{
				switch (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
				{
					case "US":
						result = (ProductValueObjectDataAdapter)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSProductValueObjectDataAdapter>());
						break;

					default:
						result = (ProductValueObjectDataAdapter)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.ICustomsProductValueObjectDataAdapter>());
						break;
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public override string RootCollectionElementName
		{
			get { return "Products"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public override string RootElementName
		{
			get { return "Product"; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return null; }
		}

		protected override void NotifyBizObjCreatedOrUpdated(INotifications notifications, BusinessObject bizObj)
		{
			var notification = new BusinessObjectCreatedOrUpdatedNotification(bizObj);
			notification.UpdateRecordCountOnlyWithoutMessage = true;

			notifications.Notify(notification);
		}

		#region Product Matching

		protected override OrgSupplierPart CreateOrUpdateFromValueObjectCore(Xsd.Product partXML, IValueObjectImportContext dataImportContext)
		{
			((Xsd.XmlInterchange)dataImportContext.Interchange).ImportEDICode = true;

			OrgSupplierPart result = null;
			if (partXML != null)
			{
				shouldImportFromValueObject = true;

				result = LoadProductFromEnterprise(dataImportContext.Factory, partXML.RelatedOrganisations, partXML.ProductCode, dataImportContext);

				if (result == null)
				{
					result = CheckDuplicatesAndCreateNewProductIfCheckPassed(partXML, dataImportContext, true);
					if (result == null)
					{
						return null;
					}
				}
				else
				{
					var relatedPartyWithCodes = result.RelatedOrganisations.Cast<OrgPartRelation>().Select(r => new RelatedPartyWithCode(r.OU_Relationship, r.Organisation.OH_Code)).ToList();
					if (IsDuplicateProduct(dataImportContext, result.PK, result.OP_PartNum, relatedPartyWithCodes, out var duplicateOwnerCode, out var duplicateSupplierCode))
					{
						NotifyDuplicateProduct(dataImportContext, result.OP_PartNum, duplicateOwnerCode, duplicateSupplierCode);
						return null;
					}
					reasonWhyProductUpdateSkippedText = Res.GetString("c8b91185-10f9-484b-a42e-8defc76df285", "User canceled product update");
					shouldImportFromValueObject = ShouldUpdateExistingObject(result, dataImportContext);
				}

				if (shouldImportFromValueObject)
				{
					ImportFromValueObject(result, partXML, dataImportContext);
				}
				else
				{
					OnUserDeclinedImport(result, partXML, dataImportContext);
				}
			}
			return result;
		}

		OrgSupplierPart CheckDuplicatesAndCreateNewProductIfCheckPassed(Xsd.Product partXML, IValueObjectImportContext dataImportContext, bool addNotificationIfPartCannotBeMatched)
		{
			OrgSupplierPart result = null;
			var relatedPartiesMatched = GetRelatedPartiesMatched(partXML, dataImportContext, out var matchedOrganizationPKs);

			if (CheckDuplicatePivots(relatedPartiesMatched, partXML.ProductCode, dataImportContext))
			{
				var productsWithTheSameNumberFromEnterprise = dataImportContext.Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, partXML.ProductCode));
				if (productsWithTheSameNumberFromEnterprise.Length == 0 && relatedPartiesMatched.Count > 0)
				{
					return dataImportContext.Factory.New<OrgSupplierPart>();
				}

				if (relatedPartiesMatched.Count == 0)
				{
					if (addNotificationIfPartCannotBeMatched)
					{
						var message = Res.GetString("fbf73ea5-c1c7-4d33-802b-3d4a1d67e09b",
	@"Product cannot be matched or created: Product Number [{0}], Owner [<No Owner>], Supplier [<No Supplier>].
PRODUCT SKIPPED: Cannot match or create Product without Related Organization.
No Related Organization specified in XML file or {1} cannot match organization from XML file.",
		partXML.ProductCode,
		Core.Constants.ProductName);

						dataImportContext.Notify(new WarningNotification(WarningType.Warning, message));
					}
				}
				else
				{
					if (IsDuplicateProduct(dataImportContext, ZGuid.NewZGuid(), partXML.ProductCode, relatedPartiesMatched, out var duplicateOwnerCode, out var duplicateSupplierCode))
					{
						NotifyDuplicateProduct(dataImportContext, partXML.ProductCode, duplicateOwnerCode, duplicateSupplierCode);
					}
					else
					{
						result = dataImportContext.Factory.New<OrgSupplierPart>();
					}
				}
			}

			if (result == null)
			{
				DeleteNewlyCreatedOrganizations(dataImportContext, matchedOrganizationPKs);
			}

			return result;
		}

		bool IsDuplicateProduct(IValueObjectImportContext dataImportContext, ZGuid productPK, ZString productCode, List<RelatedPartyWithCode> relatedPartiesMatched, out ZString duplicateOwnerCode, out ZString duplicateSupplierCode)
		{
			var duplicateDetector = new DuplicateProductDetectorNoBizO(productPK, productCode, true, relatedPartiesMatched, GetUnsavedRelations(dataImportContext, productCode));
			duplicateDetector.CheckInactiveProducts = false;
			duplicateDetector.Validate();
			duplicateOwnerCode = duplicateDetector.OwnerCodeFromLastErrorOrWarning;
			duplicateSupplierCode = duplicateDetector.SupplierCodeFromLastErrorOrWarning;
			return duplicateDetector.HasError;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		static void NotifyDuplicateProduct(IValueObjectImportContext dataImportContext, ZString productCode, ZString ownerCode, ZString supplierCode)
		{
			var message = Res.GetString("d55c3a07-44ba-4ddf-9f7e-961edf451848",
				@"Duplicate Product detected: Product Number [{0}], Owner [{1}], Supplier [{2}].
PRODUCT SKIPPED: Importing would create a duplicate preventing {3} from being able to decide the right Product to use for the given Owner/Supplier combination.",
				productCode,
				ownerCode.IsEmpty ? new ZString("<No Owner>") : ownerCode.Trim(),
				supplierCode.IsEmpty ? new ZString("<No Supplier>") : supplierCode.Trim(),
				Core.Constants.ProductName);

			dataImportContext.Notify(new WarningNotification(WarningType.RecordAlreadyExists, message));
		}

		bool CheckDuplicatePivots(List<RelatedPartyWithCode> relatedParties, string productNumber, IValueObjectImportContext dataImportContext)
		{
			var result = true;
			foreach (var relatedParty in relatedParties)
			{
				var indexerAsIEnumerable = new RelatedPartyWithCode[] { relatedParty };
				var listExceptIndexer = relatedParties.Except(indexerAsIEnumerable).ToList();
				if (OrgPartRelationValidationHelper.HasDuplicateRelationship(listExceptIndexer, relatedParty.RelatedOrganisationCode, relatedParty.RelationshipCode))
				{
					NotifyDuplicatePivotsWarning(relatedParty, productNumber, dataImportContext, OrgPartRelation.DuplicateRelationshipError);
					result = false;
				}

				if (OrgPartRelationValidationHelper.HasSameSupplierAndOwner(listExceptIndexer, relatedParty.RelatedOrganisationCode, relatedParty.RelationshipCode))
				{
					NotifyDuplicatePivotsWarning(relatedParty, productNumber, dataImportContext, OrgPartRelation.SameOrgAsOwnerAndSupplier);
					result = false;
				}
			}

			return result;
		}

		void NotifyDuplicatePivotsWarning(RelatedPartyWithCode relatedParty, string productNumber, IValueObjectImportContext dataImportContext, string message)
		{
			dataImportContext.Notify(new WarningNotification(WarningType.Warning, Res.GetString("720dbf73-56d4-4c9f-a52d-d2eaad6bcebb",
				@"Duplicate related organization detected on Product: Product Number [{0}], {1} [{2}].
PRODUCT SKIPPED: {3}", productNumber, relatedParty.RelationshipCode, relatedParty.RelatedOrganisationCode, message)));
		}

		List<RelatedPartyWithCode> GetRelatedPartiesMatched(Xsd.Product partXML, IValueObjectImportContext dataImportContext, out HashSet<ZGuid> matchedOrganizationPKs)
		{
			matchedOrganizationPKs = new HashSet<ZGuid>();
			var relatedPartiesMatched = new List<RelatedPartyWithCode>();

			foreach (Xsd.RelatedOrganisation relatedOrganisation in partXML.RelatedOrganisations)
			{
				var organization = FindOrCreateTempOrganisation(dataImportContext, relatedOrganisation.Organisation);

				if (organization != null)
				{
					var party = new RelatedPartyWithCode(relatedOrganisation.RelationshipType, organization.OH_Code);
					relatedPartiesMatched.Add(party);
					matchedOrganizationPKs.Add(organization.PK);
				}
			}
			return relatedPartiesMatched;
		}

		void DeleteNewlyCreatedOrganizations(IValueObjectImportContext context, ICollection<ZGuid> organizationPKs)
		{
			foreach (var orgPK in organizationPKs)
			{
				var orgHeader = context.Factory.Load<OrgHeader>(orgPK);
				if (orgHeader != null && !orgHeader.IsInDatabase)
				{
					orgHeader.Delete();
				}
			}
		}

		IOrgHeaderForMatching FindOrCreateTempOrganisation(IValueObjectImportContext dataImportContext, Xsd.Organisation organisation)
		{
			if (organisation.EDICode.IsEmpty && !organisation.OwnerCode.IsEmpty && !organisation.OrganisationDetails.IsSpecified)
			{
				((Xsd.XmlInterchange)dataImportContext.Interchange).ImportEDICode = false;
			}

			var result = dataImportContext.FindOrCreateTempOrganisation(organisation, null, MasterFiles.Integration.OrganisationTypes.None);

			((Xsd.XmlInterchange)dataImportContext.Interchange).ImportEDICode = true;
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected override bool ConfirmUpdateOfExistingBusinessObject(OrgSupplierPart bizObj, CargoWise.ComponentModel.INotifications notifications)
		{
			var queryArgs = new QueryUserYesNoYesAllNoAllEventArgs();
			queryArgs.Caption = Res.GetString("0cc2dad9-fc49-4ef4-8158-6ad9a7119a87", "Notification");
			queryArgs.Message = Res.GetString("9DECD2D1-6703-41F9-8C23-DBD316B58DD1", @"Found Product: 

 Number  [{0}]
 Owners  [{1}]
 Suppliers  [{2}]

Is it OK to update it?", bizObj.OP_PartNum,
						bizObj.AllOwners.IsEmpty ? new ZString("<No owners>") : bizObj.AllOwners,
						bizObj.AllSuppliers.IsEmpty ? new ZString("<No suppliers>") : bizObj.AllSuppliers);

			notifications.QueryUser(queryArgs);
			return queryArgs.Response;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected override void OnUserDeclinedImport(OrgSupplierPart bizObj, Xsd.Product value, IValueObjectImportContext context)
		{
			if (bizObj != null)
			{
				var message = Res.GetString("73438d10-7c37-43de-a815-077f8ff2a459", @"Found Existing Product:  Product Number [{0}], Owner [{1}], Supplier [{2}].
PRODUCT SKIPPED: {3}.", bizObj.OP_PartNum,
	bizObj.AllOwners.IsEmpty ? new ZString("<No owners>") : bizObj.AllOwners,
	bizObj.AllSuppliers.IsEmpty ? new ZString("<No suppliers>") : bizObj.AllSuppliers, reasonWhyProductUpdateSkippedText);

				var notification = new InfoNotification(message + "\r\n");

				var importContext = context as ValueObjectImportContext;
				if (importContext != null)
				{
					var buffer = importContext.Notifications as NotificationBuffer;
					if (buffer != null && !buffer.AsString.Contains(message))
					{
						context.Add(notification);
					}
				}
			}
		}

		OrgSupplierPart LoadProductFromEnterprise(BusinessObjectFactory factory, Xsd.RelatedOrganisationCollection relatedOrganisations, ZString productCode, IValueObjectImportContext context)
		{
			OrgSupplierPart partLoadedFromEnterprise = null;

			if (relatedOrganisations.Count > 0)
			{
				var partQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));
				partQuery.AddToFilter(OrgSupplierPartSchema.OP_PartNum, productCode);

				foreach (Xsd.RelatedOrganisation relatedOrganisation in relatedOrganisations)
				{
					if (relatedOrganisation.RelationshipType == OrgPartRelation.RelationshipTypes.Both
						|| relatedOrganisation.RelationshipType == OrgPartRelation.RelationshipTypes.Owner
						|| relatedOrganisation.RelationshipType == OrgPartRelation.RelationshipTypes.Supplier)
					{
						var organisation = FindOrCreateTempOrganisation(context, relatedOrganisation.Organisation);
						if (organisation != null)
						{
							var relatedOrganisationSubQuery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
							relatedOrganisationSubQuery.AddToFilter(GetAdditionalRelationshipQuery(organisation.PK, relatedOrganisation.RelationshipType));
							partQuery.AddSubQuery(relatedOrganisationSubQuery, JoinCondition.And);
						}
					}
				}

				var loadedParts = factory.Load<OrgSupplierPart>(partQuery);
				foreach (var loadedPart in loadedParts)
				{
					if (loadedPart.RelatedOrganisations.Count == relatedOrganisations.Count)
					{
						partLoadedFromEnterprise = loadedPart;
						break;
					}
				}
			}

			return partLoadedFromEnterprise;
		}

		ZQuery GetAdditionalRelationshipQuery(ZGuid organisationPK, ZString relationshipType)
		{
			var filter = new ZQuery(OrgPartRelationSchema.OU_Relationship, relationshipType);
			filter.AddToFilter(JoinCondition.And, OrgPartRelationSchema.OU_OH, organisationPK);
			return filter;
		}

		#endregion

		#region Import

		protected override void ImportFromValueObjectCore(OrgSupplierPart product, Xsd.Product value, IValueObjectImportContext context)
		{
			ImportUnitConversions(product, value.UnitConversions, context);
			ImportOtherProductDetails(product, value, context);
			ImportBillOfMaterials(product, value, context);

			foreach (IValueObjectImport importer in ObjectFactory.Get<IEnumerable>("ProductValueObjectImportList"))
			{
				if (importer.CanImport(value))
				{
					importer.Import(product, value, context);
				}
			}

			ImportRelatedOrganisations(product, value.RelatedOrganisations, context);
			ImportBarCodes(product, value.Barcodes, context);

			AddImportEvent(product);
			product.IsImportedFromXML = true;
			product.IsLightSaving = true;
		}

		void ImportBillOfMaterials(OrgSupplierPart product, Xsd.Product value, IValueObjectImportContext context)
		{
			if (value.BillOfMaterials.Count == 0)
			{
				return;
			}
			product.BillOfMaterials.DeleteAll();

			product.OP_CanResell = value.BillOfMaterials[0].AllowResale;
			product.OP_CanDisassembleKit = value.BillOfMaterials[0].AllowDisassemblyOfKit;
			product.OP_KitIsAutoReplenished = value.BillOfMaterials[0].AllowAutoReplenishKit;
			product.OP_AutoPrintAssemblyInstructions = value.BillOfMaterials[0].AutoPrintAssemblyInstruction;

			foreach (Xsd.BOMComponentPart component in value.BillOfMaterials[0].Components)
			{
				if (component.ComponentPart != null)
				{
					var uSBOMProduct = LoadProductFromEnterprise(product.Factory, component.ComponentPart.RelatedOrganisations, component.ComponentPart.ProductCode, context) ?? CheckDuplicatesAndCreateNewProductIfCheckPassed(component.ComponentPart, context, false);

					if (uSBOMProduct != null)
					{
						var bOM = product.BillOfMaterials.AddNew();
						bOM.OE_OP_Component = uSBOMProduct.PK;
						bOM.OE_ComponentQty = component.Quantity;
						bOM.OE_CanReuse = component.Reusable;
						bOM.OE_F3_NKPackType = component.StockUnit;

						uSBOMProduct.IsLightSaving = true;
						uSBOMProduct.IsImportedFromXML = true;
						ImportBarCodes(uSBOMProduct, component.ComponentPart.Barcodes, context);
						ImportOtherProductDetails(uSBOMProduct, component.ComponentPart, context);
						ImportBillOfMaterials(uSBOMProduct, component.ComponentPart, context);

						foreach (IValueObjectImport importer in ObjectFactory.Get<IEnumerable>("ProductValueObjectImportList"))
						{
							if (importer.CanImport(component.ComponentPart))
							{
								importer.Import(uSBOMProduct, component.ComponentPart, context);
							}
						}

						ImportRelatedOrganisations(uSBOMProduct, component.ComponentPart.RelatedOrganisations, context);
						ImportUnitConversions(uSBOMProduct, component.ComponentPart.UnitConversions, context);
					}
					else
					{
						var message = Res.GetString("507E839D-D232-40FD-8856-F72DE7539E4F",
@"Bill Of Material cannot be added to the Product [{0}], because BOM Product [{1}] cannot be matched or created.
BOM PRODUCT SKIPPED: no Related Organization specified or Related Organization cannot be matched by {2}.",
	product.OP_PartNum,
	component.ComponentPart.ProductCode,
	Core.Constants.ProductName);

						context.Notify(new WarningNotification(WarningType.Warning, message));
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		void ImportRelatedOrganisations(OrgSupplierPart product, Xsd.RelatedOrganisationCollection relatedOrganisations, IValueObjectImportContext context)
		{
			foreach (Xsd.RelatedOrganisation relatedOrganisation in relatedOrganisations)
			{
				IOrgHeaderForMatching orgHeader = null;
				var headerPK = ZGuid.Empty;
				if (relatedOrganisation.OrganisationSpecified)
				{
					orgHeader = FindOrCreateTempOrganisation(context, relatedOrganisation.Organisation);
					headerPK = orgHeader?.PK ?? ZGuid.Empty;
				}

				var orgRelation = product.RelatedOrganisations.FindByOH_CodeAndRelationship(relatedOrganisation.Organisation.EDICode, relatedOrganisation.RelationshipType);

				if (orgRelation == null)
				{
					orgRelation = product.RelatedOrganisations.FindByOrganisationPKAndRelationship(headerPK, relatedOrganisation.RelationshipType);
					if (orgRelation == null)
					{
						orgRelation = product.RelatedOrganisations.AddNew();
						if (orgHeader != null)
						{
							RememberUnsavedRelation(context, product.OP_PartNum, new PartAndFriends(orgRelation.OU_OP, orgRelation.OU_Relationship, orgHeader.OH_Code, true));
						}
					}
				}

				if (relatedOrganisation.OrganisationSpecified)
				{
					orgRelation.OU_OH = headerPK;
				}
				context.SetPropertyInfoValue(orgRelation.OU_ClientUQInfo, relatedOrganisation.ClientUQ, relatedOrganisation.ClientUQSpecified);
				if (relatedOrganisation.HiSpecified)
				{
					orgRelation.OU_Hi = (ZShort)relatedOrganisation.Hi;
				}
				if (relatedOrganisation.LCMarkUpPercentage1Specified)
				{
					orgRelation.OU_LandedCostMarginPercent1 = relatedOrganisation.LCMarkUpPercentage1;
				}
				if (relatedOrganisation.LCMarkUpPercentage2Specified)
				{
					orgRelation.OU_LandedCostMarginPercent2 = relatedOrganisation.LCMarkUpPercentage2;
				}
				if (relatedOrganisation.LCMarkUpPercentage3Specified)
				{
					orgRelation.OU_LandedCostMarginPercent3 = relatedOrganisation.LCMarkUpPercentage3;
				}
				context.SetPropertyInfoValue(orgRelation.OU_LocalPartDescriptionInfo, relatedOrganisation.LocalProductDescription, relatedOrganisation.LocalProductDescriptionSpecified);
				context.SetPropertyInfoValue(orgRelation.OU_LocalPartNumberInfo, relatedOrganisation.LocalProductNumber, relatedOrganisation.LocalProductNumberSpecified);
				context.SetPropertyInfoValue(orgRelation.OU_RelationshipInfo, relatedOrganisation.RelationshipType, relatedOrganisation.RelationshipTypeSpecified);
				if (relatedOrganisation.RoyaltyFlatAmountSpecified)
				{
					orgRelation.OU_RoyaltyFlatAmount = relatedOrganisation.RoyaltyFlatAmount.Value;
				}
				context.SetPropertyInfoValue(orgRelation.OU_RX_NKRoyaltyCurrencyInfo, relatedOrganisation.RoyaltyFlatAmount.CurrencyCode, relatedOrganisation.RoyaltyFlatAmount.CurrencyCodeSpecified);

				if (relatedOrganisation.RFAttributeConfirmSpecified)
				{
					context.SetPropertyInfoValue(orgRelation.OU_RFAttributeConfirmInfo,
													RFAttrConfirmToXmlCodeMappings.Instance.GetEnterpriseCode(
													relatedOrganisation.RFAttributeConfirm.ToString(), "", context), true,
													"RF Attribute Confirm");
				}
				if (relatedOrganisation.RoyaltyPercentageSpecified)
				{
					orgRelation.OU_RoyaltyPercent = relatedOrganisation.RoyaltyPercentage;
				}
				if (relatedOrganisation.TiSpecified)
				{
					orgRelation.OU_Ti = (ZShort)relatedOrganisation.Ti;
				}
				if (relatedOrganisation.UseAttribute1Specified)
				{
					orgRelation.OU_UsePartAttrib1 = relatedOrganisation.UseAttribute1;
				}
				if (relatedOrganisation.UseAttribute2Specified)
				{
					orgRelation.OU_UsePartAttrib2 = relatedOrganisation.UseAttribute2;
				}
				if (relatedOrganisation.UseAttribute3Specified)
				{
					orgRelation.OU_UsePartAttrib3 = relatedOrganisation.UseAttribute3;
				}
				if (relatedOrganisation.UseExpiryDateSpecified)
				{
					orgRelation.OU_UseExpiryDate = relatedOrganisation.UseExpiryDate;
				}
				if (relatedOrganisation.ConsigneeMinShelfLifeAccepted > 0)
				{
					orgRelation.OU_ConsigneeMinShelfLifeAccepted = relatedOrganisation.ConsigneeMinShelfLifeAccepted;
				}
				if (relatedOrganisation.UsePackingDateSpecified)
				{
					orgRelation.OU_UsePackingDate = relatedOrganisation.UsePackingDate;
				}
			}
		}

		void RememberUnsavedRelation(IValueObjectImportContext context, string partNum, PartAndFriends partAndFriends)
		{
			var partAndFriendsList = GetUnsavedRelations(context, partNum);
			partAndFriendsList.Add(partAndFriends);
		}

		List<PartAndFriends> GetUnsavedRelations(IValueObjectImportContext context, string productCode)
		{
			var unsavedRelations = context.Factory.GetCachedValue("ProductValueObjectDataAdapter_UnsavedOrgPartRelations", () => new Dictionary<string, List<PartAndFriends>>());
			if (!unsavedRelations.TryGetValue(productCode, out var partAndFriendsList))
			{
				partAndFriendsList = new List<PartAndFriends>();
				unsavedRelations.Add(productCode, partAndFriendsList);
			}

			return partAndFriendsList;
		}

		void ImportBarCodes(OrgSupplierPart product, Xsd.BarcodeCollection barcodes, IValueObjectImportContext context)
		{
			var relatedOrganisationPKs = product.RelatedOrganisations.Cast<OrgPartRelation>().Where(o => o.IsOwner).Select(o => o.OU_OH);
			var barcodeStrings = new List<ZString>();
			foreach (Xsd.Barcode barcode in barcodes)
			{
				if (barcode.BarcodeStringSpecified)
				{
					barcodeStrings.Add(barcode.BarcodeString);
				}
			}

			barcodeStrings.Add(product.OP_PartNum);
			var duplicateProductCodes = OrgPartRelationValidationHelper.GetProductNumbersWithDuplicateBarcode(product.Factory, product.PK, relatedOrganisationPKs, barcodeStrings);
			if (duplicateProductCodes.Any())
			{
				var errorNotification = new ErrorNotification(
					ErrorType.DataErrorPreventSave,
					Res.GetString("ACAD0051-987E-4999-B4E3-1E54F5E4C57D", "Attempt to add duplicate barcode for the same organization. The duplicated barcode is used as Product Code or Barcode on the following products ({0}).", string.Join(", ", duplicateProductCodes)));
				context.Add(errorNotification);
			}
			else
			{
				foreach (Xsd.Barcode barcode in barcodes)
				{
					if (barcode.PackageUQ.Trim().IsEmpty || barcode.BarcodeString.Trim().IsEmpty)
					{
						var errorNotification = new ErrorNotification(
							ErrorType.DataErrorPreventSave,
							Res.GetString("D4A72AE8-2865-457C-A7B0-E0F11A1ABAC7", "Attempt to import invalid Part Barcode for Product '{0}'. Valid Part Barcodes have a non-empty Barcode and Pack Type. The values for this invalid Part Barcode are Barcode: '{1}', Pack Type: '{2}'.", product.OP_PartNum, barcode.BarcodeString, barcode.PackageUQ));
						context.Add(errorNotification);
					}
					else
					{
						var partBarcode = product.PartBarcodes.FindPartBarcode(barcode.PackageUQ, barcode.BarcodeString);
						if (partBarcode == null)
						{
							partBarcode = product.PartBarcodes.AddNew();
							context.SetPropertyInfoValue(partBarcode.PH_F3_NKPackTypeInfo, barcode.PackageUQ, barcode.PackageUQSpecified);
							context.SetPropertyInfoValue(partBarcode.PH_BarcodeInfo, barcode.BarcodeString, barcode.BarcodeStringSpecified);
						}
					}
				}
			}
		}

		void ImportUnitConversions(OrgSupplierPart product, Xsd.UnitConversionCollection unitConversions, IValueObjectImportContext context)
		{
			foreach (Xsd.UnitConversion unitConversion in unitConversions)
			{
				OrgPartUnit unit = product.PartUnits.GetUnitConversion(unitConversion.ParentUQ, unitConversion.Package.DimensionType);
				if (unit == null)
				{
					unit = product.PartUnits.AddNew();
					context.SetPropertyInfoValue(unit.OF_PackTypeInfo, unitConversion.Package.DimensionType);
					context.SetPropertyInfoValue(unit.OF_ParentPackTypeInfo, unitConversion.ParentUQ);
				}
				if (unitConversion.PackageSpecified)
				{
					unit.OF_QuantityInParent = unitConversion.Package.Value;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		void ImportOtherProductDetails(OrgSupplierPart product, Xsd.Product value, IValueObjectImportContext context)
		{
			if (value.BasicStockControl.LastCostSpecified)
			{
				context.SetPropertyInfoValue(product.OP_LastCostInfo, value.BasicStockControl.LastCost, OrgSupplierPartSchema.OP_LastCost);
			}
			if (value.BasicStockControl.QtyInStockSpecified)
			{
				context.SetPropertyInfoValue(product.OP_QtyInStockInfo, value.BasicStockControl.QtyInStock, OrgSupplierPartSchema.OP_QtyInStock);
			}
			if (value.BasicStockControl.WeightedCostSpecified)
			{
				context.SetPropertyInfoValue(product.OP_WeightedCostInfo, value.BasicStockControl.WeightedCost, OrgSupplierPartSchema.OP_WeightedCost);
			}

			ImportCurrency(product, value, context);

			context.SetPropertyInfoValue(product.OP_BrandInfo, value.BrandName, value.BrandNameSpecified);
			context.SetPropertyInfoValue(product.OP_DepartmentInfo, value.ClientDefinedDetails.Department, value.ClientDefinedDetails.DepartmentSpecified);
			context.SetPropertyInfoValue(product.OP_DivisionInfo, value.ClientDefinedDetails.Division, value.ClientDefinedDetails.DivisionSpecified);
			if (value.ClientDefinedDetails.VendorPackSpecified)
			{
				context.SetPropertyInfoValue(product.OP_VendorPackQtyInfo, value.ClientDefinedDetails.VendorPack.Value, OrgSupplierPartSchema.OP_VendorPackQty);
			}
			context.SetPropertyInfoValue(product.OP_F3_NKPackTypeInfo, value.ClientDefinedDetails.VendorPack.DimensionType, value.ClientDefinedDetails.VendorPack.DimensionTypeSpecified);
			if (value.ClientDefinedDetails.OrderMultipleQtySpecified)
			{
				context.SetPropertyInfoValue(product.OP_OrderMultipleQtyInfo, value.ClientDefinedDetails.OrderMultipleQty.Value, OrgSupplierPartSchema.OP_OrderMultipleQty);
			}
			context.SetPropertyInfoValue(product.OP_OrderMultipleUnitInfo, value.ClientDefinedDetails.OrderMultipleQty.DimensionType, value.ClientDefinedDetails.OrderMultipleQty.DimensionTypeSpecified);
			if (value.DecimalPlacesSpecified)
			{
				product.OP_CountDecimalPlaces = Convert.ToByte(value.DecimalPlaces.ToString());
			}
			if (value.DimensionDetails.DepthSpecified)
			{
				context.SetPropertyInfoValue(product.OP_DepthInfo, value.DimensionDetails.Depth, OrgSupplierPartSchema.OP_Depth);
			}

			var dimensionUnit = value.DimensionDetails.DimensionUnit;
			context.SetPropertyInfoValue(product.OP_MeasureUQInfo, dimensionUnit, value.DimensionDetails.DimensionUnitSpecified);
			if (!dimensionUnit.IsEmpty && !Constants.Length.ContainsCode(dimensionUnit))
			{
				context.Add(new WarningNotification(WarningType.Warning, Res.GetString("BDE8146C-B065-44EC-9BCE-C6F01B294AD6", "Invalid Dimension Unit Code '{0}'", dimensionUnit)));
			}

			if (value.DimensionDetails.GrossWeightSpecified)
			{
				context.SetPropertyInfoValue(product.OP_WeightInfo, value.DimensionDetails.GrossWeight.Value, OrgSupplierPartSchema.OP_Weight);
			}

			var weightDimensionType = value.DimensionDetails.GrossWeight.DimensionType;
			context.SetPropertyInfoValue(product.OP_WeightUQInfo, weightDimensionType, value.DimensionDetails.GrossWeight.DimensionTypeSpecified);
			if (!weightDimensionType.IsEmpty && !Constants.Weight.ContainsCode(weightDimensionType))
			{
				context.Add(new WarningNotification(WarningType.Warning, Res.GetString("2DBB869D-4119-476B-8B77-53511B66C28F", "Invalid Weight Code '{0}'", weightDimensionType)));
			}

			if (value.DimensionDetails.HeightSpecified)
			{
				context.SetPropertyInfoValue(product.OP_HeightInfo, value.DimensionDetails.Height, OrgSupplierPartSchema.OP_Height);
			}
			if (value.DimensionDetails.NetWeightSpecified)
			{
				context.SetPropertyInfoValue(product.OP_NetWeightInfo, value.DimensionDetails.NetWeight, OrgSupplierPartSchema.OP_NetWeight);
			}
			if (value.DimensionDetails.VolumeSpecified)
			{
				context.SetPropertyInfoValue(product.OP_CubicInfo, value.DimensionDetails.Volume.Value, OrgSupplierPartSchema.OP_Cubic);
			}

			var volumeDimensionType = value.DimensionDetails.Volume.DimensionType;
			context.SetPropertyInfoValue(product.OP_CubicUQInfo, volumeDimensionType, value.DimensionDetails.Volume.DimensionTypeSpecified);
			if (!volumeDimensionType.IsEmpty && !Constants.Volume.ContainsCode(volumeDimensionType))
			{
				context.Add(new WarningNotification(WarningType.Warning, Res.GetString("005CC462-9B16-40A0-8330-BC2E45185EAC", "Invalid Volume Code '{0}'", volumeDimensionType)));
			}

			if (value.DimensionDetails.WidthSpecified)
			{
				context.SetPropertyInfoValue(product.OP_WidthInfo, value.DimensionDetails.Width, OrgSupplierPartSchema.OP_Width);
			}

			context.SetPropertyInfoValue(product.OP_ModelInfo, value.Model, value.ModelSpecified);
			context.SetPropertyInfoValue(product.OP_PartNumInfo, value.ProductCode, value.ProductCodeSpecified);
			context.SetPropertyInfoValue(product.OP_DescInfo, value.ProductDescription, value.ProductDescriptionSpecified);
			context.SetPropertyInfoValue(product.OP_StockKeepingUnitInfo, value.StockUnit, value.StockUnitSpecified);
			product.OP_IsActive = value.IsActive;

			ImportNotes(product, value, context);

			value.DangerousGoods.ImportToUNDGDataItems(() => product.UNDGs, "Product " + product.OP_PartNum, context);
			value.UNDG.ImportSingleItemToUNDGDataItems(() => product.UNDGs);

			if (!value.IsActive && ObjectFactory.Get<Enterprise.Integration.Customs.ZA.IZACustomsRegistry>().IsWarehouseOperatorTransactionsModuleEnabled)
			{
				context.Add(new ErrorNotification(ErrorType.Error, OrgSupplierPartValidation.CannotDeactivateAProductWithActiveTransactionAndSOHError));
			}
		}

		void ImportCurrency(OrgSupplierPart product, Xsd.Product value, IValueObjectImportContext context)
		{
			if (value.BasicStockControl.CostCurrencySpecified)
			{
				if (value.BasicStockControl.CostCurrency.IsEmpty)
				{
					product.OP_RX_NKLastWeightedCostCurr = ZString.Empty;
				}
				else
				{
					RefCurrency refCurrency = RefCurrency.LoadFromCurrencyCode(context.Factory, value.BasicStockControl.CostCurrency);
					if (refCurrency != null)
					{
						product.OP_RX_NKLastWeightedCostCurr = refCurrency.RX_Code;
					}
					else
					{
						ErrorNotification errorNotification = new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("bcd68fe1-7566-4006-b525-8e086c98a691", "Invalid Currency Code '{0}'", value.BasicStockControl.CostCurrency));
						context.Add(errorNotification);
					}
				}
			}
		}

		void ImportNotes(OrgSupplierPart product, Xsd.Product value, IValueObjectImportContext context)
		{
			new NoteValueObjectDataAdapter().ImportNotesAndAttachToBusinessObjectNotes(product.Notes, value.Notes, context);
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(OrgSupplierPart product, Xsd.Product result, IValueObjectExportContext context)
		{
			ExportBarcodes(product, result.Barcodes);
			ExportBasicStockControl(product, result.BasicStockControl);
			result.BasicStockControlSpecified = true;
			ExportBillOfMaterials(product, result, context);
			ExportClientDefinedDetails(product, result.ClientDefinedDetails);
			result.ClientDefinedDetailsSpecified = true;

			foreach (IValueObjectExport exporter in ObjectFactory.Get<IEnumerable>("ProductValueObjectExportList"))
			{
				if (exporter.CanExport(result))
				{
					exporter.Export(product, result, context);
				}
			}

			ExportDimensionDetails(product, result.DimensionDetails);
			result.DimensionDetailsSpecified = true;
			ExportRelatedOrganisations(product, result.RelatedOrganisations, context);
			ExportUNDG(product, result, context);
			ExportUnitConversions(product, result.UnitConversions);
			ExportOtherProductDetails(product, result, context);

			AddExportEvent(result, product, context);
		}

		void ExportRelatedOrganisations(OrgSupplierPart product, Xsd.RelatedOrganisationCollection relatedOrganisations, IValueObjectExportContext context)
		{
			foreach (OrgPartRelation org in product.RelatedOrganisations)
			{
				Xsd.RelatedOrganisation relatedOrganisation = new Xsd.RelatedOrganisation();

				if (!org.OU_ClientUQ.IsEmpty)
				{
					relatedOrganisation.ClientUQ = org.OU_ClientUQ;
					relatedOrganisation.ClientUQSpecified = true;
				}

				if (org.OU_Hi > 0)
				{
					relatedOrganisation.Hi = org.OU_Hi;
					relatedOrganisation.HiSpecified = true;
				}

				if (org.OU_LandedCostMarginPercent1 > 0)
				{
					relatedOrganisation.LCMarkUpPercentage1 = org.OU_LandedCostMarginPercent1;
					relatedOrganisation.LCMarkUpPercentage1Specified = true;
				}

				if (org.OU_LandedCostMarginPercent2 > 0)
				{
					relatedOrganisation.LCMarkUpPercentage2 = org.OU_LandedCostMarginPercent2;
					relatedOrganisation.LCMarkUpPercentage2Specified = true;
				}

				if (org.OU_LandedCostMarginPercent3 > 0)
				{
					relatedOrganisation.LCMarkUpPercentage3 = org.OU_LandedCostMarginPercent3;
					relatedOrganisation.LCMarkUpPercentage3Specified = true;
				}

				if (!org.OU_LocalPartDescription.IsEmpty)
				{
					relatedOrganisation.LocalProductDescription = org.OU_LocalPartDescription;
					relatedOrganisation.LocalProductDescriptionSpecified = true;
				}

				if (!org.OU_LocalPartNumber.IsEmpty)
				{
					relatedOrganisation.LocalProductNumber = org.OU_LocalPartNumber;
					relatedOrganisation.LocalProductNumberSpecified = true;
				}

				if (org.Organisation != null)
				{
					relatedOrganisation.Organisation = OrganisationDataAdapter.ExportToValueObject(org.Organisation, context);
					relatedOrganisation.OrganisationSpecified = true;
				}

				if (!org.OU_Relationship.IsEmpty)
				{
					relatedOrganisation.RelationshipType = org.OU_Relationship;
				}

				if (!org.OU_RFAttributeConfirm.IsEmpty)
				{
					relatedOrganisation.RFAttributeConfirm = RFAttrConfirmToXmlCodeMappings.Instance.GetExternalCode(org.OU_RFAttributeConfirm, "", context);
					relatedOrganisation.RFAttributeConfirmSpecified = true;
				}

				if (org.OU_RoyaltyFlatAmount > 0)
				{
					relatedOrganisation.RoyaltyFlatAmount = Xsd.FinancialValue.FromAmountAndCurrency(org.OU_RoyaltyFlatAmount, org.RoyaltyCurrency);
					relatedOrganisation.RoyaltyFlatAmountSpecified = true;
				}

				if (org.OU_RoyaltyPercent > 0)
				{
					relatedOrganisation.RoyaltyPercentage = org.OU_RoyaltyPercent;
					relatedOrganisation.RoyaltyPercentageSpecified = true;
				}

				if (org.OU_Ti > 0)
				{
					relatedOrganisation.Ti = org.OU_Ti;
					relatedOrganisation.TiSpecified = true;
				}

				if (org.OU_UsePartAttrib1)
				{
					relatedOrganisation.UseAttribute1 = org.OU_UsePartAttrib1;
					relatedOrganisation.UseAttribute1Specified = true;
				}

				if (org.OU_UsePartAttrib2)
				{
					relatedOrganisation.UseAttribute2 = org.OU_UsePartAttrib2;
					relatedOrganisation.UseAttribute2Specified = true;
				}

				if (org.OU_UsePartAttrib3)
				{
					relatedOrganisation.UseAttribute3 = org.OU_UsePartAttrib3;
					relatedOrganisation.UseAttribute3Specified = true;
				}

				if (org.OU_UseExpiryDate)
				{
					relatedOrganisation.UseExpiryDate = org.OU_UseExpiryDate;
					relatedOrganisation.UseExpiryDateSpecified = true;
				}

				if (org.OU_ConsigneeMinShelfLifeAccepted > 0)
				{
					relatedOrganisation.ConsigneeMinShelfLifeAccepted = org.OU_ConsigneeMinShelfLifeAccepted;
					relatedOrganisation.ConsigneeMinShelfLifeAcceptedSpecified = true;
				}

				if (org.OU_UsePackingDate)
				{
					relatedOrganisation.UsePackingDate = org.OU_UsePackingDate;
					relatedOrganisation.UsePackingDateSpecified = true;
				}

				relatedOrganisations.Add(relatedOrganisation);
			}
		}

		void ExportBillOfMaterials(OrgSupplierPart product, Xsd.Product result, IValueObjectExportContext context)
		{
			Xsd.BillOfMaterials billOfMaterials = new Xsd.BillOfMaterials();

			if (product.OP_CanResell)
			{
				billOfMaterials.AllowResale = product.OP_CanResell;
				billOfMaterials.AllowResaleSpecified = true;
			}
			if (product.OP_CanDisassembleKit)
			{
				billOfMaterials.AllowDisassemblyOfKit = product.OP_CanDisassembleKit;
				billOfMaterials.AllowDisassemblyOfKitSpecified = true;
			}
			if (product.OP_KitIsAutoReplenished)
			{
				billOfMaterials.AllowAutoReplenishKit = product.OP_KitIsAutoReplenished;
				billOfMaterials.AllowAutoReplenishKitSpecified = true;
			}
			if (product.OP_AutoPrintAssemblyInstructions)
			{
				billOfMaterials.AutoPrintAssemblyInstruction = product.OP_AutoPrintAssemblyInstructions;
				billOfMaterials.AutoPrintAssemblyInstructionSpecified = true;
			}

			foreach (OrgPartBOM bOM in product.BillOfMaterials)
			{
				Xsd.BOMComponentPart component = new Xsd.BOMComponentPart();
				var bOMComponentProduct = bOM.Component;

				if (bOMComponentProduct != null)
				{
					ExportBarcodes(bOMComponentProduct, component.ComponentPart.Barcodes);
					ExportBasicStockControl(bOMComponentProduct, component.ComponentPart.BasicStockControl);
					component.ComponentPart.BasicStockControlSpecified = true;
					ExportOtherProductDetails(bOMComponentProduct, component.ComponentPart, context);
					ExportClientDefinedDetails(bOMComponentProduct, component.ComponentPart.ClientDefinedDetails);

					foreach (IValueObjectExport exporter in ObjectFactory.Get<IEnumerable>("ProductValueObjectExportList"))
					{
						if (exporter.CanExport(component.ComponentPart))
						{
							exporter.Export(bOMComponentProduct, component.ComponentPart, context);
						}
					}

					ExportDimensionDetails(bOMComponentProduct, component.ComponentPart.DimensionDetails);
					component.ComponentPart.DimensionDetailsSpecified = true;
					ExportRelatedOrganisations(bOMComponentProduct, component.ComponentPart.RelatedOrganisations, context);
					ExportUNDG(bOMComponentProduct, component.ComponentPart, context);
					ExportUnitConversions(bOMComponentProduct, component.ComponentPart.UnitConversions);
					component.ComponentPartSpecified = true;
					ExportBillOfMaterials(bOMComponentProduct, component.ComponentPart, context);
				}

				component.Quantity = bOM.OE_ComponentQty;

				if (bOM.OE_CanReuse)
				{
					component.Reusable = bOM.OE_CanReuse;
					component.ReusableSpecified = true;
				}
				if (!bOM.OE_F3_NKPackType.IsEmpty)
				{
					component.StockUnit = bOM.OE_F3_NKPackType;
					component.StockUnitSpecified = true;
				}
				billOfMaterials.Components.Add(component);
			}
			result.BillOfMaterials.Add(billOfMaterials);
		}

		void ExportDimensionDetails(OrgSupplierPart product, Xsd.Dimensions dimensionDetails)
		{
			if (product.OP_Depth > 0)
			{
				dimensionDetails.Depth = product.OP_Depth;
				dimensionDetails.DepthSpecified = true;
			}

			if (!product.OP_MeasureUQ.IsEmpty)
			{
				dimensionDetails.DimensionUnit = product.OP_MeasureUQ;
				dimensionDetails.DimensionUnitSpecified = true;
			}

			if (product.OP_Weight > 0)
			{
				dimensionDetails.GrossWeight = Xsd.DimensionValue.FromAmountAndUnit(product.OP_Weight, product.OP_WeightUQ);
				dimensionDetails.GrossWeightSpecified = true;
			}

			if (product.OP_Height > 0)
			{
				dimensionDetails.Height = product.OP_Height;
				dimensionDetails.HeightSpecified = true;
			}

			if (product.OP_NetWeight > 0)
			{
				dimensionDetails.NetWeight = product.OP_NetWeight;
				dimensionDetails.NetWeightSpecified = true;
			}

			if (product.OP_Cubic > 0)
			{
				dimensionDetails.Volume = Xsd.DimensionValue.FromAmountAndUnit(product.OP_Cubic, product.OP_CubicUQ);
				dimensionDetails.VolumeSpecified = true;
			}

			if (product.OP_Width > 0)
			{
				dimensionDetails.Width = product.OP_Width;
				dimensionDetails.WidthSpecified = true;
			}
		}

		void ExportBarcodes(OrgSupplierPart product, Xsd.BarcodeCollection barcodes)
		{
			foreach (OrgSupplierPartBarcode bar in product.PartBarcodes)
			{
				Xsd.Barcode barcode = new Xsd.Barcode();

				if (!bar.PH_Barcode.IsEmpty)
				{
					barcode.BarcodeString = bar.PH_Barcode;
					barcode.BarcodeStringSpecified = true;
				}

				if (!bar.PH_F3_NKPackType.IsEmpty)
				{
					barcode.PackageUQ = bar.PH_F3_NKPackType;
					barcode.PackageUQSpecified = true;
				}

				barcodes.Add(barcode);
			}
		}

		void ExportBasicStockControl(OrgSupplierPart product, Xsd.BasicStockControl basicStockControl)
		{
			if (product.OP_LastCost > 0)
			{
				basicStockControl.LastCost = product.OP_LastCost;
				basicStockControl.LastCostSpecified = true;
			}

			if (product.OP_QtyInStock > 0)
			{
				basicStockControl.QtyInStock = product.OP_QtyInStock;
				basicStockControl.QtyInStockSpecified = true;
			}

			if (product.OP_WeightedCost > 0)
			{
				basicStockControl.WeightedCost = product.OP_WeightedCost;
				basicStockControl.WeightedCostSpecified = true;
			}

			if (!product.OP_RX_NKLastWeightedCostCurr.IsEmpty)
			{
				basicStockControl.CostCurrency = product.OP_RX_NKLastWeightedCostCurr;
				basicStockControl.CostCurrencySpecified = true;
			}
		}

		void ExportClientDefinedDetails(OrgSupplierPart product, Xsd.ClientDefinedFields clientDefinedDetails)
		{
			if (!product.OP_Department.IsEmpty)
			{
				clientDefinedDetails.Department = product.OP_Department;
				clientDefinedDetails.DepartmentSpecified = true;
			}

			if (!product.OP_Division.IsEmpty)
			{
				clientDefinedDetails.Division = product.OP_Division;
				clientDefinedDetails.DivisionSpecified = true;
			}

			if (product.OP_VendorPackQty > 0)
			{
				clientDefinedDetails.VendorPack = Xsd.DimensionValue.FromAmountAndUnit(product.OP_VendorPackQty, product.OP_F3_NKPackType);
				clientDefinedDetails.VendorPackSpecified = true;
			}

			if (product.OP_OrderMultipleQty > 0)
			{
				clientDefinedDetails.OrderMultipleQty = Xsd.DimensionValue.FromAmountAndUnit(product.OP_OrderMultipleQty, product.OP_OrderMultipleUnit);
				clientDefinedDetails.OrderMultipleQtySpecified = true;
			}
		}

		void ExportUnitConversions(OrgSupplierPart product, Xsd.UnitConversionCollection unitConversions)
		{
			foreach (OrgPartUnit unit in product.PartUnits)
			{
				Xsd.UnitConversion unitConversion = new Xsd.UnitConversion();
				unitConversion.Package = Xsd.DimensionValue.FromAmountAndUnit(unit.OF_QuantityInParent, unit.OF_PackType);
				unitConversion.PackageSpecified = true;

				unitConversion.ParentUQ = unit.OF_ParentPackType;
				unitConversion.ParentUQSpecified = true;

				unitConversions.Add(unitConversion);
			}
		}

		void ExportOtherProductDetails(OrgSupplierPart product, Xsd.Product result, IValueObjectExportContext context)
		{
			if (product.OP_CountDecimalPlaces > 0)
			{
				result.DecimalPlaces = ZShort.ParseSafe(product.OP_CountDecimalPlaces.ToString(), 0);
				result.DecimalPlacesSpecified = true;
			}

			if (!product.OP_Brand.IsEmpty)
			{
				result.BrandName = product.OP_Brand;
				result.BrandNameSpecified = true;
			}

			if (!product.OP_Model.IsEmpty)
			{
				result.Model = product.OP_Model;
				result.ModelSpecified = true;
			}

			result.ProductCode = product.OP_PartNum;
			result.ProductCodeSpecified = true;
			result.ProductDescription = product.OP_Desc;
			result.StockUnit = product.OP_StockKeepingUnit;
			result.StockUnitSpecified = true;
			result.IsActive = product.OP_IsActive;

			ExportNotes(product, result, context);
		}

		void ExportNotes(OrgSupplierPart product, Xsd.Product result, IValueObjectExportContext context)
		{
			result.Notes = new NoteValueObjectDataAdapter().ExportToXmlValueObjectCollection(product.Notes, context);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		void ExportUNDG(OrgSupplierPart product, Xsd.Product result, IValueObjectExportContext context)
		{
			result.DangerousGoods.ExportFromUNDGDataItems(product.UNDGs.ToArray(), "Product " + product.OP_PartNum, context);
			result.UNDG.ExportSingleItemFromUNDGDataItems(product.UNDGs.ToArray());
		}

		#endregion

		public override XmlSchema Schema
		{
			get { return CustomsXmlSchemaDefinitions.Instance.ProductsSchema; }
		}

		OrganisationValueObjectDataAdapter OrganisationDataAdapter
		{
			get { return organisationDataAdapter ?? (organisationDataAdapter = new OrganisationValueObjectDataAdapter()); }
		}
		OrganisationValueObjectDataAdapter organisationDataAdapter;

		ZString reasonWhyProductUpdateSkippedText { get; set; }
		bool shouldImportFromValueObject;
	}
}
