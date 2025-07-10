using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Registry.Business.SystemDataRegistry;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(StaffLeaveTypeAlertsDataType))]
	sealed class StaffLeaveTypeAlertsDataTypeTest : RegistryDataTypeTestCase<StaffLeaveTypeAlertsDataType>
	{
		protected override StaffLeaveTypeAlertsDataType GetNewDataType()
		{
			return new StaffLeaveTypeAlertsDataType(LeaveTypeCodeMaxLength);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var list1 = new CodeDescriptionPairList();
			list1.AddPair("ANN", "test alert 1");
			list1.AddPair("SIC", "Test alert 2");
			var list2 = new CodeDescriptionPairList();
			list2.AddPair("CAS", "Test alert 3");

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(list1, new StaffLeaveTypeAlertsDataType(LeaveTypeCodeMaxLength).Serialise(list1)),
				new ValidSampleAndBinaryValueInDB(list2, new StaffLeaveTypeAlertsDataType(LeaveTypeCodeMaxLength).Serialise(list2))
			};
		}
	}
}
