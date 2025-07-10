using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMComponentController))]
	class BMComponentControllerTest : BMControllerTest
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

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.BMComponent;
		}
	}
}
