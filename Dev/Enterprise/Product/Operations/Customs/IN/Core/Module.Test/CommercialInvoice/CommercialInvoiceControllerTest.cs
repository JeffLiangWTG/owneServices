using System;
using Enterprise.Customs.IN.Business;
using Enterprise.Customs.IN.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Module.Testing;

[TestedType(typeof(CommercialInvoiceController))]
sealed class CommercialInvoiceControllerTest : Customs.Module.Testing.CommercialInvoiceControllerTest
{
	protected override Customs.Business.BaseJobComInvoiceHeader GetNewInvoiceHeader() => Factory.New<JobComInvoiceHeader>();

	protected override Type ExpectedFormType => typeof(CommercialInvoiceForm);
}
