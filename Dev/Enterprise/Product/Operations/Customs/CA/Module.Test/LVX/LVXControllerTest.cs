using System;
using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(LVXController))]
	sealed class LVXControllerTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType => typeof(LVXController);

		public void TestDefaultJE_OH_Importer()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG";
			Factory.Save();

			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_InvoiceNumber = "INV0001";
			invoice.JZ_OH_Buyer = org.PK;
			Factory.Save();

			var lvx = LVXController.CreateNewBusinessObject(Factory, invoice);
			AssertEquals("INV0001", lvx.LVXInvoiceHeader.JZ_InvoiceNumber);
			AssertEquals(invoice.PK, lvx.LVXInvoiceHeader.PK);
			AssertEquals(org.PK, lvx.JE_OH_Importer);
		}

		public void TestCreateNewBusinessObject()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_InvoiceNumber = "INV0001";
			Factory.Save();
			var lvx1 = LVXController.CreateNewBusinessObject(Factory);
			AssertEquals("", lvx1.LVXInvoiceHeader.JZ_InvoiceNumber);

			var lvx2 = LVXController.CreateNewBusinessObject(Factory, invoice);
			AssertEquals("INV0001", lvx2.LVXInvoiceHeader.JZ_InvoiceNumber);
			AssertEquals(invoice.PK, lvx2.LVXInvoiceHeader.PK);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CA.CALVXJobs;

		protected override Type GetBusinessObjectType() => typeof(JobDeclaration);

		protected override string CountryCode => Core.Constants.CountryCodes.Canada;
	}
}
