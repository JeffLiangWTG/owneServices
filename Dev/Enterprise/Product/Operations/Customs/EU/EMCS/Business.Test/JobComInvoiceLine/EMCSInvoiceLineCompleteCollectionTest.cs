using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSInvoiceLineCompleteCollection))]
	public class EMCSInvoiceLineCompleteCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultForFirstInvoiceLine()
		{
			var invoiceLine = declaration.InvoiceLines.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("JI_WeightUQ is defaulted to KG", Core.Constants.Weight.Kilograms, invoiceLine.JI_WeightUQ);
				AssertEquals("JI_NetWeightUQ is defaulted to KG", Core.Constants.Weight.Kilograms, invoiceLine.JI_NetWeightUQ);
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new EMCSInvoiceLineCompleteCollection(declaration);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<EMCSJobDeclaration>();
		}
		EMCSJobDeclaration declaration;
	}
}
