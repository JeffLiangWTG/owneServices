using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters
{
	public class SysMergeOrganisationValueObjectDataAdapter : ValueObjectDataAdapter<OrgHeaderForDataTransfer, Xsd.SysMergeOrganisation>
	{
		public SysMergeOrganisationValueObjectDataAdapter()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public override string RootCollectionElementName
		{
			get { return "Organisations"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public override string RootElementName
		{
			get { return "Organisation"; }
		}

		public override XmlSchema Schema
		{
			get { return XmlSchemaDefinitions.Instance.SingleOrganisationSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return null; }
		}

		#region FindBusinessObject

		protected override OrgHeaderForDataTransfer FindBusinessObject(Xsd.SysMergeOrganisation value, IValueObjectImportContext context)
		{
			return FindOrganisationByPk(value, context);
		}

		OrgHeaderForDataTransfer FindOrganisationByPk(Xsd.SysMergeOrganisation xsdOrg, IValueObjectImportContext context)
		{
			ZGuid pkTofind = new ZGuid(xsdOrg.OrganisationDetail.OrgHeader.PK);
			return context.Factory.Load<OrgHeaderForDataTransfer>(pkTofind);
		}

		#endregion

		#region ImportFromValueObject

		protected override OrgHeaderForDataTransfer NewBusinessObject(Xsd.SysMergeOrganisation value, IValueObjectImportContext context)
		{
			return context.Factory.NewWithPrimaryKey<OrgHeaderForDataTransfer>(new Guid(value.OrganisationDetail.OrgHeader.PK));
		}

		protected override bool ConfirmUpdateOfExistingBusinessObject(OrgHeaderForDataTransfer obj, INotifications notifications)
		{
			return false;
		}

		protected override void OnUserDeclinedImport(OrgHeaderForDataTransfer bizObj, Xsd.SysMergeOrganisation value, IValueObjectImportContext context)
		{
			string message = Res.GetString("89ecea85-d439-45ee-be7d-817951551444", "Import of organization [({0}) - {1} - {2}] skipped. Reason: Organization already exists.", bizObj.PK.ToString(), bizObj.OH_Code, bizObj.OH_FullName) + "\r\n";
			context.Notify(new InfoNotification(message));
		}

		protected override void NotifyBizObjCreatedOrUpdated(INotifications notifications, BusinessObject bizObj)
		{
			// Don't notify here. It will be notified if save succeeds.
		}

		protected override void ImportFromValueObjectCore(OrgHeaderForDataTransfer bizObj, Xsd.SysMergeOrganisation value, IValueObjectImportContext context)
		{
			string errorContext = Res.GetString("4a431794-2fc9-4a8a-b38a-1c22d3b51eda", "Organization with code '{0}'", value.OrganisationDetail.OrgHeader.Code);
			ImportOrganisationDetails(bizObj, value.OrganisationDetail, context, errorContext);
			EnsureCodeExistsAndIsUnique(bizObj);

			AddImportEvent(bizObj);
		}

		void ImportOrganisationDetails(OrgHeaderForDataTransfer org, Xsd.SysMergeOrganisationDetail organisationDetailsValue, IValueObjectImportContext context, string errorContext)
		{
			if (organisationDetailsValue.IsSpecified)
			{
				ImportRegistrationNumbers(org, organisationDetailsValue.OrgCusCodes, context);
				ImportEDICodeMapping(org, organisationDetailsValue, context, errorContext);

				SysMergeOrgHeaderValueObjectHelper orgHeaderHelper = new SysMergeOrgHeaderValueObjectHelper(errorContext);
				orgHeaderHelper.ImportFromValueObject(organisationDetailsValue.OrgHeader, org, context);

				SysMergeContactValueObjectHelper contactHelper = new SysMergeContactValueObjectHelper(errorContext);
				contactHelper.ImportFromValueObjectCollection(organisationDetailsValue.OrgContacts, org, context);

				SysMergeOrgMiscServValueObjectHelper orgMiscServHelper = new SysMergeOrgMiscServValueObjectHelper(errorContext);
				orgMiscServHelper.ImportFromValueObject(organisationDetailsValue.OrgMiscServ, org, context);

				SysMergeAddressValueObjectHelper addressHelper = new SysMergeAddressValueObjectHelper(errorContext);
				addressHelper.ImportFromValueObjectCollection(organisationDetailsValue.OrgAddresses, org, context);

				SysMergeOrgWebUrlValueObjectHelper webUrlHelper = new SysMergeOrgWebUrlValueObjectHelper(errorContext);
				webUrlHelper.ImportFromValueObjectCollection(organisationDetailsValue.OrgWebUrls, org, context);

				SysMergeCompanyDataValueObjectHelper companyDataHelper = new SysMergeCompanyDataValueObjectHelper(errorContext);
				companyDataHelper.ImportFromValueObjectCollection(organisationDetailsValue.OrgCompanyDatas, org, context, importCompanyMapper);

				SysMergeOrgCountryDataValueObjectHelper countryDataHelper = new SysMergeOrgCountryDataValueObjectHelper();
				countryDataHelper.ImportFromValueObjectCollection(organisationDetailsValue.OrgCountryDatas, org, context);

				SysMergeOrgStaffAssignmentValueObjectHelper staffAssignmentsHelper = new SysMergeOrgStaffAssignmentValueObjectHelper();
				staffAssignmentsHelper.ImportFromValueObjectCollection(organisationDetailsValue.OrgStaffAssignments, org, context);

				SysMergeOrgNoteValueObjectHelper noteHelper = new SysMergeOrgNoteValueObjectHelper();
				noteHelper.ImportFromValueObjectCollection(organisationDetailsValue.OrgNotes, org, context);

				ImportOrgRealatedParties(org, organisationDetailsValue, context, errorContext);
				ImportBrandAndCompanyNames(org, organisationDetailsValue);
				ImportOrgCustomLabels(org, organisationDetailsValue, context);
				ImportOrgEDICommunication(organisationDetailsValue, context);
				ImportWhsClientParametersByWarehouse(org, organisationDetailsValue, context, errorContext);
			}
		}

		void ImportOrgRealatedParties(OrgHeaderForDataTransfer organisation, Xsd.SysMergeOrganisationDetail xsdOrg, IValueObjectImportContext context, string errorContext)
		{
			foreach (Xsd.OrgRelatedParty xsdRelatedParty in xsdOrg.OrgRelatedParties)
			{
				ZGuid relatedOrgPk = new ZGuid(xsdRelatedParty.OH_RelatedParty_PK);
				OrgHeaderForDataTransfer relatedOrg = organisation.Factory.Load<OrgHeaderForDataTransfer>(relatedOrgPk);

				if (relatedOrg != null)
				{
					if (xsdRelatedParty.GC_Code.IsEmpty)
					{
						AddNewOrgRelatedPartySafe(organisation, xsdRelatedParty, relatedOrgPk, context);
					}
					else
					{
						ZString mappedCompanyCode = importCompanyMapper.GetMappedCode(xsdRelatedParty.GC_Code);

						if (mappedCompanyCode.IsEmpty)
						{
							string skipMessage = Res.GetString("9f98f643-c734-4b20-a7a8-a5c66b9babde", "{0}: related party info skipped for company [{1}].",
								errorContext, xsdRelatedParty.GC_Code);
							context.Notify(new InfoNotification(skipMessage));
						}
						else
						{
							OrgRelatedParty relatedParty = AddNewOrgRealatedParty(organisation, xsdRelatedParty, relatedOrgPk);
							GlbCompany company = importCompanyMapper.GetCompanyByCodeThrowingErrorIfNotFound(organisation.Factory, mappedCompanyCode);
							relatedParty.PR_GC = company.PK;
						}
					}
				}
			}
		}

		OrgRelatedParty AddNewOrgRealatedParty(OrgHeaderForDataTransfer organisation, Xsd.OrgRelatedParty xsdRelatedParty, ZGuid relatedOrgPk)
		{
			OrgRelatedParty relatedParty = organisation.Factory.New<OrgRelatedParty>();
			relatedParty.PR_OH_Parent = organisation.PK;

			relatedParty.PR_FreightDirection = xsdRelatedParty.FreightDirection;
			relatedParty.PR_FreightTransportMode = xsdRelatedParty.FreightMode;
			relatedParty.PR_FreightContainerMode = xsdRelatedParty.FreightContainerMode;
			relatedParty.PR_OH_RelatedParty = relatedOrgPk;
			relatedParty.PR_PartyType = xsdRelatedParty.PartyType;
			relatedParty.PR_Service = xsdRelatedParty.Service;
			relatedParty.PR_Location = xsdRelatedParty.PR_Location;

			return relatedParty;
		}

		OrgRelatedParty AddNewOrgRelatedPartySafe(OrgHeaderForDataTransfer organisation, Xsd.OrgRelatedParty xsdRelatedParty, ZGuid relatedOrgPk, IValueObjectImportContext context)
		{
			var relatedParty = AddNewOrgRealatedParty(organisation, xsdRelatedParty, relatedOrgPk);

			ZQuery query = new ZQuery();
			query.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, relatedParty.PR_OH_Parent);
			if (relatedParty.PR_GC.IsValid)
			{
				query.AddToFilter(OrgRelatedPartySchema.PR_GC, relatedParty.PR_GC);
			}
			else
			{
				query.AddToFilter(OrgRelatedPartySchema.PR_GC, SQLComparisonOperator.Equal, null);
			}
			query.AddToFilter(OrgRelatedPartySchema.PR_PartyType, relatedParty.PR_PartyType);
			query.AddToFilter(OrgRelatedPartySchema.PR_FreightTransportMode, relatedParty.PR_FreightTransportMode);
			query.AddToFilter(OrgRelatedPartySchema.PR_FreightContainerMode, relatedParty.PR_FreightContainerMode);
			query.AddToFilter(OrgRelatedPartySchema.PR_FreightDirection, relatedParty.PR_FreightDirection);
			query.AddToFilter(OrgRelatedPartySchema.PR_Service, relatedParty.PR_Service);
			query.AddToFilter(OrgRelatedPartySchema.PR_Location, relatedParty.PR_Location);
			query.AddToFilter(OrgRelatedPartySchema.PR_OA, relatedParty.PR_OA);
			if (organisation.Factory.Load<OrgRelatedParty>(query).Length > 1)
			{
				string errorMessage = string.Format("Cannot create related party info for organization [({0}) - {1} - {2}]. Reason: Cannot insert duplicate key row in object 'dbo.OrgRelatedParty' with unique index 'NR_UX__PR_OH_Parent_PR_GC_PR_PartyType_PR_FreightTransportMode_PR_FreightContainerMode_PR_FreightDirection_PR_Service_PR_Locatio'. The duplicate key value is ({3}). OH_RelatedParty is {4}.",
					organisation.PK, organisation.OH_Code, organisation.OH_FullName,
					string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}", relatedParty.PR_OH_Parent, relatedParty.PR_GC, relatedParty.PR_PartyType, relatedParty.PR_FreightTransportMode, relatedParty.PR_FreightContainerMode, relatedParty.PR_FreightDirection, relatedParty.PR_Service, relatedParty.PR_Location, relatedParty.PR_OA), relatedParty.PR_OH_RelatedParty);
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, errorMessage));
				relatedParty.Delete();
			}

			return relatedParty;
		}

		void ImportRegistrationNumbers(OrgHeaderForDataTransfer organisation, Xsd.SysMergeOrgCusCodeCollection xsdCustomsCodeCollection, IValueObjectImportContext context)
		{
			if (xsdCustomsCodeCollection.IsSpecified)
			{
				foreach (Xsd.SysMergeOrgCusCode xsdCustomsCode in xsdCustomsCodeCollection)
				{
					OrgCusCode cusCode = organisation.Factory.New<OrgCusCode>();
					cusCode.OK_OH = organisation.PK;

					cusCode.OK_CustomsRegNo = xsdCustomsCode.CustomsRegNo;
					cusCode.OK_CodeType = xsdCustomsCode.CodeType;
					cusCode.OK_CountryDefault = xsdCustomsCode.CountryDefault;

					if (!xsdCustomsCode.RefCountryNk.IsEmpty)
					{
						RefCountry country = context.GetCountryByCodeThrowingErrorIfNotFound(organisation.Factory, xsdCustomsCode.RefCountryNk);
						cusCode.OK_RN_NKCodeCountry = country.Code;
					}

					if (!xsdCustomsCode.OrgAddressPK.IsEmpty)
					{
						cusCode.OK_OA_PremisesAddress = new ZGuid(xsdCustomsCode.OrgAddressPK);
					}
				}
			}
		}

		void ImportEDICodeMapping(OrgHeaderForDataTransfer organisation, Xsd.SysMergeOrganisationDetail xsdOrg, IValueObjectImportContext context, string errorContext)
		{
			foreach (Xsd.SysMergeEDICodeMapping codeMapping in xsdOrg.EDICodeMappings)
			{
				bool isGuid = true;
				bool isCode = false;

				ZGuid localGuid = ZGuid.Empty;
				switch (codeMapping.Relationship)
				{
					case Constants.OrgPatternMatchOverrideRelationships.Organisation:
						OrgHeaderForDataTransfer loadedOrg = context.Factory.LoadFromNaturalKey<OrgHeaderForDataTransfer>(OrgHeaderSchema.OH_Code, codeMapping.EDICode);
						if (loadedOrg != null)
						{
							localGuid = loadedOrg.PK;
						}
						break;
					case Constants.OrgPatternMatchOverrideRelationships.Port:
						RefUNLOCO loadedPort = context.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, codeMapping.EDICode);
						if (loadedPort != null)
						{
							localGuid = loadedPort.PK;
						}
						break;
					case Constants.OrgPatternMatchOverrideRelationships.Currency:
						RefCurrency loadedCurrency = RefCurrency.LoadFromCurrencyCode(context.Factory, codeMapping.EDICode);
						if (loadedCurrency != null)
						{
							localGuid = loadedCurrency.PK;
						}
						break;
					case Constants.OrgPatternMatchOverrideRelationships.Country:
						RefCountry loadedCountry = RefCountry.LoadFromCountryCode(context.Factory, codeMapping.EDICode);
						if (loadedCountry != null)
						{
							localGuid = loadedCountry.PK;
						}
						break;
					case Constants.OrgPatternMatchOverrideRelationships.ContainerType:
						RefContainer loadedContainer = new RefContainer.Loader(context.Factory).LoadFromCode(codeMapping.EDICode);
						if (loadedContainer != null)
						{
							localGuid = loadedContainer.PK;
						}
						break;
					case Constants.OrgPatternMatchOverrideRelationships.IncoTerm:
						isGuid = false;
						isCode = true;
						break;
				}

				if (isCode || (isGuid && !localGuid.IsEmpty))
				{
					OrgPatternMatchOverride patternMatch = organisation.Factory.New<OrgPatternMatchOverride>();
					patternMatch.OO_OH = organisation.PK;

					patternMatch.OO_Relationship = codeMapping.Relationship;
					patternMatch.OO_ForeignCode = codeMapping.ForeignCode;

					if (isCode)
					{
						patternMatch.OO_LocalCode = codeMapping.ForeignCode;
					}
					else
					{
						patternMatch.OO_LocalGuid = localGuid;
					}
				}
			}
		}

		void ImportBrandAndCompanyNames(OrgHeaderForDataTransfer organisation, Xsd.SysMergeOrganisationDetail xsdOrg)
		{
			for (int i = 0; i < xsdOrg.BrandNames.Count; i++)
			{
				Xsd.SysMergeOrganisationDetailBrandName xsdBrandName = xsdOrg.BrandNames[i];

				OrgBrandOrRelatedName brandName = organisation.Factory.New<OrgBrandOrRelatedName>();
				brandName.P1_OH = organisation.PK;
				brandName.P1_RelatedName = xsdBrandName.Value;
			}
		}

		#region ImportOrgCustomLabels

		void ImportOrgCustomLabels(OrgHeaderForDataTransfer organisation, Xsd.SysMergeOrganisationDetail xsdOrganisation, IValueObjectImportContext context)
		{
			foreach (Xsd.SysMergeCustomLabel xsdCustomLabel in xsdOrganisation.CustomLabels)
			{
				var customLabel = context.Factory.NewWithPrimaryKey<OrgCustomLabels>(new Guid(xsdCustomLabel.PK));
				customLabel.OT_OH = organisation.PK;

				context.SetPropertyInfoValueIfValueNotEmpty(customLabel.OT_TypeInfo, xsdCustomLabel.Type);
				context.SetPropertyInfoValueIfValueNotEmpty(customLabel.OT_FieldNameInfo, xsdCustomLabel.FieldName);
				context.SetPropertyInfoValueIfValueNotEmpty(customLabel.OT_CaptionInfo, xsdCustomLabel.Caption);
				context.SetPropertyInfoValueIfValueNotEmpty(customLabel.OT_HintInfo, xsdCustomLabel.Hint);
				context.SetPropertyInfoValueIfValueNotEmpty(customLabel.OT_RuleInfo, xsdCustomLabel.Rule);

				context.SetPropertyInfoValueIfValueNotEmpty(customLabel.OT_PositionInfo, xsdCustomLabel.Position.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(customLabel.OT_ColumnSizeInfo, xsdCustomLabel.ColumnSize.ToString());

				context.SetPropertyInfoValueIfValueNotEmpty(customLabel.OT_IsMandatoryInfo, xsdCustomLabel.IsMandatory.ToString());
			}
		}

		#endregion

		#region ImportOrgEDICommunication

		void ImportOrgEDICommunication(Xsd.SysMergeOrganisationDetail xsdOrganisation, IValueObjectImportContext context)
		{
			foreach (Xsd.SysMergeEDICommunicationsMode xsdEdiCommunication in xsdOrganisation.EDICommunicationsMode)
			{
				var customEdiCommunication = context.Factory.NewWithPrimaryKey<EDICommunicationsMode>(new Guid(xsdEdiCommunication.PK));
				customEdiCommunication.EK_CommsDirection = xsdEdiCommunication.CommsDirection;
				customEdiCommunication.EK_CommunicationsTransport = xsdEdiCommunication.CommunicationsTransport;
				customEdiCommunication.EK_Destination = xsdEdiCommunication.Destination;
				customEdiCommunication.EK_DestinationFolder = xsdEdiCommunication.DestinationFolder;
				customEdiCommunication.EK_FileFormat = xsdEdiCommunication.FileFormat;
				customEdiCommunication.EK_Filename = xsdEdiCommunication.Filename;
				customEdiCommunication.EK_FtpLockingMethod = xsdEdiCommunication.FtpLockingMethod;
				customEdiCommunication.EK_LocalPartyVanID = xsdEdiCommunication.LocalPartyVanID;
				customEdiCommunication.EK_LoginName = xsdEdiCommunication.LoginName;
				customEdiCommunication.EK_MessagePurpose = xsdEdiCommunication.MessagePurpose;
				customEdiCommunication.EK_Module = xsdEdiCommunication.Module;
				customEdiCommunication.EK_ParentID = new ZGuid(xsdEdiCommunication.ParentID);
				customEdiCommunication.EK_ParentTableCode = xsdEdiCommunication.ParentTableCode;
				customEdiCommunication.EK_Password = xsdEdiCommunication.Password;
				customEdiCommunication.EK_PortNumber = xsdEdiCommunication.PortNumber;
				customEdiCommunication.EK_PublishInternalMilestones = xsdEdiCommunication.PublishInternalMilestones;
				customEdiCommunication.EK_RelatedPartyVanID = xsdEdiCommunication.RelatedPartyVanID;
				customEdiCommunication.EK_ServerAddressSubject = xsdEdiCommunication.ServerAddressSubject;
				customEdiCommunication.EK_SourceFolder = xsdEdiCommunication.SourceFolder;

				//EK_GG and EK_OH_MessageVAN are excluded for FK constraints
				customEdiCommunication.EK_LastFailed = new ZDateTime(xsdEdiCommunication.LastFailed);
			}
		}

		#endregion

		#region ImportWhsClientParametersByWarehouse

		void ImportWhsClientParametersByWarehouse(OrgHeaderForDataTransfer organisation, Xsd.SysMergeOrganisationDetail organisationDetailsValue, IValueObjectImportContext context, string errorContext)
		{
			var allExistingWhsClientParams = LoadExistingClientParametersByWarehouse(organisation);
			var allWarehouses = LoadAllExistingWarehouses(organisation.Factory);

			foreach (Xsd.SysMergeWhsClientParameterByWarehouse xsdWhsClientParam in organisationDetailsValue.WhsClientParametersByWarehouse)
			{
				var clientParamPK = new ZGuid(xsdWhsClientParam.PK);
				if (!allExistingWhsClientParams.ContainsKey(clientParamPK))
				{
					var warehousePK = new ZGuid(xsdWhsClientParam.WarehousePK);
					if (!allWarehouses.Contains(warehousePK))
					{
						var message = Res.GetString("7000d815-0939-4130-ab87-46ce3f990d43", "{0}: warehouse parameter skipped for warehouse [{1}].", errorContext, xsdWhsClientParam.WarehousePK);
						context.Notify(new InfoNotification(message));
					}
					else
					{
						var clientParamBizOType = ObjectFactory.GetType<IWhsClientParameterByWarehouse>();
						var clientParam = (IWhsClientParameterByWarehouse)organisation.Factory.New(clientParamBizOType, clientParamPK.ToGuid());
						clientParam.WY_OH_Client = organisation.PK;
						clientParam.WY_WW_Whs = warehousePK;
					}
				}
			}
		}

		Dictionary<ZGuid, IWhsClientParameterByWarehouse> LoadExistingClientParametersByWarehouse(OrgHeaderForDataTransfer organisation)
		{
			var result = new Dictionary<ZGuid, IWhsClientParameterByWarehouse>();
			var query = new ZQuery(WhsClientParameterByWarehouseSchema.WY_OH_Client, organisation.PK);
			var clientParams = organisation.Factory.Load<IWhsClientParameterByWarehouse>(query);
			foreach (var clientParam in clientParams)
			{
				result.Add(clientParam.PK, clientParam);
			}
			return result;
		}

		IEnumerable<ZGuid> LoadAllExistingWarehouses(BusinessObjectFactory factory)
		{
			return factory.Load<IWhsWarehouse>(new ZQuery()).Cast<BusinessObject>().Select(w => w.PK);
		}

		#endregion

		#region EnsureCodeExistsAndIsUnique

		void EnsureCodeExistsAndIsUnique(OrgHeaderForDataTransfer organisation)
		{
			if (!organisation.OH_Code.IsEmpty)
			{
				EnsureOrgCodeIsUnique(organisation);
			}
		}

		void EnsureOrgCodeIsUnique(OrgHeaderForDataTransfer org)
		{
			// TODO: IMPROVE THIS ALGORITHM 1 ZDBONLYQUERY IS ENOUGH
			ZString originalCode = org.OH_Code;
			int i = 1;
			bool foundDuplicate = true;
			while (foundDuplicate)
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(OrgHeaderSchema.OH_Code, SQLComparisonOperator.Equal, org.OH_Code);
				filter.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, org.PK);
				OrgHeaderForDataTransfer[] existingOrgs = (OrgHeaderForDataTransfer[])org.Factory.Load(typeof(OrgHeaderForDataTransfer), filter);

				foundDuplicate = false;
				foreach (OrgHeaderForDataTransfer existingOrg in existingOrgs)
				{
					if (existingOrg.OH_Code == org.OH_Code)
					{
						foundDuplicate = true;
						org.OH_Code = originalCode.SubstringSafe(0, org.OH_CodeInfo.MaxLength - i.ToString().Length) + i.ToString();
						i++;
						break;
					}
				}
			}
		}

		#endregion

		readonly SysMergeCompanyMapper importCompanyMapper = new SysMergeCompanyMapper();

		#endregion

		#region ExportToValueObject

		protected override void ExportToValueObjectCore(OrgHeaderForDataTransfer bizObj, Xsd.SysMergeOrganisation constructedValueObject, IValueObjectExportContext context)
		{
			string errorContext = Res.GetString("4a431794-2fc9-4a8a-b38a-1c22d3b51eda", "Organization with code '{0}'", bizObj.OH_Code);

			var result = constructedValueObject;
			result.OrganisationDetail = new Xsd.SysMergeOrganisationDetail();

			SysMergeOrgHeaderValueObjectHelper orgHeaderHelper = new SysMergeOrgHeaderValueObjectHelper(errorContext);
			orgHeaderHelper.ExportToValueObject(bizObj, result.OrganisationDetail.OrgHeader, context);

			SysMergeOrgMiscServValueObjectHelper orgMiscServHelper = new SysMergeOrgMiscServValueObjectHelper(errorContext);
			orgMiscServHelper.ExportToValueObject(bizObj, result.OrganisationDetail.OrgMiscServ, context);

			SysMergeAddressValueObjectHelper addressHelper = new SysMergeAddressValueObjectHelper(errorContext);
			addressHelper.ExportToValueObjectCollection(bizObj, result.OrganisationDetail.OrgAddresses, context);

			SysMergeContactValueObjectHelper contactHelper = new SysMergeContactValueObjectHelper(errorContext);
			contactHelper.ExportToValueObjectCollection(bizObj, result.OrganisationDetail.OrgContacts, context);

			SysMergeOrgWebUrlValueObjectHelper webUrlHelper = new SysMergeOrgWebUrlValueObjectHelper(errorContext);
			webUrlHelper.ExportToValueObjectCollection(bizObj, result.OrganisationDetail.OrgWebUrls, context);

			SysMergeCompanyDataValueObjectHelper companyDataHelper = new SysMergeCompanyDataValueObjectHelper(errorContext);
			companyDataHelper.ExportToValueObjectCollection(bizObj, result.OrganisationDetail.OrgCompanyDatas);

			SysMergeOrgCountryDataValueObjectHelper countryDataHelper = new SysMergeOrgCountryDataValueObjectHelper();
			countryDataHelper.ExportToValueObjectCollection(bizObj, result.OrganisationDetail.OrgCountryDatas, context);

			SysMergeOrgStaffAssignmentValueObjectHelper staffAssignmentsHelper = new SysMergeOrgStaffAssignmentValueObjectHelper();
			staffAssignmentsHelper.ExportToValueObjectCollection(bizObj, result.OrganisationDetail.OrgStaffAssignments, context);

			SysMergeOrgNoteValueObjectHelper notesHelper = new SysMergeOrgNoteValueObjectHelper();
			notesHelper.ExportToValueObjectCollection(bizObj, result.OrganisationDetail.OrgNotes, context);

			ExportRegistrationNumbers(bizObj, result.OrganisationDetail);
			ExportOrgRelatedParty(bizObj, result.OrganisationDetail);
			ExportBrandAndCompanyNames(bizObj, result.OrganisationDetail);
			ExportOrgCustomLabels(bizObj, result.OrganisationDetail);
			ExportOrgEDICommunication(bizObj, result.OrganisationDetail);
			ExportWhsClientParametersByWarehouse(bizObj, result.OrganisationDetail);
		}

		/// <summary>
		/// Export OrgCusCode records
		/// </summary>
		void ExportRegistrationNumbers(OrgHeaderForDataTransfer organisation, Xsd.SysMergeOrganisationDetail xsdOrg)
		{
			ZQuery query = new ZQuery(OrgCusCodeSchema.OK_OH, organisation.PK);
			OrgCusCode[] customsCodes = organisation.Factory.Load<OrgCusCode>(query);

			for (int i = 0; i < customsCodes.Length; i++)
			{
				if (!customsCodes[i].OK_CustomsRegNo.Trim().IsEmpty)
				{
					Xsd.SysMergeOrgCusCode xsdCustomsCode = xsdOrg.OrgCusCodes.AddNew();
					RefCountry country = customsCodes[i].CodeCountry;
					xsdCustomsCode.RefCountryNk = (country == null) ? null : country.Code;
					xsdCustomsCode.OrgHeaderPK = organisation.PK.ToString();

					if (!customsCodes[i].OK_OA_PremisesAddress.IsEmpty)
					{
						xsdCustomsCode.OrgAddressPK = customsCodes[i].OK_OA_PremisesAddress.ToString();
					}

					xsdCustomsCode.CustomsRegNo = customsCodes[i].OK_CustomsRegNo;
					xsdCustomsCode.CodeType = customsCodes[i].OK_CodeType;

					if (customsCodes[i].OK_CountryDefault)
					{
						xsdCustomsCode.CountryDefault = customsCodes[i].OK_CountryDefault;
						xsdCustomsCode.CountryDefaultSpecified = true;
					}
				}
			}
		}

		void ExportOrgRelatedParty(OrgHeaderForDataTransfer organisation, Xsd.SysMergeOrganisationDetail xsdOrg)
		{
			ZQuery query = new ZQuery(OrgRelatedPartySchema.PR_OH_Parent, organisation.PK);
			OrgRelatedParty[] relatedParties = organisation.Factory.Load<OrgRelatedParty>(query);

			foreach (OrgRelatedParty relatedParty in relatedParties)
			{
				Xsd.OrgRelatedParty xsdRelatedparty = xsdOrg.OrgRelatedParties.AddNew();

				xsdRelatedparty.FreightDirection = relatedParty.PR_FreightDirection;
				xsdRelatedparty.FreightMode = relatedParty.PR_FreightTransportMode;
				xsdRelatedparty.FreightContainerMode = relatedParty.PR_FreightContainerMode;
				xsdRelatedparty.OH_Parent_PK = relatedParty.PR_OH_Parent.ToString();
				xsdRelatedparty.OH_RelatedParty_PK = relatedParty.PR_OH_RelatedParty.ToString();
				xsdRelatedparty.PartyType = relatedParty.PR_PartyType;
				xsdRelatedparty.Service = relatedParty.PR_Service;
				xsdRelatedparty.PR_Location = relatedParty.PR_Location;

				if (!relatedParty.PR_GC.IsEmpty)
				{
					xsdRelatedparty.GC_Code = relatedParty.Factory.Load<GlbCompany>(relatedParty.PR_GC).GC_Code;
				}
			}
		}

		void ExportBrandAndCompanyNames(OrgHeaderForDataTransfer organisation, Xsd.SysMergeOrganisationDetail xsdOrg)
		{
			ZQuery query = new ZQuery(OrgBrandOrRelatedNameSchema.P1_OH, organisation.PK);
			OrgBrandOrRelatedName[] brandNames = organisation.Factory.Load<OrgBrandOrRelatedName>(query);

			foreach (OrgBrandOrRelatedName brandName in brandNames)
			{
				Xsd.SysMergeOrganisationDetailBrandName xsdBrandName = xsdOrg.BrandNames.AddNew();
				xsdBrandName.OrgHeaderPK = brandName.P1_OH.ToString();
				xsdBrandName.Value = brandName.P1_RelatedName;
				xsdBrandName.ValueSpecified = true;
			}
		}

		#region ExportOrgCustomLabels

		void ExportOrgCustomLabels(OrgHeaderForDataTransfer organisation, Xsd.SysMergeOrganisationDetail xsdOrganisation)
		{
			var query = new ZQuery(OrgCustomLabelsSchema.OT_OH, organisation.PK);
			var customLabels = organisation.Factory.Load<OrgCustomLabels>(query);
			foreach (var customLabel in customLabels)
			{
				var xsdCustomLabel = xsdOrganisation.CustomLabels.AddNew();
				xsdCustomLabel.PK = customLabel.PK.ToString();

				xsdCustomLabel.Type = customLabel.OT_Type;
				xsdCustomLabel.FieldName = customLabel.OT_FieldName;
				xsdCustomLabel.Caption = customLabel.OT_Caption;
				xsdCustomLabel.Hint = customLabel.OT_Hint;
				xsdCustomLabel.Rule = customLabel.OT_Rule;

				xsdCustomLabel.Position = customLabel.OT_Position;
				xsdCustomLabel.ColumnSize = customLabel.OT_ColumnSize;

				if (customLabel.OT_IsMandatory)
				{
					xsdCustomLabel.IsMandatory = xsdCustomLabel.IsMandatorySpecified = true;
				}
			}
		}

		#endregion

		#region ExportOrgEDICommunication

		void ExportOrgEDICommunication(OrgHeaderForDataTransfer organisation, Xsd.SysMergeOrganisationDetail xsdOrganisation)
		{
			var ediCommunications = organisation.Factory.Load<EDICommunicationsMode>(new ZQuery(EDICommunicationsModeSchema.EK_ParentID, organisation.PK));
			foreach (var ediCommunication in ediCommunications)
			{
				var xsdCommunication = xsdOrganisation.EDICommunicationsMode.AddNew();
				xsdCommunication.PK = ediCommunication.PK.ToString();
				xsdCommunication.CommsDirection = ediCommunication.EK_CommsDirection;
				xsdCommunication.CommunicationsTransport = ediCommunication.EK_CommunicationsTransport;
				xsdCommunication.Destination = ediCommunication.EK_Destination;
				xsdCommunication.DestinationFolder = ediCommunication.EK_DestinationFolder;
				xsdCommunication.FileFormat = ediCommunication.EK_FileFormat;
				xsdCommunication.Filename = ediCommunication.EK_Filename;
				xsdCommunication.FtpLockingMethod = ediCommunication.EK_FtpLockingMethod;
				xsdCommunication.LocalPartyVanID = ediCommunication.EK_LocalPartyVanID;
				xsdCommunication.LoginName = ediCommunication.EK_LoginName;
				xsdCommunication.MessagePurpose = ediCommunication.EK_MessagePurpose;
				xsdCommunication.Module = ediCommunication.EK_Module;
				xsdCommunication.ParentID = ediCommunication.EK_ParentID.ToString();
				xsdCommunication.ParentTableCode = ediCommunication.EK_ParentTableCode;
				xsdCommunication.Password = ediCommunication.EK_Password;
				xsdCommunication.PortNumber = ediCommunication.EK_PortNumber;
				xsdCommunication.PublishInternalMilestones = ediCommunication.EK_PublishInternalMilestones;
				xsdCommunication.RelatedPartyVanID = ediCommunication.EK_RelatedPartyVanID;
				xsdCommunication.ServerAddressSubject = ediCommunication.EK_ServerAddressSubject;
				xsdCommunication.SourceFolder = ediCommunication.EK_SourceFolder;

				//EK_GG and EK_OH_MessageVAN are excluded for FK constraints
				if (!ediCommunication.EK_LastFailed.IsEmpty)
				{
					xsdCommunication.LastFailed = ediCommunication.EK_LastFailed.ToDateTime();
					xsdCommunication.LastFailedSpecified = true;
				}
			}
		}

		#endregion

		#region ExportWhsClientParametersByWarehouse

		void ExportWhsClientParametersByWarehouse(OrgHeaderForDataTransfer organisation, Xsd.SysMergeOrganisationDetail xsdOrganisation)
		{
			var query = new ZQuery(WhsClientParameterByWarehouseSchema.WY_OH_Client, organisation.PK);
			var clientParameters = organisation.Factory.Load<IWhsClientParameterByWarehouse>(query);
			foreach (var clientParam in clientParameters)
			{
				var xsdWhsClientParameter = xsdOrganisation.WhsClientParametersByWarehouse.AddNew();
				xsdWhsClientParameter.PK = clientParam.PK.ToString();
				xsdWhsClientParameter.WarehousePK = clientParam.WY_WW_Whs.ToString();
			}
		}

		#endregion

		#endregion
	}
}
