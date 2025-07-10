using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Module.Testing
{
	[TestedType(typeof(CommercialInvoiceController))]
	public class CommercialInvoiceControllerTest : EU.Module.Testing.CommercialInvoiceControllerTest
	{
		public override Type ControllerToBashType => typeof(CommercialInvoiceController);

		protected override BaseJobComInvoiceHeader GetNewInvoiceHeader() => Factory.New<JobComInvoiceHeader>();

		protected override Type ExpectedFormType => typeof(CommercialInvoiceForm);

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedKingdom;
	}
}
