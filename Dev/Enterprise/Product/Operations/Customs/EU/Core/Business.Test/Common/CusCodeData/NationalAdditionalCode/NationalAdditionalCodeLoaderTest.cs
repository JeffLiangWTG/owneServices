using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(NationalAdditionalCode.Loader))]
	sealed class NationalAdditionalCodeLoaderTest : LoaderTestCase
	{
		[ExpectNoExceptions]
		public void TestLoad()
		{
			var code = loader.LoadOrCreate(jobComInvoiceLine, 1);
			NUnit.Framework.Assert.That(loader.Load(jobComInvoiceLine, 1), NUnit.Framework.Is.EqualTo(code));
		}

		[ExpectNoExceptions]
		public void TestLoadOrCreate()
		{
			var code = loader.LoadOrCreate(jobComInvoiceLine, 1);
			NUnit.Framework.Assert.That(code, NUnit.Framework.Is.Not.EqualTo(default(NationalAdditionalCode)), "Code must be created - should not be [null]");
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var classificationViaNewFactory = newFactory.Load<JobComInvoiceLine>(jobComInvoiceLine.PK);
			var codeViaNewFactory = loader.LoadOrCreate(classificationViaNewFactory, 1);
			NUnit.Framework.Assert.That(codeViaNewFactory.PK, NUnit.Framework.Is.EqualTo(code.PK), "Code must be loaded");
		}

		protected override void SetUp()
		{
			base.SetUp();
			var jobDeclaration = Factory.New<JobDeclaration>();
			var invoice = jobDeclaration.Invoices.AddNew();
			jobComInvoiceLine = invoice.InvoiceLines.AddNew();

			loader = new NationalAdditionalCode.Loader(Factory);
		}
		JobComInvoiceLine jobComInvoiceLine;
		NationalAdditionalCode.Loader loader;

		protected override BusinessObject.Loader GetNewLoaderToTest() => loader;
	}
}


