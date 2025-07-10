using System.Linq;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing
{
	[TestedType(typeof(JobDeclarationUserControl))]
	class JobDeclarationUserControlTest : Customs.GUI.Testing.BaseCustomsDeclarationUserControlAbstractTest<JobDeclarationUserControl, JobDeclaration>
	{
		public void TestImporterDocAddressType()
		{
			using (var userControl = new JobDeclarationUserControl())
			{
				AssertType<FRDocAddressControl>(userControl.ImporterDocAddress);
			}
		}

		public void TestSupplierDocAddressType()
		{
			using (var userControl = new JobDeclarationUserControl())
			{
				AssertType<FRDocAddressControl>(userControl.SupplierDocAddress);
			}
		}

		public void TestAirRouteTypeDropEditVisibility()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			using (var jobDeclarationForm = new JobDeclarationForm(jobDeclaration))
			using (var jobDeclarationUserControl = new JobDeclarationUserControl())
			{
				jobDeclarationUserControl.JobDeclaration = jobDeclaration;
				var airRouteTypeDropEdit = (ZDropEdit)(jobDeclarationUserControl.Controls.Find("AirRouteTypeDropEdit", true).Single());
				jobDeclarationForm.Show();
				// The AirRouteTypeDropEdit should be invisible initially.
				AssertEquals("AirRouteTypeDropEdit is invisible initially.", false, airRouteTypeDropEdit.Visible);

				// The AirRouteTypeDropEdit is still invisible with "MessageType" is "Import".
				jobDeclaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				AssertEquals("AirRouteTypeDropEdit is still invisible when message is import type.", false, airRouteTypeDropEdit.Visible);

				// The AirRouteTypeDropEdit should be visible with "TransportMode" is "Air".
				jobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("AirRouteTypeDropEdit is visible with \"Import\" and \"Air\" combo-condition.", true, airRouteTypeDropEdit.Visible);

				// The AirRouteTypeDropEdit is invisible when "MessageType" is "Export".
				jobDeclaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
				AssertEquals("AirRouteTypeDropEdit is invisible when message is export type.", false, airRouteTypeDropEdit.Visible);

				// The AirRouteTypeDropEdit is invisible.
				jobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("AirRouteTypeDropEdit is invisible.", false, airRouteTypeDropEdit.Visible);
			}
		}

		public void TestGoodsLocationUserControlVisible()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(jobDeclaration))
			using (var control = new JobDeclarationUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var goodsLocationDropEdit = control.Controls.Find("GoodsLocationDropEdit", true).Single();
				Assert(!goodsLocationDropEdit.Visible);

				var goodsLocationFindBox = control.Controls.Find("GoodsLocationFindBox", true).Single();
				Assert(goodsLocationFindBox.Visible);
			}
		}

		public void TestCustomsLastEntryStatusDateEdit()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(jobDeclaration))
			using (var control = new JobDeclarationUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var customsLastEntryStatusDateEdit = control.Controls.Find("CustomsLastEntryStatusDateEdit", true).Single();

				AssertEquals("customsLastEntryStatusDateEdit should be bind to jobDeclaration.CustomsLastEntryStatusDate", "CustomsLastEntryStatusDate", customsLastEntryStatusDateEdit.GetBindingMember());
			}
		}

		public void TestModeDeltaControlVisible()
		{
			CombineAssertions("deltaModeDropEdit control visibility.", () =>
			{
				AssertControlVisible(true, DeclarationApplicationCodeList.Codes.DeltaG, "deltaModeDropEdit");
				AssertControlVisible(false, DeclarationApplicationCodeList.Codes.DeltaIE, "deltaModeDropEdit");
				AssertControlVisible(false, DeclarationApplicationCodeList.Codes.Interface, "deltaModeDropEdit");
			});
		}

		public void TestJE_DeclarationLanguageDropEditControlVisible()
		{
			CombineAssertions("JE_DeclarationLanguage control visibility", () =>
			{
				AssertControlVisible(false, DeclarationApplicationCodeList.Codes.DeltaG, "JE_DeclarationLanguageDropEdit");
				AssertControlVisible(true, DeclarationApplicationCodeList.Codes.DeltaIE, "JE_DeclarationLanguageDropEdit");
			});
		}

		public void AssertControlVisible(bool expectVisible, ZString applicationCode, string controlName)
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_ApplicationCode = applicationCode;
			using (var form = new JobDeclarationForm(jobDeclaration))
			using (var control = new JobDeclarationUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var dropEdit = control.ShipmentTypeGroupBox.Controls.Find(controlName, true).Single();
				AssertEquals($"JE_ApplicationCode: {applicationCode}", expectVisible, dropEdit.Visible);
			}
		}

		public void TestJE_LandedPiecesCalcEditVisible()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				{
					var controlJE_LandedPiecesCalcEdit = jobDeclarationUserControl.FindSingle<ZCalcEdit>(x => x.Name == "JE_LandedPiecesCalcEdit");
					declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
					Assert(controlJE_LandedPiecesCalcEdit.Visible);
					declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
					Assert(controlJE_LandedPiecesCalcEdit.Visible);
					declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.MiscellaneousCustoms;
					Assert(controlJE_LandedPiecesCalcEdit.Visible);
				}
			}
		}

		public void TestOrganizationImportUserControlType()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(jobDeclaration))
			using (var control = new JobDeclarationUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var organizationImportUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("OrganizationImportUserControl");
				AssertEquals("OrganizationImportUserControl UserControlType", typeof(ImportOrganizationUserControl), organizationImportUserControl.UserControlType);
			}
		}

		public void TestOrganizationTabShouldBeSelectedByDefault()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var userControl = new JobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var tabControl = form.FindSingle<ZTabControl>("RightTabControl");
				var selectedTab = tabControl.SelectedTab;

				AssertEquals("The Organizations tab should be selected by default, even though it is not the first tab.", userControl.OrganisationsTabPage, selectedTab);
			}
		}

		public void TestIsHighValueOvrdCheckBoxVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var userControl = new JobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
				var isHighValueOvrdCheckBox = form.FindSingle<ZCheckBox>("IsHighValueOvrdCheckBox");
				AssertEquals("IsHighValueOvrdCheckBox should be visible when DeltaG Import.", true, isHighValueOvrdCheckBox.Visible);

				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				AssertEquals("IsHighValueOvrdCheckBox should not be visible when DeltaIE Import.", false, isHighValueOvrdCheckBox.Visible);

				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
				AssertEquals("IsHighValueOvrdCheckBox should not be visible when DeltaG Export.", false, isHighValueOvrdCheckBox.Visible);

				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				AssertEquals("IsHighValueOvrdCheckBox should not be visible when DeltaIE Export.", false, isHighValueOvrdCheckBox.Visible);
			}
		}
	}
}
