using System.Linq;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(TaxRegimeCollection))]
	class TaxRegimeCollectionTest : CusSupportingInfoCollectionTest<TaxRegime>
	{
		protected override Customs.Business.CusSupportingInfoCollection<TaxRegime> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();
			return new TaxRegimeCollection(jobComInvoice);
		}

		public void TestAddNewWithSubject()
		{
			var collection = GetCusSupportingInfoCollection() as TaxRegimeCollection;
			var taxRegime = collection.AddNew(TaxRegimeTypeList.Codes.IPI);
			AssertEquals("CSI_SubType", TaxRegimeTypeList.Codes.IPI, taxRegime.CSI_SubType);
			AssertEquals("TaxRegime Has Changes should be False", false, taxRegime.HasChanges);
		}

		public void TestFindBySubject()
		{
			var collection = GetCusSupportingInfoCollection() as TaxRegimeCollection;
			var legalAct = collection.AddNew(TaxRegimeTypeList.Codes.IPI);
			AssertEquals(legalAct, collection.FindBySubject(TaxRegimeTypeList.Codes.IPI));
			AssertNull(collection.FindBySubject(TaxRegimeTypeList.Codes.Duty));
		}

		public void TestDeleteBySubject()
		{
			var collection = GetCusSupportingInfoCollection() as TaxRegimeCollection;
			collection.AddNew(TaxRegimeTypeList.Codes.IPI);
			collection.AddNew(TaxRegimeTypeList.Codes.Duty);

			AssertEquals("TaxRegimeCollection must contain two taxes", 2, collection.Count);

			collection.DeleteBySubject(TaxRegimeTypeList.Codes.Duty);
			CombineAssertions(() =>
			{
				Assert("TaxRegimeCollection must contain CSI_SubType = 2", collection.Cast<TaxRegime>().Any(x => x.CSI_SubType == TaxRegimeTypeList.Codes.IPI));
				Assert("TaxRegimeCollection must NOT contain CSI_SubType = 1", !collection.Cast<TaxRegime>().Any(x => x.CSI_SubType == TaxRegimeTypeList.Codes.Duty));
			});
		}
	}
}
