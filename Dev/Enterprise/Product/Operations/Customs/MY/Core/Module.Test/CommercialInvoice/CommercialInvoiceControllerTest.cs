using System;
using Enterprise.Customs.MY.Business;
using Enterprise.Customs.MY.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.MY.Module.Testing
{
	[TestedType(typeof(CommercialInvoiceController))]
	class CommercialInvoiceControllerTest : Customs.Module.Testing.CommercialInvoiceControllerTest
	{
		protected override Customs.Business.BaseJobComInvoiceHeader GetNewInvoiceHeader() => Factory.New<JobComInvoiceHeader>();

		protected override Type ExpectedFormType => typeof(CommercialInvoiceForm);
	}
}
