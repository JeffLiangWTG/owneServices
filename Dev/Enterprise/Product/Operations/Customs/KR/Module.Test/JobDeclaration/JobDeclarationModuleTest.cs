using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.GUI;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(JobDeclarationModule))]
	sealed class JobDeclarationModuleTest : Customs.Module.Testing.JobDeclarationModuleAbstractTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUniversalXmlMessageImport()
		{
			using (var module = new JobDeclarationModuleForTest())
			{
				var importMenuItem = module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("Add Inbound Customs Message");
				AssertNotNull("ImportInboundMessageMenuItem", importMenuItem);
				AssertNoExceptionThrown("KRCustomsMessageImport", () => importMenuItem.PerformClick());
				AssertEquals("A message was saved with message number [1].", UnitTestUserNotification.Instance.LastMessage.Text);

				var ediMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.KRCustoms));
				AssertNotNull("ediMessage", ediMessage);
				AssertEquals("ediMessage.EM_MessageType", ElectronicDocumentTypeList.Codes._R20, ediMessage.EM_MessageType);
				AssertEquals("ediMessage.EM_ReceiveTransmit", EDIMessage.Direction.Receive, ediMessage.EM_ReceiveTransmit);
				AssertEquals("ediMessage.EM_MessageNum", "1", ediMessage.EM_MessageNum);
				AssertEquals("ediMessage.EM_Status", EDIMessage.Status.Queued, ediMessage.EM_Status);
				AssertEquals("ediMessage.EM_GB", GlbBranch.CurrentBranch.PK, ediMessage.EM_GB);
				AssertEquals("ediMessage.EM_GE", GlbDepartment.CurrentDepartment.PK, ediMessage.EM_GE);
			}
		}

		public void TestNewDeclarationD87()
		{
			using (var module = new JobDeclarationModuleForTest())
			{
				var d87Menu = module.FormActionMenu.FindByText("&New").MenuItems.FindByText("Carnet Temporary Import Certificate (D87)");
				AssertNotNull("d87Menu", d87Menu);

				Customs.Module.JobDeclarationController controller = null;
				try
				{
					d87Menu.PerformClick();

					controller = (JobDeclarationController)((IFilterGridModuleInternalsForTesting)module).LastController;
					AssertEquals(typeof(MiscDeclarationForm), controller.LastShownForm.GetType());

					((ZForm)controller.LastShownForm).Close();//not saved
					((ZForm)controller.LastShownForm).Dispose();

					d87Menu.PerformClick();
					controller = (JobDeclarationController)((IFilterGridModuleInternalsForTesting)module).LastController;
					controller.Factory.Save();

					AssertEquals(1, new BusinessObjectFactory().Load<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_MessageType, ElectronicDocumentTypeList.Codes._D87)).Length);
				}
				finally
				{
					controller?.LastShownForm?.Dispose();
				}
			}
		}

		public void TestNewDeclaration008()
		{
			using (var module = new JobDeclarationModuleForTest())
			{
				var pidMenu = module.FormActionMenu.FindByText("&New").MenuItems.FindByText("Personal Items Declaration (008)");
				AssertNotNull("pidMenu", pidMenu);

				Customs.Module.JobDeclarationController controller = null;
				try
				{
					pidMenu.PerformClick();

					controller = (JobDeclarationController)((IFilterGridModuleInternalsForTesting)module).LastController;
					AssertEquals(typeof(MiscDeclarationForm), controller.LastShownForm.GetType());

					((ZForm)controller.LastShownForm).Close();//not saved
					((ZForm)controller.LastShownForm).Dispose();

					pidMenu.PerformClick();
					controller = (JobDeclarationController)((IFilterGridModuleInternalsForTesting)module).LastController;
					controller.Factory.Save();

					AssertEquals(1, new BusinessObjectFactory().Load<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_MessageType, ElectronicDocumentTypeList.Codes._008)).Length);
				}
				finally
				{
					controller?.LastShownForm?.Dispose();
				}
			}
		}

		public void TestNewDeclaration5SM()
		{
			using (var module = new JobDeclarationModuleForTest())
			{
				var valuationDeclarationTemplateMenu = module.FormActionMenu.FindByText("&New").MenuItems.FindByText("Valuation Declaration Template (5SM)");
				AssertNotNull(valuationDeclarationTemplateMenu);

				Customs.Module.JobDeclarationController controller = null;
				try
				{
					valuationDeclarationTemplateMenu.PerformClick();

					controller = (JobDeclarationController)((IFilterGridModuleInternalsForTesting)module).LastController;
					AssertEquals(typeof(MiscDeclarationForm), controller.LastShownForm.GetType());

					((ZForm)controller.LastShownForm).Close();//not saved
					((ZForm)controller.LastShownForm).Dispose();

					valuationDeclarationTemplateMenu.PerformClick();
					controller = (JobDeclarationController)((IFilterGridModuleInternalsForTesting)module).LastController;
					controller.Factory.Save();

					AssertEquals(1, new BusinessObjectFactory().Load<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_MessageType, ElectronicDocumentTypeList.Codes._5SM)).Length);
				}
				finally
				{
					controller?.LastShownForm?.Dispose();
				}
			}
		}

		public void TestEditViewDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			Factory.Save();
			AssertEditViewFormType(declaration, typeof(JobDeclarationForm));

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.PersonalItems;
			Factory.Save();
			AssertEditViewFormType(declaration, typeof(MiscDeclarationForm));

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Carnet;
			Factory.Save();
			AssertEditViewFormType(declaration, typeof(MiscDeclarationForm));
		}

		void AssertEditViewFormType(JobDeclaration declaration, Type type)
		{
			using (var module = new JobDeclarationModuleForTest())
			{
				var controller = (JobDeclarationController)module.GetNewController();

				try
				{
					AssertEquals(type, controller.ShowEditForm(declaration).GetType());
					AssertEquals(type, controller.ShowViewForm(declaration).GetType());
				}
				finally
				{
					controller.LastShownForm.Dispose();
				}
			}
		}

		protected override void SetupDeclarantForFetchHintTest(int i, OrgHeader[] organisations, BaseJobDeclaration declaration)
		{
			// KR does not make use of the column.
		}

		protected override List<string> FetchHintIgnoreField =>
			base.FetchHintIgnoreField.Union(new[] { nameof(BaseJobDeclaration.DeclarantCode), nameof(BaseJobDeclaration.DeclarantName) }).ToList();

		protected override string CountryCode => Core.Constants.CountryCodes.KoreaSouth;

		protected override Type GetExpectedJobDeclarationType() => typeof(JobDeclaration);

		protected override Type GetExpectedInvoiceHeaderType() => typeof(JobComInvoiceHeader);

		protected override Type GetExpectedInvoiceLineType() => typeof(JobComInvoiceLine);

		sealed class JobDeclarationModuleForTest : JobDeclarationModule
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
			protected override Stream GetFileStream() => File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Module.Test\TestFiles\GOVCBRR20.xml"));
		}
	}
}
