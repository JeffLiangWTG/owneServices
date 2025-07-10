using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.GUI.CommercialInvoice;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(CommercialInvoiceController))]
	public class CommercialInvoiceControllerTest : Customs.Module.Testing.CommercialInvoiceControllerTest
	{
		public override Type ControllerToBashType => typeof(CommercialInvoiceController);

		protected override string CountryCode => Core.Constants.CountryCodes.Latvia;

		protected override BaseJobComInvoiceHeader GetNewInvoiceHeader() => Factory.New<JobComInvoiceHeader>();

		protected override Type ExpectedFormType => typeof(CommercialInvoiceForm);
	}
}
