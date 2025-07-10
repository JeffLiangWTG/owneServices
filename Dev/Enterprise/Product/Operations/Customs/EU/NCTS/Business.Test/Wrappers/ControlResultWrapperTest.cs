using System;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class ControlResultWrapperTest : Customs.Business.Testing.DataProviderTestCase<ControlResultWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ControlResultWrapper(null));
		}

		public void TestCorrectedValue()
		{
			result.G9_CorrectedValue = "correctedValueTest";
			AssertEquals("correctedValueTest", wrapper.CorrectedValue);
		}

		public void TestDescription()
		{
			result.G9_Description = "descriptionTest";
			AssertEquals("descriptionTest", wrapper.Description);
		}

		public void TestDescription_Length()
		{
			result.G9_Description = new string('A', 150);
			AssertEquals(140, wrapper.Description.Length);
		}

		public void TestDescriptionLNG()
		{
			AssertEquals(ZString.Empty, wrapper.DescriptionLNG);
		}

		public void TestPointerToTheAttribute()
		{
			result.G9_PointerToTheAttribute = "pointerTest";
			AssertEquals("pointerTest", wrapper.PointerToTheAttribute);
		}

		public void TestControlIndicator()
		{
			result.G9_ControlIndicator = "TS";
			AssertEquals("TS", wrapper.ControlIndicator);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			result = header.ResultsOfControlCollection.AddNew().Data;
			wrapper = new ControlResultWrapper(result);
		}
		ResultsOfControlAddInfo result;
		ControlResultWrapper wrapper;
		NctsHeader header;

		protected override ControlResultWrapper GetProvider() => wrapper;
	}
}
