using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ChequeController))]
	sealed class ChequeControllerTest : ZControllerBasherTest
	{
		public override void TestNewForm()
		{
			Assert("Currently not support", true);
		}

		public override void TestViewForm()
		{
			Assert("Currently not support", true);
		}

		public override void TestEditForm()
		{
			Assert("Currently not support", true);
		}

		public override void TestDeleteForm()
		{
			Assert("Currently not support", true);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Cheque;
		}
	}
}
