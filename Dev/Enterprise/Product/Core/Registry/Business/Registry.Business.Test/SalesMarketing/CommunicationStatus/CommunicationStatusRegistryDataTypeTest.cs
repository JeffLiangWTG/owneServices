using System.Linq;
using System.Text;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CommunicationStatusRegistryDataType))]
	sealed class CommunicationStatusRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeWithEnabledItemTest<CommunicationStatusRegistryDataType>
	{
		public void TestDeserialised_WithSystemDefined()
		{
			var defaultStatuses = new CommunicationStatusCollection(false, false);
			defaultStatuses.AddSystemDefined("AAA", (NoResString)"AAA Description", true, true);
			defaultStatuses.AddSystemDefined("BBB", (NoResString)"BBB System Description", false, true);
			defaultStatuses.AddSystemDefined("XXX", (NoResString)"XXX System Description", true, true);
			defaultStatuses.Add("ZZZ", (NoResString)"ZZZ Description", true, true);

			var dataType = new CommunicationStatusRegistryDataType(defaultStatuses);
			var deserializedData = dataType.Deserialise(GetValidSamples()[0].BinaryValue);

			AssertContainsExactElementsInAnyOrder(
				new[] { "AAA", "BBB", "XXX" },
				deserializedData.Cast<CommunicationStatus>().Select(item => item.Code.ToString()));

			CombineAssertions(() =>
			{
				var aaaItem = deserializedData.Cast<CommunicationStatus>().First(item => item.Code == "AAA");
				AssertEquals("Description", "AAA Description", aaaItem.Description);
				AssertEquals("Closed", false, aaaItem.Closed);
				AssertEquals("Bool", true, aaaItem.Bool);
				AssertEquals("SystemDefined", true, aaaItem.SystemDefined);
			});

			CombineAssertions(() =>
			{
				var bbbItem = deserializedData.Cast<CommunicationStatus>().First(item => item.Code == "BBB");
				AssertEquals("Description", "BBB Description", bbbItem.Description);
				AssertEquals("Closed", true, bbbItem.Closed);
				AssertEquals("Bool", false, bbbItem.Bool);
				AssertEquals("SystemDefined", false, bbbItem.SystemDefined);
			});

			CombineAssertions(() =>
			{
				var xxxItem = deserializedData.Cast<CommunicationStatus>().First(item => item.Code == "XXX");
				AssertEquals("Description", "XXX System Description", xxxItem.Description);
				AssertEquals("Closed", true, xxxItem.Closed);
				AssertEquals("Bool", true, xxxItem.Bool);
				AssertEquals("SystemDefined", true, xxxItem.SystemDefined);
			});
		}

		#region Implementation

		protected override CommunicationStatusRegistryDataType GetNewDataType()
		{
			return new CommunicationStatusRegistryDataType(new CommunicationStatusCollection());
		}

		protected override string ExpectedEditorName
		{
			get { return null; }
		}

		protected override bool HasEditor
		{
			get { return false; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new CommunicationStatusCollection();

			var status1 = collection.AddNew();
			status1.Code = "AAA";
			status1.Description = (NoResString)"AAA Description";
			status1.Closed = false;
			status1.Bool = true;

			var status2 = collection.AddNew();
			status2.Code = "BBB";
			status2.Description = (NoResString)"BBB Description";
			status2.Closed = true;
			status2.Bool = false;

			byte[] byteArrayValue = Encoding.Unicode.GetBytes("<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfCommunicationStatus xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><CommunicationStatus><CodeMaxLength>3</CodeMaxLength><Code>AAA</Code><Description>AAA Description</Description><Bool>Y</Bool><Closed>N</Closed></CommunicationStatus><CommunicationStatus><CodeMaxLength>3</CodeMaxLength><Code>BBB</Code><Description>BBB Description</Description><Bool>N</Bool><Closed>Y</Closed></CommunicationStatus></ArrayOfCommunicationStatus>");

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		protected override IRegistryItem GetRegistryItem() => OrganisationsDataRegistry.Instance.CommunicationStatusList;

		protected override RegistryBusinessObjectCollection[] RegistriesWithEnabledItem()
		{
			var defaultValues = new CommunicationStatusCollection();
			defaultValues.Add("NEW", (NoResString)"New", closed: true, booleanValue: true);

			return [defaultValues];
		}

		protected override RegistryBusinessObjectCollection[] RegistriesWithoutEnabledItem()
		{
			var defaultValues1 = new CommunicationStatusCollection();

			var defaultValues2 = new CommunicationStatusCollection();
			defaultValues2.Add("NEW", (NoResString)"New", closed: true, booleanValue: false);

			return [defaultValues1, defaultValues2];
		}

		#endregion
	}
}
