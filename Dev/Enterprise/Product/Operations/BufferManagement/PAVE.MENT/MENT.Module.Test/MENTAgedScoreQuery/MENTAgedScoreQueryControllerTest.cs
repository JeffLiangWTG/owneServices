using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Module.Test;
using Enterprise.PAVE.MENT.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.Module.Test
{
	[TestedType(typeof(MENTAgedScoreQueryController))]
	class MENTAgedScoreQueryControllerTest : BMControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.MENTAgedScoreQuery;
		}

		public void TestGetForm()
		{
			using (var form = new MENTAgedScoreQueryController().ShowFormForNewEntity(Factory.NewWithValidTestData<MENTAgedScoreQuery>()))
			{
				AssertNotNull(form);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}
	}
}
