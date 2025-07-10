using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CTORECAmendmentGeneratorTest : TestCaseWithFactory
	{
		public void TestCTORECAmendmentGenerator()
		{
			var header = Factory.New<AirCTOExportCustomsManifestHeader>();
			var amendmentGenerator = new CTORECAmendmentGenerator(header.Lines.AddNew());
			var messages = amendmentGenerator.GenerateAmendmentMessageSet();
			AssertEquals(1, messages.Length);
		}

		public void TestUniqueIdentifier()
		{
			var header = Factory.New<AirCTOExportCustomsManifestHeader>();
			var line = header.Lines.AddNew();
			var amendmentGenerator = new CTORECAmendmentGeneratorForTest(line);
			AssertEquals(1, amendmentGenerator.UniqueIdentifierInfos.Length);
			AssertEquals(line.EL_UserReferenceNumInfo, amendmentGenerator.UniqueIdentifierInfos[0]);
		}

		sealed class CTORECAmendmentGeneratorForTest : CTORECAmendmentGenerator
		{
			public CTORECAmendmentGeneratorForTest(ExportCustomsManifestLines line)
				: base(line)
			{
			}

			internal new ZPropertyInfo[] UniqueIdentifierInfos => base.UniqueIdentifierInfos;
		}
	}
}
