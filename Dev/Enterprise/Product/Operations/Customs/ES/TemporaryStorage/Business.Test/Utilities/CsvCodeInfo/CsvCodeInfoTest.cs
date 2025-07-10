using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.Testing;
[TestedType(typeof(CsvCodeInfo))]
public class CsvCodeInfoTest : NonPersistentBusinessObjectTestCase
{
	public void TestLoadNew()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Exception should be thrown when invoiceLine.Declaration is null", () => CsvCodeInfo.LoadNew(null));
			var temporaryStorageHeader = TemporaryStorageHeaderForTesting();
			AssertNoExceptionThrown("No exception expected", () => CsvCodeInfo.LoadNew(temporaryStorageHeader));
			var csvCodeInfo = CsvCodeInfo.LoadNew(temporaryStorageHeader);
			AssertEquals("CsvCodeFromUser is CSVClearance from EntryHeader", "ABCDEFGHIJKLMNOP", csvCodeInfo.CsvCodeFromUser);
			AssertEquals("ClearanceDateFromUser is CH_EntryReleaseDate", new ZDateTime(2023, 06, 14, 11, 12, 13), csvCodeInfo.ClearanceDateFromUser);
		});
	}

	public void TestValidation()
	{
		var temporaryStorageHeader = TemporaryStorageHeaderForTesting();
		var csvCodeInfo = CsvCodeInfo.LoadNew(temporaryStorageHeader);
		AssertType<CsvCodeInfoValidation>(csvCodeInfo.Validation);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var entryHeader = TemporaryStorageHeaderForTesting();
		var csvCodeInfo = CsvCodeInfo.LoadNew(entryHeader);
		return csvCodeInfo;
	}

	TemporaryStorageHeader TemporaryStorageHeaderForTesting()
	{
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		temporaryStorageHeader.ClearanceDate = new ZDateTime(2023, 06, 14, 11, 12, 13);
		temporaryStorageHeader.ClearanceNumber = "ABCDEFGHIJKLMNOP";
		return temporaryStorageHeader;
	}
}
