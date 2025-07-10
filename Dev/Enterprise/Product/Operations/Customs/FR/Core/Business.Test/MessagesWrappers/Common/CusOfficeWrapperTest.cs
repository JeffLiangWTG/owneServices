using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.MessagesWrappers.Testing;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	class CusOfficeWrapperTest : TestCaseWithFactory
	{
		public void TestCusOfficeWrapperConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CusOfficeWrapper(null));
		}

		public void TestExitOffice()
		{
			AssertEquals(WrapperTestHelper.OfficeOfExit, cusOfficeWrapper.ExitOffice);
		}

		public void TestECSExitType()
		{
			AssertEquals("02", cusOfficeWrapper.ECSExitType);
		}

		public void TestECSMotivation()
		{
			AssertEquals(WrapperTestHelper.ExportExitTypeReason, cusOfficeWrapper.ECSMotivation);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var jobHeader = new WrapperTestHelper().CreateTestCusEntryHeader(false);
			cusOfficeWrapper = new CusOfficeWrapper(jobHeader);
		}
		CusOfficeWrapper cusOfficeWrapper;
	}
}
