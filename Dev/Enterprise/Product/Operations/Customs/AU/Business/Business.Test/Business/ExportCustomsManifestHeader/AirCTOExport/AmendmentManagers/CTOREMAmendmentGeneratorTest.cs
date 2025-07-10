using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CTOREMAmendmentGeneratorTest : TestCaseWithFactory
	{
		public void TestCTOREMAmendmentGenerator()
		{
			var header = Factory.New<AirCTOExportCustomsManifestHeader>();
			var amendmentGenerator = new CTOREMAmendmentGenerator(header.Lines.AddNew());
			var messages = amendmentGenerator.GenerateAmendmentMessageSet();
			AssertEquals(1, messages.Length);
		}

		public void TestUniqueIdentifier()
		{
			var header = Factory.New<AirCTOExportCustomsManifestHeader>();
			var line = header.Lines.AddNew();
			var amendmentGenerator = new CTOREMAmendmentGeneratorForTest(line);
			AssertEquals(line.EL_UserReferenceNumInfo, amendmentGenerator.UniqueIdentifierInfos[0]);
		}

		sealed class CTOREMAmendmentGeneratorForTest : CTOREMAmendmentGenerator
		{
			public CTOREMAmendmentGeneratorForTest(ExportCustomsManifestLines line)
				: base(line)
			{
			}

			internal new ZPropertyInfo[] UniqueIdentifierInfos => base.UniqueIdentifierInfos;
		}
	}
}
