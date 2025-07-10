using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.HR.Test
{
	class LeaveTypeConverterTest : TestCaseWithFactory
	{
		public void TestConvertToInternalLeaveType()
		{
			var defaultValue = new CodeDescriptionPairList();
			defaultValue.AddPair("Annual Leave", "ANN");
			defaultValue.AddPair("Personal Leave", "SIC");
			var converter = new LeaveTypeConverter(defaultValue);
			AssertEquals("ANN", converter.ConvertToInternalLeaveType("Annual Leave"));
			AssertEquals("SIC", converter.ConvertToInternalLeaveType("Personal Leave"));
			AssertEquals("ANN", converter.ConvertToInternalLeaveType("Long Service Leave"));
		}
	}
}
