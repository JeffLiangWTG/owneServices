using System.Text;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Test.ExternalMonitoringAlert;

[TestedType(typeof(ExternalMonitoringSqlExecutionPlanQueryUriRegistryItem))]
public class ExternalMonitoringSqlExecutionPlanQueryUriRegistryItemTest : StronglyTypedRegistryItemTestCase<string>
{
	protected override StronglyTypedRegistryItem<string, string> GetNewRegistryItem()
	{
		return new ExternalMonitoringSqlExecutionPlanQueryUriRegistryItem(string.Empty, null, null, null,
			new TextRegistryEditorInfo(TextEditorType.Url), RegistryStorageFlags.System, RegistryOptions.Default,
			@"http://a.b.c/CPUUsageNetCore/api/Search/SearchQueryHash?QueryHash={0}&ServerInstanceName={1}");
	}

	protected override string ValidValue => @"http://a.b.c/CPUUsageNetCore/api/Search/SearchQueryHash?QueryHash={0}&ServerInstanceName={1}";
}

[TestedType(typeof(ExternalMonitoringSqlExecutionPlanQueryUriRegistryDataType))]
public class ExternalMonitoringSqlExecutionPlanQueryUriRegistryDataTypeTest : RegistryDataTypeTestCase<ExternalMonitoringSqlExecutionPlanQueryUriRegistryDataType>
{
	protected override ExternalMonitoringSqlExecutionPlanQueryUriRegistryDataType GetNewDataType()
	{
		return new ExternalMonitoringSqlExecutionPlanQueryUriRegistryDataType();
	}

	protected override ValidSampleAndBinaryValueInDB[] GetValidSamples() =>
	[
		CreateValidSampleAndBinaryValueInDB(string.Empty),
		CreateValidSampleAndBinaryValueInDB(
			@"http://a.b.c/CPUUsageNetCore/api/Search/SearchQueryHash?QueryHash={0}&ServerInstanceName={1}"),
		CreateValidSampleAndBinaryValueInDB(
			@"https://a.b.c/CPUUsageNetCore/api/Search/SearchQueryHash?QueryHash={0}&ServerInstanceName={1}"),
		CreateValidSampleAndBinaryValueInDB(
			@"https://a.b.c/CPUUsageNetCore/api-v2/Search/SearchQueryHash?QueryHashValue={0}&ServerInstanceFullName={1}"),
	];

	protected override object[] GetInvalidSamples() =>
	[
		"invalid uri",
		@"http://a.b.c/CPUUsageNetCore/api/Search/SearchQueryHash?QueryHash={0}",
		@"a.b.c/CPUUsageNetCore/api/Search/SearchQueryHash?QueryHash={0}&ServerInstanceName={1}",
	];

	protected override object GetNullRepresentation()
	{
		return StringRegistryDataTypeTest.GetNullStringRepresentation();
	}

	ValidSampleAndBinaryValueInDB CreateValidSampleAndBinaryValueInDB(string value)
	{
		return new ValidSampleAndBinaryValueInDB(value, Encoding.Unicode.GetBytes(value));
	}
}
