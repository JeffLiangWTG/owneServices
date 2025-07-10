using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CH;

namespace Enterprise.Customs.CH.Business.Testing;

public class MergeManagerTest : Customs.Business.Testing.MergeManagerTest
{
	protected override Type GetLineMergerType() => typeof(LineMerger);

	protected override BaseJobDeclaration GetJobDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.CustomsEntryHeaders.AddNew();
		return declaration;
	}

	public void TestRequiresMergeCore() => CombineAssertions(() =>
	{
		var declaration = GetJobDeclaration();
		declaration.CustomsEntryHeaders.AddNew();

		declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		AssertEquals($"{declaration.JE_MessageType}: #CusEntryHeader={declaration.CustomsEntryHeaders.Count}", true, declaration.MergeManager.RequiresMerge);

		declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		declaration.Factory.Save();
		AssertEquals($"{declaration.JE_MessageType}: #CusEntryHeader={declaration.CustomsEntryHeaders.Count}", false, declaration.MergeManager.RequiresMerge);

		declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		declaration.Factory.Save();
		AssertEquals($"{declaration.JE_MessageType}: #CusEntryHeader={declaration.CustomsEntryHeaders.Count}, returns false due to WasExportActivation being true", false, declaration.MergeManager.RequiresMerge);

		declaration.CustomsEntryHeaders.RemoveAll();
		declaration.Factory.Save();
		AssertEquals($"{declaration.JE_MessageType}: #CusEntryHeader={declaration.CustomsEntryHeaders.Count}", false, declaration.MergeManager.RequiresMerge);
	});

	public void TestSupportsAutoMergeCore() => CombineAssertions(() =>
	{
		var declaration = GetJobDeclaration();

		declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		AssertEquals($"{declaration.JE_MessageType}", true, declaration.MergeManager.SupportsAutoMerge);

		declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		declaration.Factory.Save();
		AssertEquals($"{declaration.JE_MessageType}", false, declaration.MergeManager.SupportsAutoMerge);

		declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		AssertEquals($"{declaration.JE_MessageType} returns false due to WasExportActivation being true", false, declaration.MergeManager.SupportsAutoMerge);

		declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		declaration.Factory.Save();
		AssertEquals($"{declaration.JE_MessageType}", true, declaration.MergeManager.SupportsAutoMerge);
	});
}
