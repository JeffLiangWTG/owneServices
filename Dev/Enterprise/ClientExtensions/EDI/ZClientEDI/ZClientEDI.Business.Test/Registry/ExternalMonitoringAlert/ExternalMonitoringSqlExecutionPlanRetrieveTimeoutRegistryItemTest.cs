using System.Text;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Test.ExternalMonitoringAlert;

[TestedType(typeof(ExternalMonitoringSqlExecutionPlanRetrieveTimeoutRegistryItem))]
public class ExternalMonitoringSqlExecutionPlanRetrieveTimeoutRegistryItemTest : StronglyTypedRegistryItemTestCase<int>
{
	protected override StronglyTypedRegistryItem<int, int> GetNewRegistryItem()
	{
		return new ExternalMonitoringSqlExecutionPlanRetrieveTimeoutRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, 1000);
	}

	protected override int ValidValue => 1000;
}

[TestedType(typeof(ExternalMonitoringSqlExecutionPlanRetrieveTimeoutRegistryDataType))]
public class ExternalMonitoringSqlExecutionPlanRetrieveTimeoutRegistryDataTypeTest : RegistryDataTypeTestCase<ExternalMonitoringSqlExecutionPlanRetrieveTimeoutRegistryDataType>
{
	protected override ExternalMonitoringSqlExecutionPlanRetrieveTimeoutRegistryDataType GetNewDataType()
	{
		return new ExternalMonitoringSqlExecutionPlanRetrieveTimeoutRegistryDataType();
	}

	protected override bool IsValidatedOnSetEvenIfEqualDefaultValue => true;

	protected override ValidSampleAndBinaryValueInDB[] GetValidSamples() =>
	[
		new ValidSampleAndBinaryValueInDB(1, Encoding.Unicode.GetBytes("1")),
		new ValidSampleAndBinaryValueInDB(5000, Encoding.Unicode.GetBytes("5000")),
		new ValidSampleAndBinaryValueInDB(10000, Encoding.Unicode.GetBytes("10000"))
	];

	protected override object[] GetInvalidSamples() => [0, -1, -1000];
}
