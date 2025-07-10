using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Module.Testing
{
	[TestedType(typeof(CommercialInvoiceController))]
	public class CommercialInvoiceControllerTest : EU.Module.Testing.CommercialInvoiceControllerTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Ireland;

		protected override BaseJobComInvoiceHeader GetNewInvoiceHeader() => Factory.New<JobComInvoiceHeader>();

		protected override Type ExpectedFormType => typeof(CommercialInvoiceForm);

		public override Type ControllerToBashType => typeof(CommercialInvoiceController);
	}
}
