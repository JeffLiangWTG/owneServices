using Enterprise.BarcodeParsing.Business.Testing;
using Enterprise.BarcodeParsing.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BarcodeParsing.Module.Testing
{
	[TestedType(typeof(BarcodeRuleSetController))]
	abstract class BarcodeRuleSetControllerTest : ZControllerBasherTest
	{
		#region TestSecurity

		public void TestSecurity()
		{
			AssertEquals(Env.Security.BarcodeParsingDelete, Controller.GetCheckPointForDelete(null));
			AssertEquals(Env.Security.BarcodeParsingEdit, Controller.GetCheckPointForEdit(null));
			AssertEquals(Env.Security.BarcodeParsingNew, Controller.GetCheckPointForNew(null));
			AssertEquals(Env.Security.BarcodeParsingView, Controller.GetCheckPointForView(null));
		}

		#endregion

		#region TestForm

		public void TestForm()
		{
			using (var form = Controller.ShowNewForm())
			{
				AssertEquals(typeof(BarcodeRuleSetForm), form.GetType());
			}
		}

		public override void TestDeleteForm()
		{
			AssertControllerNotNull();
			var bizo = GetBusinessObjectThatIsInTheDatabase();
			AssertNull(Controller.ShowDeleteForm(bizo));
		}

		#endregion

		#region Implementation

		protected override ControllerID GetControllerID() => ControllerIDs.BarcodeParsing;

		protected BarcodeParsingTestHelper Helper => helper ?? (helper = new BarcodeParsingTestHelper(Factory));

		BarcodeParsingTestHelper helper;

		#endregion
	}
}
