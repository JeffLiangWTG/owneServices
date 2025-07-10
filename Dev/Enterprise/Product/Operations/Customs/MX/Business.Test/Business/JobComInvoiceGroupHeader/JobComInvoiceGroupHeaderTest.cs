using System;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(JobComInvoiceGroupHeader))]
	class JobComInvoiceGroupHeaderTest : Customs.Business.Testing.BaseJobComInvoiceGroupHeaderTest
	{
		public void TestICurrencyConverterDataProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceGroupHeader = declaration.AllGroupHeaders[0];

			var invGroupHeaderAsProvider = invoiceGroupHeader as ICurrencyConverterDataProvider;
			AssertEquals("JE_MessageType = IMP, Rate Type should be", ExchangeRateType.Customs, invGroupHeaderAsProvider.RateType);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("JE_MessageType = EXP, Rate Type should be", ExchangeRateType.CustomsSecondary, invGroupHeaderAsProvider.RateType);
		}

		protected override Type ExpectedTypeOfCharges => typeof(Common.JobComInvChargeCollection<GroupInvoiceCharge>);
	}
}
