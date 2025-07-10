using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.IO;
using Enterprise.DataTransfer.Native.Business.Requests;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.NativeSchemaGenerator
{
	public class NativeXMLSchemaGeneratorTest : TestCase
	{
		static readonly Dictionary<string, string> fileContents = new Dictionary<string, string>();

		protected override void MasterSetUp()
		{
			base.MasterSetUp();

			using (var tempDir = new TempDirectory())
			{
				new NativeXMLSchemaGenerator().GenerateXsdFiles(tempDir);

				var files = Directory.GetFiles(tempDir); 

				foreach (var file in files)
				{
					fileContents[Path.GetFileName(file)] = File.ReadAllText(file);
				}
			}
		}

		public void TestLatestSchemasGeneratedCountMatch()
		{
			var assembly = typeof(HeaderData).Assembly;

			var resourceNames = assembly.GetManifestResourceNames();

			var filteredResources = resourceNames
				.Where(name => name.Contains($".{NativeXMLSchemaGenerator.EmbedResourceFolder}."))
				.ToList();

			AssertEquals("The number of resources does not match the number of generated files.\n Please run 'Enterprise.DataTransfer.Native.Schema.Generator.exe [serverName] [databaseName]' to extract the latest native schema XSD files.", filteredResources.Count, fileContents.Count);
		}

		public void TestTestMethodsExistForAllSchemaFiles()
		{
			var assembly = typeof(HeaderData).Assembly;
			var resourceNames = assembly.GetManifestResourceNames();

			var filteredResources = resourceNames
				.Where(name => name.Contains($".{NativeXMLSchemaGenerator.EmbedResourceFolder}."))
				.ToList();
			var prefix = $"{assembly.GetName().Name}.{NativeXMLSchemaGenerator.EmbedResourceFolder}.";
			foreach (var resourceName in filteredResources)
			{
				var resoureNameWithoutExtension = Path.GetFileNameWithoutExtension(resourceName);
				var fileName = resoureNameWithoutExtension.StartsWith(prefix) ? resoureNameWithoutExtension.Substring(prefix.Length) : resoureNameWithoutExtension;
				AssertNotNull($"Test method of {fileName}", typeof(NativeXMLSchemaGeneratorTest).GetMethod($"Test{fileName}"));
			}
		}

		void CompareFileContents(string fileName)
		{
			var assembly = typeof(HeaderData).Assembly;
			var prefix = $"{assembly.GetName().Name}.{NativeXMLSchemaGenerator.EmbedResourceFolder}.";
			using (var resourceStream = assembly.GetManifestResourceStream(prefix + fileName))
			{
				AssertNotNull($"Resource not found: {fileName}.", resourceStream);
				Assert($"File not found: {fileName}\n Please run 'Enterprise.DataTransfer.Native.Schema.Generator.exe [serverName] [databaseName]' to extract the latest native schema XSD files.", fileContents.ContainsKey(fileName));
				using (var resourceReader = new StreamReader(resourceStream))
				{
					var resourceContent = resourceReader.ReadToEnd();
					resourceContent = Regex.Replace(resourceContent, @"^\s*<!--.*?-->\s*\r?\n?", string.Empty, RegexOptions.Singleline);
					var fileContent = fileContents[fileName];
					fileContent = Regex.Replace(fileContent, @"^\s*<!--.*?-->\s*\r?\n?", string.Empty, RegexOptions.Singleline);
					AssertMultilineASCIIEquals($"Mismatch found in file: {fileName}\n Please run 'Enterprise.DataTransfer.Native.Schema.Generator.exe [serverName] [databaseName]' to extract the latest native schema XSD files.", resourceContent, fileContent);
				}
			}
		}

		#region Compare Embedded Resources
		[FrequentlyFailing]
		public void TestNative()
		{
			CompareFileContents("Native.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeAcceptabilityBand()
		{
			CompareFileContents("NativeAcceptabilityBand.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeAddOnRule()
		{
			CompareFileContents("NativeAddOnRule.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeAirline()
		{
			CompareFileContents("NativeAirline.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeBMControlCustomisation()
		{
			CompareFileContents("NativeBMControlCustomisation.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeBMSystem()
		{
			CompareFileContents("NativeBMSystem.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeCarrierAccount()
		{
			CompareFileContents("NativeCarrierAccount.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeCommodityCode()
		{
			CompareFileContents("NativeCommodityCode.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeCommunication()
		{
			CompareFileContents("NativeCommunication.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeCompany()
		{
			CompareFileContents("NativeCompany.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeContainer()
		{
			CompareFileContents("NativeContainer.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeCountry()
		{
			CompareFileContents("NativeCountry.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeCurrencyExchangeRate()
		{
			CompareFileContents("NativeCurrencyExchangeRate.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeCusStatement()
		{
			CompareFileContents("NativeCusStatement.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeDangerousGood()
		{
			CompareFileContents("NativeDangerousGood.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeDangerousGoodsCountryReferenceMapping()
		{
			CompareFileContents("NativeDangerousGoodsCountryReferenceMapping.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeEDICodeMapping()
		{
			CompareFileContents("NativeEDICodeMapping.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeNewsAnnouncement()
		{
			CompareFileContents("NativeNewsAnnouncement.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeOpportunity()
		{
			CompareFileContents("NativeOpportunity.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeOrganization()
		{
			CompareFileContents("NativeOrganization.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeProduct()
		{
			CompareFileContents("NativeProduct.xsd");
		}

		[FrequentlyFailing]
		public void TestNativePutawayGroup()
		{
			CompareFileContents("NativePutawayGroup.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeRate()
		{
			CompareFileContents("NativeRate.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeRequest()
		{
			CompareFileContents("NativeRequest.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeSalesChannel()
		{
			CompareFileContents("NativeSalesChannel.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeServiceLevel()
		{
			CompareFileContents("NativeServiceLevel.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeStaff()
		{
			CompareFileContents("NativeStaff.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeTag()
		{
			CompareFileContents("NativeTag.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeTagRule()
		{
			CompareFileContents("NativeTagRule.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeTransportZone()
		{
			CompareFileContents("NativeTransportZone.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeUNLOCO()
		{
			CompareFileContents("NativeUNLOCO.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeVessel()
		{
			CompareFileContents("NativeVessel.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeWarehouseClientAccountAssociation()
		{
			CompareFileContents("NativeWarehouseClientAccountAssociation.xsd");
		}

		[FrequentlyFailing]
		public void TestNativeWorkflowTemplate()
		{
			CompareFileContents("NativeWorkflowTemplate.xsd");
		}
		#endregion
	}
}
