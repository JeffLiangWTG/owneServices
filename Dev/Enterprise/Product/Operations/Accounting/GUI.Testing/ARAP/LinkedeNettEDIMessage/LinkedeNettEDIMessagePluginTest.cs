using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	public class LinkedeNettEDIMessagePluginTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestNoExceptionOnReversing()
		{
			APCreditNote creditNote = ObjectCreator.CreateAPCreditNote("123", ObjectCreator.AALSHI, ObjectCreator.AUD, 1m, "took me 4 hours to write this test!");
			Factory.Save();

			ReversingFactory reversingFactory = new ReversingFactory();
			ReversingBase reversing = reversingFactory.NewReversing(creditNote);
			reversing.Reverse();
			((APInvoice)reversing.ReverseTransaction).AH_TransactionNum = "REVERSE0001";

			using (APInvoiceFormTest.MockAPInvoiceForm form = new APInvoiceFormTest.MockAPInvoiceForm(reversing.ReverseTransaction as APInvoice) { DisplayMode = Enterprise.ZArchitecture.Core.ODisplayMode.Delete })
			{
				form.Show();
				form.PlugIns.GetPlugIn(ControllerIDs.LinkedeNettEDIMessage).SelectTabPage();
				form.Delete_Exposed();
			}
		}

		#region Implementation

		TestObjectCreator fObjectCreator;
		TestObjectCreator ObjectCreator
		{
			get { return fObjectCreator ?? (fObjectCreator = new TestObjectCreator(Factory)); }
		}

		#endregion
	}
}
