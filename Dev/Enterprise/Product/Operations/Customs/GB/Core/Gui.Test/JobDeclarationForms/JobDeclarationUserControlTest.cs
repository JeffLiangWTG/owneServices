using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using DeclarationApplicationCodeList = Enterprise.Customs.GB.Registry.Business.DeclarationApplicationCodeList;

namespace Enterprise.Customs.GB.GUI.JobDeclarationForms.Testing
{
	[TestedType(typeof(JobDeclarationUserControl))]
	class JobDeclarationUserControlTest : Customs.GUI.Testing.BaseCustomsDeclarationUserControlAbstractTest<JobDeclarationUserControl, JobDeclaration>
	{
		public void TestDeclarantOfficeAddressGroupBoxIsVisible()
		{
			using (var control = new JobDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				Assert(control.DeclarationDetailsGroupBox.Visible);
			}
		}

		public void TestEntrySubStyleVisible()
		{
			using (var control = new JobDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				Assert(control.BadgeCodeDropEdit.Visible);

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
				Assert(control.JE_MessageSubTypeBoundDropDownEdit.Visible);
			}
		}

		public void TestSpecificCircumstanceIndicatorInvisible()
		{
			using (var control = new JobDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				AssertEquals("Field Circumstance must be true for CDS Export jobs", true, control.FindSingle<ZDropEdit>("SpecificCircumstanceDropEdit").Visible);
			}
		}

		public void TestCaptionWithApplicationCodeChanged()
		{
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			using (var form = new ZForm(declaration) { AutoSize = true })
			using (var control = new JobDeclarationUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertCaption("[49] Customs Warehouse", control, "BondedWarehouseDocAddressControl");
				AssertCaption("[21] Nationality", control, "TransportNationalityFindBox");
				AssertCaption("[21] Trans", control, "DepartureTransportIDTextBox");
				AssertCaption("[30] Goods Location", control, "GoodsLocationDropEdit");
				declaration.JE_TransportMode = Enterprise.Customs.Business.TransportTypeList.Codes.Air;
				AssertCaption("IATA", control, "JE_IATALoadPortCodeFindBox");
				declaration.JE_TransportMode = Enterprise.Customs.Business.TransportTypeList.Codes.Sea;
			}

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			using (var form = new ZForm(declaration) { AutoSize = true })
			using (var control = new JobDeclarationUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertCaption("[UCC 2/7] Customs Warehouse", control, "BondedWarehouseDocAddressControl");
				AssertCaption("[UCC 5/23] Location of Goods", control, "GoodsLocationDropEdit");

				declaration.JE_TransportMode = Enterprise.Customs.Business.TransportTypeList.Codes.Air;
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertCaptionOneOf(new List<string> { "5/21", "[UCC 5/21]", "[UCC 5/21] IATA" }, control, "JE_IATALoadPortCodeFindBox");
			}
		}

		void AssertCaption(string caption, JobDeclarationUserControl userControl, string controlName)
		{
			var control = userControl.FindSingle<Control>(controlName);
			AssertEquals(caption, control.GetExtension<ILabelCaptionRenderer>().Caption);
		}

		void AssertCaptionOneOf(IEnumerable<string> captions, JobDeclarationUserControl userControl, string controlName)
		{
			var control = userControl.FindSingle<Control>(controlName);
			var actualCaption = control.GetExtension<ILabelCaptionRenderer>().Caption;
			var anyMatch = captions.Any(s => s == actualCaption);
			Assert($"Caption of {controlName} should be one of {string.Join(", ", captions)}", anyMatch);
		}

		public void TestBondedWarehouseVisibility_CDS()
		{
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			using (var form = new ZForm(declaration))
			using (var userControl = new JobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				userControl.RightTabControl.SelectedTab = userControl.OrganisationsTabPage;
				AssertEquals(false, userControl.BondedWarehouseDocAddressControl.Visible);
			}
		}

		public void TestIsHighValueOvrdCheckBoxVisibility()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, nameof(GB.Business.DeclarationConfiguration.DV1DetailsSupport) + "Core", true))
			{
				using (var form = new ZForm(declaration))
				using (var userControl = new JobDeclarationUserControl())
				{
					form.Controls.Add(userControl);
					form.Show();
					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

					AssertEquals("IsHighValueOvrdCheckBox Visible for CDS", true, userControl.IsHighValueOvrdCheckBox.Visible);

					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;

					AssertEquals("IsHighValueOvrdCheckBox not Visible for Chief", true, userControl.IsHighValueOvrdCheckBox.Visible);
				}
			}
		}

		public void TestCustomsOfficesUserControlType()
		{
			using (var form = new ZForm(declaration))
			using (var control = new JobDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();
				var customsOfficesUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("CustomsOfficesUserControl");
				AssertType<CustomsOfficesUserControl>(customsOfficesUserControl.HostedControl);
			}
		}

		public void TestOrganizationUserControlType()
		{
			using (var form = new ZForm(declaration))
			using (var userControl = new JobDeclarationUserControl())
			{
				var organizationExportUserControl = userControl.FindSingle<ZDynamicControlCreationUserControl>("OrganizationExportUserControl");
				var organizationImportUserControl = userControl.FindSingle<ZDynamicControlCreationUserControl>("OrganizationImportUserControl");
				AssertNull("organizationExportUserControl.UserControlType", organizationExportUserControl.UserControlType);
				AssertNull("organizationImportUserControl.UserControlType", organizationImportUserControl.UserControlType);

				form.Controls.Add(userControl);
				form.Show();

				AssertEquals("organizationExportUserControl.UserControlType", typeof(ExportOrganizationUserControl), organizationExportUserControl.UserControlType);
				AssertEquals("organizationImportUserControl.UserControlType", typeof(ImportOrganizationUserControl), organizationImportUserControl.UserControlType);
			}
		}

		public void TestReloadAfterReceivedMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			Factory.Save();

			using var helper = new MessageResponseSemaphoreHelper();
			helper.CreateSemaphoreForDeclaration(declaration);

			using var form = new ZForm(declaration);
			using var control = new JobDeclarationUserControlForTest();

			form.ControllerID = ControllerIDs.Customs.JobDeclaration;
			form.DisplayMode = ODisplayMode.Edit;
			control.JobDeclaration = declaration;
			form.Controls.Add(control);
			form.Show();

			var testControl = control.FindSingleOrDefault<ZTextBox>("DepartureTransportIDTextBox");
			AssertNotNull("Pre-requisite: need to find a control to test readonly", testControl);

			AssertEquals("Declaration made read only", expected: true, testControl.ReadOnly);

			AssertNotNull("Find MessageResponseSemaphoreTimer", control.MessageResponseSemaphoreTimer);
			AssertEquals("MessageResponseSemaphoreTimer.Enabled", expected: true, control.MessageResponseSemaphoreTimer.Enabled);
			AssertEquals("MessageResponseSemaphoreTimer.Interval", 10000, control.MessageResponseSemaphoreTimer.Interval);

			control.MessageResponseSemaphoreTimer.Interval = 10;
			Application.DoEvents();
			Thread.Sleep(TimeSpan.FromSeconds(0.1));
			Application.DoEvents();

			AssertEquals("MessageResponseSemaphoreTimer.Enabled", expected: true, control.MessageResponseSemaphoreTimer.Enabled);
			AssertEquals("Declaration still read only", expected: true, testControl.ReadOnly);

			MessageResponseSemaphoreHelper.RemoveSemaphoreForDeclaration(declaration);
			Thread.Sleep(TimeSpan.FromSeconds(0.1));
			Application.DoEvents();

			AssertEquals("MessageResponseSemaphoreTimer.Enabled", expected: false, control.MessageResponseSemaphoreTimer.Enabled);

			using var newForm = control.ReloadedForm;
			AssertNotNull("Form should be reloaded", newForm);
			testControl = newForm.FindSingleOrDefault<ZTextBox>("DepartureTransportIDTextBox");
			AssertNotNull("Pre-requisite: should be able to find the control to test readonly on the new form", testControl);
			AssertEquals("Declaration no longer read only", expected: false, testControl.ReadOnly);
		}

		public void TestClearSemaphoreAfterTimeout()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			using var helper = new MessageResponseSemaphoreHelper();
			helper.CreateSemaphoreForDeclaration(declaration);

			MessageResponseSemaphoreHelper.SemaphoreExistsForDeclaration(declaration, out var semaphoreInfo);
			using var cmd = Db.Connection.Command("UPDATE dbo.StmServiceSemaphore SET SS_AcquiredTimeUtc = @dateTime WHERE SS_ServiceClass = @category AND SS_LockInfo = @lockinfo");
			cmd.AddParameterBasedOnDbColumn("@dateTime", ZDateTime.UtcNow.AddMinutes(-31), StmServiceSemaphoreSchema.SS_AcquiredTimeUtc);
			cmd.AddParameterBasedOnDbColumn("@category", semaphoreInfo.Semaphore.Category, StmServiceSemaphoreSchema.SS_ServiceClass);
			cmd.AddParameterBasedOnDbColumn("@lockinfo", semaphoreInfo.Semaphore.LockInfo, StmServiceSemaphoreSchema.SS_LockInfo);
			cmd.ExecuteNonQuery();

			using (var form = new ZForm(declaration))
			using (var control = new JobDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals("Semaphore should have been removed", expected: false, MessageResponseSemaphoreHelper.SemaphoreExistsForDeclaration(declaration));

					var testControl = control.FindSingleOrDefault<ZTextBox>("DepartureTransportIDTextBox");
					AssertNotNull("Pre-requisite: need to find a control to test readonly", testControl);
					if (testControl != null)
					{
						AssertEquals("Declaration not read only", expected: false, testControl.ReadOnly);
					}
				});
			}
		}

		class JobDeclarationUserControlForTest : JobDeclarationUserControl
		{
			public new ZDynamicControlCreationUserControl OrganizationExportUserControl => base.OrganizationExportUserControl;
			public new ZDynamicControlCreationUserControl OrganizationImportUserControl => base.OrganizationImportUserControl;
			public new ZDropEdit JE_ApplicationCodeBoundDropEdit => base.JE_ApplicationCodeBoundDropEdit;
			public new System.Windows.Forms.Timer MessageResponseSemaphoreTimer => base.MessageResponseSemaphoreTimer;
			public new ZForm ReloadedForm => base.ReloadedForm;
		}

		protected override IEnumerable<Action> SetupDeclarationForTestScenarios(BaseJobDeclaration declaration)
		{
			foreach (var applicationCode in declaration.Lookups.ApplicationCodeList.GetAllCodes())
			{
				foreach (var messageType in declaration.Lookups.MessageTypeList.GetAllCodes())
				{
					yield return () =>
					{
						declaration.JE_ApplicationCode = applicationCode;
						declaration.JE_MessageType = messageType;
					};
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;
	}
}
