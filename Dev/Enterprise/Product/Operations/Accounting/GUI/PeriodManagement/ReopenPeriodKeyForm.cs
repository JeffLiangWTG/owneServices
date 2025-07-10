using System;
using System.ComponentModel;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.CustomerService.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	public partial class ReopenPeriodKeyForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public ReopenPeriodKeyForm()
		{
			InitializeComponent();
		}

		public ReopenPeriodKeyForm(ReopenPeriodKeyBusinessObject bo)
			: base(bo)
		{
			InitializeComponent();
			if (bo != null)
			{
				this.Text = Res.GetString("245f28ff-47d9-4db8-9bd3-2b17e7cb7692", "Reopen Period {0}", bo.Period.AM_Period);
			}
#if DEBUG
			MissingResourceStringChecker.ExcludeFromTest(MessageLabel);
			TypeDescriptor.AddAttributes(MessageLabel, new SuppressFormsLocalizedTestAttribute());
#endif
			MessageLabel.GetExtension<ILabelCaptionRenderer>().Caption = bo.Message;
		}

		void RequestRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			if (RequestRadioButton.Checked)
			{
				setRequestControlsEnabled(true);
				setReopenControlsEnabled(false);
			}
		}

		void ReopenRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			if (ReopenRadioButton.Checked)
			{
				setReopenControlsEnabled(true);
				setRequestControlsEnabled(false);
			}
		}

		void setRequestControlsEnabled(bool value)
		{
			RequestButton.Enabled = value;
		}

		void setReopenControlsEnabled(bool value)
		{
			KeyTextBox.Enabled = value;
			ReopenSubLedgerCheckBox.Enabled = value;
			ReopenGeneralLedgerCheckBox.Enabled = value;
			ReopenForAdjustmentsCheckBox.Enabled = value;
			ReopenButton.Enabled = value;
		}

		void RequestButton_Click(object sender, EventArgs e)
		{
			ZController controller = ZControllerFactory.Create(ControllerIDs.ServiceRequest);
			((IServiceRequestController)controller).SetParentForm(this);
			controller.SetFormsModalTo(this);
			BusinessObjectFactory factory = new BusinessObjectFactory();
			IIncidentApproval incident = (IIncidentApproval)factory.New(ObjectFactory.GetType("IIncidentApproval"));
			incident.IA_IncidentSummary = Res.GetString("acb8d9f4-b990-400f-9ed5-27dea92434b6", "Request for Reopen GL Period Key");
			AccPeriodManagement period = ((ReopenPeriodKeyBusinessObject)BusinessEntity).Period;
			incident.IA_IncidentDetails = Res.GetString("2e28e2a9-9dd0-4f91-a3f2-91fca3e1aee5", "Please issue a Reopen GL Period Key with the following details:\r\n1. User's Staff Code: {0}\r\n2. Period to reopen: {1}", GlbStaff.CurrentUser.GS_Code, period.AM_Period.ToString());
			incident.IA_Criticality = Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
			incident.IA_Module = ((ICustomerServiceCodes)Activator.CreateInstance((ObjectFactory.GetType("ICustomerServiceCodes")))).ReopenClosedGLPeriodCode;
			controller.ShowFormForNewEntity(incident);
		}

		void ReopenButton_Click(object sender, EventArgs e)
		{
			ReopenPeriodKeyBusinessObject bo = (ReopenPeriodKeyBusinessObject)BusinessEntity;
			AccPeriodManagement period = bo.Period;

			ZString message = bo.ReopenPeriod(KeyTextBox.Text, bo.ReopenSubLedgerSelected, bo.ReopenGeneralLedgerSelected, bo.ReopenForAdjustmentsSelected);

			if (message.StartsWith(Res.GetString("2df9ceac-3a81-4bac-8b05-cef56ece8753", "Period {0} is Reopened for:", period.AM_Period.ToString())))
			{
				Globals.Message.Show(message);
				this.Close();
				new PeriodReopenedEmail(period).Send();
			}
			else
			{
				Globals.Message.ShowError(message);
			}
		}
	}
}
