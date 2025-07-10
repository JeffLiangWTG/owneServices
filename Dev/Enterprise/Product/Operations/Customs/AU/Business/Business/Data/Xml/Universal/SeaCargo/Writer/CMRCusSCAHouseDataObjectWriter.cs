using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Integration;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.SeaManifest;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	internal sealed class CMRCusSCAHouseDataObjectWriter : CusSCAHouseDataObjectWriter<CusSCAHouse, CusSCAPivot>
	{
		public CMRCusSCAHouseDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		#region Overrides
		protected override IEnumerable<IPropertyValue> GetUserDefinedValues(CusSCAHouse sourceBO)
		{
			return sourceBO.GetUserDefinedValues();
		}

		protected override void PopulateCountrySpecificDetails(IColumnIndexer houseBill, Shipment data, bool keepExistingData)
		{
			data.PaymentMethod = PopulateValue(data.PaymentMethod, keepExistingData,
				() => ListHelper.GetWithDescription<CodeDescriptionPair>(houseBill.GetValue(CusSCAHouseSchema.CA_PrepaidCollectOther), Factory.GetCachedValue<CMRMethodsOfPayment>()));
			data.ConsigneeBussinessNumber = PopulateValue(data.ConsigneeBussinessNumber, keepExistingData,
				() => houseBill.GetValue(CusSCAHouseSchema.CA_ConsigneeBusinessNumber));
			data.ConsigneeIdentifier = PopulateValue(data.ConsigneeIdentifier, keepExistingData,
				() => houseBill.GetValue(CusSCAHouseSchema.CA_ConsigneeIdentifier));
			data.ConsignorIdentifier = PopulateValue(data.ConsignorIdentifier, keepExistingData,
				() => houseBill.GetValue(CusSCAHouseSchema.CA_ConsignorIdentifier));
			data.VendorIdentifier = PopulateValue(data.VendorIdentifier, keepExistingData,
				() => houseBill.GetValue(CusSCAHouseSchema.CA_VendorIdentifier));
			PopulateMasterHouse(houseBill, data, keepExistingData);
			PopulateResponsibleParty(houseBill, data, keepExistingData);
		}

		protected override OrganizationAddress GetConsigneeAddress(IColumnIndexer houseBill)
		{
			if (!houseBill.GetValue(CusSCAHouseSchema.CA_IsMasterHouse))
			{
				var addressType = GetConsigneeAddressType(houseBill);
				var consigneeAddress = Factory.Load<OrgAddress>(houseBill.GetValue(CusSCAHouseSchema.CA_OA_ConsigneeAddress));
				var orgWriter = new OrganizationDataObjectWriter(writeManager, addressType.ToString(), null);
				return
					orgWriter.GetDataObject(consigneeAddress) ??
					OrganizationDataObjectWriter.GetDataObject(Factory, writeManager, houseBill, addressType.ToString(),
						CusSCAHouseSchema.CA_ConsigneeName,
						CusSCAHouseSchema.CA_ConsigneeAddress1,
						CusSCAHouseSchema.CA_ConsigneeAddress2,
						CusSCAHouseSchema.CA_ConsigneeSuburb,
						CusSCAHouseSchema.CA_ConsigneeState,
						CusSCAHouseSchema.CA_ConsigneePostcode,
						CusSCAHouseSchema.CA_RN_NKConsigneeCountryCode,
						CusSCAHouseSchema.CA_ConsigneePhone,
						CusSCAHouseSchema.CA_ConsigneeFax,
						CusSCAHouseSchema.CA_ConsigneeContactName);
			}
			else
			{
				return base.GetConsigneeAddress(houseBill);
			}
		}

		protected override DocAddressType GetConsigneeAddressType(IColumnIndexer houseBill)
		{
			return houseBill.GetValue(CusSCAHouseSchema.CA_IsMasterHouse)
				? DocAddressType.ReceivingForwarderAddress
				: base.GetConsigneeAddressType(houseBill);
		}

		protected override OrganizationAddress GetConsignorAddress(IColumnIndexer houseBill)
		{
			if (!houseBill.GetValue(CusSCAHouseSchema.CA_IsMasterHouse))
			{
				var addressType = GetConsignorAddressType(houseBill);
				var consignorAddress = Factory.Load<OrgAddress>(houseBill.GetValue(CusSCAHouseSchema.CA_OA_ConsignorAddress));
				var orgWriter = new OrganizationDataObjectWriter(writeManager, addressType.ToString(), null);
				return
					orgWriter.GetDataObject(consignorAddress) ??
					OrganizationDataObjectWriter.GetDataObject(Factory, writeManager, houseBill, addressType.ToString(),
						CusSCAHouseSchema.CA_ConsignorName,
						CusSCAHouseSchema.CA_ConsignorAddress1,
						CusSCAHouseSchema.CA_ConsignorAddress2,
						CusSCAHouseSchema.CA_ConsignorSuburb,
						CusSCAHouseSchema.CA_ConsignorState,
						CusSCAHouseSchema.CA_ConsignorPostcode,
						CusSCAHouseSchema.CA_RN_NKConsignorCountryCode,
						CusSCAHouseSchema.CA_ConsignorPhone,
						CusSCAHouseSchema.CA_ConsignorFax,
						CusSCAHouseSchema.CA_ConsignorContactName);
			}
			else
			{
				return base.GetConsignorAddress(houseBill);
			}
		}

		protected override DocAddressType GetConsignorAddressType(IColumnIndexer houseBill)
		{
			return houseBill.GetValue(CusSCAHouseSchema.CA_IsMasterHouse)
				? DocAddressType.SendingForwarderAddress
				: base.GetConsignorAddressType(houseBill);
		}

		protected override ICodeDescriptionPairList GetMessageStatuses()
		{
			return Factory.GetCachedValue<CMRStatuses>();
		}

		protected override ICodeDescriptionPairList GetShipmentStatuses()
		{
			return Factory.GetCachedValue<CMRShipmentStatuses>();
		}

		protected override string GetWayBillTypeCode(IColumnIndexer houseBill)
		{
			return houseBill.GetValue(CusSCAHouseSchema.CA_IsMasterHouse)
				? WayBillTypeList.Codes.MasterHouse
				: WayBillTypeList.Codes.House;
		}

		#endregion // Overrides

		#region Implementation

		void PopulateMasterHouse(IColumnIndexer houseBill, Shipment data, bool keepExistingData)
		{
			var masterHouseBillNumber = houseBill.GetValue(CusSCAHouseSchema.CA_MasterHouseBill);
			if (!masterHouseBillNumber.IsEmpty)
			{
				data.SetAdditionalBillCollection(() =>
				{
					var masterHouseBill = new AdditionalBill(writeManager.WriterStrategy)
					{
						BillNumber = houseBill.GetValue(CusSCAHouseSchema.CA_HouseBill),
						BillType = GetWayBillType(GetWayBillTypeCode(houseBill)),
						ParentBillNumber = masterHouseBillNumber
					};
					return data.AdditionalBillCollection.MergeCollection(
					   new[] { masterHouseBill }, keepExistingData, UniversalCommonHelper.IsBillMatched);
				});
			}
		}

		static void PopulateResponsibleParty(IColumnIndexer houseBill, Shipment data, bool keepExistingData)
		{
			var responsiblePartyID = houseBill.GetValue(CusSCAHouseSchema.CA_ResponsiblePartyID);
			if (!responsiblePartyID.IsEmpty)
			{
				data.SetAdditionalReferenceCollection(() =>
				{
					var responsiblePartyReference = new AdditionalReference()
					{
						ReferenceNumber = responsiblePartyID,
						Type = new UniversalDataBuss.DataObjects.Universal.EntryType()
						{
							Code = DataTransfer.Universal.Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID,
							Description = DataTransfer.Universal.Constants.AdditionalReference.EntryType.Descriptions.ResponsiblePartyID
						}
					};

					return data.AdditionalReferenceCollection.MergeCollectionByCandidateKey(
						new[] { responsiblePartyReference }, keepExistingData);
				});
			}
		}

		protected override CusSCAPivotDataObjectWriter<CusSCAPivot> GetNewCusSCAPivotDataObjectWriter()
		{
			return new CMRCusSCAPivotDataObjectWriter(writeManager);
		}

		#endregion // Implementation
	}
}
