using System;
using System.Text;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(GuidArrayRegistryDataType))]
	sealed class GuidArrayRegistryDataTypeTest : RegistryDataTypeTestCase<GuidArrayRegistryDataType>
	{
		protected override GuidArrayRegistryDataType GetNewDataType()
		{
			return new GuidArrayRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			Guid[] newGuids = new Guid[] { Guid.NewGuid() };
			Guid[] emptyList = Array.Empty<Guid>();
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(emptyList, Encoding.Unicode.GetBytes("")),
				new ValidSampleAndBinaryValueInDB(newGuids, Encoding.Unicode.GetBytes(newGuids[0].ToString()))
			};
		}

		protected override void AssertValuesEqual(string message, object lhs, object rhs)
		{
			Guid[] lhsGuids = lhs as Guid[];
			AssertNotNull("LhsGuids should not be null.", lhsGuids);
			Guid[] rhsGuids = rhs as Guid[];
			AssertNotNull("RhsGuids should not be null.", rhsGuids);
			AssertEquals(message, lhsGuids.Length, rhsGuids.Length);

			foreach (Guid item in lhsGuids)
			{
				AssertCollectionContains(item, rhsGuids);
			}
		}
	}
}
