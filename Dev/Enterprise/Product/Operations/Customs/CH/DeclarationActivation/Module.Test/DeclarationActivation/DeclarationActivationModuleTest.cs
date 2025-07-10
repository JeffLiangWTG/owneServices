using System.Linq;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.CH.DeclarationActivation.Business;
using Enterprise.Customs.CH.DeclarationActivation.Module;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.DeclarationActivation.Testing;

[TestedType(typeof(DeclarationActivationModule))]
sealed class DeclarationActivationModuleTest : ZModuleBasherWithFetchHintsTest
{
	protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CH.DeclarationActivation;

	public void TestAllows() => CombineAssertions(() =>
	{
		using var module = new DeclarationActivationModule();
		AssertEquals("AllowNew", true, module.AllowNew);
		AssertEquals("AllowEdit", true, module.AllowEdit);
		AssertEquals("AllowView", true, module.AllowView);
		AssertEquals("AllowDelete", false, module.AllowDelete);
	});

	public void TestLicenceAndSecurityCheckPoint() => CombineAssertions(() =>
	{
		using var module = new DeclarationActivationModule();
		AssertEquals("Licence Checkpoint", Env.Licence.ImportBroker, module.LicenceCheckPoint);
		AssertEquals("SecurityCheckpoint", Env.Security.CustomsDeclarationEnquiryEdit, module.SecurityCheckpoint);
	});

	protected override void SetupDataForFetchHintsTest()
	{
		for (var i = 0; i < 10; i++)
		{
			CreateStatementForFetchHintTest(i);
		}
		Factory.Save();
	}

	protected override ZFilterModule CreateModuleForFetchHintsTest() => new DeclarationActivationModule();

	void CreateStatementForFetchHintTest(int i)
	{
		new RefDataTestHelper(Factory).CreateCodeList(CH.Business.UniversalReferenceConstants.RefCusCodeList.PassarTypes.NextProcedure).CreateCode($"NP{i}");
		new RefDataTestHelper(Factory).CreateCodeList(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TransportationType).CreateCode($"2{i}");
		new RefDataTestHelper(Factory).CreateCodeList(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensub).CreateCode($"{i}");

		var orgExporter = Factory.New<OrgHeader>();
		orgExporter.OH_Code = $"OE{i}";
		orgExporter.OH_FullName = $"ON{i}";

		var header = Factory.New<DeclarationActivationHeader>();
		header.CXH_ApplicationCode = CusExitHeaderApplicationCodeList.Codes.CHDeclarationActivation;
		header.CXH_JobReference = $"DA{i}";
		header.CXH_OwnerReference = $"OR{i}";
		header.CXH_OH_Exporter = orgExporter.PK;
		header.CXC_MovementReference = $"MR{i}";
		header.CXC_ReferenceNumber = $"RN{i}";
		header.CER_Location = $"Lo{i}";
		header.CER_OfficeOfExport = $"OE{i}";
		header.CER_TransportID = $"TI{i}";
		var transportModeList = header.Report.Lookups.TransportModeList;
		header.CER_TransportMode = header.Report.Lookups.TransportModeList[i % transportModeList.Count].Code;
		var transportTypeCodes = header.Report.Lookups.TransportTypeList.GetAllCodes().ToArray();
		header.CER_TransportType = transportTypeCodes[i % transportTypeCodes.Length];
		var transportNationalities = header.Report.Lookups.TransportNationalities;
		header.CER_RN_NKTransportNationality = transportNationalities[i % transportNationalities.Count].Code;
		var activationTypeList = header.Report.Lookups.ActivationTypeList;
		header.Report.CER_Type = activationTypeList[i % activationTypeList.Count].Code;
		header.NextProcedure = $"NP{i}";
		header.EdecOriginalTraderUID = $"ED{i}";
		header.CER_AdditionalDeclarationType = $"{i}";
		var messageStatusList = header.Report.Lookups.MessageStatusList;
		header.Report.CER_MessageStatus = messageStatusList[i % messageStatusList.Count].Code;
		var customsStatusList = header.Report.Lookups.CustomsStatusList;
		header.Report.CER_Status = customsStatusList[i % customsStatusList.Count].Code;
	}
}
