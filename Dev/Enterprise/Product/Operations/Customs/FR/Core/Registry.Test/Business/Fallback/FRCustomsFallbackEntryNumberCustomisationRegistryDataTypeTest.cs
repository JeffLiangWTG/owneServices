using System;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Registry.Testing
{
	[TestedType(typeof(FRCustomsFallbackEntryNumberCustomisationRegistryDataType))]
	class FRCustomsFallbackEntryNumberCustomisationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<FRCustomsFallbackEntryNumberCustomisationRegistryDataType>
	{
		protected override string ExpectedEditorName => "BillOfLadingNumberCustomisationRegistryItemEditor";

		protected override FRCustomsFallbackEntryNumberCustomisationRegistryDataType GetNewDataType()
		{
			return new FRCustomsFallbackEntryNumberCustomisationRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var customisation = new FRCustomsFallbackEntryNumberCustomisation();
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
				new ValidSampleAndBinaryValueInDB(new FRCustomsFallbackEntryNumberCustomisation(), Array.Empty<byte>()),
				new ValidSampleAndBinaryValueInDB(customisation, DataType.Serialise(customisation))
			};
		}
	}

	[TestedType(typeof(FRCustomsFallbackEntryNumberCustomisation))]
	class FRCustomsFallbackEntryNumberCustomisationTest : RegistryBusinessObjectTemplateTestCase<FRCustomsFallbackEntryNumberCustomisation>
	{
		public static void Set(FRCustomsFallbackEntryNumberCustomisation customisation, string key, byte order, bool fountain, string detail = null)
		{
			var element = customisation.UnFilteredElements[key];
			element.Include = true;
			element.Order = order;
			element.Fountain = fountain;
			element.Detail = detail;
		}

		#region Overrides of RegistryBusinessObjectTemplateTestCase<CDSEntryLocalReferenceNumberCustomisation>

		protected override FRCustomsFallbackEntryNumberCustomisation GetBusinessObjectToClone()
		{
			return new FRCustomsFallbackEntryNumberCustomisation();
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new FRCustomsFallbackEntryNumberCustomisation();
		}
		protected override FRCustomsFallbackEntryNumberCustomisation GetBusinessObjectToSerialise()
		{
			return new FRCustomsFallbackEntryNumberCustomisation();
		}

		#endregion
	}
}
