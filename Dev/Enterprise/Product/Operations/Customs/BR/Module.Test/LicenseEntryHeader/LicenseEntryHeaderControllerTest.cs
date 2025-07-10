using System;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.BR.GUI;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(Customs.Module.EntryHeaderController))]
	class LicenseEntryHeaderControllerTest : Customs.Module.Testing.EntryHeaderControllerTest
	{
		public void TestModuleID()
		{
			var controller = new LicenseEntryHeaderController();
			AssertEquals(controller.ModuleID, ModuleIDs.Customs.BR.LicenseEntryHeader);
		}

		public void TestShowEditForm()
		{
			var entryHeader = GetNewEntryHeader();
			Factory.Save();

			var controller = new LicenseEntryHeaderController();
			using (var form = controller.ShowEditForm(entryHeader))
			{
				AssertType<JobDeclarationForm>(form);
				var declaration = (form as JobDeclarationForm).Declaration as JobDeclaration;

				AssertEquals("JE_MessageType is LIC", BRJobMessageTypeList.Codes.ImportLicense, declaration.JE_MessageType);
				AssertEquals("FixedJobMessageType", BRJobMessageTypeList.Codes.ImportLicense, declaration.FixedJobMessageType);
			}
		}

		public override Type ControllerToBashType => typeof(LicenseEntryHeaderController);

		protected override Type ExpectedFormType => typeof(JobDeclarationForm);

		protected override Customs.Business.CusEntryHeader GetNewEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			return declaration.CustomsEntryHeaders.AddNew();
		}
	}
}
