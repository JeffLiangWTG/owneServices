using System;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(Customs.Module.EntryHeaderController))]
	public class EntryHeaderControllerTest : Customs.Module.Testing.EntryHeaderControllerTest
	{
		public void TestID()
		{
			var entryHeaderController = new EntryHeaderController();

			AssertEquals("Entry header controller ID must be EUEntryHeaderController", "EUEntryHeaderController", entryHeaderController.ID.Name);
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
