using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI.CommercialInvoice;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(CommercialInvoiceController))]
	sealed class CommercialInvoiceControllerTest : Customs.Module.Testing.CommercialInvoiceControllerTest
	{
		protected override Business.BaseJobComInvoiceHeader GetNewInvoiceHeader() => Factory.New<JobComInvoiceHeader>();

		protected override Type ExpectedFormType => typeof(CommercialInvoiceForm);
	}
}
