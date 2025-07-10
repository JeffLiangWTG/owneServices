using System;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.MY.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeader))]
	sealed class JobComInvoiceHeaderTest : Customs.Business.Testing.BaseJobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
	{
		public override string GetLocalCurrencyCode() => Core.Constants.CurrencyCodes.Malaysia;

		protected override bool RatesAreReciprocal
		{
			get { return true; }
		}

		protected override Type ExpectedTypeOfGroupCharges => typeof(JobComInvApportionedChargeCollection<InvoiceApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceCharge>);
	}

	public class JobComInvoiceHeaderFunctionalTest : Customs.Business.Testing.BaseJobComInvoiceHeaderFunctionalTest
	{
		protected override Customs.Business.BaseJobDeclaration GetNewDeclaration()
		{
			return JobDeclaration.New(Factory);
		}
	}

	public class JobComInvoiceHeaderApportionTest : Customs.Business.Testing.BaseJobComInvoiceHeaderApportionTest
	{
		protected override Customs.Business.BaseJobDeclaration GetNewDeclaration()
		{
			return JobDeclaration.New(Factory);
		}
	}

	public class JobComInvoiceHeaderCalculationTest : Customs.Business.Testing.BaseJobComInvoiceHeaderCalculationTest
	{
		protected override Customs.Business.BaseJobDeclaration GetNewDeclaration()
		{
			return JobDeclaration.New(Factory);
		}
	}

	public class JobComInvoiceHeaderTestForDocumentWrappert : Customs.Business.Testing.BaseJobComInvoiceHeaderTestForDocumentWrapper
	{
		protected override Customs.Business.BaseJobDeclaration GetNewDeclaration()
		{
			return JobDeclaration.New(Factory);
		}
	}
}
