using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class DIFDefaultValuesCusPermitHeaderWrapperTest : TestCaseWithFactory
	{
		public void TestICADIFDefaultValues()
		{
			ICADIFDefaultValues valueProvider = new DIFDefaultValuesCusPermitHeaderWrapper(cusPermitHeader);
			AssertEquals("DocumentNumber", "Per123", valueProvider.DocumentNumber);
			AssertEquals("EffectiveDate", ZDate.BrettsBirthday, valueProvider.EffectiveDate);
			AssertEquals("ExpiryDate", new ZDate(2017, 1, 1), valueProvider.ExpiryDate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusPermitHeader = Factory.New<CusPermitHeader>();
			cusPermitHeader.CPH_Number = "Per123";
			cusPermitHeader.CPH_StartDate = ZDate.BrettsBirthday;
			cusPermitHeader.CPH_EndDate = new ZDate(2017, 1, 1);
		}
		CusPermitHeader cusPermitHeader;
	}
}
