using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	[TestsSubclassesOf(typeof(ZPopupController), ExcludeClientDlls = true)]
	public abstract class ZPopupControllerBasherTest : ZControllerBasherTest
	{
		public override void TestDeleteForm()
		{
			Assert("Cannot delete a form on a 'popup' controller", true);
		}

		public override void TestEditForm()
		{
			Assert("Cannot edit a form on a 'popup' controller", true);
		}

		public override void TestTemplateCopyForm()
		{
			Assert("Cannot template copy a form on a 'popup' controller", true);
		}

		public override void TestViewForm()
		{
			Assert("Cannot view a form on a 'popup' controller", true);
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert("Cannot edit/view/delete form on a 'popup' controller", true);
		}
	}
}
