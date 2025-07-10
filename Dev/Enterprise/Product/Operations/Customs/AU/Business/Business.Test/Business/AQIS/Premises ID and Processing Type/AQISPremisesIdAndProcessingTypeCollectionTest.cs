using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AQISPremisesIdAndProcessingTypeCollection))]
	sealed class AQISPremisesIdAndProcessingTypeCollectionTest : AQISCollectionTest<AQISPremisesIdAndProcessingTypeCollection, AQISPremisesIdAndProcessingType>
	{
		public void TestLoadingOneAQISPremisesIdAndProcessingType()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.AddInfo.ZA_AQISPremIdProcessType_Hidden = "Prem/Process";
			AQISPremisesIdAndProcessingTypeCollection collection = new AQISPremisesIdAndProcessingTypeCollection(Factory, invoiceLine.AddInfo);
			collection.SplitAndAddAQISElements(invoiceLine.AddInfo.ZA_AQISPremIdProcessType_Hidden);
			AssertEquals("Collection count", 1, collection.Count);
			AssertEquals("Premsis Id", "Prem", collection[0].PremisesId);
			AssertEquals("Processing Type", "Process", collection[0].ProcessingType);
		}

		public void TestLoadingProcessingTypeWithoutPremisesId()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.AddInfo.ZA_AQISPremIdProcessType_Hidden = "/Process";
			AQISPremisesIdAndProcessingTypeCollection collection = new AQISPremisesIdAndProcessingTypeCollection(Factory, invoiceLine.AddInfo);
			collection.SplitAndAddAQISElements(invoiceLine.AddInfo.ZA_AQISPremIdProcessType_Hidden);
			AssertEquals("Collection count", 1, collection.Count);
			AssertEquals("Premsis Id", "", collection[0].PremisesId);
			AssertEquals("Processing Type", "Process", collection[0].ProcessingType);
		}

		public void TestLoadingMultipleAQISPremisesIdAndProcessingType()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.AddInfo.ZA_AQISPremIdProcessType_Hidden = "Prem1/Process1,Prem2/Process2";
			AQISPremisesIdAndProcessingTypeCollection collection = new AQISPremisesIdAndProcessingTypeCollection(Factory, invoiceLine.AddInfo);
			collection.SplitAndAddAQISElements(invoiceLine.AddInfo.ZA_AQISPremIdProcessType_Hidden);
			AssertEquals("Collection count", 2, collection.Count);

			AssertEquals("Premsis Id", false, collection[0].PremisesId.IsEmpty);
			AssertEquals("Processing Type", false, collection[0].ProcessingType.IsEmpty);
			AssertEquals("Premsis Id", false, collection[1].PremisesId.IsEmpty);
			AssertEquals("Processing Type", false, collection[1].ProcessingType.IsEmpty);
		}

		public void TestGetNewAQISPackageString()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			AQISPremisesIdAndProcessingTypeCollection collection = new AQISPremisesIdAndProcessingTypeCollection(Factory, invoiceLine.AddInfo);

			AQISPremisesIdAndProcessingType premisesIdAndProcessingType1 = new AQISPremisesIdAndProcessingType(Factory, JobDeclaration.New(Factory));
			premisesIdAndProcessingType1.PremisesId = "Prem1";
			premisesIdAndProcessingType1.ProcessingType = "Process1";
			collection.Add(premisesIdAndProcessingType1);
			collection.ReBuildAndSaveAQISElements();
			AssertEquals("PremisesIdAndProcessingType", "Prem1/Process1", invoiceLine.AddInfo.ZA_AQISPremIdProcessType_Hidden);

			AQISPremisesIdAndProcessingType premisesIdAndProcessingType2 = new AQISPremisesIdAndProcessingType(Factory, JobDeclaration.New(Factory));
			premisesIdAndProcessingType2.PremisesId = "Prem2";
			premisesIdAndProcessingType2.ProcessingType = "Process2";
			collection.Add(premisesIdAndProcessingType2);
			collection.ReBuildAndSaveAQISElements();
			AssertEquals("PremisesIdAndProcessingType", true, invoiceLine.AddInfo.ZA_AQISPremIdProcessType_Hidden.Contains("Prem1/Process1"));
			AssertEquals("PremisesIdAndProcessingType", true, invoiceLine.AddInfo.ZA_AQISPremIdProcessType_Hidden.Contains("Prem2/Process2"));
			AssertEquals("PremisesIdAndProcessingType", false, invoiceLine.AddInfo.ZA_AQISPremIdProcessType_Hidden.EndsWith(","));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => new AQISPremisesIdAndProcessingType(Factory, JobDeclaration.New(Factory));

		protected override AQISPremisesIdAndProcessingTypeCollection GetCollectionToTest()
		{
			var declaration = JobDeclaration.New(Factory);
			return new AQISPremisesIdAndProcessingTypeCollection(Factory, declaration.AddInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(CMRAqisPremises.Schema.TableName);
		}
	}
}
