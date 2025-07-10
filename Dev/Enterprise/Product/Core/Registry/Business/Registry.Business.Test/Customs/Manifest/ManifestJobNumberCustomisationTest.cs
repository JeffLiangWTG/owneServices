using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Customs.Manifest.Testing
{
	[TestedType(typeof(ManifestJobNumberCustomisation))]
	public sealed class ManifestJobNumberCustomisationTest : RegistryBusinessObjectTemplateTestCase<ManifestJobNumberCustomisation>
	{
		public static void Set(ManifestJobNumberCustomisation customisation, string key, byte order, bool fountain, string detail = null)
		{
			var element = customisation.UnFilteredElements[key];
			element.Include = true;
			element.Order = order;
			element.Fountain = fountain;
			element.Detail = detail;
		}

		#region Overrides of RegistryBusinessObjectTemplateTestCase<ManifestJobNumberCustomisationRegistryDataType>

		protected override ManifestJobNumberCustomisation GetBusinessObjectToClone()
		{
			return new ManifestJobNumberCustomisation();
		}

		protected override ManifestJobNumberCustomisation GetBusinessObjectToSerialise()
		{
			return new ManifestJobNumberCustomisation();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ManifestJobNumberCustomisation();
		}

		#endregion
	}
}
