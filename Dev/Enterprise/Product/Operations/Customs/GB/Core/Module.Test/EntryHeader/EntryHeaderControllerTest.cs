using System;
using Enterprise.Customs.GB.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Module.Testing
{
	[TestedType(typeof(Customs.Module.EntryHeaderController))]
	sealed class EntryHeaderControllerTest : Customs.Module.Testing.EntryHeaderControllerTest
	{
		public void TestID()
		{
			var entryHeaderController = new EntryHeaderController();

			AssertEquals("Entry header controller ID must be GBEntryHeader", "GBEntryHeader", entryHeaderController.ID.Name);
		}

		public override Type ControllerToBashType => typeof(EntryHeaderController);

		protected override Type ExpectedFormType => typeof(Customs.GUI.BaseJobDeclarationForm);

		protected override Customs.Business.CusEntryHeader GetNewEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			return declaration.CustomsEntryHeaders.AddNew();
		}
	}
}
