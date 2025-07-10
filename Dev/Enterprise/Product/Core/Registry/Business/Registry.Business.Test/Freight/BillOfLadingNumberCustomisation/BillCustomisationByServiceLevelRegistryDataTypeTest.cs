using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BillCustomisationByServiceLevelRegistryDataType))]
	sealed class BillCustomisationByServiceLevelRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<BillCustomisationByServiceLevelRegistryDataType>
	{
		public void TestValidateCustomisations()
		{
			var registryItem = new BillCustomisationByServiceLevelRegistryItem(
						"HouseBillNumberCustomisation",
						FreightDataRegistry.Categories.Freight_HouseBills_NumberCustomizations,
						 (NoResString)"House Bill Number",
						 (NoResString)"Override this value to customize how House Bills are formatted for ALL Transport Modes.",
						RegistryStorageFlags.All,
						RegistryOptions.PreserveTestValue,
						DataType,
						null
						);

			var customisations = new BillOfLadingNumberCustomisationsByServiceLevel();
			customisations.BillOfLadingNumberCustomisations.RemoveAndDeleteAll();

			AssertExceptionThrown("Exception should be thrown when no service levels exist", typeof(RegistryValidationException), () =>
			{
				DataType.Validate(registryItem, customisations, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
			});

			var customisation = customisations.BillOfLadingNumberCustomisations.AddNew();
			customisation.ServiceLevel = "D2D";

			AssertExceptionThrown("Exception should be thrown when 'ALL' service level does not exist", typeof(RegistryValidationException), () =>
			{
				DataType.Validate(registryItem, customisations, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
			});
		}

		public void TestDefaultCategory()
		{
			AssertEquals(NumberCustomisationElementCategories.Default, DataType.DefaultValue.Categories);
			AssertCustomisationEquals(DataType.DefaultValue, NumberCustomisationElementCategories.Default, (c) => c.Categories);

			const NumberCustomisationElementCategories newValue = NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.LinerAgency;
			DataType.Categories = newValue;
			AssertEquals(newValue, DataType.DefaultValue.Categories);
			AssertCustomisationEquals(DataType.DefaultValue, newValue, (c) => c.Categories);
		}

		public void TestDefaultAllowNonAlphanumericCharacters()
		{
			AssertEquals(false, DataType.DefaultValue.AllowNonAlphanumericCharacters);
			AssertCustomisationEquals(DataType.DefaultValue, false, (c) => c.AllowNonAlphanumericCharacters);

			DataType.AllowNonAlphanumericCharacters = true;
			AssertEquals(true, DataType.DefaultValue.AllowNonAlphanumericCharacters);
			AssertCustomisationEquals(DataType.DefaultValue, true, (c) => c.AllowNonAlphanumericCharacters);
		}

		public void TestDefaultEnableMacroInsertion()
		{
			AssertEquals(false, DataType.DefaultValue.EnableMacroInsertion);
			AssertCustomisationEquals(DataType.DefaultValue, false, (c) => c.EnableMacroInsertion);

			DataType.EnableMacroInsertion = true;
			AssertEquals(true, DataType.DefaultValue.EnableMacroInsertion);
			AssertCustomisationEquals(DataType.DefaultValue, true, (c) => c.EnableMacroInsertion);
		}

		public void TestMacroType()
		{
			AssertNull("Macro Type should be null for default.", DataType.MacroType);
		}

		public void TestDeserialiseCategory()
		{
			ValidSampleAndBinaryValueInDB sample = GetValidSamples()[0];

			AssertEquals(NumberCustomisationElementCategories.Default, DataType.Deserialise(sample.BinaryValue).Categories);
			AssertCustomisationEquals(DataType.DefaultValue, NumberCustomisationElementCategories.Default, (c) => c.Categories);

			const NumberCustomisationElementCategories newValue = NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.LinerAgency;
			DataType.Categories = newValue;
			AssertEquals(newValue, DataType.Deserialise(sample.BinaryValue).Categories);
			AssertCustomisationEquals(DataType.DefaultValue, newValue, (c) => c.Categories);
		}

		public void TestDeserialiseAllowNonAlphanumericCharactersAndMaxAllowedLength()
		{
			ValidSampleAndBinaryValueInDB sample = GetValidSamples()[0];
			DataType.MaxLength = 6;
			AssertEquals(false, DataType.Deserialise(sample.BinaryValue).AllowNonAlphanumericCharacters);
			AssertEquals(6, DataType.Deserialise(sample.BinaryValue).MaxAllowedLength);
			AssertCustomisationEquals(DataType.Deserialise(sample.BinaryValue), false, (c) => c.AllowNonAlphanumericCharacters);

			DataType.AllowNonAlphanumericCharacters = true;
			AssertEquals(true, DataType.Deserialise(sample.BinaryValue).AllowNonAlphanumericCharacters);
			AssertCustomisationEquals(DataType.Deserialise(sample.BinaryValue), true, (c) => c.AllowNonAlphanumericCharacters);
		}

		public void TestPrefixLength()
		{
			var dataType = new BillCustomisationByServiceLevelRegistryDataType();
			AssertEquals("Precondition", 1, dataType.DefaultValue.PrefixLength);

			dataType.FountainPrefix = "ABC";
			AssertEquals("Prefix Length should match the Fountain Prefix's Lentgh.", 3, dataType.DefaultValue.PrefixLength);
		}

		public void TestIBillCustomisationRegistryDataType()
		{
			var dataType = new BillCustomisationByServiceLevelRegistryDataType
			{
				FountainPrefix = "TEST1",
				GeneratedNumberName = (NoResString)"TEST2",
				SequenceNumberName = (NoResString)"TEST3",
				MaxLength = 42,
				Categories = NumberCustomisationElementCategories.WarehouseJob,
				AllowNonAlphanumericCharacters = true,
				EnableMacroInsertion = true,
				MacroType = typeof(string),
			};

			CombineAssertions(() =>
			{
				var dataTypeInterface = dataType as IBillCustomisationRegistryDataType;
				AssertNotNull("Should implement IBillCustomisationRegistryDataType.", dataTypeInterface);

				AssertEquals(nameof(dataTypeInterface.FountainPrefix), dataType.FountainPrefix, dataTypeInterface.FountainPrefix);
				AssertEquals(nameof(dataTypeInterface.GeneratedNumberName), dataType.GeneratedNumberName, dataTypeInterface.GeneratedNumberName);
				AssertEquals(nameof(dataTypeInterface.SequenceNumberName), dataType.SequenceNumberName, dataTypeInterface.SequenceNumberName);
				AssertEquals(nameof(dataTypeInterface.MaxLength), dataType.MaxLength, dataTypeInterface.MaxLength);
				AssertEquals(nameof(dataTypeInterface.Categories), dataType.Categories, dataTypeInterface.Categories);
				AssertEquals(nameof(dataTypeInterface.AllowNonAlphanumericCharacters), dataType.AllowNonAlphanumericCharacters, dataTypeInterface.AllowNonAlphanumericCharacters);
				AssertEquals(nameof(dataTypeInterface.EnableMacroInsertion), dataType.EnableMacroInsertion, dataTypeInterface.EnableMacroInsertion);
				AssertEquals(nameof(dataTypeInterface.MacroType), dataType.MacroType, dataTypeInterface.MacroType);
			});
		}

		#region Implementation

		void AssertCustomisationEquals<T>(BillOfLadingNumberCustomisationsByServiceLevel billCustomisations, T expected, Converter<BillOfLadingNumberCustomisation, T> getter)
		{
			foreach (BillOfLadingNumberCustomisation customisation in billCustomisations.BillOfLadingNumberCustomisations)
			{
				AssertEquals(expected, getter(customisation));
			}
		}

		protected override BillCustomisationByServiceLevelRegistryDataType GetNewDataType()
		{
			return new BillCustomisationByServiceLevelRegistryDataType(new BillOfLadingNumberCustomisationsByServiceLevel());
		}

		protected override string ExpectedEditorName
		{
			get { return "BillOfLadingNumberCustomisationByServiceLevelRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			BillOfLadingNumberCustomisationsByServiceLevel customisations = new BillOfLadingNumberCustomisationsByServiceLevel();

			foreach (BillOfLadingNumberCustomisation customisation in customisations.BillOfLadingNumberCustomisations)
			{
				customisation.RemoveFountainPrefix = true;
				customisation.UseShipmentSequenceNumber = false;
				customisation.CheckDigitAlgorithm = CheckDigitAlgorithmList.Codes.None;

				customisation.UnFilteredElements.Sort(BillOfLadingNumberCustomisationElement.Schema.Key);
				for (int i = 0; i < customisation.UnFilteredElements.Count; i++)
				{
					BillOfLadingNumberCustomisationElement element = customisation.UnFilteredElements[i];
					element.Include = true;
					element.Order = (byte)i;

					switch (element.Key)
					{
						case BillOfLadingNumberCustomisationElement.Keys.ClientCoded1:
							element.Detail = new string('A', element.DetailInfo.MaxLength);
							break;

						case BillOfLadingNumberCustomisationElement.Keys.SequenceNumber:
							element.Detail = "8";
							break;

						case BillOfLadingNumberCustomisationElement.Keys.YearAsDigit:
							element.Detail = "1";
							break;

						default:
							element.Detail = "";
							break;
					}
				}
			}

			byte[] byteArrayValue = new byte[]
						{
				255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,
				0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,66,0,105,0,108,0,108,0,79,0,102,0,76,0,97,0,100,0,105,0,110,0,103,0,78,0,117,0,109,0,98,0,101,0,114,0,67,0,117,0,115,0,116,0,111,0,109,0,105,
				0,115,0,97,0,116,0,105,0,111,0,110,0,115,0,66,0,121,0,83,0,101,0,114,0,118,0,105,0,99,0,101,0,76,0,101,0,118,0,101,0,108,0,62,0,60,0,83,0,101,0,114,0,118,0,105,0,99,0,101,0,76,0,101,0,118,
				0,101,0,108,0,67,0,117,0,115,0,116,0,111,0,109,0,105,0,115,0,97,0,116,0,105,0,111,0,110,0,115,0,62,0,60,0,67,0,117,0,115,0,116,0,111,0,109,0,105,0,115,0,97,0,116,0,105,0,111,0,110,0,66,0,121,
				0,83,0,101,0,114,0,118,0,105,0,99,0,101,0,76,0,101,0,118,0,101,0,108,0,62,0,60,0,83,0,101,0,114,0,118,0,105,0,99,0,101,0,76,0,101,0,118,0,101,0,108,0,62,0,65,0,76,0,76,0,60,0,47,0,83,
				0,101,0,114,0,118,0,105,0,99,0,101,0,76,0,101,0,118,0,101,0,108,0,62,0,60,0,82,0,101,0,109,0,111,0,118,0,101,0,70,0,111,0,117,0,110,0,116,0,97,0,105,0,110,0,80,0,114,0,101,0,102,0,105,0,120,
				0,62,0,89,0,60,0,47,0,82,0,101,0,109,0,111,0,118,0,101,0,70,0,111,0,117,0,110,0,116,0,97,0,105,0,110,0,80,0,114,0,101,0,102,0,105,0,120,0,62,0,60,0,67,0,104,0,101,0,99,0,107,0,68,0,105,
				0,103,0,105,0,116,0,65,0,108,0,103,0,111,0,114,0,105,0,116,0,104,0,109,0,62,0,78,0,79,0,78,0,60,0,47,0,67,0,104,0,101,0,99,0,107,0,68,0,105,0,103,0,105,0,116,0,65,0,108,0,103,0,111,0,114,
				0,105,0,116,0,104,0,109,0,62,0,60,0,85,0,115,0,101,0,83,0,104,0,105,0,112,0,109,0,101,0,110,0,116,0,83,0,101,0,113,0,117,0,101,0,110,0,99,0,101,0,78,0,117,0,109,0,98,0,101,0,114,0,62,0,78,
				0,60,0,47,0,85,0,115,0,101,0,83,0,104,0,105,0,112,0,109,0,101,0,110,0,116,0,83,0,101,0,113,0,117,0,101,0,110,0,99,0,101,0,78,0,117,0,109,0,98,0,101,0,114,0,62,0,60,0,69,0,108,0,101,0,109,
				0,101,0,110,0,116,0,115,0,62,0,60,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,32,0,107,0,101,0,121,0,61,0,34,0,66,0,114,0,97,0,110,0,99,0,104,0,67,0,111,0,100,0,101,0,34,0,62,0,60,0,79,
				0,114,0,100,0,101,0,114,0,62,0,48,0,60,0,47,0,79,0,114,0,100,0,101,0,114,0,62,0,60,0,47,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,62,0,60,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,32,
				0,107,0,101,0,121,0,61,0,34,0,67,0,97,0,114,0,114,0,105,0,101,0,114,0,80,0,114,0,105,0,110,0,99,0,105,0,112,0,97,0,108,0,67,0,111,0,100,0,101,0,34,0,62,0,60,0,79,0,114,0,100,0,101,0,114,
				0,62,0,49,0,60,0,47,0,79,0,114,0,100,0,101,0,114,0,62,0,60,0,47,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,62,0,60,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,32,0,107,0,101,0,121,0,61,
				0,34,0,67,0,108,0,105,0,101,0,110,0,116,0,67,0,111,0,100,0,101,0,100,0,34,0,62,0,60,0,79,0,114,0,100,0,101,0,114,0,62,0,50,0,60,0,47,0,79,0,114,0,100,0,101,0,114,0,62,0,60,0,68,0,101,
				0,116,0,97,0,105,0,108,0,62,0,65,0,65,0,65,0,65,0,65,0,60,0,47,0,68,0,101,0,116,0,97,0,105,0,108,0,62,0,60,0,47,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,62,0,60,0,69,0,108,0,101,
				0,109,0,101,0,110,0,116,0,32,0,107,0,101,0,121,0,61,0,34,0,67,0,111,0,109,0,112,0,97,0,110,0,121,0,67,0,111,0,100,0,101,0,34,0,62,0,60,0,79,0,114,0,100,0,101,0,114,0,62,0,51,0,60,0,47,
				0,79,0,114,0,100,0,101,0,114,0,62,0,60,0,47,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,62,0,60,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,32,0,107,0,101,0,121,0,61,0,34,0,67,0,111,0,110,
				0,116,0,97,0,105,0,110,0,101,0,114,0,84,0,114,0,97,0,110,0,115,0,104,0,105,0,112,0,109,0,101,0,110,0,116,0,73,0,110,0,100,0,105,0,99,0,97,0,116,0,111,0,114,0,34,0,62,0,60,0,79,0,114,0,100,
				0,101,0,114,0,62,0,52,0,60,0,47,0,79,0,114,0,100,0,101,0,114,0,62,0,60,0,47,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,62,0,60,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,32,0,107,0,101,
				0,121,0,61,0,34,0,68,0,101,0,115,0,116,0,105,0,110,0,97,0,116,0,105,0,111,0,110,0,73,0,65,0,84,0,65,0,34,0,62,0,60,0,79,0,114,0,100,0,101,0,114,0,62,0,53,0,60,0,47,0,79,0,114,0,100,
				0,101,0,114,0,62,0,60,0,47,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,62,0,60,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,32,0,107,0,101,0,121,0,61,0,34,0,68,0,101,0,115,0,116,0,105,0,110,
				0,97,0,116,0,105,0,111,0,110,0,85,0,78,0,76,0,79,0,67,0,79,0,34,0,62,0,60,0,79,0,114,0,100,0,101,0,114,0,62,0,54,0,60,0,47,0,79,0,114,0,100,0,101,0,114,0,62,0,60,0,47,0,69,0,108,
				0,101,0,109,0,101,0,110,0,116,0,62,0,60,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,32,0,107,0,101,0,121,0,61,0,34,0,68,0,105,0,114,0,101,0,99,0,116,0,105,0,111,0,110,0,34,0,62,0,60,0,79,
				0,114,0,100,0,101,0,114,0,62,0,55,0,60,0,47,0,79,0,114,0,100,0,101,0,114,0,62,0,60,0,47,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,62,0,60,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,32,
				0,107,0,101,0,121,0,61,0,34,0,68,0,105,0,115,0,99,0,104,0,97,0,114,0,103,0,101,0,73,0,65,0,84,0,65,0,34,0,62,0,60,0,79,0,114,0,100,0,101,0,114,0,62,0,56,0,60,0,47,0,79,0,114,0,100,
				0,101,0,114,0,62,0,60,0,47,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,62,0,60,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,32,0,107,0,101,0,121,0,61,0,34,0,68,0,105,0,115,0,99,0,104,0,97,
				0,114,0,103,0,101,0,85,0,78,0,76,0,79,0,67,0,79,0,34,0,62,0,60,0,79,0,114,0,100,0,101,0,114,0,62,0,57,0,60,0,47,0,79,0,114,0,100,0,101,0,114,0,62,0,60,0,47,0,69,0,108,0,101,0,109,
				0,101,0,110,0,116,0,62,0,60,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,32,0,107,0,101,0,121,0,61,0,34,0,69,0,110,0,116,0,101,0,114,0,112,0,114,0,105,0,115,0,101,0,67,0,111,0,100,0,101,0,34,
				0,62,0,60,0,79,0,114,0,100,0,101,0,114,0,62,0,49,0,48,0,60,0,47,0,79,0,114,0,100,0,101,0,114,0,62,0,60,0,47,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,62,0,60,0,69,0,108,0,101,0,109,
				0,101,0,110,0,116,0,32,0,107,0,101,0,121,0,61,0,34,0,76,0,111,0,97,0,100,0,73,0,65,0,84,0,65,0,34,0,62,0,60,0,79,0,114,0,100,0,101,0,114,0,62,0,49,0,49,0,60,0,47,0,79,0,114,0,100,
				0,101,0,114,0,62,0,60,0,47,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,62,0,60,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,32,0,107,0,101,0,121,0,61,0,34,0,76,0,111,0,97,0,100,0,85,0,78,
				0,76,0,79,0,67,0,79,0,34,0,62,0,60,0,79,0,114,0,100,0,101,0,114,0,62,0,49,0,50,0,60,0,47,0,79,0,114,0,100,0,101,0,114,0,62,0,60,0,47,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,62,
				0,60,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,32,0,107,0,101,0,121,0,61,0,34,0,77,0,111,0,110,0,116,0,104,0,65,0,115,0,76,0,101,0,116,0,116,0,101,0,114,0,34,0,62,0,60,0,79,0,114,0,100,
				0,101,0,114,0,62,0,49,0,51,0,60,0,47,0,79,0,114,0,100,0,101,0,114,0,62,0,60,0,47,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,62,0,60,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,32,0,107,
				0,101,0,121,0,61,0,34,0,79,0,114,0,105,0,103,0,105,0,110,0,73,0,65,0,84,0,65,0,34,0,62,0,60,0,79,0,114,0,100,0,101,0,114,0,62,0,49,0,52,0,60,0,47,0,79,0,114,0,100,0,101,0,114,0,62,
				0,60,0,47,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,62,0,60,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,32,0,107,0,101,0,121,0,61,0,34,0,79,0,114,0,105,0,103,0,105,0,110,0,85,0,78,0,76,
				0,79,0,67,0,79,0,34,0,62,0,60,0,79,0,114,0,100,0,101,0,114,0,62,0,49,0,53,0,60,0,47,0,79,0,114,0,100,0,101,0,114,0,62,0,60,0,47,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,62,0,60,
				0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,32,0,107,0,101,0,121,0,61,0,34,0,83,0,101,0,113,0,117,0,101,0,110,0,99,0,101,0,78,0,117,0,109,0,98,0,101,0,114,0,34,0,62,0,60,0,79,0,114,0,100,
				0,101,0,114,0,62,0,49,0,54,0,60,0,47,0,79,0,114,0,100,0,101,0,114,0,62,0,60,0,68,0,101,0,116,0,97,0,105,0,108,0,62,0,56,0,60,0,47,0,68,0,101,0,116,0,97,0,105,0,108,0,62,0,60,0,47,
				0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,62,0,60,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,32,0,107,0,101,0,121,0,61,0,34,0,83,0,101,0,114,0,118,0,101,0,114,0,67,0,111,0,100,0,101,0,34,
				0,62,0,60,0,79,0,114,0,100,0,101,0,114,0,62,0,49,0,55,0,60,0,47,0,79,0,114,0,100,0,101,0,114,0,62,0,60,0,47,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,62,0,60,0,69,0,108,0,101,0,109,
				0,101,0,110,0,116,0,32,0,107,0,101,0,121,0,61,0,34,0,83,0,101,0,114,0,118,0,105,0,99,0,101,0,76,0,101,0,118,0,101,0,108,0,34,0,62,0,60,0,79,0,114,0,100,0,101,0,114,0,62,0,49,0,56,0,60,
				0,47,0,79,0,114,0,100,0,101,0,114,0,62,0,60,0,47,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,62,0,60,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,32,0,107,0,101,0,121,0,61,0,34,0,84,0,114,
				0,97,0,110,0,115,0,112,0,111,0,114,0,116,0,77,0,111,0,100,0,101,0,34,0,62,0,60,0,79,0,114,0,100,0,101,0,114,0,62,0,49,0,57,0,60,0,47,0,79,0,114,0,100,0,101,0,114,0,62,0,60,0,47,0,69,
				0,108,0,101,0,109,0,101,0,110,0,116,0,62,0,60,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,32,0,107,0,101,0,121,0,61,0,34,0,85,0,110,0,105,0,118,0,101,0,114,0,115,0,97,0,108,0,79,0,102,0,102,
				0,105,0,99,0,101,0,67,0,111,0,100,0,101,0,34,0,62,0,60,0,79,0,114,0,100,0,101,0,114,0,62,0,50,0,48,0,60,0,47,0,79,0,114,0,100,0,101,0,114,0,62,0,60,0,47,0,69,0,108,0,101,0,109,0,101,
				0,110,0,116,0,62,0,60,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,32,0,107,0,101,0,121,0,61,0,34,0,89,0,101,0,97,0,114,0,65,0,115,0,68,0,105,0,103,0,105,0,116,0,34,0,62,0,60,0,79,0,114,
				0,100,0,101,0,114,0,62,0,50,0,49,0,60,0,47,0,79,0,114,0,100,0,101,0,114,0,62,0,60,0,68,0,101,0,116,0,97,0,105,0,108,0,62,0,49,0,60,0,47,0,68,0,101,0,116,0,97,0,105,0,108,0,62,0,60,
				0,47,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,62,0,60,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,32,0,107,0,101,0,121,0,61,0,34,0,89,0,101,0,97,0,114,0,65,0,115,0,76,0,101,0,116,0,116,
				0,101,0,114,0,34,0,62,0,60,0,79,0,114,0,100,0,101,0,114,0,62,0,50,0,50,0,60,0,47,0,79,0,114,0,100,0,101,0,114,0,62,0,60,0,47,0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,62,0,60,0,47,
				0,69,0,108,0,101,0,109,0,101,0,110,0,116,0,115,0,62,0,60,0,47,0,67,0,117,0,115,0,116,0,111,0,109,0,105,0,115,0,97,0,116,0,105,0,111,0,110,0,66,0,121,0,83,0,101,0,114,0,118,0,105,0,99,0,101,
				0,76,0,101,0,118,0,101,0,108,0,62,0,60,0,47,0,83,0,101,0,114,0,118,0,105,0,99,0,101,0,76,0,101,0,118,0,101,0,108,0,67,0,117,0,115,0,116,0,111,0,109,0,105,0,115,0,97,0,116,0,105,0,111,0,110,
				0,115,0,62,0,60,0,47,0,66,0,105,0,108,0,108,0,79,0,102,0,76,0,97,0,100,0,105,0,110,0,103,0,78,0,117,0,109,0,98,0,101,0,114,0,67,0,117,0,115,0,116,0,111,0,109,0,105,0,115,0,97,0,116,0,105,
				0,111,0,110,0,115,0,66,0,121,0,83,0,101,0,114,0,118,0,105,0,99,0,101,0,76,0,101,0,118,0,101,0,108,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(customisations, byteArrayValue)
			};
		}

		#endregion
	}
}
