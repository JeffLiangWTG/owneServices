using System;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class LineMergerTest : Customs.Business.Testing.LineMergerTest
	{
		public void TestSetCH_BGMReference()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_DeclarationReference = "B00001001";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.ASY_LocalReferenceNumber = "NUMBER1";
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();
			var entry = declaration.ActiveEntryHeaders[0];
			AssertEquals("NUMBER1", entry.CH_BGMReference);
			instruction.ASY_LocalReferenceNumber = "NUMBER2";
			declaration.DoMerge();
			Factory.Save();
			AssertEquals("NUMBER2", entry.CH_BGMReference);
			instruction.ASY_LocalReferenceNumber = "";
			declaration.DoMerge();
			Factory.Save();
			AssertEquals("NUMBER2", entry.CH_BGMReference);
		}

		protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

		protected override Customs.Business.LineMerger GetNewLineMerger(BaseJobDeclaration declaration) => new LineMerger((JobDeclaration)declaration);

		protected override void SetCountrySpecificValueToTheKeyForLinesToGenerateEntryLinePerInvoiceLine(BaseJobComInvoiceHeader header)
		{
			foreach (JobComInvoiceLine invoiceLine in header.InvoiceLines)
			{
				invoiceLine.JI_Description = invoiceLine.PK.ToString();
			}
		}

		protected override bool ApplyClassificationToKeyForLineForMerge => false;
		protected override bool ApplyPartNumberToKeyForLineForMerge => false;

		protected override Type[] ExpectedEntryCreationStrategiesType => new[] { typeof(EntryCreationStrategy) };

		protected override Type ExpectedDutyCalculatorStrategyType => typeof(DutyCalculatorStrategy);
	}
}
