using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Matching;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusHAWBDataObjectWriter : DataTransfer.Universal.AirManifest.CusHAWBDataObjectWriter<CusHAWB, AirManifestDataObjectWriterHelper>
	{
		public CusHAWBDataObjectWriter(IDataWritingManager manager, AirManifestDataObjectWriterHelper helper)
			: base(manager, helper)
		{
		}

		protected override AirManifestDataObjectWriterHelper GetNewAirManifestLineDataObjectWriterHelper(CusHAWB hawbBO)
		{
			return helper == null ? new AirManifestDataObjectWriterHelper(hawbBO) : new AirManifestDataObjectWriterHelper(hawbBO, helper);
		}

		protected override DataTransfer.Universal.AirManifest.CusHAWBDataObjectWriter<CusHAWB, AirManifestDataObjectWriterHelper> GetNewCusHAWBDataObjectWriter(AirManifestDataObjectWriterHelper helper)
		{
			return new CusHAWBDataObjectWriter(writeManager, helper);
		}

		protected override void PopulateCountrySpecificDetails(Shipment hawbData, CusHAWB hawbBO, AirManifestDataObjectWriterHelper hawbHelper, bool keepExistingData)
		{
			base.PopulateCountrySpecificDetails(hawbData, hawbBO, hawbHelper, keepExistingData);
			var isSpecialReporter = hawbBO.CS_IsSpecialReporter;
			hawbData.ShipmentSubType = PopulateValue(hawbData.ShipmentSubType, keepExistingData, () => new CodeDescriptionPair()
			{
				Code = isSpecialReporter ? AirCargo.Constants.ShipmentSubTypes.IsSpecialReporter : AirCargo.Constants.ShipmentSubTypes.IsNotSpecialReporter,
				Description = isSpecialReporter ? "Is Special Reporter" : "Is Not Special Reporter"
			});

			var isSelfAssessedClearance = hawbBO.CS_IsSelfAssessedClearance;
			hawbData.MessageSubType = PopulateValue(hawbData.MessageSubType, keepExistingData, () => new CodeDescriptionPair()
			{
				Code = isSelfAssessedClearance ? JobDeclaration.MessageSubType.SelfAssessedClearance : JobDeclaration.MessageSubType.FormalEntry,
				Description = isSelfAssessedClearance ? "Is Self Assessed Clearance" : "Is Formal Entry"
			});
			hawbData.PaymentMethod = PopulateValue(hawbData.PaymentMethod, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(hawbBO.CS_FreightPrepaidCollect, hawbHelper.CusHAWBPrepaidCollectList));
			hawbData.ShipmentType = PopulateValue(hawbData.ShipmentType, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(hawbBO.CS_ShipmentType, hawbHelper.CusHAWBShipmentTypeList));
			hawbData.IsPersonalEffects = PopulateValue(hawbData.IsPersonalEffects, keepExistingData, () => hawbBO.CS_IsPersonalEffects);

			hawbData.WarehouseLocation = PopulateValue(hawbData.WarehouseLocation, keepExistingData, () => hawbBO.CS_WarehouseLocation);
			hawbData.Folio = PopulateValue(hawbData.Folio, keepExistingData, () => hawbBO.CS_FolioReference);
			hawbData.ActualChargeable = PopulateValue(hawbData.ActualChargeable, keepExistingData, () => hawbBO.CS_ChargableWeight);
			hawbData.ConsolidatedCargoStatus = PopulateValue(hawbData.ConsolidatedCargoStatus, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(hawbBO.CS_CustomsStatus, hawbHelper.CusHAWBConsolidatedCargoStatuses));
			hawbData.ServiceLevel = PopulateValue(hawbData.ServiceLevel, keepExistingData, () => ListHelper.GetWithDescription<ServiceLevel>(hawbBO.CS_RS_NK_ServiceLevel, hawbHelper.CusHAWBServiceLevels));

			var notes = hawbBO.Notes.GetAllNotesVisibleToCurrentCompany().OrderBy(x => x.ST_Description);
			hawbData.SetNoteCollection(() => ProcessCollection(notes, new NoteDataObjectWriter(writeManager), CollectionContent.Partial));

			PopulateConsigneeAdditionalInformation(hawbData, hawbBO);

			PopulateConsignorAdditionalInformation(hawbData, hawbBO);

			hawbData.VendorIdentifier = PopulateValue(hawbData.VendorIdentifier, keepExistingData, () => hawbBO.CS_VendorIdentifier);
		}

		void PopulateConsignorAdditionalInformation(Shipment hawbData, CusHAWB hawbBO)
		{
			var consignorAddress = hawbData.OrganizationAddressCollection.FirstOrDefault(AddressTypeMatchHelper.GetConsignorAddressTypesInPreferredOrder());
			var cidConsignor = consignorAddress?.RegistrationNumberCollection?.FirstOrDefault(number => number.Type.Code == (ZString?)OrgCusCode.CodeTypes.CustomsClientID);

			if (!hawbBO.CS_ConsignorIdentifier.IsEmpty)
			{
				if (cidConsignor != null)
				{
					consignorAddress.RegistrationNumberCollection.Remove(cidConsignor);
				}

				if (consignorAddress != null && consignorAddress.RegistrationNumberCollection == null)
				{
					consignorAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
				}

				consignorAddress?.RegistrationNumberCollection?.Add(new RegistrationNumber
				{
					Type = new RegistrationNumberType
					{
						Code = OrgCusCode.CodeTypes.CustomsClientID,
						Description = new OrgCodeLists().CustomsCodes_List(Core.Constants.CountryCodes.Australia).GetDescriptionFromCode(OrgCusCode.CodeTypes.CustomsClientID)
					},
					CountryOfIssue = Country.New(hawbBO.ConsignorCountry),
					Value = hawbBO.CS_ConsignorIdentifier
				});
			}
		}

		void PopulateConsigneeAdditionalInformation(Shipment hawbData, CusHAWB hawbBO)
		{
			var consigneeAddress = hawbData.OrganizationAddressCollection.FirstOrDefault(AddressTypeMatchHelper.GetConsigneeAddressTypesInPreferredOrder());

			if (!hawbBO.CS_ConsigneeIdentifier.IsEmpty)
			{
				var cidConsignee = consigneeAddress?.RegistrationNumberCollection?.FirstOrDefault(number => number.Type.Code == (ZString?)OrgCusCode.CodeTypes.CustomsClientID);

				if (cidConsignee != null)
				{
					consigneeAddress.RegistrationNumberCollection.Remove(cidConsignee);
				}

				if (consigneeAddress != null && consigneeAddress.RegistrationNumberCollection == null)
				{
					consigneeAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
				}
				consigneeAddress?.RegistrationNumberCollection?.Add(new RegistrationNumber
				{
					Type = new RegistrationNumberType
					{
						Code = OrgCusCode.CodeTypes.CustomsClientID,
						Description = new OrgCodeLists().CustomsCodes_List(Core.Constants.CountryCodes.Australia).GetDescriptionFromCode(OrgCusCode.CodeTypes.CustomsClientID)
					},
					CountryOfIssue = Country.New(hawbBO.ConsigneeCountry),
					Value = hawbBO.CS_ConsigneeIdentifier
				});
			}

			if (!hawbBO.CS_ConsigneeBusinessNumber.IsEmpty)
			{
				var abnConsignee = consigneeAddress?.RegistrationNumberCollection?.FirstOrDefault(number => number.Type.Code == (ZString?)OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber);

				if (abnConsignee != null)
				{
					consigneeAddress.RegistrationNumberCollection.Remove(abnConsignee);
				}

				if (consigneeAddress != null && consigneeAddress.RegistrationNumberCollection == null)
				{
					consigneeAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
				}
				consigneeAddress?.RegistrationNumberCollection?.Add(new RegistrationNumber
				{
					Type = new RegistrationNumberType
					{
						Code = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber,
						Description = "Australian Business Number"
					},
					CountryOfIssue = Country.New(hawbBO.ConsigneeCountry),
					Value = hawbBO.CS_ConsigneeBusinessNumber
				});
			}
		}

		protected override void PopulateConsignee(Shipment hawbData, CusHAWB hawbBO, AirManifestDataObjectWriterHelper hawbHelper, bool keepExistingData)
		{
			if (!hawbBO.CS_IsMasterHouse)
			{
				var addressType = nameof(DocAddressType.ConsigneeDocumentaryAddress);
				OrganizationAddress consigneeAddress = null;
				if (!hawbBO.CS_OA_ConsigneeAddress.IsEmpty)
				{
					consigneeAddress = new OrganizationDataObjectWriter(writeManager, addressType).GetDataObject(hawbBO.ConsigneeAddress);
				}
				else
				{
					consigneeAddress = CreateOrgAddress(hawbBO, hawbHelper, addressType, CusHAWBSchema.CS_ConsigneeName, CusHAWBSchema.CS_ConsigneeStreet, CusHAWBSchema.CS_ConsigneeStreet2, CusHAWBSchema.CS_ConsigneeCity, CusHAWBSchema.CS_ConsigneeState, CusHAWBSchema.CS_ConsigneePostcode, CusHAWBSchema.CS_RN_NKConsigneeCountry, CusHAWBSchema.CS_ConsigneePhone, CusHAWBSchema.CS_ConsigneeContactName, keepExistingData);
				}
				hawbData.SetOrganizationAddressCollection(() => hawbData.OrganizationAddressCollection.MergeCollection(new[] { consigneeAddress }, keepExistingData, UniversalCommonHelper.IsOrganizationAddressTypeMatched));
			}
			else
			{
				base.PopulateConsignee(hawbData, hawbBO, hawbHelper, keepExistingData);
			}
		}

		protected override void PopulateConsignor(Shipment hawbData, CusHAWB hawbBO, AirManifestDataObjectWriterHelper hawbHelper, bool keepExistingData)
		{
			if (!hawbBO.CS_IsMasterHouse)
			{
				var addressType = nameof(DocAddressType.ConsignorDocumentaryAddress);
				OrganizationAddress consignorAddress = null;
				if (!hawbBO.CS_OA_ConsignorAddress.IsEmpty)
				{
					consignorAddress = new OrganizationDataObjectWriter(writeManager, addressType).GetDataObject(hawbBO.ConsignorAddress);
				}
				else
				{
					consignorAddress = CreateOrgAddress(hawbBO, hawbHelper, addressType, CusHAWBSchema.CS_ConsignorName, CusHAWBSchema.CS_ConsignorStreet, CusHAWBSchema.CS_ConsignorStreet2, CusHAWBSchema.CS_ConsignorCity, CusHAWBSchema.CS_ConsignorState, CusHAWBSchema.CS_ConsignorPostcode, CusHAWBSchema.CS_RN_NKConsignorCountry, CusHAWBSchema.CS_ConsignorPhone, CusHAWBSchema.CS_ConsignorContactName, keepExistingData);
				}
				hawbData.SetOrganizationAddressCollection(() => hawbData.OrganizationAddressCollection.MergeCollection(new[] { consignorAddress }, keepExistingData, UniversalCommonHelper.IsOrganizationAddressTypeMatched));
			}
			else
			{
				base.PopulateConsignor(hawbData, hawbBO, hawbHelper, keepExistingData);
			}
		}

		protected override bool ShouldKeepExistingData(CusHAWB hawbBO)
		{
			return !HasRecipientRole(RecipientRoleType.AAD);
		}

		protected override IEnumerable<IPropertyValue> GetUserDefinedValues(CusHAWB sourceBO)
		{
			return sourceBO.GetUserDefinedValues();
		}
	}
}
