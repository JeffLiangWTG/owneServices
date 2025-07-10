using System;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Diagnostics.Testing
{
	[TestedType(typeof(DirectxTDiagnosticsForm))]
	class DirectxTDiagnosticsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new DirectxTDiagnosticsForm();
		}

		public void TestSendButton_Click()
		{
			GlbCompany.CurrentCompany.GC_Code = "DEM";
			ObjectFactory.Get<IProductRegistration>().KeyForTest.EnterpriseCodeForTest = "";

			var tester = new DirectxTDiagnosticsFormTester();
			Assert(!tester.ProceedOrPopupErrorMessage(tester, new EventArgs()));
			AssertEquals(tester.Message, "You cannot use this feature when you are logged into the DEMO company. Please log into a different company.");

			tester.Dispose();
		}
	}
}
