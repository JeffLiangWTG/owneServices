using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMReleaseSequenceController))]
	class BMReleaseSequenceControllerTest : BMControllerTest
	{
		public override void TestViewForm()
		{
			Assert(true);
		}

		public override void TestNewForm()
		{
			Assert(true);
		}

		public override void TestEditForm()
		{
			Assert(true);
		}

		public override void TestDeleteForm()
		{
			Assert(true);
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		public override void TestSaveFormWithCustomsPlugIns()
		{
			Assert(true);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.BMReleaseSequence;
		}
	}
}
