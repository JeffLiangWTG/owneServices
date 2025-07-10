using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Registry.Business.Testing
{
	[TestedType(typeof(CDSEntryLocalReferenceNumberCustomisationRegistryDataType))]
	public class CDSEntryLocalReferenceNumberCustomisationRegistryDataTypeTests : NonPersistentBusinessObjectRegistryDataTypeTestCase<CDSEntryLocalReferenceNumberCustomisationRegistryDataType>
	{
		protected override string ExpectedEditorName => "BillOfLadingNumberCustomisationRegistryItemEditor";

		protected override CDSEntryLocalReferenceNumberCustomisationRegistryDataType GetNewDataType()
		{
			return new CDSEntryLocalReferenceNumberCustomisationRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var bytes = System.Array.Empty<byte>();
			return new[]
			{
				new ValidSampleAndBinaryValueInDB(new CDSEntryLocalReferenceNumberCustomisation(), bytes)
			};
		}
	}

	[TestedType(typeof(CDSEntryLocalReferenceNumberCustomisation))]
	public class CDSEntryLocalReferenceNumberCustomisationTest : RegistryBusinessObjectTemplateTestCase<CDSEntryLocalReferenceNumberCustomisation>
	{
		public static void Set(CDSEntryLocalReferenceNumberCustomisation customisation, string key, byte order, bool fountain, string detail = null)
		{
			var element = customisation.UnFilteredElements[key];
			element.Include = true;
			element.Order = order;
			element.Fountain = fountain;
			element.Detail = detail;
		}

		#region Overrides of RegistryBusinessObjectTemplateTestCase<CDSEntryLocalReferenceNumberCustomisation>

		protected override CDSEntryLocalReferenceNumberCustomisation GetBusinessObjectToClone()
		{
			return new CDSEntryLocalReferenceNumberCustomisation();
		}

		protected override CDSEntryLocalReferenceNumberCustomisation GetBusinessObjectToSerialise()
		{
			return new CDSEntryLocalReferenceNumberCustomisation();
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CDSEntryLocalReferenceNumberCustomisation();
		}

		#endregion
	}
}
