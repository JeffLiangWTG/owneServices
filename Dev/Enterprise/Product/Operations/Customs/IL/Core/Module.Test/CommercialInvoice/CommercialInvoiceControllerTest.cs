using System;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Module.Testing
{
	[TestedType(typeof(CommercialInvoiceController))]
	class CommercialInvoiceControllerTest : Customs.Module.Testing.CommercialInvoiceControllerTest
	{
		protected override Customs.Business.BaseJobComInvoiceHeader GetNewInvoiceHeader() => Factory.New<JobComInvoiceHeader>();

		protected override Type ExpectedFormType => typeof(CommercialInvoiceForm);
	}
}
