using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CusExitControlHeader))]
	class CusExitControlHeaderTest : EnterpriseBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestCEH_LocationOfGoods_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(exitHeader.CEH_LocationOfGoodsInfo).Caption, Is.EqualTo("Loading Place"));
		}

		[ExpectNoExceptions]
		public void TestLookups()
		{
			NUnit.Framework.Assert.That(exitHeader.Lookups, Is.TypeOf<CusExitControlHeaderLookups>());
		}

		[ExpectNoExceptions]
		public void TestValidation()
		{
			NUnit.Framework.Assert.That(exitHeader.Validation, Is.TypeOf<CusExitControlHeaderValidation>());
		}

		[ExpectNoExceptions]
		public void TestCusExitDetails()
		{
			NUnit.Framework.Assert.That(exitHeader.CusExitDetails, Is.TypeOf<CusExitDetailCollection>());
		}

		protected override void SetUp()
		{
			base.SetUp();

			exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
		}
		CusExitControlHeader exitHeader;
	}
}
