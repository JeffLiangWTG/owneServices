using System;
using Enterprise.Customs.CN.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Module.Testing
{
	[TestedType(typeof(Customs.Module.EntryHeaderController))]
	class EntryHeaderControllerTest : Customs.Module.Testing.EntryHeaderControllerTest
	{
		protected override Type ExpectedFormType => typeof(Customs.GUI.BaseJobDeclarationForm);

		protected override Customs.Business.CusEntryHeader GetNewEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			return declaration.CustomsEntryHeaders.AddNew();
		}
	}
}
