using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	public class GVMSMessageDataObjectExtensionTest : TestCaseWithFactory
	{
		public void TestGetManifest()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_ManifestType = GVMSManifestType.Codes.GoodsVehicleMovementSystemGvms;
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedKingdom;
			var entryNumber = CusEntryNumber.LoadOrCreate(manifestHeader, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.UnitedKingdom);
			entryNumber.CE_EntryNum = "GMRO0000F2KW";

			var ediMessage = Factory.New<GVMSEDIMessage>();

			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_MessageText = @"{
  ""messageId"": ""c68a4442-6336-439f-8dda-5d729fff775c"",
  ""gmrId"": ""GMRO0000F2KW"",
  ""gmrStatusVersion"": 3,
  ""createdDateTime"": ""2021-09-11T10:58:12.384Z"",
  ""updatedDateTime"": ""2021-09-24T04:23:50.384Z"",
  ""state"": ""CHECKED IN"",
  ""inspectionRequired"": false,
  ""reportToLocations"": [{
      ""inspectionTypeId"": ""1"",
      ""locationIds"": [
        ""L0029A"", ""L0030A"", ""L0031A""
      ]
    }, {
      ""inspectionTypeId"": ""2"",
      ""locationIds"": [
        ""L0029A"", ""L0031A""
      ]
    }
  ]
}
";
			Factory.Save();

			var messageDataObject = ediMessage.MessageDataObject;
			var factory = new BusinessObjectFactory();
			var header = messageDataObject.GetMainfest(factory);
			AssertEquals(manifestHeader.PK, header.PK);
		}
	}
}
