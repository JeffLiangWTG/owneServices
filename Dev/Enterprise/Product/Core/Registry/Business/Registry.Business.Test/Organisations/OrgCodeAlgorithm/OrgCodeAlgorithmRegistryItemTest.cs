using CargoWise.Organizations.CodeGeneration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OrgCodeAlgorithmRegistryItem))]
	sealed class OrgCodeAlgorithmRegistryItemTest : StronglyTypedRegistryItemTestCase<OrgCodeAlgorithm>
	{
		new OrgCodeAlgorithmRegistryItem Item
		{
			get { return (OrgCodeAlgorithmRegistryItem)base.Item; }
		}

		protected override StronglyTypedRegistryItem<OrgCodeAlgorithm, OrgCodeAlgorithm> GetNewRegistryItem()
		{
			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Length = 1;
			return new OrgCodeAlgorithmRegistryItem("", null, null, null, algorithm);
		}

		public void TestDataType()
		{
			AssertEquals("DataType.AlgorithmType", OrgCodeAlgorithmType.Undefined, ((OrgCodeAlgorithmRegistryDataType)Item.DataType).AlgorithmType);
			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Override;
			OrgCodeAlgorithmRegistryItem anotherItem = new OrgCodeAlgorithmRegistryItem("", null, null, null, algorithm);
			AssertEquals("DataType.AlgorithmType", OrgCodeAlgorithmType.Override, ((OrgCodeAlgorithmRegistryDataType)anotherItem.DataType).AlgorithmType);
		}

		public void TestStorage()
		{
			AssertEquals("Storage", RegistryStorageFlags.System, Item.Storage);
		}
	}
}
