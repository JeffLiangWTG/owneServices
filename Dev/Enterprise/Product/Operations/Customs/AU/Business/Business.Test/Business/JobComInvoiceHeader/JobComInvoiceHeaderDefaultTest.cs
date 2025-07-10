using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeader))]
	class JobComInvoiceHeaderDefaultTest : Customs.Business.Testing.BaseJobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
	{
		#region Implementation

		public override string GetLocalCurrencyCode() => Core.Constants.CurrencyCodes.Australia;

		protected override IEnumerable<string> MessageTypesForDefaultCurrencyToLocalCurrency(BaseJobDeclaration declaration)
		{
			return base.MessageTypesForDefaultCurrencyToLocalCurrency(declaration).Except(AUJobMessageTypeList.Codes.ExWarehouse);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			return groupHeader.JobComInvoiceHeaders.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			JobComInvoiceHeader header = (JobComInvoiceHeader)GetNewBusinessObject();
			header.MarkAsNeedingValidation();
			return header;
		}

		protected override Type ExpectedTypeOfGroupCharges => typeof(JobComInvApportionedChargeCollection<InvoiceApportionedCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceCharge>);

		#endregion
	}
}
