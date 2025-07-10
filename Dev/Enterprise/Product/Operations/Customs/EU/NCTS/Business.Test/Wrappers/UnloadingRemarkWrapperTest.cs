using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class UnloadingRemarkWrapperTest : Customs.Business.Testing.DataProviderTestCase<UnloadingRemarkWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new UnloadingRemarkWrapper(null));
		}

		public void TestStateOfSealsOk()
		{
			unloadingRemark.G9_StateOfSealsOk = YesNoList.Codes.Yes;
			AssertEquals(YesNoList.Codes.Yes, wrapper.StateOfSealsOk);
		}

		public void TestConform()
		{
			unloadingRemark.G9_Conform = "T";
			AssertEquals("T", wrapper.Conform);
		}

		public void TestUnloadingCompletion()
		{
			unloadingRemark.G9_UnloadingCompletion = "T";
			AssertEquals("T", wrapper.UnloadingCompletion);
		}

		public void TestUnloadingDate()
		{
			unloadingRemark.G9_UnloadingDate = new ZDateTime(2020, 12, 19);
			AssertEquals("20201219", wrapper.UnloadingDate);
		}

		public void TestNoOfSeals()
		{
			CombineAssertions(() =>
			{
				unloadingRemark.G9_StateOfSealsOk = YesNoList.Codes.No;
				unloadingRemark.G9_NoOfSeals = 123;
				AssertEquals("State Of Seals N", 123, wrapper.NoOfSeals);
				unloadingRemark.G9_StateOfSealsOk = YesNoList.Codes.Yes;
				AssertEquals("State Of Seals Y", ZInt.Zero, wrapper.NoOfSeals);
			});
		}

		public void TestRawNoOfSeals()
		{
			CombineAssertions(() =>
			{
				unloadingRemark.G9_StateOfSealsOk = YesNoList.Codes.No;
				unloadingRemark.G9_NoOfSeals = 123;
				AssertEquals("State Of Seals N", 123, wrapper.RawNoOfSeals);
				unloadingRemark.G9_StateOfSealsOk = YesNoList.Codes.Yes;
				AssertEquals("State Of Seals Y", 123, wrapper.RawNoOfSeals);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			unloadingRemark = header.UnloadingRemark;
			wrapper = new UnloadingRemarkWrapper(header.UnloadingRemark);
		}
		UnloadingRemarkAddInfo unloadingRemark;
		IUnloadingRemarkInterface wrapper;

		protected override UnloadingRemarkWrapper GetProvider() => (UnloadingRemarkWrapper)wrapper;
	}
}
