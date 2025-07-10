namespace Enterprise.Accounting.Business.ComplianceReport.Testing
{
	using CargoWise.EntityFramework.Testing;

	internal class AccTaxReturnLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAccTaxReturnStatusList()
		{
			AssertEquals("Status Count", 2, Lookups.TaxReturnStatusList.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { AccTaxReturn.Status.Saved, AccTaxReturn.Status.Submitted }, Lookups.TaxReturnStatusList.GetAllCodes());

			TaxReturn.ATR_ReturnType = AccTaxReturn.ReturnType.TPAR;
			AssertEquals("Status Count", 3, Lookups.TaxReturnStatusList.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { AccTaxReturn.Status.Saved, AccTaxReturn.Status.Generated, AccTaxReturn.Status.Submitted }, Lookups.TaxReturnStatusList.GetAllCodes());
		}

		AccTaxReturnLookups Lookups
		{
			get { return TaxReturn.Lookups; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			TaxReturn = Factory.NewWithValidTestData<AccTaxReturn>();
		}

		AccTaxReturn TaxReturn;
	}
}