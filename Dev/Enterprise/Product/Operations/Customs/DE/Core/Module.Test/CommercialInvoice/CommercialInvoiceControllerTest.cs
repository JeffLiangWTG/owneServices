using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Module.Testing
{
	[TestedType(typeof(CommercialInvoiceController))]
	public class CommercialInvoiceControllerTest : EU.Module.Testing.CommercialInvoiceControllerTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Germany;

		protected override BaseJobComInvoiceHeader GetNewInvoiceHeader() => Factory.New<JobComInvoiceHeader>();

		protected override Type ExpectedFormType => typeof(CommercialInvoiceForm);

		public override Type ControllerToBashType => typeof(CommercialInvoiceController);
	}
}
