using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Moq;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(ServiceManagerHelper))]

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(VerboseLoggingRegistryDataType))]
	public class VerboseLoggingRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<VerboseLoggingRegistryDataType>
	{
		public void TestSerialise()
		{
			// Arrange
			var codeDescriptionDateTimeRegistryDataType = new VerboseLoggingRegistryDataType();
			var collection = NewDateTimeCollection();

			// Act
			// Assert
			AssertNoExceptionThrown(() => codeDescriptionDateTimeRegistryDataType.Serialise(collection));
		}

		public void TestDeserialise()
		{
			// Arrange
			var data = GetSerialisedValue();
			var codeDescriptionDateTimeRegistryDataType = new VerboseLoggingRegistryDataType();

			// Act
			var result = codeDescriptionDateTimeRegistryDataType.Deserialise(data);

			// Assert
			AssertContainsExactElementsInExactOrder(
				NewDateTimeCollection()
					.Cast<VerboseLoggingBusinessObject>()
					.Select(time => (time.Code, time.Description, time.Value)),
				result
					.Cast<VerboseLoggingBusinessObject>()
					.Select(time => (time.Code, time.Description, time.Value)));

			byte[] GetSerialisedValue()
			{
				var dataType = new VerboseLoggingRegistryDataType();
				var collection = NewDateTimeCollection();
				return dataType.Serialise(collection);
			}
		}

		public void TestDefaultValue()
		{
			// Arrange
			var registryDataType = new VerboseLoggingRegistryDataType();

			// Act
			var result = registryDataType
				.DefaultValue
				.Cast<VerboseLoggingBusinessObject>()
				.Select(codeDescriptionDateTime => (
					codeDescriptionDateTime.Code,
					codeDescriptionDateTime.Description.GetUnresolvedString(),
					codeDescriptionDateTime.Value,
					codeDescriptionDateTime.SystemDefined));

			// Assert
			AssertContainsExactElementsInExactOrder(new (ZString code, string description, ZDateTime value, bool systemDefined)[]
				{
					(ServiceManagerHelper.HostLoggerCode, "System Logging", ZDateTime.Empty, true),
				},
				result);
		}

		public class ValidationTest : TestCase
		{
			public void TestDuplicatesAreNotAllowed()
			{
				Test(
					new VerboseLoggingCollection
					{
						new VerboseLoggingBusinessObject { Code = "TS1", EnglishDescription = "Description 1", Value = ZDateTime.Now.AddHours(1) },
						new VerboseLoggingBusinessObject { Code = "TS1", EnglishDescription = "Description 2", Value = ZDateTime.Now.AddHours(2) },
					},
					"There should not be duplicated task codes. Duplicates: TS1.");
				Test(
					new VerboseLoggingCollection
					{
						new VerboseLoggingBusinessObject { Code = "TS1", EnglishDescription = "Description 1", Value = ZDateTime.Now.AddHours(1) },
						new VerboseLoggingBusinessObject { Code = "TS1", EnglishDescription = "Description 2", Value = ZDateTime.Now.AddHours(2) },
						new VerboseLoggingBusinessObject { Code = "TS1", EnglishDescription = "Description 3", Value = ZDateTime.Now.AddHours(3) },
					},
					"There should not be duplicated task codes. Duplicates: TS1.");
				Test(
					new VerboseLoggingCollection
					{
						new VerboseLoggingBusinessObject { Code = "TS1", EnglishDescription = "Description 11", Value = ZDateTime.Now.AddHours(1) },
						new VerboseLoggingBusinessObject { Code = "TS2", EnglishDescription = "Description 21", Value = ZDateTime.Now.AddHours(2) },
						new VerboseLoggingBusinessObject { Code = "TS1", EnglishDescription = "Description 12", Value = ZDateTime.Now.AddHours(3) },
						new VerboseLoggingBusinessObject { Code = "TS2", EnglishDescription = "Description 22", Value = ZDateTime.Now.AddHours(4) },
					},
					"There should not be duplicated task codes. Duplicates: TS1, TS2.");
				Test(
					new VerboseLoggingCollection
					{
						new VerboseLoggingBusinessObject { Code = "TS2", EnglishDescription = "Description 21", Value = ZDateTime.Now.AddHours(2) },
						new VerboseLoggingBusinessObject { Code = "TS2", EnglishDescription = "Description 22", Value = ZDateTime.Now.AddHours(4) },
						new VerboseLoggingBusinessObject { Code = "TS1", EnglishDescription = "Description 11", Value = ZDateTime.Now.AddHours(1) },
						new VerboseLoggingBusinessObject { Code = "TS1", EnglishDescription = "Description 12", Value = ZDateTime.Now.AddHours(3) },
					},
					"There should not be duplicated task codes. Duplicates: TS1, TS2.");

				void Test(VerboseLoggingCollection proposedValue, string errorMessage)
				{
					// Arrange
					var dataType = new VerboseLoggingRegistryDataType();
					var registryItemMock = new Mock<IRegistryItem>();

					// Act
					// Assert
					var result = AssertExceptionThrown<RegistryValidationException>(
						() => dataType.Validate(registryItemMock.Object, proposedValue, Guid.Empty, Guid.Empty, Guid.Empty));
					AssertEquals(errorMessage, result.Message);
				}
			}
		}

		static VerboseLoggingCollection NewDateTimeCollection()
		{
			var codeDescriptionDateTimeCollection = new VerboseLoggingCollection(
				new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty),
				new Pairs(),
				ZDateTime.Empty);
			AddPair("AAA", "A Description", new ZDateTime(2006, 12, 26));
			AddPair("BBB", "B Description", new ZDateTime(2019, 12, 23));
			AddPair("CCC", "C Description", new ZDateTime(2022, 7, 21));
			return codeDescriptionDateTimeCollection;

			void AddPair(string code, string description, ZDateTime dt)
			{
				var record = codeDescriptionDateTimeCollection.AddNew();
				record.Code = code;
				record.Description = (NoResString)description;
				record.Value = dt;
			}
		}

		class Pairs : CodeDescriptionPairList
		{
			public Pairs()
			{
				AddPair("AAA", "AAA Description");
				AddPair("BBB", "BBB Description");
				AddPair("CCC", "CCC Description");
			}
		}

		protected override string ExpectedEditorName => "VerboseLoggingRegistryItemEditor";

		protected override VerboseLoggingRegistryDataType GetNewDataType()
		{
			return new VerboseLoggingRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var dataType = new VerboseLoggingRegistryDataType();
			var zDateTimeUtcNow = ZDateTime.UtcNow;
			var zDateTimeWithoutMilliseconds = zDateTimeUtcNow.AddMilliseconds(-zDateTimeUtcNow.Millisecond);
			var collection1 = new VerboseLoggingCollection
			{
				new VerboseLoggingBusinessObject { Code = "HOST", EnglishDescription = "Description 1", Value = zDateTimeWithoutMilliseconds.AddHours(1), SystemDefined = true, },
				new VerboseLoggingBusinessObject { Code = "UPG", EnglishDescription = "Description 2", Value = zDateTimeWithoutMilliseconds.AddHours(2) },
				new VerboseLoggingBusinessObject { Code = "LWK", EnglishDescription = "Description 3", Value = zDateTimeWithoutMilliseconds.AddHours(3) },
			};
			var collection2 = new VerboseLoggingCollection
			{
				new VerboseLoggingBusinessObject { Code = "HOST", EnglishDescription = "Description 1", Value = ZDateTime.Empty, SystemDefined = true, },
				new VerboseLoggingBusinessObject { Code = "FBK", EnglishDescription = "Description 2", Value = zDateTimeWithoutMilliseconds.AddHours(-2) },
				new VerboseLoggingBusinessObject { Code = "IMS", EnglishDescription = "Description 3", Value = zDateTimeWithoutMilliseconds.AddHours(-3) },
			};

			return new[] { new ValidSampleAndBinaryValueInDB(collection1, dataType.Serialise(collection1)), new ValidSampleAndBinaryValueInDB(collection2, dataType.Serialise(collection2)) };
		}
	}
}
