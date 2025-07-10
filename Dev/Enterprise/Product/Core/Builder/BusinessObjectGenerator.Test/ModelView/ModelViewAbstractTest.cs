using System.IO;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.BusinessObjectGenerator.ModelView.Testing
{
	abstract class ModelViewAbstractTest<TCodeGenerator> : TestCase where TCodeGenerator : ModelViewSourceFile
	{
		public virtual void TestSourceCode()
		{
			var expectedOutput = EmbeddedResourceRetriever.GetString(ExpectedOutputResourceName);
			var codeResult = CodeGenerator.SourceCode;

			AssertNotNull(codeResult);
			AssertEquals(expectedOutput, codeResult);
		}

		protected abstract string ExpectedOutputResourceName { get; }

		protected EmbeddedResourceRetriever EmbeddedResourceRetriever;
		protected ModelViewContextGenerator ContextGenerator;

		protected override void SetUp()
		{
			base.SetUp();
			EmbeddedResourceRetriever = new EmbeddedResourceRetriever();

			var countryFolder = "CountryFolder";

			modelViewXmlFileName = EmbeddedResourceRetriever.SaveResourceToFile(TestResources.DummyBizO.Xml, $"{TestResources.DummyBizO.Name}.model.xml", Path.Combine(ModelViewConstants.ScriptsDefinitionsNamespace, countryFolder));
			ModelViewWithoutIndexXmlFileName = EmbeddedResourceRetriever.SaveResourceToFile(TestResources.SampleBizO.Xml, $"{TestResources.SampleBizO.Name}.model.xml", Path.Combine(ModelViewConstants.ScriptsDefinitionsNamespace, countryFolder));

			_ = EmbeddedResourceRetriever.SaveResourceToFile(TestResources.SchemaMain.Resource, TestResources.SchemaMain.FileName, TestResources.SchemaMain.DestinationSubFolder);

			ContextGenerator = new ModelViewContextGenerator(EmbeddedResourceRetriever.DirectoryPath);
			ModelViewContextForTest = ContextGenerator.GetCodeGeneratorContext(modelViewXmlFileName);
		}

		protected override void TearDown()
		{
			EmbeddedResourceRetriever?.Dispose();
			EmbeddedResourceRetriever = null;

			base.TearDown();
		}

		protected string modelViewXmlFileName;

		protected string ModelViewWithoutIndexXmlFileName;

		protected abstract TCodeGenerator GetCodeGeneratorForTest();

		protected TCodeGenerator CodeGenerator => GetCodeGeneratorForTest();

		protected ModelViewContext ModelViewContextForTest;
	}
}
