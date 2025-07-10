using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(ViewComponentChangeLogController))]
	class ViewComponentChangeLogControllerTest : BMControllerTest
	{
		public override void TestViewForm()
		{
			Assert(true); // Actions not supported
		}

		public override void TestNewForm()
		{
			Assert(true); // Actions not supported
		}

		public override void TestEditForm()
		{
			Assert(true); // Actions not supported
		}

		public override void TestDeleteForm()
		{
			Assert(true); // Actions not supported
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true); // Actions not supported
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ViewComponentChangeLog;
		}

		public override void TestTemplateCopyForm()
		{
			Assert(true); // Cannot copy rows in this view
		}

		public override void TestSaveFormWithCustomsPlugIns()
		{
			Assert(true); // non Customs Controller
		}
	}
}
