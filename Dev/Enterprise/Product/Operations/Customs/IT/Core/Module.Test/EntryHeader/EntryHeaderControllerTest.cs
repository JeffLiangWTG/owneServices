using System;
using Enterprise.Customs.IT.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Module.Testing;

[TestedType(typeof(Customs.Module.EntryHeaderController))]
sealed class EntryHeaderControllerTest : Customs.Module.Testing.EntryHeaderControllerTest
{
	public void TestID()
	{
		var entryHeaderController = new EntryHeaderController();

		AssertEquals("Entry header controller ID must be ITEntryHeaderController", "ITEntryHeaderController", entryHeaderController.ID.Name);
	}

	public override Type ControllerToBashType => typeof(EntryHeaderController);

	protected override Type ExpectedFormType => typeof(Customs.GUI.BaseJobDeclarationForm);

	protected override Customs.Business.CusEntryHeader GetNewEntryHeader()
	{
		var declaration = Factory.New<JobDeclaration>();
		return declaration.CustomsEntryHeaders.AddNew();
	}
}
