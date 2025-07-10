using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC504C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES.Testing
{
	class CC504CProviderTest : TestCaseWithFactory
	{
		public void TestExportOperation()
		{
			CC504CProvider provider = new CC504CProvider(new Cc504C
			{
				ExportOperation = new ExportOperationType03
				{
					Mrn = "MRN",
					Lrn = "LRN",
					AmendmentAcceptanceDateAndTime = ZDateTime.BrettsBirthday.ToDateTime()
				}
			});
			CombineAssertions(() =>
			{
				AssertEquals("MRN", "MRN", provider.MovementReferenceNumber);
				AssertEquals("LRN", "LRN", provider.LocalReferenceNumber);
				AssertEquals("AmendmentAcceptanceDate", ZDateTime.BrettsBirthday, provider.AmendmentAcceptanceDate);
			});
		}
	}
}
