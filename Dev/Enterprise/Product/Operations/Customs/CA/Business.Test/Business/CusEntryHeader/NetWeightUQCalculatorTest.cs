using System;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class NetWeightUQCalculatorTest : Customs.Business.Testing.WeightUQCalculatorTest
	{
		protected override Type JobComInvoiceHeaderType
		{
			get { return typeof(JobComInvoiceHeader); }
		}

		protected override Type CusEntryHeaderType
		{
			get { return typeof(CusEntryHeader); }
		}

		public void TestNetWeightCalculator()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_NetWeight = 2m;
			declaration.CA_NetWeightUQ = "T";
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_NetWeight = 1m;
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_NetWeight = 3m;
			var line2 = invoice2.JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var calculator = new NetWeightUQCalculator(declaration.ActiveEntryHeaders[0]);
			AssertEquals(4m, calculator.Weight);
			AssertEquals("KG", calculator.UQ);
			invoice1.JZ_NetWeight = 0m;
			invoice2.JZ_NetWeight = 0m;
			AssertEquals(2m, calculator.Weight);
			AssertEquals("T", calculator.UQ);
		}
	}
}
