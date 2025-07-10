using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	partial class AsycudaWriterTest
	{
		public void TestExportAsycudaBillToEntryInstruction()
		{
			PrepareCusCodeDataForTesting();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_MasterBill = "MKS23432";
			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = "NOT";
			bill.ABL_SenderReference = "SR001";
			bill.CustomsEntryNumber = "EN001";
			bill.CustomsEntryNumberType = "ENT";
			bill.ABL_GoodsLocation = "AAA";
			bill.ABL_LocationInformation = "LocationInfo";
			bill.ABL_ShipmentType = "IMP";
			bill.ABL_BillIssuer = "BI";

			Factory.SaveForTesting();
			var headerHelper = new AsycudaManifestHeaderDataObjectWriterHelper(header);

			var writer = new AsycudaBillEntryInstructionDataObjectWriter<AsycudaBill>(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)), headerHelper);
			var billEntryInstructionData = writer.GetDataObject(bill);

			AssertEquals("AAA", billEntryInstructionData.LocationAtClearance.Code);
			AssertEquals("IMP", billEntryInstructionData.Style);
			AssertEquals(1, billEntryInstructionData.OrganizationAddressCollection.Count);
			AssertEquals(nameof(DocAddressType.HouseBillIssuingParty), billEntryInstructionData.OrganizationAddressCollection[0].AddressType);
			AssertEquals(1, billEntryInstructionData.OrganizationAddressCollection[0].RegistrationNumberCollection.Count);
			AssertEquals(Constants.RegistrationTypes.BillIssuer, billEntryInstructionData.OrganizationAddressCollection[0].RegistrationNumberCollection[0].Type.Code);
			AssertEquals(header.AMA_RN_NKCountry, billEntryInstructionData.OrganizationAddressCollection[0].RegistrationNumberCollection[0].CountryOfIssue.Code);
			AssertEquals("BI", billEntryInstructionData.OrganizationAddressCollection[0].RegistrationNumberCollection[0].Value);

			var locationInfo = billEntryInstructionData.AddInfoCollection.FirstOrDefault(x => x.Key.Value == AsycudaBillSchema.Constants.ABL_LocationInformation);
			AssertEquals("LocationInfo", locationInfo.Value);
		}

		public void TestCycleAndBatch()
		{
			var accessEnable = (RegistryItemWrapper)ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable;
			using (accessEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				header.AMA_MasterBill = "MKS23432";
				header.AMA_ManifestType = "MGI";
				var bill = header.Bills.AddNew();
				var country = (Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill)bill;
				country.CycleDate = new ZDateTime(2018, 4, 11);
				country.CycleNumber = "CY1000001";

				Factory.SaveForTesting();

				var headerData = (UniversalShipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, header);
				var billData = headerData.SubShipmentCollection[0];
				var billEntryInstructionData = billData.EntryInstructionCollection[0];
				var cycleDate = billEntryInstructionData.AddInfoCollection.FirstOrDefault(x => x.Key.Value == "CycleDate").Value;
				var cycleNumber = billEntryInstructionData.AddInfoCollection.FirstOrDefault(x => x.Key.Value == "CycleNumber").Value;

				AssertEquals(new ZDateTime(2018, 4, 11).ToISO8601String(), cycleDate);
				AssertEquals("CY1000001", cycleNumber);

				header.AMA_ManifestType = "MGE";
				country.BatchDate = new ZDateTime(2018, 4, 12);
				country.BatchNumber = "CY1000002";
				Factory.SaveForTesting();

				headerData = (UniversalShipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, header);
				billData = headerData.SubShipmentCollection[0];
				billEntryInstructionData = billData.EntryInstructionCollection[0];
				var batchDate = billEntryInstructionData.AddInfoCollection.FirstOrDefault(x => x.Key.Value == "BatchDate").Value;
				var batchNumber = billEntryInstructionData.AddInfoCollection.FirstOrDefault(x => x.Key.Value == "BatchNumber").Value;
				AssertEquals(new ZDateTime(2018, 4, 12).ToISO8601String(), batchDate);
				AssertEquals("CY1000002", batchNumber);
			}
		}
	}
}
