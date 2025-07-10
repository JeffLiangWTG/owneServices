using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DirectorySearch))]
	sealed class DirectorySearchTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			DirectorySearch result = new DirectorySearch();
			result.DirectoryPath = @"X:\Directory";
			result.SearchSubdirectories = true;
			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}
