using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CTORECStatusCalculator))]
	sealed class CTORECStatusCalculatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestStatusInfoName()
		{
			AssertEquals("Code", calculator.StatusInfo.Name);
		}

		public void TestInterestedMessageTypes()
		{
			AssertEquals(1, calculator.InterestedMessageTypes.Length);
			AssertEquals(CMRMessage.CMRMessageTypes.CTOREC, calculator.InterestedMessageTypes[0]);
		}

		protected override BusinessObject GetNewBusinessObject() => calculator;

		protected override void SetUp()
		{
			base.SetUp();

			ExportCustomsManifestLines line = Factory.New<ExportCustomsManifestHeader>().Lines.AddNew();
			calculator = new CTORECStatusCalculator(line);
		}

		CTORECStatusCalculator calculator;
	}
}
