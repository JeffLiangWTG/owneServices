using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(DocBuilderDataSource))]
	class DocBuilderDataSourceTest : RegistryBusinessObjectTemplateTestCase<DocBuilderDataSource>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override DocBuilderDataSource GetBusinessObjectToClone()
		{
			var result = new DocBuilderDataSource();
			result.Freight = true;
			return result;
		}

		protected override DocBuilderDataSource GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
