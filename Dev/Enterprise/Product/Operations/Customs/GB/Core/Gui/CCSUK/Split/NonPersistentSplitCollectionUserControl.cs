using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class NonPersistentSplitCollectionUserControl : ZUserControl, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public NonPersistentSplitCollectionUserControl(NonPersistentSplitLineOrchestrator controller, VoidMethodToClose closeMethod)
		{
			this.controller = controller;
			closeFormMethod = closeMethod;
			SetDataBinding(controller.SplitsAndFlightData, string.Empty);

			InitializeComponent();

			FlightDetailsPanel.Enabled = !controller.SplitsAndFlightData.ReadOnlyFlightDetails;

			FRDButton.AllowOverlap(FcsButton);
			GenralButton.AllowOverlap(LoadFrdButton);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (LicenceAndPimaHelper.IsSimpleAgentProfile(controller.Awb))
			{
				FcsButton.Enabled = false;
				FcsButton.Visible = false;
				LoadFrdButton.Visible = false;
			}
			else if (LicenceAndPimaHelper.IsFallbackShed(controller.Awb) || LicenceAndPimaHelper.IsFullShed(controller.Awb))
			{
				FRDButton.Enabled = false;
				GenralButton.Enabled = false;
				FRDButton.Visible = false;
				GenralButton.Visible = false;
			}
		}

		void LoadFrdButton_Click(object sender, EventArgs e)
		{
			var result = controller.PerformLoadFromLastFrd();
			if (result.IsEmpty)
			{
				result = "FRD message parsed OK";
				Globals.Message.ShowInformation(result);
			}
			else
			{
				Globals.Message.ShowWarning(result);
			}
		}

		void FRDButton_Click(object sender, EventArgs e)
		{
			SendActionAndThenCloseOrWarnInvalid(controller.PerformSplit_FRD);
		}

		void FcsButton_Click(object sender, EventArgs e)
		{
			SendActionAndThenCloseOrWarnInvalid(controller.PerformSplit_FCS);
		}

		void GenralButton_Click(object sender, EventArgs e)
		{
			SendActionAndThenCloseOrWarnInvalid(controller.PerformSplit_Genral);
		}

		void SendActionAndThenCloseOrWarnInvalid(SendMessageAction sendMessageAction)
		{
			controller.SplitsAndFlightData.UpdateTotal();
			var errorMessages = sendMessageAction(SendsMessageToCustomsGui);
			if (errorMessages.IsEmpty && closeFormMethod != null)
			{
				closeFormMethod();
			}
			else
			{
				SendsMessageToCustomsGui.NotifyUserOfAnInvalidOperation(errorMessages);
			}
		}

		void RemoveSplitsButton_Click(object sender, EventArgs e)
		{
			if (FcsButton.Enabled) // shed
			{
				SendActionAndThenCloseOrWarnInvalid(controller.PerformRemoveAllSplitsAndManagePiecesForShed);
			}
			else if (FRDButton.Enabled) // agent
			{
				var resultOfJustUpdatingNPXs = controller.PerformRemoveAllSplitstNoMessage();
				if (!resultOfJustUpdatingNPXs.IsEmpty)
				{
					Globals.Message.ShowError(resultOfJustUpdatingNPXs, "Cannot delete");
				}
				else
				{
					SendActionAndThenCloseOrWarnInvalid(controller.PerformSplit_FRD);
				}
			}
		}

		delegate ZString SendMessageAction(Customs.Business.ISendMessagesToCustomsExtraMembersAndDetermineRequiredMessagesAndSendThem initiator);

		public delegate void VoidMethodToClose();
		readonly NonPersistentSplitLineOrchestrator controller;
		readonly VoidMethodToClose closeFormMethod;
		Customs.GUI.SendsMessagesToCustomsGUI sendsMessageToCustomsGui;
		Customs.GUI.SendsMessagesToCustomsGUI SendsMessageToCustomsGui
		{
			get { return sendsMessageToCustomsGui ?? (sendsMessageToCustomsGui = new Customs.GUI.SendsMessagesToCustomsGUI()); }
		}

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return control.Name.Contains("Button") || previousControl.Name.Contains("Button");
		}
	}
}
