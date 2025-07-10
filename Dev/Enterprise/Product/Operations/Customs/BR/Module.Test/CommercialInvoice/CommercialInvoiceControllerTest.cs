using System;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.BR.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
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
}
