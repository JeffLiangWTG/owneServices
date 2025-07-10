using System;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Registry.Testing
{
	[TestedType(typeof(StatementNumberCustomisationRegistryDataType))]
	class StatementNumberCustomisationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<StatementNumberCustomisationRegistryDataType>
	{
		protected override string ExpectedEditorName => "BillOfLadingNumberCustomisationRegistryItemEditor";

		protected override StatementNumberCustomisationRegistryDataType GetNewDataType()
		{
			return new StatementNumberCustomisationRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var customisation = new StatementNumberCustomisation();
			foreach (BillOfLadingNumberCustomisationElement element in customisation.UnFilteredElements)
			{
				switch (element.Key)
				{
					case BillOfLadingNumberCustomisationElement.Keys.SequenceNumber:
						element.Detail = "4";
						break;
					case BillOfLadingNumberCustomisationElement.Keys.YearAsDigit:
						element.Detail = "1";
						break;
				}
			}

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(new StatementNumberCustomisation(), Array.Empty<byte>()),
				new ValidSampleAndBinaryValueInDB(customisation, DataType.Serialise(customisation))
			};
		}
	}

	[TestedType(typeof(StatementNumberCustomisation))]
	class StatementNumberCustomisationTest : RegistryBusinessObjectTemplateTestCase<StatementNumberCustomisation>
	{
		public void TestSequenceNumber()
		{
			var customisation = new StatementNumberCustomisation();
			var sequenceNumber = customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber];
			AssertEquals("8", sequenceNumber.Detail);
		}

		protected override StatementNumberCustomisation GetBusinessObjectToClone()
		{
			return new StatementNumberCustomisation();
		}

		protected override StatementNumberCustomisation GetBusinessObjectToSerialise()
		{
			return new StatementNumberCustomisation();
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;
	}
}
