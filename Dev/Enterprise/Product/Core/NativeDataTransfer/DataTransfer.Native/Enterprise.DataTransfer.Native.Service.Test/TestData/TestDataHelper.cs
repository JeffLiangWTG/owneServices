using System.IO;
using System.Reflection;
using System.Resources;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DataTransfer.Native.Service.TestData
{
	class TestDataHelper
	{
		public static string GetEmbeddedResource(string name)
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{typeof(TestDataHelper).Namespace}.{name}.xml"))
			{
				using (var reader = new StreamReader(stream ?? throw new MissingManifestResourceException(name)))
				{
					return reader.ReadToEnd();
				}
			}
		}

		public static string OrgHeaderAndQuery => GetEmbeddedResource(nameof(OrgHeaderAndQuery));
		public static string OrgHeaderOrAndQuery => GetEmbeddedResource(nameof(OrgHeaderOrAndQuery));
		public static string OrgHeaderOrQuery => GetEmbeddedResource(nameof(OrgHeaderOrQuery));
		public static string OrgHeaderOrQuerySingleResult => GetEmbeddedResource(nameof(OrgHeaderOrQuerySingleResult));

		[ThreadSafe]
		public static string[] TestRetrieveOrgHeaders =
		{
			OrgHeaderAndQuery,
			OrgHeaderOrAndQuery,
			OrgHeaderOrQuery,
			OrgHeaderOrQuerySingleResult,
		};
	}
}
