using System.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Core.Testing;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Testing
{
	sealed class GetEventLastDateTimeFunctionExtractorTest : BaseFunctionExtractorTest
	{
		public void TestGetEventLastDateTimeMacro()
		{
			var dummy = Factory.New<OrgHeader>();
			dummy.Logs.AddNew(Events.Arrival, new ZDateTimeOffset(2011, 08, 31, 14, 06, 00));

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody]
{B}-[ARV=<DateTimeAsString('<GetEventLastDateTime(ARV)>','yyyy-MM-dd')>]
{B}-[DEP=<DateTimeAsString('<GetEventLastDateTime(DEP)>','yyyy-MM-dd')>]
{A}-[#EndOfReport]");
			template.SO_DataContext = "Organisation";

			const string expected =
@"{B}-[ARV=2011-08-31]
{B}-[DEP=]
";

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						AssertMultilineASCIIEquals("Custom fields on children of collection should work.", expected, excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		public override void TestGetMethodInfoChainLink()
		{
			var dummy = Factory.New<OrgHeader>();
			dummy.Logs.AddNew(Events.Arrival, new ZDateTimeOffset(2011, 08, 31, 14, 06, 00));

			var extractor = new GetEventLastDateTimeFunctionExtractor("ARV");
			var chainLink = extractor.GetMethodInfoChainLink(dummy.GetType());

			AssertEquals(new ZDateTime(2011, 08, 31, 14, 06, 00), chainLink.ReflectOutObject(dummy, dummy));
		}
	}
}
