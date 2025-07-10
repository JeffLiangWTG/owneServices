using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using AddInfoGroup = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.AddInfoGroup;
using CustomsSupportingInformation = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.CustomsSupportingInformation;
using EntryNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.EntryNumber;
using GoodsLocationQualifierCodes = Enterprise.Customs.Business.CusGoodsLocationQualifierList.Codes;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class JobDeclarationDataObjectReader : Customs.DataTransfer.Universal.JobDeclarationDataObjectReader<JobDeclaration, Bill, CusContainer, JobComInvoiceGroupHeader>
	{
		public JobDeclarationDataObjectReader(UniversalShipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment forwardingShipment = null)
			: base(declarationDataObject, logger, factory, forwardingShipment)
		{
		}

		protected new UniversalDataObjectReaderHelper Helper
		{
			get { return (UniversalDataObjectReaderHelper)base.Helper; }
		}

		protected override Customs.DataTransfer.Universal.CustomsContainerDataObjectReader<JobDeclaration, CusContainer> GetNewCustomsContainerDataObjectReader(Container containerDataObject, JobDeclaration declaration, ILandedCostDataReader landedCostDataReader)
		{
			return new CustomsContainerDataObjectReader(containerDataObject, logger, Helper, declaration, landedCostDataReader);
		}

		protected override Customs.DataTransfer.Universal.AdditionalBillDataObjectReader<Bill> GetNewAdditionalBillDataObjectReader(AdditionalBill additionalBillDataObject, Customs.DataTransfer.Universal.AdditionalBillDataProvider<Bill> additionalBillDataProvider, Customs.DataTransfer.Universal.BillDetail primaryMasterBillDetail, Customs.DataTransfer.Universal.BillDetail primaryHouseBillDetail)
		{
			return new Customs.DataTransfer.Universal.AdditionalBillDataObjectReader<Bill>(additionalBillDataObject, logger, Helper, additionalBillDataProvider, primaryMasterBillDetail, primaryHouseBillDetail);
		}

		protected override void FillCountrySpecificDetails(JobDeclaration declaration, UniversalShipment dataObject, Dictionary<string, ValueSetter> delaySetters)
		{
			base.FillCountrySpecificDetails(declaration, dataObject, delaySetters);
			var entryNumberCollection = dataObject.EntryNumberCollection;
			var addInfoCollection = dataObject.AddInfoCollection;
			if (entryNumberCollection?.Any() ?? false)
			{
				var countryCode = declaration.CountryCode;
				FillEntryNumbers(declaration, entryNumberCollection, countryCode, CusEntryNumberTypes.EU.MasterUCR);
			}

			if (addInfoCollection?.Any() ?? false)
			{
				if (!dataObject.DefermentAccountNumber.HasValue)
				{
					SetValue(declaration, JobDeclarationSchema.JE_DefermentAccountNumber, addInfoCollection.GetZStringValue(Constants.OldEUAddInfo.Fields.OtherDeferNumber), delaySetters);
				}
				if (dataObject.CustomsValuationPort == null)
				{
					SetValue(declaration, JobDeclarationSchema.JE_IATALoadPort, addInfoCollection.GetZStringValue(Constants.OldEUAddInfo.Fields.OSAirTransportLoad), delaySetters);
				}
			}
			ImportOldSupportingInformationSchemaDataIfNeeded(declaration, dataObject);

			var officeCodeAddInfoGroups = dataObject.AddInfoGroupCollection?.Where(x => x.Type.GetCodeAsUpperCase() == OfficeCodeAddInfoConstants.EUOfficeCode).ToArray() ?? Array.Empty<AddInfoGroup>();
			if (officeCodeAddInfoGroups.Any()
				&& (dataObject.CustomsReferenceCollection == null
				|| dataObject.CustomsReferenceCollection.All(x => x.Type.GetCodeAsUpperCase() != CusCodeDataTypeList.Codes.OfficeCode)))
			{
				var declarationRow = GetColumnIndexer(declaration);
				var declarationPK = declarationRow.GetValue(JobDeclarationSchema.PK);

				var query = new ZQuery(CusCodeDataSchema.CY_ParentID, declarationPK);
				query.AddToFilter(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.OfficeCode);
				var existingCusCodeDatas = Helper.Factory.Load<Customs.Business.CusCodeData>(query);
				existingCusCodeDatas.DeleteAll();

				foreach (var addInfoGroup in officeCodeAddInfoGroups)
				{
					new CustomsReferenceDataObjectReader(new CustomsReference
					{
						Type = new CodeDescriptionPair { Code = CusCodeDataTypeList.Codes.OfficeCode, Description = CusCodeDataTypeList.Descriptions.OfficeCode },
						SubType = ListHelper.GetWithDescription<CodeDescriptionPair35Char>(addInfoGroup.AddInfoCollection.GetZStringValue(OfficeCodeAddInfoConstants.Purpose).GetValueOrDefault(), PurposeList),
						Reference = addInfoGroup.AddInfoCollection.GetZStringValue(OfficeCodeAddInfoConstants.OfficeCode).GetValueOrDefault(),
						IsOverridden = false,
						Order = 0,
						DateCollection = new List<Date>
						{
							new Date
							{
								Type = DateType.DateAtOffice,
								Value = addInfoGroup.AddInfoCollection.GetZDateTimeValue(OfficeCodeAddInfoConstants.Time).GetValueOrDefault()
							}
						}
					}, logger, declarationPK, JobDeclarationSchema.Constants.Prefix, Helper.Factory).ReadIntoDataRowCusCodeData();
				}
			}
			FillGuarantees(declaration, dataObject);
			FillEquipments(declaration, dataObject);
			SetValue(declaration, JobEUDeclarationSchema.EUD_AgreedPlaceCode, dataObject.AgreedPlaceCode, delaySetters, JobDeclarationSchema.PK);
			if (declaration.IsUCC6)
			{
				FillDeclarationGoodsLocationData(declaration, dataObject);
			}
			if (declaration.AllowUCC6PropertiesWithUCC5)
			{
				FillDeclarationInlandTransportMeansData(declaration, dataObject, delaySetters);
			}
			var dutyPayerAddress = dataObject.OrganizationAddressCollection?.FirstOrDefault(x => (x.AddressType ?? ZString.Empty) == nameof(MasterFiles.Integration.DocAddressType.DutyPayer));
			if (dutyPayerAddress != null)
			{
				var orgReader = new MasterFiles.DataTransfer.Universal.OrganisationDataObjectReader(dutyPayerAddress, logger, factory);
				var orgAddress = orgReader.GetMatched(declaration, MasterFiles.Integration.OrganisationTypes.None);
				if (orgAddress != null)
				{
					SetOrganisationPK(GetColumnIndexer(declaration), JobDeclarationSchema.JE_OH_DutyPayer, orgAddress, delaySetters);
				}
			}

			FillDV1Details(declaration, dataObject);
		}

		void FillDV1Details(JobDeclaration declaration, UniversalShipment dataObject)
		{
			if (dataObject.CustomsValueInformationCollection != null)
			{
				declaration.DV1Details.RemoveAndDeleteAll();

				foreach (var customsValueInformation in dataObject.CustomsValueInformationCollection)
				{
					var dv1Details = declaration.DV1Details.AddNew();
					var indexer = GetColumnIndexer(dv1Details);

					var link = customsValueInformation.Link;
					if (link.HasValue)
					{
						Helper.RegisterDv1DetailsPK(link.Value, dv1Details.PK);
					}

					var typeLookup = customsValueInformation.CustomsValueDetailCollection.ToDictionary(x => x.Type);
					if (typeLookup.TryGetValue(Constants.CustomsValueTypes.Relationship, out var relationship))
					{
						SetValue(indexer, CusDV1DetailSchema.DV1_Relationship, relationship.Code.GetValueOrDefault());
					}

					if (typeLookup.TryGetValue(Constants.CustomsValueTypes.PriceInfluence, out var priceInfluence))
					{
						SetValue(indexer, CusDV1DetailSchema.DV1_PriceInfluence, priceInfluence.Code.GetValueOrDefault());
					}

					if (typeLookup.TryGetValue(Constants.CustomsValueTypes.RelationDetails, out var relationDetails))
					{
						SetValue(indexer, CusDV1DetailSchema.DV1_RelationDetails, relationDetails.Details.GetValueOrDefault());
					}

					if (typeLookup.TryGetValue(Constants.CustomsValueTypes.Restrictions, out var restrictions))
					{
						SetValue(indexer, CusDV1DetailSchema.DV1_Restrictions, restrictions.Code.GetValueOrDefault());
					}

					if (typeLookup.TryGetValue(Constants.CustomsValueTypes.Consideration, out var consideration))
					{
						SetValue(indexer, CusDV1DetailSchema.DV1_Consideration, consideration.Code.GetValueOrDefault());
					}

					if (typeLookup.TryGetValue(Constants.CustomsValueTypes.RestrictionConsiderationDetails,
							out var restrictionConsiderationDetails))
					{
						SetValue(indexer, CusDV1DetailSchema.DV1_RestrictionConsiderationDetails, restrictionConsiderationDetails.Details.GetValueOrDefault());
					}

					if (typeLookup.TryGetValue(Constants.CustomsValueTypes.RoyaltiesLicence,
							out var royaltiesLicence))
					{
						SetValue(indexer, CusDV1DetailSchema.DV1_RoyaltiesLicence, royaltiesLicence.Code.GetValueOrDefault());
						SetValue(indexer, CusDV1DetailSchema.DV1_RoyaltiesLicenceDetails, royaltiesLicence.Details.GetValueOrDefault());
					}

					if (typeLookup.TryGetValue(Constants.CustomsValueTypes.Resale,
							out var resale))
					{
						SetValue(indexer, CusDV1DetailSchema.DV1_Resale, resale.Code.GetValueOrDefault());
						SetValue(indexer, CusDV1DetailSchema.DV1_ResaleDetails, resale.Details.GetValueOrDefault());
					}

					if (typeLookup.TryGetValue(Constants.CustomsValueTypes.DecisionNumber,
							out var decisionNumber))
					{
						SetValue(indexer, CusDV1DetailSchema.DV1_CustomsDecisionNumber, decisionNumber.Details.GetValueOrDefault());
					}
				}
			}
		}

		void FillDeclarationInlandTransportMeansData(JobDeclaration declaration, UniversalShipment dataObject, Dictionary<string, ValueSetter> delaySetters)
		{
			if (TransportMeansGroupByTransportType.TryGetValue(TransportTypeCode.Inland, out var transportMeansCollection))
			{
				var transportMeans = transportMeansCollection[0];
				var declarationRow = GetColumnIndexer(declaration);
				SetValue(declarationRow, JobDeclarationSchema.JE_TransportIDInland, transportMeans.IdentificationNumber, delaySetters);
				SetValue(declarationRow, JobDeclarationSchema.JE_RN_NKTransportNationalityInland, transportMeans.Nationality, delaySetters);
				SetValue(declarationRow, JobDeclarationSchema.JE_TransportMeans, transportMeans.TypeOfIdentification, delaySetters);
				if (transportMeansCollection.Count > 1)
				{
					var inlandModeOfTransport = dataObject.AddInfoCollection.GetZStringValue(Customs.DataTransfer.Universal.Constants.AddInfoKeys.Declaration.InlandModeOfTransport);
					if (inlandModeOfTransport.HasValue ? inlandModeOfTransport.Value.EqualsIgnoringCase(Core.Constants.TransportModes.Road) : declaration.IsRoadInland)
					{
						var trailerTransportMeansCollection = transportMeansCollection.Skip(1).Where(x => x.TypeOfIdentification.GetCodeAsUpperCase() == Customs.Business.TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer).OrderBy(x => x.Order.GetValueOrDefault()).Take(2).ToArray();
						TransportMeans trailer1TransportMeans = null;
						TransportMeans trailer2TransportMeans = null;
						switch (trailerTransportMeansCollection.Length)
						{
							case 2:
								trailer1TransportMeans = trailerTransportMeansCollection[0];
								trailer2TransportMeans = trailerTransportMeansCollection[1];
								break;
							case 1:
								trailer1TransportMeans = trailerTransportMeansCollection[0];
								break;
						}
						if (trailer1TransportMeans != null)
						{
							SetValue(declarationRow, JobDeclarationSchema.JE_Trailer1RegNo, trailer1TransportMeans.IdentificationNumber, delaySetters);
							SetValue(declarationRow, JobDeclarationSchema.JE_RN_NKTrailer1Nationality, trailer1TransportMeans.Nationality, delaySetters);
						}
						if (trailer2TransportMeans != null)
						{
							SetValue(declarationRow, JobDeclarationSchema.JE_Trailer2RegNo, trailer2TransportMeans.IdentificationNumber, delaySetters);
							SetValue(declarationRow, JobDeclarationSchema.JE_RN_NKTrailer2Nationality, trailer2TransportMeans.Nationality, delaySetters);
						}
					}
				}
			}
		}

		void FillDeclarationGoodsLocationData(JobDeclaration declaration, UniversalShipment dataObject)
		{
			var goodsLocation = dataObject.LocationOfGoodsCollection?.FirstOrDefault();
			if (goodsLocation is null)
			{
				return;
			}

			var goodsLocationBO = declaration.GoodsLocation;
			goodsLocationBO.CGL_Qualifier = goodsLocation.Qualifier?.Code.GetValueOrDefault() ?? ZString.Empty;
			goodsLocationBO.CGL_Type = goodsLocation.LocationType?.Code.GetValueOrDefault() ?? ZString.Empty;

			FillGoodsLocationAddress(declaration, dataObject);

			goodsLocationBO.CGL_AdditionalIdentifier = goodsLocation.AdditionalIdentifier.GetValueOrDefault();

			var addressBO = goodsLocationBO.Address;
			addressBO.AuthorisationNumber = goodsLocation.AuthorizationNumber.GetValueOrDefault();
			addressBO.E2_Contact = goodsLocation.Contact?.Name.GetValueOrDefault() ?? ZString.Empty;
			addressBO.E2_Phone = goodsLocation.Contact?.PhoneNumber.GetValueOrDefault() ?? ZString.Empty;
			addressBO.E2_Email = goodsLocation.Contact?.Email.GetValueOrDefault() ?? ZString.Empty;
		}

		void FillGoodsLocationAddress(JobDeclaration declaration, UniversalShipment dataObject)
		{
			var qualifier = dataObject.LocationOfGoodsCollection?.FirstOrDefault()?.Qualifier?.Code ?? ZString.Empty;
			var goodsLocationAddressFromDataObject = dataObject.OrganizationAddressCollection?.FirstOrDefault(x => (x.AddressType ?? ZString.Empty) == nameof(MasterFiles.Integration.DocAddressType.LocationOfGoods));

			if (qualifier.IsEmpty || goodsLocationAddressFromDataObject is null)
			{
				return;
			}

			if (this.TryGetMatchedOrganisation(out var addressBO, dataObject, declaration, nameof(MasterFiles.Integration.DocAddressType.LocationOfGoods), MasterFiles.Integration.OrganisationTypes.None))
			{
				if (qualifier == GoodsLocationQualifierCodes.Address)
				{
					if (goodsLocationAddressFromDataObject.AddressOverride ?? false)
					{
						SetGoodsLocationAddressManually(declaration, goodsLocationAddressFromDataObject);
						return;
					}

					declaration.GoodsLocation.Address.E2_OA_Address = addressBO?.PK ?? ZGuid.Empty;
					return;
				}

				if (qualifier == GoodsLocationQualifierCodes.AuthorizationNumber)
				{
					declaration.GoodsLocation.Address.IdentificationHolderPK = addressBO?.OA_OH ?? ZGuid.Empty;
				}
			}
		}

		static void SetGoodsLocationAddressManually(JobDeclaration declaration, OrganizationAddress goodsLocationAddress)
		{
			var addressFromDeclaration = declaration.GoodsLocation.Address;

			addressFromDeclaration.E2_AddressOverride = goodsLocationAddress.AddressOverride ?? false;
			addressFromDeclaration.E2_CompanyName = goodsLocationAddress.CompanyName.GetValueOrDefault();
			addressFromDeclaration.E2_Address1 = goodsLocationAddress.Address1.GetValueOrDefault();
			addressFromDeclaration.E2_Address2 = goodsLocationAddress.Address2.GetValueOrDefault();
			addressFromDeclaration.E2_City = goodsLocationAddress.City.GetValueOrDefault();
			addressFromDeclaration.E2_Postcode = goodsLocationAddress.Postcode.GetValueOrDefault();
			addressFromDeclaration.E2_RN_NKCountryCode = goodsLocationAddress.Country?.Code.GetValueOrDefault() ?? ZString.Empty;
		}

		static void FillGuarantees(JobDeclaration declaration, UniversalShipment dataObject)
		{
			if (dataObject.GuaranteeCollection != null)
			{
				foreach (var guarantee in dataObject.GuaranteeCollection)
				{
					var guaranteeBO = declaration.Guarantees.AddNew();
					guaranteeBO.PW_ActivityCode = guarantee.ActivityCode?.GetNullableCodeAsUpperCase() ?? ZString.Empty;
					guaranteeBO.PW_BondType = guarantee.BondType?.GetNullableCodeAsUpperCase() ?? ZString.Empty;
					guaranteeBO.PW_BondFiledPort = guarantee.BondFiledPort?.GetNullableCodeAsUpperCase() ?? ZString.Empty;
					guaranteeBO.PW_BondNumber = guarantee.BondNumber.GetValueOrDefault();
					guaranteeBO.PW_BondNumber2 = guarantee.BondNumber2.GetValueOrDefault();
					guaranteeBO.PW_SuretyCode = guarantee.SuretyCode.GetValueOrDefault();
					guaranteeBO.PW_Password = guarantee.AccessCode.GetValueOrDefault();
					guaranteeBO.PW_BondAmount = guarantee.BondAmount.GetValueOrDefault();
					guaranteeBO.PW_HolderIdentification = guarantee.HolderIdentification.GetValueOrDefault();
					guaranteeBO.PW_RX_NKCurrency = guarantee.BondCurrency?.GetNullableCodeAsUpperCase() ?? ZString.Empty;
					guaranteeBO.PW_RN_NKCountryOfIssue = guarantee.CountryOfIssue?.GetNullableCodeAsUpperCase() ?? ZString.Empty;
					guaranteeBO.PW_ValidityLimitation = guarantee.ValidityLimitation.GetValueOrDefault();
				}
			}
		}

		void ImportOldSupportingInformationSchemaDataIfNeeded(JobDeclaration declaration, UniversalShipment dataObject)
		{
			if (dataObject.CustomsSupportingInformationCollection == null && dataObject.CommercialInfo?.AddInfoGroupCollection != null && Helper.IsSourceAndTargetCountrySame)
			{
				var shipment = CreateDummyShipmentWithSupportingInfo<UniversalShipment>(dataObject.CommercialInfo.AddInfoGroupCollection, DefaultDataObjectWriterStrategy.Instance);
				if (shipment.CustomsSupportingInformationCollection.Count > 0)
				{
					new Customs.DataTransfer.Universal.CustomsSupportingInformationCollectionDataObjectReader(logger, Helper).ReadIntoDataRows(declaration.PK, declaration.TablePrefix, declaration.IsInDatabase, shipment);
				}
			}
		}

		public static T CreateDummyShipmentWithSupportingInfo<T>(List<AddInfoGroup> addInfoGroupCollection, IDataObjectWriterStrategy strategy)
			where T : ICustomsSupportingInformationCollectionParent, new()
		{
			var result = new T();
			result.SetWriterStrategy(strategy);
			result.SetCustomsSupportingInformationCollection(() => new List<CustomsSupportingInformation>());
			foreach (var addInfoGroup in addInfoGroupCollection)
			{
				switch (addInfoGroup.Type.GetCodeAsUpperCase())
				{
					case Constants.OldCusAddInfoTypes.AdditionalInfo.Type:
						result.CustomsSupportingInformationCollection?.Add(CreateAdditionalInfo(addInfoGroup));
						break;
					case Constants.OldCusAddInfoTypes.PreviousDocument.Type:
						result.CustomsSupportingInformationCollection?.Add(CreatePreviousDocument(addInfoGroup));
						break;
					case Constants.OldCusAddInfoTypes.SupportingDocument.Type:
						result.CustomsSupportingInformationCollection?.Add(CreateSupportingDocument(addInfoGroup));
						break;
				}
			}
			return result;
		}

		static CustomsSupportingInformation CreateSupportingDocument(AddInfoGroup addInfoGroup)
		{
			var result = new CustomsSupportingInformation()
			{
				Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument }
			};
			var addInfoCollection = addInfoGroup.AddInfoCollection;
			if (addInfoCollection != null)
			{
				result.Type = new CodeDescriptionPair6Char() { Code = addInfoCollection.GetZStringValue(Constants.OldCusAddInfoTypes.SupportingDocument.Fields.TypeCode) };
				result.ReferenceNumber = addInfoCollection.GetZStringValue(Constants.OldCusAddInfoTypes.SupportingDocument.Fields.Reference);
				result.Description = addInfoCollection.GetZStringValue(Constants.OldCusAddInfoTypes.SupportingDocument.Fields.Reason);
				result.Quantity = addInfoCollection.GetZDecimalValue(Constants.OldCusAddInfoTypes.SupportingDocument.Fields.Qty);
				result.SubType = new CodeDescriptionPair5Char() { Code = addInfoCollection.GetZStringValue(Constants.OldCusAddInfoTypes.SupportingDocument.Fields.Part) };
				var actions = addInfoCollection.GetZStringValue(Constants.OldCusAddInfoTypes.SupportingDocument.Fields.Actions);
				var availability = addInfoCollection.GetZStringValue(Constants.OldCusAddInfoTypes.SupportingDocument.Fields.Availability);
				if (actions.HasValue || availability.HasValue)
				{
					result.Status = new CodeDescriptionPair() { Code = availability.GetValueOrDefault().Left(1).PadLeft(1) + actions.GetValueOrDefault().Left(1).PadLeft(1) };
				}
			}
			return result;
		}

		static CustomsSupportingInformation CreatePreviousDocument(AddInfoGroup addInfoGroup)
		{
			var result = new CustomsSupportingInformation()
			{
				Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument }
			};
			var addInfoCollection = addInfoGroup.AddInfoCollection;
			if (addInfoCollection != null)
			{
				result.Type = new CodeDescriptionPair6Char() { Code = addInfoCollection.GetZStringValue(Constants.OldCusAddInfoTypes.PreviousDocument.Fields.TypeCode) };
				result.DateOfIssue = addInfoCollection.GetZDateValue(Constants.OldCusAddInfoTypes.PreviousDocument.Fields.DateOfIssue);
				result.Description = addInfoCollection.GetZStringValue(Constants.OldCusAddInfoTypes.PreviousDocument.Fields.MoreInfo);
				result.ReferenceNumber = addInfoCollection.GetZStringValue(Constants.OldCusAddInfoTypes.PreviousDocument.Fields.Reference);
				result.SubType = new CodeDescriptionPair5Char() { Code = addInfoCollection.GetZStringValue(Constants.OldCusAddInfoTypes.PreviousDocument.Fields.Class) };
			}
			return result;
		}

		static CustomsSupportingInformation CreateAdditionalInfo(AddInfoGroup addInfoGroup)
		{
			var result = new CustomsSupportingInformation()
			{
				Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo }
			};
			var addInfoCollection = addInfoGroup.AddInfoCollection;
			if (addInfoCollection != null)
			{
				result.Type = new CodeDescriptionPair6Char() { Code = addInfoCollection.GetZStringValue(Constants.OldCusAddInfoTypes.AdditionalInfo.Fields.TypeCode) };
				result.Description = addInfoCollection.GetZStringValue(Constants.OldCusAddInfoTypes.AdditionalInfo.Fields.Description);
				result.Country = new Country() { Code = addInfoCollection.GetZStringValue(Constants.OldCusAddInfoTypes.AdditionalInfo.Fields.NctsExportFromCountry) };
				result.SubType = new CodeDescriptionPair5Char() { Code = addInfoCollection.GetZStringValue(Constants.OldCusAddInfoTypes.AdditionalInfo.Fields.NctsExportFromEC) };
			}
			return result;
		}

		void FillEntryNumbers(JobDeclaration declaration, IEnumerable<EntryNumber> entryNumbers, ZString countryCode, ZString entryType)
		{
			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, declaration.PK);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, countryCode);
			var existingNumbers = new List<CusEntryNumber>(factory.Load<CusEntryNumber>(query));
			foreach (var entryNumberDataObject in entryNumbers.Where(x => x.CountryOfIssue.GetCodeAsUpperCase() == countryCode && x.Type.GetCodeAsUpperCase() == entryType))
			{
				var entryNumber = new EntryNumberDataObjectReader(entryNumberDataObject, logger, factory, declaration, (dataObject) => new EntryNumberBusinessObjectFinder(dataObject).Find(existingNumbers)).ReadIntoBusinessObject();
				existingNumbers.Remove(entryNumber);
			}
			existingNumbers.DeleteAll();
		}

		protected override Customs.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectReader<JobComInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader(JobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceData, UniversalShipment dataObject, ILandedCostDataReader landedCostDataReader)
		{
			return new CommercialInvoiceHeaderDataObjectReader(invoiceData, logger, base.Helper, groupHeader, dataObject, landedCostDataReader);
		}

		protected override Customs.DataTransfer.Universal.CustomsEntryInstructionDataObjectReader CreateCustomsEntryInstructionDataObjectReader(EntryInstruction entryInstructionDataObject, JobDeclaration declaration)
		{
			return new CustomsEntryInstructionDataObjectReader(entryInstructionDataObject, logger, Helper, factory, declaration);
		}

		protected override Customs.DataTransfer.Universal.CustomsEntryHeaderDataObjectReader CreateCustomsEntryHeaderDataObjectReader(
			EntryHeader entryHeaderDataObject,
			JobDeclaration declaration,
			List<ZString> matchingKeys = null)
		{
			return new CustomsEntryHeaderDataObjectReader(entryHeaderDataObject, logger, Helper, declaration, ZGuid.Empty, matchingKeys);
		}

		protected override ZString? GetLocationAtClearanceCore(CodeDescriptionPair35Char locationAtClearance)
		{
			return locationAtClearance == null ? null : locationAtClearance.Code;
		}

		protected override void PopulateTransportMode(UniversalShipment dataObject, IColumnIndexer declarationRow, Dictionary<string, ValueSetter> delaySetters)
		{
			var usesWCONotation = false;
			if (dataObject.TransportMode != null)
			{
				var universalShipmentTransportMode = dataObject.TransportMode.Code.GetValueOrDefault();
				usesWCONotation = universalShipmentTransportMode.IsNumbersOnlyOrEmpty;
				if (usesWCONotation)
				{
					var cargoWiseNotationTransportMode = new TransportModeTranslator().TranslateToCargoWiseCode(universalShipmentTransportMode);
					SetValue(declarationRow, JobDeclarationSchema.JE_TransportMode, cargoWiseNotationTransportMode, delaySetters);
				}
			}

			if (!usesWCONotation)
			{
				base.PopulateTransportMode(dataObject, declarationRow, delaySetters);
			}
		}

		void FillEquipments(JobDeclaration declaration, UniversalShipment dataObject)
		{
			if (declaration.EquipmentsRequired && dataObject.TransportEquipmentCollection != null)
			{
				var equipments = declaration.Equipments;
				var existingEquipments = equipments.Cast<CusEquipment>().GroupBy(x => x.CEQ_IdentificationNumber.ToUpperInvariant()).ToDictionary(x => x.Key, y => new Queue<CusEquipment>(y));
				foreach (var transportEquipment in dataObject.TransportEquipmentCollection)
				{
					if (GetOrAddEquipment(equipments, existingEquipments, transportEquipment.IdentificationNumber.GetValueOrDefault()) is CusEquipment equipment)
					{
						if (transportEquipment.SealNumberCollection != null)
						{
							using (((ICusSealSequenceNumberGeneratorProvider)equipment).SequenceNumberGenerator.GetLineNumberSuspender())
							{
								var seals = equipment.Seals;
								var existingSeals = seals.Cast<CusSeal>().GroupBy(x => x.BK_SealNumber.ToUpperInvariant()).ToDictionary(x => x.Key, y => new Queue<CusSeal>(y));
								foreach (var seal in transportEquipment.SealNumberCollection)
								{
									AddSealIfNeeded(seals, existingSeals, seal.Number.GetValueOrDefault());
								}
								existingSeals.Values.SelectMany(x => x).DeleteAll();
								var sequenceNumber = ZShort.Zero;
								seals.OrderBy(x => x.BK_SealNumber).ForEach(x =>
								{
									x.BK_SequenceNumber = ++sequenceNumber;
								});
							}
						}
					}
				}
				existingEquipments.SelectMany(x => x.Value).DeleteAll();
			}
		}

		void AddSealIfNeeded(CusSealCollection seals, Dictionary<ZString, Queue<CusSeal>> existingSeals, ZString sealNumber)
		{
			if (!sealNumber.IsEmpty)
			{
				CusSeal seal;
				if (existingSeals.TryGetValue(sealNumber, out var queue))
				{
					_ = queue.Dequeue();
					if (queue.Count == 0)
					{
						existingSeals.Remove(sealNumber);
					}
				}
				else
				{
					seal = seals.AddNew();
					var sealRow = GetColumnIndexer(seal);
					SetValue(sealRow, CusSealSchema.BK_SealNumber, sealNumber);
				}
			}
		}

		CusEquipment GetOrAddEquipment(Customs.Business.ICusEquipmentCollection<CusEquipment> equipments, Dictionary<ZString, Queue<CusEquipment>> existingEquipments, ZString identificationNumber)
		{
			CusEquipment result = null;
			if (!identificationNumber.IsEmpty)
			{
				if (existingEquipments.TryGetValue(identificationNumber, out var queue))
				{
					result = queue.Dequeue();
					if (queue.Count == 0)
					{
						existingEquipments.Remove(identificationNumber);
					}
				}
				else
				{
					result = equipments.AddNew();
					var equipmentRow = GetColumnIndexer(result);
					SetValue(equipmentRow, CusEquipmentSchema.CEQ_IdentificationNumber, identificationNumber);
				}
			}
			return result;
		}

		ZArchitecture.Core.CodeDescriptionPairList PurposeList
		{
			get
			{
				if (purposeList == null)
				{
					purposeList = new ZArchitecture.Core.CodeDescriptionPairList();
					purposeList.AddRange(new OfficeCodes_ECS());
					purposeList.AddRange(new OfficeCodes_NCTS());
					purposeList.AddRange(new OfficeCodes_EMCS());
				}
				return purposeList;
			}
		}
		ZArchitecture.Core.CodeDescriptionPairList purposeList;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		static class OfficeCodeAddInfoConstants
		{
			public const string EUOfficeCode = "EUO";
			public const string Purpose = "Purpose";
			public const string Time = "Time";
			public const string OfficeCode = "OfficeCode";
		}
	}
}
