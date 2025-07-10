using System.Drawing;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.DocumentEngineCore.Registry.PrincipalBrandingCollectionRegistryItem;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(PrincipalBrandingRegistryDataType))]
	class PrincipalBrandingRegistryDataTypeFallbackMergedTest : FallbackMergedRegistryBusinessObjectCollectionDataTypeTestCase
	{
		protected override string ExpectedEditorName => "PrincipalBrandingRegistryItemEditor";

		protected override IRegistryDataType GetNewDataType()
		{
			return new PrincipalBrandingRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new PrincipalBrandingCollection();
			var branding = collection.AddNew();
			branding.BrandName = "test";
			branding.BrandEmailAddress = "test@wtg.com";
			branding.Code = "XX";
			branding.Image = new Bitmap(3, 4);

			var collection2 = new PrincipalBrandingCollection();
			var branding2 = collection.AddNew();
			branding2.BrandName = "test2";
			branding2.BrandEmailAddress = "test@wtg.com";
			branding2.Code = "XY";
			branding2.Image = new Bitmap(3, 4);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new PrincipalBrandingRegistryDataType().Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, new PrincipalBrandingRegistryDataType().Serialise(collection2))
			};
		}
	}
}
