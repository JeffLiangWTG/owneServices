using System;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.GUI.CommercialInvoice;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(CommercialInvoiceController))]
	sealed class CommercialInvoiceControllerTest : Customs.Module.Testing.CommercialInvoiceControllerTest
	{
		protected override Customs.Business.BaseJobComInvoiceHeader GetNewInvoiceHeader()
		{
			return Factory.New<JobComInvoiceHeader>();
		}

		protected override Type ExpectedFormType
		{
			get { return typeof(CommercialInvoiceForm); }
		}
	}
}
