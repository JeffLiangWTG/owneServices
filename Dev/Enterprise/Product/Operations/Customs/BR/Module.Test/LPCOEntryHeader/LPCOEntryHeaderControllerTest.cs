using System;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.BR.GUI;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(Customs.Module.EntryHeaderController))]
	class LPCOEntryHeaderControllerTest : Customs.Module.Testing.EntryHeaderControllerTest
	{
		public void TestModuleID()
		{
			var controller = new LPCOEntryHeaderController();
			AssertEquals(controller.ModuleID, ModuleIDs.Customs.BR.LPCOEntryHeader);
		}

		public void TestShowEditForm()
		{
			var entryHeader = GetNewEntryHeader();
			Factory.Save();

			var controller = new LPCOEntryHeaderController();
			using (var form = controller.ShowEditForm(entryHeader))
			{
				AssertType<JobDeclarationForm>(form);
				var declaration = (form as JobDeclarationForm).Declaration as JobDeclaration;

				AssertEquals("JE_MessageType is LPC", BRJobMessageTypeList.Codes.LPCO, declaration.JE_MessageType);
				AssertEquals("FixedJobMessageType", BRJobMessageTypeList.Codes.LPCO, declaration.FixedJobMessageType);
			}
		}

		protected override Type ExpectedFormType => typeof(JobDeclarationForm);

		protected override Customs.Business.CusEntryHeader GetNewEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			return declaration.CustomsEntryHeaders.AddNew();
		}
	}
}
