using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX562;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AES.Testing
{
	class EX562ProviderTest : TestCaseWithFactory
	{
		public void TestDeclaration()
		{
			CombineAssertions(() =>
			{
				AssertEquals("MRN", provider.MovementReferenceNumber);
				AssertEquals("CaseId", provider.CaseId);
				AssertEquals("Remarks", provider.Remarks);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			ex562 = new Ex562
			{
				ExportOperation = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_ctypes.DeclarationType
				{
					Mrn = "MRN",
					CaseId = "CaseId",
					Remarks = "Remarks"
				}
			};
			provider = new EX562Provider(ex562);
		}
		Ex562 ex562;
		EX562Provider provider;
	}
}
