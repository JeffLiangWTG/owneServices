using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(B2AdjustmentsModule))]
	sealed class B2AdjustmentsModuleTest : ZModuleBasherTest
	{
		public void TestCopyToNewVersionForB2()
		{
			using (var module = new B2AdjustmentsModuleForTest())
			{
				var copyToNewB2Menu = module.GetNewActionMenuItems().FindByText("Copy to new version for B2");
				AssertNotNull("Copy to new version for B2 menu item is invisible", copyToNewB2Menu);

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
				copyToNewB2Menu.PerformClick();
				AssertEquals("Information Please select one Declaration to copy.", UnitTestUserNotification.Instance.LastMessage.ToString());
				module.selectedElements = new JobDeclaration[1];
				module.selectedElements[0] = declaration;
				copyToNewB2Menu.PerformClick();
				AssertEquals("Information You may only copy Import or other Import Copy for B2 type declarations to a new version for B2.", UnitTestUserNotification.Instance.LastMessage.ToString());

				declaration.TransactionNumber.AccountSecurityCode = "12345";
				declaration.TransactionNumber.SequentialNumber = "00006789";
				declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;

				copyToNewB2Menu.PerformClick();
				AssertEquals("Information You may only copy declarations that have been accepted (decided).", UnitTestUserNotification.Instance.LastMessage.ToString());
				declaration.CA_B2AcceptedDate = new ZDateTime(2016, 09, 08);
				declaration.CA_OriginalTransactionNo = "12345000067901";

				var nextJob = Factory.New<JobDeclaration>();
				nextJob.JE_DeclarationReference = "B00000003";
				nextJob.CA_OriginalTransactionNo = "12345000067901";
				nextJob.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
				nextJob.CA_Version = 3;
				Factory.Save();
				copyToNewB2Menu.PerformClick();
				AssertEquals("Information The new version already exists. If you wish to ignore the existing version then open that version and deactivate it by selecting Actions=>Mark Inactive, then retry the copy to a new version.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestShowTemplateCopyForm()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var module = new B2AdjustmentsModuleForTest())
			using (module.ShowPopup())
			{
				module.CurrentBusinessObjectInGrid_Exposed = declaration;
				module.ShowTemplateCopyForm_Exposed();
				AssertNotEquals("Copy Allowed For Non-IM2", CopyNewVersionForB2Helper.CopyNotAllowedForIM2Message, UnitTestUserNotification.Instance.LastMessage?.Text);

				declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
				module.ShowTemplateCopyForm_Exposed();
				AssertEquals("CopyNotAllowedForIM2", CopyNewVersionForB2Helper.CopyNotAllowedForIM2Message, UnitTestUserNotification.Instance.LastMessage?.Text);

				declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
				module.ShowTemplateCopyForm_Exposed();
				AssertNotEquals("Copy Allowed For B3X", CopyNewVersionForB2Helper.CopyNotAllowedForIM2Message, UnitTestUserNotification.Instance.LastMessage?.Text);
			}
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Canada;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CA.B2Adjustments;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var bizo = factory.NewWithValidTestData<JobDeclaration>();
			bizo.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			return bizo;
		}

		sealed class B2AdjustmentsModuleForTest : B2AdjustmentsModule
		{
			internal new ZController GetNewController(BusinessObject selectedBusinessObject) => GetNewController(selectedBusinessObject);

			internal new MenuItem[] GetNewActionMenuItems() => base.GetNewActionMenuItems();

			internal BusinessObject[] selectedElements;

			internal IZForm lastEditForm;

			internal BusinessObject CurrentBusinessObjectInGrid_Exposed;

			internal IZForm ShowTemplateCopyForm_Exposed() => ShowTemplateCopyForm(CurrentBusinessObjectInGrid);

			protected override BusinessObject[] GetSelectedElements() => selectedElements;

			protected override IZForm ShowEditForm(BusinessObject selectedBusinessObject)
			{
				lastEditForm = base.ShowEditForm(selectedBusinessObject);
				return lastEditForm;
			}

			protected override BusinessObject CurrentBusinessObjectInGrid => CurrentBusinessObjectInGrid_Exposed;
		}
	}
}
