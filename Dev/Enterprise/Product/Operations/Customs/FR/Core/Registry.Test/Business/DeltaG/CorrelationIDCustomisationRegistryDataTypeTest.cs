using System;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Registry.Testing
{
	[TestedType(typeof(CorrelationIDCustomisationRegistryDataType))]
	class CorrelationIDCustomisationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CorrelationIDCustomisationRegistryDataType>
	{
		protected override string ExpectedEditorName => "BillOfLadingNumberCustomisationRegistryItemEditor";

		protected override CorrelationIDCustomisationRegistryDataType GetNewDataType()
		{
			return new CorrelationIDCustomisationRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var customisation = new CorrelationIDCustomisation();
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
				new ValidSampleAndBinaryValueInDB(new CorrelationIDCustomisation(), Array.Empty<byte>()),
				new ValidSampleAndBinaryValueInDB(customisation, DataType.Serialise(customisation))
			};
		}
	}

	[TestedType(typeof(CorrelationIDCustomisation))]
	class CorrelationIDCustomisationTest : RegistryBusinessObjectTemplateTestCase<CorrelationIDCustomisation>
	{
		public static void Set(CorrelationIDCustomisation customisation, string key, byte order, bool fountain, string detail = null)
		{
			var element = customisation.UnFilteredElements[key];
			element.Include = true;
			element.Order = order;
			element.Fountain = fountain;
			element.Detail = detail;
		}

		#region Overrides of RegistryBusinessObjectTemplateTestCase<CDSEntryLocalReferenceNumberCustomisation>

		protected override CorrelationIDCustomisation GetBusinessObjectToClone()
		{
			return new CorrelationIDCustomisation();
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CorrelationIDCustomisation();
		}
		protected override CorrelationIDCustomisation GetBusinessObjectToSerialise()
		{
			return new CorrelationIDCustomisation();
		}
		#endregion
	}
}
