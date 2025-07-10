using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineCompleteCollection))]
	public class InvoiceLineCompleteCollectionTest : EU.Business.Declaration.Testing.InvoiceLineCompleteCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new InvoiceLineCompleteCollection(Declaration);
		}

		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override BaseJobDeclaration GetMeANewJobDeclaration()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			return dec;
		}

		public void TestDefaultWeightUnitValues()
		{
			var declaration = Declaration;
			var invoiceLine = declaration.InvoiceLines.AddNew();

			AssertEquals("JI_WeightUQ is defaulted to KG", Core.Constants.Weight.Kilograms, invoiceLine.JI_WeightUQ);
			AssertEquals("JI_NetWeightUQ is defaulted to KG", Core.Constants.Weight.Kilograms, invoiceLine.JI_NetWeightUQ);
		}
	}
}
