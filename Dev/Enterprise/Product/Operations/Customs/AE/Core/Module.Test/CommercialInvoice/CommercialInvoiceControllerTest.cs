using System;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.AE.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Module.Testing;

[TestedType(typeof(CommercialInvoiceController))]
public class CommercialInvoiceControllerTest : Customs.Module.Testing.CommercialInvoiceControllerTest
{
	protected override Customs.Business.BaseJobComInvoiceHeader GetNewInvoiceHeader()
	{
		return Factory.New<JobComInvoiceHeader>();
	}

	protected override Type ExpectedFormType
	{
		get
		{
			return typeof(CommercialInvoiceForm);
		}
	}
}
