using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using NUnit.Framework;

namespace CargoWise.BuildTools.Testing
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	sealed class BuildXmlAspectsTest : TestCase
	{
		public void TestAspectPathExist()
		{
			var aspects = BuildXml.Instance.SelectNodesWithNamespace("//build:DatSettings/build:Aspects/build:Aspect");
			AssertGreaterThan(aspects.Count, 0);

			CombineAssertions(() =>
			{
				foreach (var aspect in aspects)
				{
					if (aspect is not XmlElement aspectElement)
					{
						continue;
					}

					AssertAspectElement(aspectElement);
				}
			});
		}

		static void AssertAspectElement(XmlElement aspect)
		{
			var aspectName = GetRequiredAspectAttribute(aspect, "Name");
			var dataExtractorType = GetRequiredAspectAttribute(aspect, "DataExtratorType");

			var hasPathParameter = DataExtractorTypeWithPathParameter.Contains(dataExtractorType);
			var hasKnownDataExtractorType = hasPathParameter || DataExtractorTypeWithoutPathParameter.Contains(dataExtractorType);
			Assert($"Aspect '{aspectName}' has unknown DataExtratorType, add the new type to DataExtractorTypeWithPathParameter or DataExtractorTypeWithoutPathParameter", hasKnownDataExtractorType);

			if (!hasPathParameter)
			{
				return;
			}

			var parameter = GetRequiredAspectAttribute(aspect, "Parameter");
			var pathParameters = parameter.Split('\r', '\n')
				.Select(pathParameter => pathParameter.Trim())
				.Where(pathParameter => !string.IsNullOrEmpty(pathParameter));

			foreach (var pathParameter in pathParameters)
			{
				if (pathParameter.StartsWith("filepattern:"))
				{
					continue;
				}

				var fullPath = Path.Combine(BuildConstants.LocalEnterprisePath, pathParameter.TrimStart('/', '!'));
				Assert($"Aspect '{aspectName}' has invalid path '{pathParameter}'", Directory.Exists(fullPath) || File.Exists(fullPath));
			}
		}

		static readonly HashSet<string> DataExtractorTypeWithPathParameter = new(new DataExtractorTypeComparer())
		{
			"Dat.AspectImplementations.SourceTreeAreaAspect, Dat.AspectImplementations",
			"Dat.AspectImplementations.SourceTreeAreaDiffAspect, Dat.AspectImplementations",
		};

		static readonly HashSet<string> DataExtractorTypeWithoutPathParameter = new(new DataExtractorTypeComparer())
		{
			"Dat.AspectImplementations.RoslynAspectDataExtractor, Dat.AspectImplementations",
			"Enterprise.DocumentEngine.ReportSheetNameAspect, Enterprise.DocumentEngine",
		};

		static string GetRequiredAspectAttribute(XmlElement element, string attributeName)
		{
			var attribute = element.Attributes[attributeName];
			AssertNotNull($"Attribute '{attributeName}' should be defined on Aspect element", attribute);
			AssertNotNullOrEmpty($"Attribute '{attributeName}' should have value", attribute?.Value);
			return attribute?.Value;
		}

		class DataExtractorTypeComparer : IEqualityComparer<string>
		{
			public bool Equals(string x, string y)
			{
				return string.Equals(Normalize(x), Normalize(y));
			}

			public int GetHashCode(string obj)
			{
				return Normalize(obj).GetHashCode();
			}

			static string Normalize(string value)
			{
				return value.Replace(" ", string.Empty);
			}
		}
	}
}
