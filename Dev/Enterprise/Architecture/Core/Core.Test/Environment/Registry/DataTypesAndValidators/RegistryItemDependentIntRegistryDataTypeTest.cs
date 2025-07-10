using System;
using System.Text;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(RegistryItemDependentIntRegistryDataType))]
	sealed class RegistryItemDependentIntRegistryDataTypeTest : RegistryDataTypeTestCase<RegistryItemDependentIntRegistryDataType>
	{
		protected override RegistryItemDependentIntRegistryDataType GetNewDataType()
		{
			IntRegistryItem item1 = new IntRegistryItem("A", null, null, null, RegistryStorageFlags.All);
			IntRegistryItem item2 = new IntRegistryItem("B", null, null, null, RegistryStorageFlags.All);
			item1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			item2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 23);
			return new RegistryItemDependentIntRegistryDataType(item1, item2);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(10, Encoding.Unicode.GetBytes("10")),
				new ValidSampleAndBinaryValueInDB(23, Encoding.Unicode.GetBytes("23"))
			};
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] { -1000, 2000 };
		}
	}
}
