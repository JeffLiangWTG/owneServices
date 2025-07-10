using System;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Module.Testing;

[TestedType(typeof(CommercialInvoiceController))]
class CommercialInvoiceControllerTest : Customs.Module.Testing.CommercialInvoiceControllerTest
{
	protected override Customs.Business.BaseJobComInvoiceHeader GetNewInvoiceHeader() => Factory.New<JobComInvoiceHeader>();

	protected override Type ExpectedFormType => typeof(CommercialInvoiceForm);
}
