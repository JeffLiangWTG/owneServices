using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DpsWebServiceItemRegistryDataType))]
	sealed class DpsWebServiceItemRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DpsWebServiceItemRegistryDataType>
	{
		protected override DpsWebServiceItemRegistryDataType GetNewDataType()
		{
			return new DpsWebServiceItemRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = new DpsWebServiceItemCollection();
			var collection2 = new DpsWebServiceItemCollection
			{
				new DpsWebServiceItem { Code = "DPS1", WebServiceUrl = "https://dpsv4.wisegrid.net", Role = RoleHelper.Code.Production },
				new DpsWebServiceItem { Code = "DPS2", WebServiceUrl = "https://dpsv4-usord.wisegrid.net", Role = RoleHelper.Code.ProductionFailover },
				new DpsWebServiceItem { Code = "STG1", WebServiceUrl = "https://dpsv4-test.wisegrid.net", Role = RoleHelper.Code.Staging }
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, DataType.Serialise(collection1)),
				new ValidSampleAndBinaryValueInDB(collection2, DataType.Serialise(collection2))
			};
		}

		protected override string ExpectedEditorName
		{
			get { return "DpsWebServiceItemRegistryItemEditor"; }
		}

		public void TestValidateBeforeRegistryFormSaveCore()
		{
			var webServiceUrl = new DpsWebServiceItemCollection();
			var proposedValue = webServiceUrl.DefaultValue;
			var registryItem = new DpsWebServiceItemCollectionRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, new DpsWebServiceItemCollection());
			var validate = new AnonymousMethod(() => DataType.ValidateBeforeRegistryFormSave(registryItem, proposedValue, Guid.Empty, Guid.Empty, Guid.Empty));

			AssertNoExceptionThrown(validate);

			proposedValue.RemoveAll();

			validate = () => DataType.ValidateBeforeRegistryFormSave(registryItem, proposedValue, Guid.Empty, Guid.Empty, Guid.Empty);
			var ex = AssertExceptionThrown<RegistryValidationException>(validate);

			AssertMultilineASCIIEquals("Please add at least one Web Service URL when this registry is overridden.", ex.Message);
		}
	}
}
