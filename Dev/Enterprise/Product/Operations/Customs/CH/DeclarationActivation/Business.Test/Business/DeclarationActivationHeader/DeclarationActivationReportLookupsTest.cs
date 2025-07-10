using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.DeclarationActivation.Business.Testing;

[TestedType(typeof(DeclarationActivationReportLookups))]
sealed class DeclarationActivationReportLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestActivationTypeList()
	{
		AssertSame(CommonLookups.ActivationTypeList(Factory), Lookups.ActivationTypeList);
	}

	public void TestNextProcedureList()
	{
		AssertSame(CommonLookups.NextProcedureList(DeclarationActivationReport), Lookups.NextProcedureList);
	}

	public void TestCommunicationLanguageList()
	{
		AssertSame(CommonLookups.CommunicationLanguageList(Factory), Lookups.CommunicationLanguageList);
	}

	public void TestAuthorizationsList() => CombineAssertions(() =>
	{
		DeclarationActivationReport.CER_Type = ActivationTypeList.Codes.Passar;
		AssertSame(CommonLookups.ExportAuthorizationsList(DeclarationActivationReport, false, GlbCompany.CurrentCompany.GC_OH_OrgProxy), Lookups.AuthorizationsList);
		DeclarationActivationReport.CER_Type = ActivationTypeList.Codes.Edec;
		AssertSame(CommonLookups.ExportAuthorizationsList(DeclarationActivationReport, true, GlbCompany.CurrentCompany.GC_OH_OrgProxy), Lookups.AuthorizationsList);
	});

	public void TestTransportModeList()
	{
		AssertSame(CommonLookups.TransportTypeList(Factory), Lookups.TransportModeList);
	}

	public void TestTypeOfTransportTypeList()
	{
		AssertSame(CommonLookups.TransportModeList(Factory), Lookups.TransportTypeList);
	}

	public void TestCustomsOfficeList()
	{
		AssertSame(CommonLookups.CustomsOfficeList(DeclarationActivationReport), Lookups.CustomsOfficeList);
	}

	public void TestDeclarationTimeList()
	{
		AssertSame(CommonLookups.DeclarationTimeCodeList(DeclarationActivationReport), Lookups.DeclarationTimeCodeList);
	}

	public void TestMessageStatusList()
	{
		AssertSame(CommonLookups.MessageStatusList(Factory), Lookups.MessageStatusList);
	}

	public void TestCustomsStatusList()
	{
		AssertSame(CommonLookups.CustomsStatusList(Factory), Lookups.CustomsStatusList);
	}

	DeclarationActivationReport DeclarationActivationReport => declarationActivationReport ??= Factory.New<DeclarationActivationHeader>().Report;
	DeclarationActivationReport declarationActivationReport;

	DeclarationActivationReportLookups Lookups => DeclarationActivationReport.Lookups;
}
