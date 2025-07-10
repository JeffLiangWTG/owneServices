using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.GB.GVMS.UniversalDataTransfer.Testing
{
	class GVMSAsycudaManifestHeaderDataObjectWriterHelperTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestExportAsycudaManifestHeaderFields()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_ManifestType = GVMSManifestType.Codes.GoodsVehicleMovementSystemGvms;
			header.RouteId = "35";
			header.IsUnaccompanied = ZBool.True;
			header.EmptyVehicle = GVMSEmptyVehicle.Codes.EmptyVehicleIsBeingMovedUnderAContractOfCarriage;

			var inspectionLocation1 = header.InspectionLocations.AddNew();
			inspectionLocation1.CY_Code = "1";
			inspectionLocation1.CY_Data = "L0029A";
			var inspectionLocation2 = header.InspectionLocations.AddNew();
			inspectionLocation2.CY_Code = "2";
			inspectionLocation2.CY_Data = "L0030A";

			var customsRef1 = header.GvmsCustomsReferenceCollection.AddNew();
			var customsRef2 = header.GvmsCustomsReferenceCollection.AddNew();
			var customsTransitReference = header.GvmsTransitReferenceCollection.AddNew();
			var customsEidrReference = header.GvmsEidrAndOralReferenceCollection.AddNew();

			customsRef1.CSI_Code = GVMSCustomsReference.Codes.AtaCarnet;
			customsRef1.CSI_DateOfIssue = new ZDateTime(2020, 12, 01);
			customsRef1.CSI_RN_NKCountryCode = "GB";
			customsRef1.CSI_ReferenceNumber = "ABC123";
			customsRef1.CSI_ReferenceNumber2 = "PalletRef1";
			customsRef1.CSI_Status = "Yes";

			customsRef2.CSI_Code = GVMSCustomsReference.Codes.ImportControlSystemEntrySummaryDeclaration;
			customsRef2.CSI_DateOfIssue = new ZDateTime(2020, 12, 01);
			customsRef2.CSI_RN_NKCountryCode = "GB";
			customsRef2.CSI_ReferenceNumber = "ABC234";
			customsRef2.CSI_ReferenceNumber2 = "PalletRef2";
			customsRef2.CSI_Status = "Yes";

			customsTransitReference.CSI_Code = GVMSCustomsReference.Codes.NctsOrCtcTransitMovementReferenceNumber;
			customsTransitReference.CSI_DateOfIssue = new ZDateTime(2020, 12, 01);
			customsTransitReference.CSI_RN_NKCountryCode = "GB";
			customsTransitReference.CSI_ReferenceNumber = "ABC345";
			customsTransitReference.CSI_ReferenceNumber2 = "PalletRef3";
			customsTransitReference.CSI_Status = "Yes";

			customsEidrReference.CSI_Code = GVMSCustomsReference.Codes.EntryInDeclarantsRecord;
			customsEidrReference.CSI_DateOfIssue = new ZDateTime(2020, 12, 01);
			customsEidrReference.CSI_RN_NKCountryCode = "GB";
			customsEidrReference.CSI_ReferenceNumber = "ABC456";
			customsEidrReference.CSI_ReferenceNumber2 = "PalletRef4";
			customsEidrReference.CSI_Status = "Yes";

			Factory.SaveForTesting();

			var headerData = (UniversalShipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, header);

			var routeID = headerData.EntryInstructionCollection.ElementAt(0).AddInfoCollection.FirstOrDefault(x => x.Key.Value == AsycudaManifestHeader.Schema.RouteId).Value;
			var isUnaccompanied = headerData.EntryInstructionCollection.ElementAt(0).AddInfoCollection.FirstOrDefault(x => x.Key.Value == AsycudaManifestHeader.Schema.IsUnaccompanied).Value;
			var emptyVehicle = headerData.EntryInstructionCollection.ElementAt(0).AddInfoCollection.FirstOrDefault(x => x.Key.Value == AsycudaManifestHeader.Schema.EmptyVehicle).Value;

			AssertEquals("RouteId", "35", routeID);
			AssertEquals("IsUnaccompanied", ZBool.True.ToString(), isUnaccompanied);
			AssertEquals("EmptyVehicle", GVMSEmptyVehicle.Codes.EmptyVehicleIsBeingMovedUnderAContractOfCarriage, emptyVehicle);

			AssertEquals(2, header.CustomsReferences.ToList().Count);
			AssertEquals(1, header.CustomsTransitReferences.ToList().Count);
			AssertEquals(1, header.CustomsEidrAndOralReferences.ToList().Count);
			AssertEquals(2, header.InspectionLocations.Count);

			CombineAssertions("Customs References", () =>
			{
				var element = headerData.CustomsSupportingInformationCollection[0];
				AssertEquals(GVMSCustomsReference.Codes.AtaCarnet, element.Type.Code);
				AssertEquals("GVM", element.Category.Code);
				AssertEquals("ABC123", element.ReferenceNumber);
				AssertNotNull("ReferenceNumberCollection", element.ReferenceNumberCollection);
				AssertEquals(1, element.ReferenceNumberCollection.Count);
				AssertEquals(DataTransfer.Universal.Constants.ReferenceNumberTypes.Codes.LocalReferenceNumber, element.ReferenceNumberCollection[0].Type.Code);
				AssertEquals("PalletRef1", element.ReferenceNumberCollection[0].ReferenceNumber);

				element = headerData.CustomsSupportingInformationCollection[1];
				AssertEquals("ABC234", element.ReferenceNumber);
				AssertNotNull("ReferenceNumberCollection", element.ReferenceNumberCollection);
				AssertEquals(1, element.ReferenceNumberCollection.Count);
				AssertEquals(DataTransfer.Universal.Constants.ReferenceNumberTypes.Codes.LocalReferenceNumber, element.ReferenceNumberCollection[0].Type.Code);
				AssertEquals("PalletRef2", element.ReferenceNumberCollection[0].ReferenceNumber);
			});

			CombineAssertions("Customs Transit References", () =>
			{
				var element = headerData.CustomsSupportingInformationCollection[2];
				AssertEquals(GVMSCustomsReference.Codes.NctsOrCtcTransitMovementReferenceNumber, element.Type.Code);
				AssertEquals("GVM", element.Category.Code);
				AssertEquals("ABC345", element.ReferenceNumber);
				AssertNotNull("ReferenceNumberCollection", element.ReferenceNumberCollection);
				AssertEquals(1, element.ReferenceNumberCollection.Count);
				AssertEquals(DataTransfer.Universal.Constants.ReferenceNumberTypes.Codes.LocalReferenceNumber, element.ReferenceNumberCollection[0].Type.Code);
				AssertEquals("PalletRef3", element.ReferenceNumberCollection[0].ReferenceNumber);
			});

			CombineAssertions("Customs EIDR References", () =>
			{
				var element = headerData.CustomsSupportingInformationCollection[3];
				AssertEquals(GVMSCustomsReference.Codes.EntryInDeclarantsRecord, element.Type.Code);
				AssertEquals("GVM", element.Category.Code);
				AssertEquals("ABC456", element.ReferenceNumber);
				AssertEquals("Yes", element.Status.Code);
				AssertNotNull("ReferenceNumberCollection", element.ReferenceNumberCollection);
				AssertEquals(1, element.ReferenceNumberCollection.Count);
				AssertEquals(DataTransfer.Universal.Constants.ReferenceNumberTypes.Codes.LocalReferenceNumber, element.ReferenceNumberCollection[0].Type.Code);
				AssertEquals("PalletRef4", element.ReferenceNumberCollection[0].ReferenceNumber);
			});

			CombineAssertions("Inspection Locations", () =>
			{
				var locations = headerData.LocationOfGoodsCollection;
				AssertEquals(LocationOfGoodsType.Inspection, locations[0].Type);
				AssertEquals("1", locations[0].SubType.Code);
				AssertEquals("L0029A", locations[0].AdditionalIdentifier);
				AssertEquals(LocationOfGoodsType.Inspection, locations[1].Type);
				AssertEquals("2", locations[1].SubType.Code);
				AssertEquals("L0030A", locations[1].AdditionalIdentifier);
			});
		}
	}
}
