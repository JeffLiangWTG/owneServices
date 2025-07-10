using System;
using System.Text;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(HostNameStringListRegistryItem))]
	public class HostNameStringListRegistryItemTest : StronglyTypedRegistryItemTestCase<string>
	{
		protected override StronglyTypedRegistryItem<string, string> GetNewRegistryItem()
		{
			return new HostNameStringListRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.NotCached, "");
		}
	}

	[TestedType(typeof(HostNameStringListRegistryDataType))]
	public class HostNameStringListRegistryDataTypeTest : RegistryDataTypeTestCase<HostNameStringListRegistryDataType>
	{
		public void TestostNameStringListRegistryDataTypeValidation()
		{
			var hostNameStringListRegistryItem = new HostNameStringListRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.NotCached, "");

			var dataType = (HostNameStringListRegistryDataType)hostNameStringListRegistryItem.DataType;
			var invalidHostnameLists = new[] { "hostnameA, hostnameB", "hostname.", "Invalid character", "a-x.x", "www./," };
			foreach (var hostnameList in invalidHostnameLists)
			{
				AssertExceptionThrown(typeof(RegistryValidationException), "Invalid input: Please input valid machine names and separate each machine name by ','.",
					() => dataType.Validate(hostNameStringListRegistryItem, hostnameList, Guid.Empty, Guid.Empty, Guid.Empty));
			}

			AssertNoExceptionThrown(() => dataType.Validate(hostNameStringListRegistryItem, "GoodHostName-1,GoodHostName-2", Guid.Empty, Guid.Empty, Guid.Empty));
		}

		protected override HostNameStringListRegistryDataType GetNewDataType()
		{
			return new HostNameStringListRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB("HostnameA,HostnameB", Encoding.Unicode.GetBytes("HostnameA,HostnameB")),
				new ValidSampleAndBinaryValueInDB("Hostname-A", Encoding.Unicode.GetBytes("Hostname-A")),
				new ValidSampleAndBinaryValueInDB("Hostname-A,Hostname_B", Encoding.Unicode.GetBytes("Hostname-A,Hostname_B")),
				new ValidSampleAndBinaryValueInDB("", Encoding.Unicode.GetBytes(""))
			};
		}

		protected override object GetNullRepresentation()
		{
			return StringRegistryDataTypeTest.GetNullStringRepresentation();
		}
	}
}
