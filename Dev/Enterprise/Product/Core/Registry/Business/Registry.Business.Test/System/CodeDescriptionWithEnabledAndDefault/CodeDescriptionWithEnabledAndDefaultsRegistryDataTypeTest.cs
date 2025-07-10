using System;
using System.Linq;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionWithEnabledAndDefaultsRegistryDataType))]
	sealed class CodeDescriptionWithEnabledAndDefaultsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CodeDescriptionWithEnabledAndDefaultsRegistryDataType>
	{
		public void TestDeserialised_WithSystemDefined()
		{
			var defaultCollection = new CodeDescriptionWithEnabledAndDefaultCollection(256);
			defaultCollection.AddNewSystemDefined("tel", (NoResString)"Lync", false, true);
			defaultCollection.AddNewSystemDefined("callto", (NoResString)"Skype", true, true);
			var dataType = new CodeDescriptionWithEnabledAndDefaultsRegistryDataType(defaultCollection, true);

			var overridenCollection = new CodeDescriptionWithEnabledAndDefaultCollection(200);
			overridenCollection.AddNew("irc", (NoResString)"IRC", true, true);
			overridenCollection.AddNew("tel", (NoResString)"Google Chrome", false, false);
			var deserialisedOverridenCollection = dataType.Deserialise(dataType.Serialise(overridenCollection));

			AssertEquals(256, deserialisedOverridenCollection.CodeMaxLength);

			AssertContainsExactElementsInAnyOrder(
				new[] { "irc", "tel", "callto" },
				deserialisedOverridenCollection.Cast<CodeDescriptionWithEnabledAndDefault>().Select(protocol => protocol.Code.ToString()));

			var ircProtocol = deserialisedOverridenCollection.Cast<CodeDescriptionWithEnabledAndDefault>().First(item => item.Code == "irc");
			CombineAssertions("Should include the 'irc' protocol in the deserialised value", () =>
			{
				AssertEquals("Code", "irc", ircProtocol.Code);
				AssertEquals("Description", "IRC", ircProtocol.Description);
				AssertEquals("IsDefault", true, ircProtocol.IsDefault);
				AssertEquals("IsEnabled", true, ircProtocol.IsEnabled);
				AssertEquals("IsSystemDefined", false, ircProtocol.IsSystemDefined);
			});

			var telProtocol = deserialisedOverridenCollection.Cast<CodeDescriptionWithEnabledAndDefault>().First(item => item.Code == "tel");
			CombineAssertions("Should use the 'tel' protocol in the overriden value", () =>
			{
				AssertEquals("Code", "tel", telProtocol.Code);
				AssertEquals("Description", "Google Chrome", telProtocol.Description);
				AssertEquals("IsDefault", false, telProtocol.IsDefault);
				AssertEquals("IsEnabled", false, telProtocol.IsEnabled);
				AssertEquals("IsSystemDefined", true, telProtocol.IsSystemDefined);
			});

			var calltoProtocol = deserialisedOverridenCollection.Cast<CodeDescriptionWithEnabledAndDefault>().First(item => item.Code == "callto");
			CombineAssertions("Should re-add the 'callto' protocol from the default value", () =>
			{
				AssertEquals("Code", "callto", calltoProtocol.Code);
				AssertEquals("Description", "Skype", calltoProtocol.Description);
				AssertEquals("IsDefault should be false since IRC is the default", false, calltoProtocol.IsDefault);
				AssertEquals("IsEnabled", true, calltoProtocol.IsEnabled);
				AssertEquals("IsSystemDefined", true, calltoProtocol.IsSystemDefined);
			});

			// Test Default protocols remain unchanged

			var defaultTelProtocol = dataType.DefaultValue.Cast<CodeDescriptionWithEnabledAndDefault>().First(item => item.Code == "tel");
			CombineAssertions("Default 'tel' protocol should be unchanged", () =>
			{
				AssertEquals("Code", "tel", defaultTelProtocol.Code);
				AssertEquals("Description", "Lync", defaultTelProtocol.Description);
				AssertEquals("IsDefault", false, defaultTelProtocol.IsDefault);
				AssertEquals("IsEnabled", true, defaultTelProtocol.IsEnabled);
				AssertEquals("IsSystemDefined", true, defaultTelProtocol.IsSystemDefined);
			});

			var defaultCalltoProtocol = dataType.DefaultValue.Cast<CodeDescriptionWithEnabledAndDefault>().First(item => item.Code == "callto");
			CombineAssertions("Default 'callto' protocol should be unchanged", () =>
			{
				AssertEquals("Code", "callto", defaultCalltoProtocol.Code);
				AssertEquals("Description", "Skype", defaultCalltoProtocol.Description);
				AssertEquals("IsDefault", true, defaultCalltoProtocol.IsDefault);
				AssertEquals("IsEnabled", true, defaultCalltoProtocol.IsEnabled);
				AssertEquals("IsSystemDefined", true, defaultCalltoProtocol.IsSystemDefined);
			});
		}

		public void TestValidate_NoDefault()
		{
			var collection = new CodeDescriptionWithEnabledAndDefaultCollection();
			collection.AddNew("AAA", (NoResString)"AAA", false, true);
			collection.AddNew("BBB", (NoResString)"BBB", false, true);

			var dataTypeWithDefaultMandatory = new CodeDescriptionWithEnabledAndDefaultsRegistryDataType(new CodeDescriptionWithEnabledAndDefaultCollection(), true);
			AssertExceptionThrown(typeof(RegistryValidationException), "Please select a default.",
					() => { dataTypeWithDefaultMandatory.Validate(null, collection, Guid.Empty, Guid.Empty, Guid.Empty); }
				);

			var dataTypeWithDefaultOptional = new CodeDescriptionWithEnabledAndDefaultsRegistryDataType(new CodeDescriptionWithEnabledAndDefaultCollection(), false);
			AssertNoExceptionThrown(
					() => { dataTypeWithDefaultOptional.Validate(null, collection, Guid.Empty, Guid.Empty, Guid.Empty); }
				);
		}

		#region Implementation

		protected override string ExpectedEditorName
		{
			get { return "CodeDescriptionWithEnabledAndDefaultsRegistryItemEditor"; }
		}

		protected override CodeDescriptionWithEnabledAndDefaultsRegistryDataType GetNewDataType()
		{
			var collection = new CodeDescriptionWithEnabledAndDefaultCollection(256);
			collection.AddNew("tel", (NoResString)"Lync", true, true);
			collection.AddNew("callto", (NoResString)"Skype", false, false);
			return new CodeDescriptionWithEnabledAndDefaultsRegistryDataType(collection, true);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = new CodeDescriptionWithEnabledAndDefaultCollection(256);
			collection1.AddNew("tel", (NoResString)"Lync", true, true);

			var collection2 = new CodeDescriptionWithEnabledAndDefaultCollection(256);
			collection2.AddNew("tel", (NoResString)"Lync", true, true);
			collection2.AddNew("callto", (NoResString)"Skype", false, false);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, new byte[]
					{
						60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
						0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,87,0,105,0,116,0,104,
						0,69,0,110,0,97,0,98,0,108,0,101,0,100,0,65,0,110,0,100,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,
						0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,
						0,97,0,110,0,99,0,101,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,
						0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,62,0,60,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,
						0,110,0,87,0,105,0,116,0,104,0,69,0,110,0,97,0,98,0,108,0,101,0,100,0,65,0,110,0,100,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,62,0,60,0,67,0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,
						0,110,0,103,0,116,0,104,0,62,0,50,0,53,0,54,0,60,0,47,0,67,0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,116,0,101,0,108,
						0,60,0,47,0,67,0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,76,0,121,0,110,0,99,0,60,0,47,0,68,0,101,0,115,0,99,0,114,0,105,0,112,
						0,116,0,105,0,111,0,110,0,62,0,60,0,73,0,115,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,62,0,89,0,60,0,47,0,73,0,115,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,62,0,60,0,73,0,115,0,69,
						0,110,0,97,0,98,0,108,0,101,0,100,0,62,0,89,0,60,0,47,0,73,0,115,0,69,0,110,0,97,0,98,0,108,0,101,0,100,0,62,0,60,0,47,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,
						0,116,0,105,0,111,0,110,0,87,0,105,0,116,0,104,0,69,0,110,0,97,0,98,0,108,0,101,0,100,0,65,0,110,0,100,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,
						0,79,0,102,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,87,0,105,0,116,0,104,0,69,0,110,0,97,0,98,0,108,0,101,0,100,0,65,0,110,0,100,0,68,0,101,
						0,102,0,97,0,117,0,108,0,116,0,62,0
					}),

				new ValidSampleAndBinaryValueInDB(collection2, new byte[]
					{
						60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
						0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,87,0,105,0,116,0,104,
						0,69,0,110,0,97,0,98,0,108,0,101,0,100,0,65,0,110,0,100,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,
						0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,
						0,97,0,110,0,99,0,101,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,
						0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,62,0,60,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,
						0,110,0,87,0,105,0,116,0,104,0,69,0,110,0,97,0,98,0,108,0,101,0,100,0,65,0,110,0,100,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,62,0,60,0,67,0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,
						0,110,0,103,0,116,0,104,0,62,0,50,0,53,0,54,0,60,0,47,0,67,0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,116,0,101,0,108,
						0,60,0,47,0,67,0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,76,0,121,0,110,0,99,0,60,0,47,0,68,0,101,0,115,0,99,0,114,0,105,0,112,
						0,116,0,105,0,111,0,110,0,62,0,60,0,73,0,115,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,62,0,89,0,60,0,47,0,73,0,115,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,62,0,60,0,73,0,115,0,69,
						0,110,0,97,0,98,0,108,0,101,0,100,0,62,0,89,0,60,0,47,0,73,0,115,0,69,0,110,0,97,0,98,0,108,0,101,0,100,0,62,0,60,0,47,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,
						0,116,0,105,0,111,0,110,0,87,0,105,0,116,0,104,0,69,0,110,0,97,0,98,0,108,0,101,0,100,0,65,0,110,0,100,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,62,0,60,0,67,0,111,0,100,0,101,0,68,0,101,
						0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,87,0,105,0,116,0,104,0,69,0,110,0,97,0,98,0,108,0,101,0,100,0,65,0,110,0,100,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,62,0,60,0,67,
						0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,50,0,53,0,54,0,60,0,47,0,67,0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,60,
						0,67,0,111,0,100,0,101,0,62,0,99,0,97,0,108,0,108,0,116,0,111,0,60,0,47,0,67,0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,83,0,107,
						0,121,0,112,0,101,0,60,0,47,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,73,0,115,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,62,0,78,0,60,0,47,0,73,0,115,
						0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,62,0,60,0,73,0,115,0,69,0,110,0,97,0,98,0,108,0,101,0,100,0,62,0,78,0,60,0,47,0,73,0,115,0,69,0,110,0,97,0,98,0,108,0,101,0,100,0,62,0,60,
						0,47,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,87,0,105,0,116,0,104,0,69,0,110,0,97,0,98,0,108,0,101,0,100,0,65,0,110,0,100,0,68,0,101,0,102,
						0,97,0,117,0,108,0,116,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,87,0,105,0,116,0,104,
						0,69,0,110,0,97,0,98,0,108,0,101,0,100,0,65,0,110,0,100,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,62,0
					})
			};
		}

		#endregion
	}
}
