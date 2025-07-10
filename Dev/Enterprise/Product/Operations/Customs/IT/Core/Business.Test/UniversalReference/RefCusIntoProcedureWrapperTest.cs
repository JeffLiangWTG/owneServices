using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class RefCusIntoProcedureWrapperTest : TestCaseWithFactory
{
	public void TestGetNewIfProcedureCodeIsValid()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when IProcedureCodeProvider parameter is null", () => RefCusIntoProcedureWrapper.GetNewIfProcedureCodeIsValid(null, Factory));
		AssertExceptionThrown<ArgumentNullException>("Exception expected when Factory parameter is null", () => RefCusIntoProcedureWrapper.GetNewIfProcedureCodeIsValid(procedureCodeProviderMocker.Object, null));

		procedureCodeProviderMocker.Setup(m => m.ProcedureCode).Returns("");
		AssertNull("Null when ProcedureCode is empty", RefCusIntoProcedureWrapper.GetNewIfProcedureCodeIsValid(procedureCodeProviderMocker.Object, Factory));

		procedureCodeProviderMocker.Setup(m => m.ProcedureCode).Returns("XX");
		AssertNull("Null when ProcedureCode is invalid", RefCusIntoProcedureWrapper.GetNewIfProcedureCodeIsValid(procedureCodeProviderMocker.Object, Factory));

		procedureCodeProviderMocker.Setup(m => m.ProcedureCode).Returns("40");
		AssertNotNull("Not null when ProcedureCode is valid", RefCusIntoProcedureWrapper.GetNewIfProcedureCodeIsValid(procedureCodeProviderMocker.Object, Factory));
	}

	public void TestProperties()
	{
		procedureCodeProviderMocker.Setup(m => m.ProcedureCode).Returns("40");
		var refCusIntoProcedureWrapper = GetNewRefCusIntoProcedureWrapper();
		AssertProperties(refCusIntoProcedureWrapper, "40");

		procedureCodeProviderMocker.Setup(m => m.ProcedureCode).Returns("71");
		refCusIntoProcedureWrapper = GetNewRefCusIntoProcedureWrapper();
		AssertProperties(refCusIntoProcedureWrapper, "71", isIntoWarehouse: true);

		procedureCodeProviderMocker.Setup(m => m.ProcedureCode).Returns("51");
		refCusIntoProcedureWrapper = GetNewRefCusIntoProcedureWrapper();
		AssertProperties(refCusIntoProcedureWrapper, "51", isIntoInwardProcessing: true);

		procedureCodeProviderMocker.Setup(m => m.ProcedureCode).Returns("21");
		refCusIntoProcedureWrapper = GetNewRefCusIntoProcedureWrapper();
		AssertProperties(refCusIntoProcedureWrapper, "21", isIntoOutwardProcessing: true);

		procedureCodeProviderMocker.Setup(m => m.ProcedureCode).Returns("61");
		refCusIntoProcedureWrapper = GetNewRefCusIntoProcedureWrapper();
		AssertProperties(refCusIntoProcedureWrapper, "61", isReimportProcedure: true);

		procedureCodeProviderMocker.Setup(m => m.ProcedureCode).Returns("98");
		refCusIntoProcedureWrapper = GetNewRefCusIntoProcedureWrapper();
		AssertProperties(refCusIntoProcedureWrapper, "98", isTemporaryProcedure: true, isIntoTemporaryImportProcedure: true);

		procedureCodeProviderMocker.Setup(m => m.ProcedureCode).Returns("99");
		refCusIntoProcedureWrapper = GetNewRefCusIntoProcedureWrapper();
		AssertProperties(refCusIntoProcedureWrapper, "99", isTemporaryProcedure: true, isIntoTemporaryExportProcedure: true);

		procedureCodeProviderMocker.Setup(m => m.ProcedureCode).Returns("77");
		refCusIntoProcedureWrapper = GetNewRefCusIntoProcedureWrapper();
		AssertProperties(refCusIntoProcedureWrapper, "77", isIntoWarehouseForReExport: true);

		procedureCodeProviderMocker.Setup(m => m.ProcedureCode).Returns("76");
		refCusIntoProcedureWrapper = GetNewRefCusIntoProcedureWrapper();
		AssertProperties(refCusIntoProcedureWrapper, "76", isIntoWarehouseForReExport: true);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure40And71ForCurrentCountry();
		helper.CreateRefCusProcedure51ForCurrentCountry();
		helper.CreateRefCusProcedure21And22ForCurrentCountry();
		helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, "A", "61", "00", "", "One", "IMP", group: "IFD");
		var intoTemporaryProcedureImport = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, "A", "98", "00", "", "One", "IMP", group: "IFD");
		intoTemporaryProcedureImport.ZZ6_IntoTemporaryImport = "Y";
		var exportTemporaryProcedureImport = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, "A", "99", "00", "", "One", "IMP", group: "IFD");
		exportTemporaryProcedureImport.ZZ6_IntoTemporaryExport = "Y";
		helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, "A", "77", "00", "", "One", "EXP", group: "IFD");
		helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, "A", "76", "00", "", "One", "EXP", group: "IFD");

		procedureCodeProviderMocker = new Mock<IProcedureCodeProvider>();
		procedureCodeProviderMocker.Setup(m => m.CountryCode).Returns("IT");
	}
	Mock<IProcedureCodeProvider> procedureCodeProviderMocker;

	RefCusIntoProcedureWrapper GetNewRefCusIntoProcedureWrapper() => RefCusIntoProcedureWrapper.GetNewIfProcedureCodeIsValid(procedureCodeProviderMocker.Object, Factory);

	void AssertProperties(RefCusIntoProcedureWrapper refCusIntoProcedureWrapper, ZString procedureCode,
		bool isIntoWarehouse = false,
		bool isIntoInwardProcessing = false,
		bool isIntoOutwardProcessing = false,
		bool isReimportProcedure = false,
		bool isTemporaryProcedure = false,
		bool isIntoWarehouseForReExport = false,
		bool isIntoTemporaryImportProcedure = false,
		bool isIntoTemporaryExportProcedure = false)
	{
		CombineAssertions($"Check properties for Procedure {procedureCode}", () =>
		{
			AssertEquals("ProcedureCode", procedureCode, refCusIntoProcedureWrapper.ProcedureCode);
			AssertEquals("IsIntoWarehouse", isIntoWarehouse, refCusIntoProcedureWrapper.IsIntoWarehouse);
			AssertEquals("IsIntoInwardProcessing", isIntoInwardProcessing, refCusIntoProcedureWrapper.IsIntoInwardProcessing);
			AssertEquals("IsIntoOutwardProcessing", isIntoOutwardProcessing, refCusIntoProcedureWrapper.IsIntoOutwardProcessing);
			AssertEquals("IsReimportProcedure", isReimportProcedure, refCusIntoProcedureWrapper.IsReimportProcedure);
			AssertEquals("IsTemporaryProcedure", isTemporaryProcedure, refCusIntoProcedureWrapper.IsIntoTemporaryProcedure);
			AssertEquals("IsIntoWarehouseForReExport", isIntoWarehouseForReExport, refCusIntoProcedureWrapper.IsIntoWarehouseForReExport);
			AssertEquals("IsIntoTemporaryImportProcedure", isIntoTemporaryImportProcedure, refCusIntoProcedureWrapper.IsIntoTemporaryImportProcedure);
			AssertEquals("IsIntoTemporaryExportProcedure", isIntoTemporaryExportProcedure, refCusIntoProcedureWrapper.IsIntoTemporaryExportProcedure);
		});
	}
}
