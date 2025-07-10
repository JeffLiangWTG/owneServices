using System;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.AsycudaCustoms.GUI.CommercialInvoice;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Module.Testing
{
	[TestedType(typeof(CommercialInvoiceController))]
	class CommercialInvoiceControllerTest : Customs.Module.Testing.CommercialInvoiceControllerTest
	{
		protected override Customs.Business.BaseJobComInvoiceHeader GetNewInvoiceHeader() => Factory.New<JobComInvoiceHeader>();

		protected override string CountryCode => Core.Constants.CountryCodes.Botswana;

		public override Type ControllerToBashType => typeof(CommercialInvoiceController);

		protected override Type ExpectedFormType => typeof(CommercialInvoiceForm);
	}
}
