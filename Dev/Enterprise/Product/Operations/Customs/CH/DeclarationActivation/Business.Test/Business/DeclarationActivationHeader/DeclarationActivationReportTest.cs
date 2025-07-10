using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.DeclarationActivation.Business.Testing;

[TestedType(typeof(DeclarationActivationReport))]
sealed class DeclarationActivationReportTest : EnterpriseBusinessObjectTestCase
{
	public void TestGetNewValidation()
	{
		AssertType<DeclarationActivationReportValidation>(DeclarationActivationReport.Validation);
	}

	public void TestNewGetLookups()
	{
		AssertType<DeclarationActivationReportLookups>(DeclarationActivationReport.Lookups);
	}

	public void TestCER_Type()
	{
		AssertEquals("MaxLength", 3, DeclarationActivationReport.CER_TypeInfo.MaxLength);
	}

	public void TestNextProcedure() => CombineAssertions(() =>
	{
		AssertEquals("MaxLength", 35, DeclarationActivationReport.NextProcedureInfo.MaxLength);
		GenAddOnTestHelper.AssertGetterSetter(DeclarationActivationReport.NextProcedureInfo);
	});

	public void TestCommunicationLanguage() => CombineAssertions(() =>
	{
		AssertEquals("MaxLength", 2, DeclarationActivationReport.CommunicationLanguageInfo.MaxLength);
		GenAddOnTestHelper.AssertGetterSetter(DeclarationActivationReport.CommunicationLanguageInfo);
	});

	public void TestCER_Location()
	{
		AssertEquals("MaxLength", 12, DeclarationActivationReport.CER_LocationInfo.MaxLength);
	}

	public void TestCER_TransportMode()
	{
		AssertEquals("MaxLength", 3, DeclarationActivationReport.CER_TransportModeInfo.MaxLength);
	}

	public void TestCER_TransportType()
	{
		AssertEquals("MaxLength", 2, DeclarationActivationReport.CER_TransportTypeInfo.MaxLength);
	}

	public void TestCER_TransportID()
	{
		AssertEquals("MaxLength", 35, DeclarationActivationReport.CER_TransportIDInfo.MaxLength);
	}

	public void TestCER_CustomsOfficeOfExport()
	{
		AssertEquals("MaxLength", 8, DeclarationActivationReport.CER_OfficeOfExportInfo.MaxLength);
	}

	public void TestCER_AdditionalDeclarationType()
	{
		AssertEquals("MaxLength", 1, DeclarationActivationReport.CER_AdditionalDeclarationTypeInfo.MaxLength);
	}

	public void TestEdecOrginalTraderUID() => CombineAssertions(() =>
	{
		AssertEquals("MaxLength", 17, DeclarationActivationReport.EdecOriginalTraderUIDInfo.MaxLength);
		GenAddOnTestHelper.AssertGetterSetter(DeclarationActivationReport.EdecOriginalTraderUIDInfo);
	});

	public void TestCER_Status()
	{
		AssertEquals("ReadOnly", true, DeclarationActivationReport.CER_StatusInfo.ReadOnly);
	}

	public void TestCER_MessageStatus()
	{
		AssertEquals("ReadOnly", true, DeclarationActivationReport.CER_MessageStatusInfo.ReadOnly);
	}

	public void TestTypeDescription() => CombineAssertions(() =>
	{
		DeclarationActivationReport.CER_Type = ActivationTypeList.Codes.Edec;
		AssertEquals("Edec", ActivationTypeList.Descriptions.Edec, DeclarationActivationReport.TypeDescription);
		DeclarationActivationReport.CER_Type = ActivationTypeList.Codes.Passar;
		AssertEquals("Passar", ActivationTypeList.Descriptions.Passar, DeclarationActivationReport.TypeDescription);
	});

	public void TestCustomsStatusDescription() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateEntryStatusList(Factory);

		DeclarationActivationReport.CER_Status = CommonLookups.CustomsStatusList(Factory)[0].Code;
		AssertEquals(CommonLookups.CustomsStatusList(Factory)[0].Description, DeclarationActivationReport.CustomsStatusDescription);
	});

	public void TestMessageStatusDescription() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateEntryStatusList(Factory);

		DeclarationActivationReport.CER_MessageStatus = CHLogicalStatusList.Codes.Accepted;
		AssertEquals(CHLogicalStatusList.Descriptions.Accepted, DeclarationActivationReport.MessageStatusDescription);
	});

	public void TestNextProcedureDescription() => CombineAssertions(() =>
	{
		new RefDataTestHelper(Factory).CreateCodeList(CH.Business.UniversalReferenceConstants.RefCusCodeList.PassarTypes.NextProcedure).CreateCode("12").WithDescription("Desc12").Save();

		DeclarationActivationReport.NextProcedure = "12";
		AssertEquals("Desc12", DeclarationActivationReport.NextProcedureDescription);
	});

	[TestDate]
	public void TestDateOfValuation()
	{
		AssertEquals(ZDateTime.Now, DeclarationActivationReport.DateOfValuation);
	}

	public void TestIsEdecActivation() => CombineAssertions(() =>
	{
		DeclarationActivationReport.CER_Type = ActivationTypeList.Codes.Edec;
		AssertEquals("EDC", true, DeclarationActivationReport.IsEdecActivation);
		DeclarationActivationReport.CER_Type = ActivationTypeList.Codes.Passar;
		AssertEquals("PAS", false, DeclarationActivationReport.IsEdecActivation);
	});

	protected override BusinessObject GetNewBusinessObject() => CreateDeclarationActivationReport(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => CreateDeclarationActivationReport(Factory);

	DeclarationActivationReport DeclarationActivationReport => declarationActivationReport ??= CreateDeclarationActivationReport(Factory);
	DeclarationActivationReport declarationActivationReport;

	DeclarationActivationReport CreateDeclarationActivationReport(BusinessObjectFactory factory)
	{
		return factory.New<DeclarationActivationHeader>().Report;
	}
}
