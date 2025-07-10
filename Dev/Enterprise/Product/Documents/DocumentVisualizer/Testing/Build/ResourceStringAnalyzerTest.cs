using System.IO;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentVisualizer.Business;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Build.Testing
{
	sealed class ResourceStringAnalyzerTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExtractResStringData_NonTranslatableTemplate()
		{
			var nonTranslatableTemplateFilePath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentVisualizer\Testing\Build\NonTranslatableTemplate.xls");
			var nonTranslatableTemplate = Factory.New<VisualizerTemplate>();
			nonTranslatableTemplate.SO_Name = "Non-Translatable Template";
			nonTranslatableTemplate.SO_IsSystemDefined = true;
			nonTranslatableTemplate.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(nonTranslatableTemplateFilePath);

			Factory.Save();

			AssertExtractResStringData(nonTranslatableTemplate.SO_Template, new[]
			{
@"Key: FormLabel|Q3VzdG9tcw==
Caption: Customs",

@"Key: FormLabel|Tm9uIFRyYW5zbGF0YWJsZSBEb2N1bWVudA==
Caption: Non Translatable Document"
			}, ResStringsMatchMode.Full);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExtractResStringData_TranslatableTemplate()
		{
			var translatableTemplateFilePath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentVisualizer\Testing\Build\TranslatableTemplate.xls");

			var translatableTemplate = Factory.New<VisualizerTemplate>();
			translatableTemplate.SO_Name = "Translatable Template";
			translatableTemplate.SO_IsSystemDefined = true;
			translatableTemplate.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(translatableTemplateFilePath);

			Factory.Save();

			var expectedResStrings = new string[]
			{
@"Key: FormLabel|ZG9jdW1lbnQgaGVhZGVy
Caption: document header",

@"Key: FormLabel|Rmlyc3Qgc2VjdGlvbiBoZWFkZXI=
Caption: First section header",

@"Key: FormLabel|TGFkaW5nIGF0IDxQb3J0T2ZMYWRpbmcuQ29kZT4gLSA8UG9ydE9mTGFkaW5nLkRlc2NyaXB0aW9uPg==
Caption: Lading at {0} - {1}",

@"Key: FormLabel|VmVzc2Vs
Caption: Vessel",

@"Key: FormLabel|Rmlyc3Qgc2VjdGlvbiBmb290ZXI=
Caption: First section footer",

@"Key: FormLabel|U2Vjb25kIHNlY3Rpb24gaGVhZGVy
Caption: Second section header",

@"Key: FormLabel|U2Vjb25kIHNlY3Rpb24gZm9vdGVy
Caption: Second section footer",

@"Key: FormLabel|UGFnZSA8UGFnZU51bWJlcj4gb2YgPFRvdGFsUGFnZXM+
Caption: Page {0} of {1}",

@"Key: FormLabel|VHJhbnNsYXRhYmxlIERvY3VtZW50
Caption: Translatable Document"
			};

			AssertExtractResStringData(translatableTemplate.SO_Template, expectedResStrings, ResStringsMatchMode.Full);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExtractResStringData_EUR1CustomsTemplate()
		{
			var eur1CustomsTemplateFilePath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentVisualizer\Testing\Build\EUR1CustomsTemplate.xls");

			var template = Factory.New<VisualizerTemplate>();
			template.SO_Name = "EUR1 Template";
			template.SO_IsSystemDefined = true;
			template.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(eur1CustomsTemplateFilePath);

			Factory.Save();

			var expectedResStrings = new string[]
			{
@"Key: FormLabel|TU9WRU1FTlQgQ0VSVElGSUNBVEU=
Caption: MOVEMENT CERTIFICATE",

@"Key: FormLabel|RVVSLjE=
Caption: EUR.1",

@"Key: FormLabel|KE5hbWUsIGZ1bGwgYWRkcmVzcywgY291bnRyeSk=
Caption: (Name, full address, country)",

@"Key: FormLabel|NC4gQ291bnRyeSwgZ3JvdXAgb2YgY291bnRyaWVzIG9yIHRlcnJpdG9yeSBpbiB3aGljaCB0aGUgcHJvZHVjdHMgYXJlIGNvbnNpZGVyZWQgYXMgb3JpZ2luYXRpbmc=
Caption: 4. Country, group of countries or territory in which the products are considered as originating"
			};

			AssertExtractResStringData(template.SO_Template, expectedResStrings, ResStringsMatchMode.Partial);
		}

		void AssertExtractResStringData(byte[] templateData, string[] expectedResStrings, ResStringsMatchMode expectMatchMode)
		{
			var analyzer = new ResourceStringAnalyzer();
			var res = analyzer.ExtractResStringData(new[] { templateData });

			switch (expectMatchMode)
			{
				case ResStringsMatchMode.Full:
					AssertContainsExactElementsInAnyOrder("extracted res strings",
						expectedResStrings,
						res.Select(FormatResString));
					break;

				case ResStringsMatchMode.Partial:
					var formattedRes = res.Select(FormatResString).ToArray();
					var remainingResStrings = expectedResStrings.Except(formattedRes).ToArray();
					Assert("expected ResStrings are present among the extracted ones ",  !remainingResStrings.Any());
					break;
			}
		}

		enum ResStringsMatchMode
		{
			Full,
			Partial
		}

		string FormatResString(ResourceStringData res) => string.Concat("Key: ", res.Key, "\r\n", "Caption: ", res.Caption);

		public void TestExtractResStringData_InvalidTemplate()
		{
			var templateData = new byte[][]
			{
				null,
				new byte[] { 1 , 2, 3 }
			};

			var analyzer = new ResourceStringAnalyzer();
			AssertNoExceptionThrown(() => analyzer.ExtractResStringData(templateData));
		}
	}
}
