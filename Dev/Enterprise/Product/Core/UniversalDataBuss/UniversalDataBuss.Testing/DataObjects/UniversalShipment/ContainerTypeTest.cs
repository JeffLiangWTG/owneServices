using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(ContainerType))]
	class ContainerTypeTest : DataObjectTestCase<ContainerType>
	{
		protected override bool ShouldBeFlattenedIntoAttributes
		{
			get { return false; }
		}

		public void TestCategoryIsPopulated()
		{
			var containerTypeBO = Factory.New<IRefContainer>();
			containerTypeBO.RC_Code = "ZD3";
			containerTypeBO.RC_ContainerType = Enterprise.Core.Constants.ContainerTypes.DryStorage;
			containerTypeBO.RC_Description = "HELLO WORLD";
			containerTypeBO.RC_ISOType = "2010";
			var containerType = ContainerType.New(containerTypeBO);
			AssertEquals("ZD3", containerType.Code);
			AssertEquals("HELLO WORLD", containerType.Description);
			AssertEquals("2010", containerType.ISOCode);
			AssertNotNull(containerType.Category);
			AssertEquals(Enterprise.Core.Constants.ContainerTypes.DryStorage, containerType.Category.Code);
			AssertEquals(Enterprise.Core.Constants.ContainerTypeDescriptions.DryStorage, containerType.Category.Description);

			containerTypeBO.RC_ContainerType = ZString.Empty;
			containerType = ContainerType.New(containerTypeBO);
			AssertEquals("ZD3", containerType.Code);
			AssertEquals("HELLO WORLD", containerType.Description);
			AssertEquals("2010", containerType.ISOCode);
			AssertNull(containerType.Category);
		}
	}
}

