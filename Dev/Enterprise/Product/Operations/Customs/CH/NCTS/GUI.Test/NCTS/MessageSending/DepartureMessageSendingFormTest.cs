using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.GUI.Testing;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.Customs.CH.NCTS.Business.Testing;
using Enterprise.Customs.CH.NCTS.GUI.Testing.NCTS.MessageSending;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.CH.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(DepartureMessageSendingForm))]
sealed class DepartureMessageSendingFormTest : BaseMessageSendingFormTest<DepartureMessageSendingFormTest.DepartureMessageSendingFormForTesting>
{
	protected override string MovementType => NctsMovementType.Codes.Departure;

	protected override DepartureMessageSendingFormForTesting CreateMessageSendingForm() => new DepartureMessageSendingFormForTesting(MessageSendingObjectParent);

	public void TestSendingObjectsGridColumns() => CombineAssertions(() =>
	{
		using (var form = new DepartureMessageSendingFormForTesting(MessageSendingObjectParent))
		{
			form.Show();
			var messageSendingObjectsGrid = form.MessageSendingObjectsGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

			AssertEquals("Column count", 5, messageSendingObjectsGrid.Length);
			UserControlTestHelper.AssertColumnStyles(messageSendingObjectsGrid, NctsHeaderDepartureMessageSendingObject.Schema.ShouldSend, 0);
			UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(messageSendingObjectsGrid, NctsHeaderDepartureMessageSendingObject.Schema.LRN, 1);
			UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(messageSendingObjectsGrid, NctsHeaderDepartureMessageSendingObject.Schema.MRN, 2);
			UserControlTestHelper.AssertColumnStyles<ZDropEditColumnStyleInfo>(messageSendingObjectsGrid, NctsHeaderDepartureMessageSendingObject.Schema.MessageType, 3);
			UserControlTestHelper.AssertColumnStyles<ZDropEditColumnStyleInfo>(messageSendingObjectsGrid, NctsHeaderDepartureMessageSendingObject.Schema.ReasonCode, 4);
		}
	});

	public void TestReasonText() => CombineAssertions(() =>
	{
		using (var form = new DepartureMessageSendingFormForTesting(MessageSendingObjectParent))
		{
			var reasonTextTextBox = form.ReasonTextTextBox;

			AssertEquals("Multiline", true, reasonTextTextBox.Multiline);
			AssertEquals("BindTo", $"{nameof(NctsHeaderDepartureMessageSendingObjectParent.SendingObjectsCollection)}.{nameof(NctsHeaderDepartureMessageSendingObject.ReasonText)}", reasonTextTextBox.BindTo);
		}
	});

	public void TestDetailUserControlArrangement()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;

		var messageSendingObjectParent = new NctsHeaderDepartureMessageSendingObjectParent(nctsHeader);
		var messageSendingObject = messageSendingObjectParent.SendingObjectsCollection[0];

		using (var form = new DepartureMessageSendingFormForTesting(messageSendingObjectParent))
		{
			form.Show();

			CombineAssertions("NT141", () =>
			{
				messageSendingObject.MessageType = PassarMessageTypeList.Codes.NT141;
				AssertEquals("NT141DetailUserControl.Visible", true, form.NT141DetailUserControlExposed.Visible);
				AssertEquals("NC123DetailUserControl.Visible", false, form.NC123DetailUserControlExposed.Visible);
				AssertEquals("ReasonTextGroupBox.Visible", true, form.ReasonTextGroupBoxExposed.Visible);
				AssertEquals("SendWithAdditionalWarningCheckBox.Visible", false, form.SendWithAdditionalWarningCheckBoxExposed.Visible);
				AssertEquals("SendWithValidationErrorsCheckBoxExposed.Visible", false, form.SendWithValidationErrorsCheckBoxExposed.Visible);
				AssertEquals("SendButton.Enabled", true, form.SendButtonExposed.Enabled);
			});

			CombineAssertions("NC123", () =>
			{
				messageSendingObject.MessageType = PassarMessageTypeList.Codes.NC123;
				AssertEquals("NT141DetailUserControl.Visible", false, form.NT141DetailUserControlExposed.Visible);
				AssertEquals("NC123DetailUserControl.Visible", true, form.NC123DetailUserControlExposed.Visible);
				AssertEquals("ReasonTextGroupBox.Visible", false, form.ReasonTextGroupBoxExposed.Visible);
				AssertEquals("SendWithAdditionalWarningCheckBox.Visible", false, form.SendWithAdditionalWarningCheckBoxExposed.Visible);
				AssertEquals("SendWithValidationErrorsCheckBoxExposed.Visible", false, form.SendWithValidationErrorsCheckBoxExposed.Visible);
			});

			CombineAssertions("NT013", () =>
			{
				messageSendingObject.MessageType = PassarMessageTypeList.Codes.NT013;
				AssertEquals("NT141DetailUserControl.Visible", false, form.NT141DetailUserControlExposed.Visible);
				AssertEquals("NC123DetailUserControl.Visible", false, form.NC123DetailUserControlExposed.Visible);
				AssertEquals("ReasonTextGroupBox.Visible", true, form.ReasonTextGroupBoxExposed.Visible);
				AssertEquals("SendWithAdditionalWarningCheckBox.Visible", false, form.SendWithAdditionalWarningCheckBoxExposed.Visible);
				AssertEquals("SendWithValidationErrorsCheckBoxExposed.Visible", true, form.SendWithValidationErrorsCheckBoxExposed.Visible);
				AssertEquals("SendButton.Enabled (SendWithValidationErrors not ticked)", false, form.SendButtonExposed.Enabled);

				form.SendWithValidationErrorsCheckBoxExposed.Checked = true;
				AssertEquals("SendButton.Enabled (SendWithValidationErrors ticked)", true, form.SendButtonExposed.Enabled);
			});

			CombineAssertions("NC016", () =>
			{
				messageSendingObject.MessageType = PassarMessageTypeList.Codes.NC016;
				AssertEquals("NT141DetailUserControl.Visible", false, form.NT141DetailUserControlExposed.Visible);
				AssertEquals("NC123DetailUserControl.Visible", false, form.NC123DetailUserControlExposed.Visible);
				AssertEquals("ReasonTextGroupBox.Visible", true, form.ReasonTextGroupBoxExposed.Visible);
				AssertEquals("SendWithAdditionalWarningCheckBox.Visible", false, form.SendWithAdditionalWarningCheckBoxExposed.Visible);
				AssertEquals("SendWithValidationErrorsCheckBoxExposed.Visible", false, form.SendWithValidationErrorsCheckBoxExposed.Visible);
				AssertEquals("SendButton.Enabled", true, form.SendButtonExposed.Enabled);
			});
		}
	}

	public void TestSendingWithValidationErrors_SkipSupervisorApprove()
	{
		const string reasonCode = "41";
		new RefDataTestHelper(Factory).CreateCodeList(CH.Business.UniversalReferenceConstants.RefCusCodeList.PassarTypes.N1141).CreateCode(reasonCode);
		Factory.Save();

		new RefCountry.Loader(Factory).LoadForCountry(Core.Constants.CountryCodes.Switzerland).RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.NoValidationRule;

		using (var form = new DepartureMessageSendingFormForTesting(MessageSendingObjectParent))
		{
			form.Show();

			var messageSendingObject = MessageSendingObjectParent.SendingObjectsCollection[0];
			messageSendingObject.NctsHeader.CommonMovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry;
			messageSendingObject.MessageType = PassarMessageTypeList.Codes.NT141;
			messageSendingObject.ReasonCode = reasonCode;

			form.SendButtonExposed.PerformClick();

			AssertEquals(DialogResult.OK, form.DialogResult);
			AssertEquals("ATH Event is not created", false, messageSendingObject.NctsHeader.Logs.HasLogWith(log => log.SL_SE_NKEvent == AutoEvents.AuthorisedCode));
		}
	}

	public void TestSendingWithValidationErrors_WithSupervisorApprove()
	{
		new RefCountry.Loader(Factory).LoadForCountry(Core.Constants.CountryCodes.Switzerland).RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.NoValidationRule;

		using (var form = new DepartureMessageSendingFormForTesting(MessageSendingObjectParent))
		{
			form.Show();
			var messageSendingObject = MessageSendingObjectParent.SendingObjectsCollection[0];
			messageSendingObject.MessageType = PassarMessageTypeList.Codes.NT015;

			form.SendButtonExposed.Enabled = true;
			form.SendButtonExposed.PerformClick();

			AssertEquals(DialogResult.OK, form.DialogResult);
			AssertEquals("ATH Event is created", true, messageSendingObject.NctsHeader.Logs.HasLogWith(log => log.SL_SE_NKEvent == AutoEvents.AuthorisedCode));
		}
	}

	public void TestSendDisabledWhenRedErrors()
	{
		NctsHeader.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;

		var sendingObject = MessageSendingObjectParent.SendingObjectsCollection[0];
		sendingObject.MessageType = PassarMessageTypeList.Codes.NC123;
		sendingObject.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._7_FixedTransportInstallations;
		sendingObject.GoodsLocation.Address.E2_GovRegNum = "123";
		sendingObject.Validation.ValidateAll();
		AssertEquals($"Pre-check: No errors expected, but found:\n{ValidationErrors()}", false, sendingObject.HasErrors);

		using (var form = new DepartureMessageSendingFormForTesting(MessageSendingObjectParent))
		{
			form.Show();
			sendingObject.InlandTransportModeAtDeparture = ZString.Empty;
			AssertEquals("Has validation errors", false, form.SendButtonExposed.Enabled);

			sendingObject.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._7_FixedTransportInstallations;
			AssertEquals("Has no validation errors", true, form.SendButtonExposed.Enabled);
		}

		string ValidationErrors() => string.Join("\n", sendingObject.Notifications.Select(x => x.Message.Trim()));
	}

	public void TestNC123DetailUserControl()
	{
		using (var form = new DepartureMessageSendingFormForTesting(MessageSendingObjectParent))
		{
			var control = form.NC123DetailUserControlExposed;
			AssertEquals(nameof(MessageSendingObjectParent.SendingObjectsCollection), control.GetBindingMember());
		}
	}

	NctsHeaderDepartureMessageSendingObjectParent MessageSendingObjectParent => messageSendingObjectParent ?? (messageSendingObjectParent = CreateMessageSendingObjectParent());
	NctsHeaderDepartureMessageSendingObjectParent messageSendingObjectParent;

	NctsHeaderDepartureMessageSendingObjectParent CreateMessageSendingObjectParent()
	{
		return new NctsHeaderDepartureMessageSendingObjectParent(NctsHeader);
	}

	internal class DepartureMessageSendingFormForTesting : DepartureMessageSendingForm, IMessageSendingFormForTesting
	{
		internal DepartureMessageSendingFormForTesting(NctsHeaderDepartureMessageSendingObjectParent sendingObjectParent) : base(sendingObjectParent)
		{
		}

		internal new ZGrid MessageSendingObjectsGrid => base.MessageSendingObjectsGrid;

		public ZCheckBox SendWithAdditionalWarningCheckBoxExposed => SendWithAdditionalWarningCheckBox;

		public ZCheckBox SendWithValidationErrorsCheckBoxExposed => SendWithValidationErrorsCheckBox;

		public ZButton SendButtonExposed => SendButton;

		public ZUserControl NT141DetailUserControlExposed => NT141DetailsUserControl;

		public ZUserControl NC123DetailUserControlExposed => NC123DetailsUserControl;

		public ZGroupBox ReasonTextGroupBoxExposed => ReasonTextGroupBox;
	}
}

