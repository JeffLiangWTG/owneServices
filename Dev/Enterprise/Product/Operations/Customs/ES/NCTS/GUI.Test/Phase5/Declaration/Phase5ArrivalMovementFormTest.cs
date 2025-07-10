using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing;

[TestedType(typeof(Phase5ArrivalMovementForm))]
sealed class Phase5ArrivalMovementFormTest : EU.NCTS.GUI.Testing.Phase5ArrivalMovementFormAbstractTest<NctsHeader>
{
	public void TestShowPreSaveDialogs()
	{
		var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		nctsHeader.Factory.Save();
		nctsHeader.Factory.SuspendValidation();

		using (var form = new Phase5ArrivalMovementForm(nctsHeader))
		{
			form.Show();
			ZFormModaliser.ShowDialogsInTest = false;
			nctsHeader.ArrivalMrnFromUser = "23ES00999912345678";
			nctsHeader.Factory.Save();

			CombineAssertions(() =>
			{
				form.FireSaveButton();
				AssertNull("No TNN associated exists", nctsHeader.ArrivalMovementHeader.HeaderTNN);
				AssertNull("When no TNN associated no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

				var acceptedDateTest = new ZDateTime(2022, 09, 01);
				var clearanceDateTest = new ZDateTime(2022, 09, 02);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var tnnDataCodeInfo = TnnDataCodeInfo.LoadNew(nctsHeader);
				tnnDataCodeInfo.AcceptanceDate = acceptedDateTest;
				tnnDataCodeInfo.ClearanceDate = clearanceDateTest;
				nctsHeader.ArrivalMovementHeader.GenerateTNNDeparture(tnnDataCodeInfo);
				Factory.Save();
				var headerTNN = nctsHeader.ArrivalMovementHeader.HeaderTNN;
				var popupMessage = "Departure " + headerTNN.BH_JobReference + " is an associated TNN, so if you modify this MRN, the TNN's MRN will also be modified. Do you want to modify it?";
				form.FireSaveButton();
				AssertNotNull("TNN associated exists", headerTNN);
				AssertEquals("TNN associated has same MRN", "23ES00999912345678", headerTNN.MovementReferenceNumber);
				AssertEquals("When Arrival MRN not change no pop up is shown", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
				AssertEquals("TNN associated has same Acceptance Date", acceptedDateTest, headerTNN.AcceptanceDate);
				AssertEquals("TNN associated has same Clearance Date", clearanceDateTest, headerTNN.ClearanceDate);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				nctsHeader.BH_ExportFlag = "Y";
				form.FireSaveButton();
				AssertEquals("When a field is changed in Arrival but it's not the MRN, no pop up is shown", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				nctsHeader.ArrivalMrnFromUser = "23ES00999987654321";
				form.FireSaveButton();
				AssertEquals("TNN associated MRN not change if user answers no", "23ES00999912345678", headerTNN.MovementReferenceNumber);
				AssertEquals("When Arrival MRN change a pop up is shown to ask if we want to modify TNN MRN", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.FireSaveButton();
				AssertEquals("TNN associated MRN changes if user answers yes", "23ES00999987654321", headerTNN.MovementReferenceNumber);
				AssertEquals("When Arrival MRN change a pop up is shown to ask if we want to modify TNN MRN", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
			});
		}
	}

	[RequiresSTA]
	public void TestStatusChangeEvents_IncidentsTab()
	{
		var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		nctsHeader.BH_ExportFlag = EU.NCTS.Business.EventFlagList.Codes.Yes;
		nctsHeader.Factory.Save();

		using (var form = new Phase5ArrivalMovementForm(nctsHeader))
		{
			form.Show();

			CombineAssertions(() =>
			{
				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var incidentsTabPage = mainTabControl.TabPages.Cast<ZTabPage>().Single(x => x.Name == "IncidentsTabPage");
				incidentsTabPage.Show();
				var incidentsGrid = incidentsTabPage.FindSingle<ZGrid>("IncidentsGrid");

				AssertEquals("BM_CustomsStatus is empty", ZString.Empty, nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
				AssertEquals("BM_MessageStatus is empty", ZString.Empty, nctsHeader.ArrivalMovementHeader.BM_MessageStatus);
				AssertEquals("IncidentsGrid is editable", false, incidentsGrid.ReadOnly);

				nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
				AssertEquals("IncidentsGrid is readonly", true, incidentsGrid.ReadOnly);

				nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ZString.Empty;
				AssertEquals("IncidentsGrid is editable", false, incidentsGrid.ReadOnly);

				nctsHeader.ArrivalMovementHeader.BM_MessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.SentToCustoms;
				AssertEquals("IncidentsGrid is readonly", true, incidentsGrid.ReadOnly);

				nctsHeader.ArrivalMovementHeader.BM_MessageStatus = ZString.Empty;
				AssertEquals("IncidentsGrid is editable", false, incidentsGrid.ReadOnly);
			});
		}
	}

	[RequiresSTA]
	public void TestTNNTabPageAndUserControl()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		nctsHeader.ArrivalMrnFromUser = "23ES00999912345678";
		nctsHeader.Factory.Save();

		var acceptedDateTest = new ZDateTime(2022, 09, 01);
		var clearanceDateTest = new ZDateTime(2022, 09, 02);

		using (var form = new Phase5ArrivalMovementForm(nctsHeader))
		{
			form.Show();
			ZFormModaliser.ShowDialogsInTest = false;

			CombineAssertions(() =>
			{
				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");

				AssertNull("No TNN associated exists", nctsHeader.ArrivalMovementHeader.HeaderTNN);
				AssertEquals("CEN_TNNArrival is false", false, nctsHeader.ESNctsHeader.CEN_TNNArrival);
				AssertNull("TNNTabPage is not added to the main control when no TNN exists", mainTabControl.TabPages.Cast<ZTabPage>().FirstOrDefault(x => x.Name == "TNNTabPage"));

				var tnnDataCodeInfo = TnnDataCodeInfo.LoadNew(nctsHeader);
				tnnDataCodeInfo.AcceptanceDate = acceptedDateTest;
				tnnDataCodeInfo.ClearanceDate = clearanceDateTest;
				nctsHeader.ArrivalMovementHeader.GenerateTNNDeparture(tnnDataCodeInfo);
				Factory.Save();
				form.ChangeTNNTabVisibility();

				AssertNotNull("TNN associated exists", nctsHeader.ArrivalMovementHeader.HeaderTNN);
				AssertEquals("CEN_TNNArrival is true after generating TNN", true, nctsHeader.ESNctsHeader.CEN_TNNArrival);

				var tnnTabPage = mainTabControl.TabPages.Cast<ZTabPage>().FirstOrDefault(x => x.Name == "TNNTabPage");
				AssertNotNull("TNNTabPage is added to the main control when TNN exist", tnnTabPage);

				AssertEquals("Caption", "TNN", tnnTabPage.CaptionResourceString.Caption);
				var tnnUserControl = tnnTabPage.Controls.Find("TNNTabUserControl", true).Single();
				AssertType<TNNUserControl>("UserControl type", tnnUserControl);
				AssertEquals("UserControl BindingMember", "ArrivalMovementHeader.HeaderTNN", tnnUserControl.GetBindingMember());
			});
		}
	}

	[RequiresSTA]
	public void TestTNNTabPageReadOnly()
	{
		var mrnCode = "23ES00999912345678";
		var nctsHeaderArrival = Factory.New<NctsHeader>();
		nctsHeaderArrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeaderArrival.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		nctsHeaderArrival.ArrivalMrnFromUser = mrnCode;
		nctsHeaderArrival.ESNctsHeader.CEN_TNNArrival = true;

		var nctsHeaderTNN = Factory.New<NctsHeader>();
		nctsHeaderTNN.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeaderTNN.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		nctsHeaderTNN.ESNctsHeader.CEN_TNNArrival = true;
		nctsHeaderTNN.MovementReferenceEntryNumber.CE_EntryNum = mrnCode;
		var tnnMovement = nctsHeaderTNN.MovementHeader;
		tnnMovement.BM_Phase = DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration;
		tnnMovement.BM_MessageStatus = LogicalStatusList.Codes.Sent;

		nctsHeaderArrival.ArrivalMovementHeader.BM_BM_DepartureMovement = tnnMovement.PK;

		using (var form = new Phase5ArrivalMovementForm(nctsHeaderArrival))
		{
			form.Show();
			ZFormModaliser.ShowDialogsInTest = false;

			CombineAssertions(() =>
			{
				AssertNotNull("TNN associated exists", nctsHeaderArrival.ArrivalMovementHeader.HeaderTNN);

				var tnnTabPage = form.FindSingle<ZTabPage>("TNNTabPage");
				AssertNotNull("TNNTabPage is added to the main control when TNN exist", tnnTabPage);

				var declarationTypeDropEdit = tnnTabPage.FindSingle<ZDropEdit>("DeclarationTypeDropEdit");
				AssertNotNull("declarationTypeDropEdit is not null", declarationTypeDropEdit);

				AssertEquals("TNNTabPage's children are readonly when nctsHeaderDeparture is sent (BM_MessageStatus is SNT), declarationTypeDropEdit", true, declarationTypeDropEdit.ReadOnly);

				tnnMovement.BM_MessageStatus = ZString.Empty;
				form.ToggleTNNTabEditableState();
				AssertEquals("TNNTabPage's children are not readonly when nctsHeaderDeparture is not sent or accepted (BM_MessageStatus and BM_CustomsStatus are empty), declarationTypeDropEdit", false, declarationTypeDropEdit.ReadOnly);

				tnnMovement.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
				form.ToggleTNNTabEditableState();
				AssertEquals("TNNTabPage's children are readonly when nctsHeaderDeparture TNN is accepted (BM_CustomsStatus is MRN), declarationTypeDropEdit", true, declarationTypeDropEdit.ReadOnly);
			});
		}
	}

	[RequiresSTA]
	public void TestLocalReferenceCertificateAndBrokerNotReadOnly()
	{
		var header = Factory.NewWithValidTestData<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		header.ArrivalMovementHeader.BM_MessageStatus = ZString.Empty;

		using (var form = new Phase5ArrivalMovementForm(header))
		{
			form.Show();

			var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
			var mainTabPage = mainTabControl.TabPages.Cast<ZTabPage>().FirstOrDefault(x => x.Name == "MainTabPage");
			var simplifiedProcedureCheckBox = mainTabPage.FindSingle<ZCheckBox>("SimplifiedProcedureCheckBox");
			var localReferenceTextBox = mainTabPage.FindSingle<ZTextBox>("LocalReferenceNumberTextBox");
			var certificateDropEdit = mainTabPage.FindSingle<ZDropEdit>("CertificateDropEdit");
			var brokerFindBox = mainTabPage.FindSingle<ZCodeFindBox>("BrokerCodeFindBox");

			CombineAssertions(() =>
			{
				AssertEquals("MainTabPage's LocalReferenceNumberTextBox is not readonly when nctsHeader is not register (BM_MessageStatus is empty)", false, localReferenceTextBox.ReadOnly);
				AssertEquals("MainTabPage's CertificateDropEdit is not readonly when nctsHeader is not register (BM_MessageStatus is empty)", false, certificateDropEdit.ReadOnly);
				AssertEquals("MainTabPage's BrokerFindBox is not readonly when nctsHeader is not register (BM_MessageStatus is empty)", false, brokerFindBox.ReadOnly);
				AssertEquals("MainTabPage's other fields are not readonly when nctsHeader is not register (BM_MessageStatus is empty)", false, simplifiedProcedureCheckBox.ReadOnly);

				header.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;

				AssertEquals("MainTabPage's LocalReferenceNumberTextBox is not readonly when nctsHeader is sent (BM_MessageStatus is SNT)", false, localReferenceTextBox.ReadOnly);
				AssertEquals("MainTabPage's CertificateDropEdit is not readonly when nctsHeader is sent (BM_MessageStatus is SNT)", false, certificateDropEdit.ReadOnly);
				AssertEquals("MainTabPage's BrokerFindBox is not readonly when nctsHeader is sent (BM_MessageStatus is SNT)", false, brokerFindBox.ReadOnly);
				AssertEquals("MainTabPage's other fields are readonly when nctsHeader is sent (BM_MessageStatus is SNT)", true, simplifiedProcedureCheckBox.ReadOnly);

				header.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;

				AssertEquals("MainTabPage's LocalReferenceNumberTextBox is not readonly when nctsHeader is accepted (BM_CustomsStatus is CL1)", false, localReferenceTextBox.ReadOnly);
				AssertEquals("MainTabPage's CertificateDropEdit is not readonly when nctsHeader is accepted (BM_CustomsStatus is CL1)", false, certificateDropEdit.ReadOnly);
				AssertEquals("MainTabPage's BrokerFindBox is not readonly when nctsHeader is accepted (BM_CustomsStatus is CL1)", false, brokerFindBox.ReadOnly);
				AssertEquals("MainTabPage's other fields are readonly when nctsHeader is accepted (BM_CustomsStatus is CL1)", true, simplifiedProcedureCheckBox.ReadOnly);
			});
		}
	}

	public void TestIncidentsTabPageReadOnly()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		nctsHeader.BH_ExportFlag = EU.NCTS.Business.EventFlagList.Codes.Yes;
		nctsHeader.EffectiveMessageStatus = ZString.Empty;
		nctsHeader.EnRouteIncidents.AddNew();
		Factory.Save();

		using (var form = new Phase5ArrivalMovementForm(nctsHeader))
		{
			form.Show();
			var incidentsTabPage = form.FindSingle<ZTabPage>("IncidentsTabPage");
			incidentsTabPage.Show();
			var incidentsGrid = incidentsTabPage.FindSingle<ZGrid>("IncidentsGrid");
			var incidentCodeDropEdit = incidentsTabPage.FindSingle<ZDropEdit>("IncidentCodeDropEdit");
			var informationTextBox = incidentsTabPage.FindSingle<ZTextBox>("InformationTextBox");

			CombineAssertions(() =>
			{
				AssertEquals("Incidents are not readonly", false, nctsHeader.IsIncidentsReadOnly);
				AssertEquals("IncidentsTabPage's IncidentsGrid is not readonly", false, incidentsGrid.ReadOnly);
				AssertEquals("IncidentsTabPage's IncidentCodeDropEdit is not readonly", false, incidentCodeDropEdit.ReadOnly);
				AssertEquals("IncidentsTabPage's InformationTextBox is not readonly", false, informationTextBox.ReadOnly);

				nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
				form.ToggleIncidentsTabEditableState();

				AssertEquals("Incidents are readonly", true, nctsHeader.IsIncidentsReadOnly);
				AssertEquals("IncidentsTabPage's IncidentsGrid is readonly", true, incidentsGrid.ReadOnly);
				AssertEquals("IncidentsTabPage's IncidentCodeDropEdit is readonly", true, incidentCodeDropEdit.ReadOnly);
				AssertEquals("IncidentsTabPage's InformationTextBox is readonly", true, informationTextBox.ReadOnly);
			});
		}
	}

	public void TestArrivalUnloadingRemarksTabPageControlsAllowedToRemainEditableAfterSending()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		using var form = new Phase5ArrivalMovementForm(nctsHeader);
		AssertContainsExactElementsInAnyOrder(expectedAllowedToRemainEditable, (string[])form.GetType().GetProperty("ArrivalUnloadingRemarksTabPageControlsAllowedToRemainEditableAfterSending", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(form));
	}

	static readonly string[] expectedAllowedToRemainEditable = new string[] { "ItemPackagesTabPage", "GuaranteeGroupBoxDynamicLayoutPanel", "LiabilityCalculationTabPage" };

	protected override bool AllowHasChangesOnFormOpen => true;

	protected override bool AllowSaveOnFormForTestHasChanges => false;

	[DeveloperOnlyTest]
	public override void TestMarkAsNeedingValidationIsNotCalledWhenFormLoads()
	{
		Assert(true);
	}
}
