using System;
using Enterprise.Customs.CH.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Module.Testing;

[TestedType(typeof(Customs.Module.EntryHeaderController))]
sealed class EntryHeaderControllerTest : Customs.Module.Testing.EntryHeaderControllerTest
{
	public void TestID() => AssertEquals("CHEntryHeader", new EntryHeaderController().ID.Name);

	protected override Type ExpectedFormType => typeof(Customs.GUI.BaseJobDeclarationForm);

	protected override Customs.Business.CusEntryHeader GetNewEntryHeader() => Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
}
