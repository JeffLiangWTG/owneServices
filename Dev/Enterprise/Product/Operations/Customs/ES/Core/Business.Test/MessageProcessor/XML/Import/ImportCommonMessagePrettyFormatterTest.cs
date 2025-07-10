using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.ImportacionCompletaV1Sal;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business.Testing
{
	class ImportCommonMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new ImportCommonMessagePrettyFormatter<ImportacionCompletaV1Sal>(null, Factory.New<CusEntryHeader>()));
			AssertExceptionThrown<ArgumentNullException>("Null entry header", () => new ImportCommonMessagePrettyFormatter<ImportacionCompletaV1Sal>(new ImportacionCompletaV1Sal(), null));
		}
	}
}
