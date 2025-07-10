using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI.Testing
{
	class TestCusHAWBGuiEventHandlers : CusHAWBGuiEventHandlers
	{
		public bool IsEventHooked;
		public TestCusHAWBGuiEventHandlers(UPECusHAWB cusHAWB) : base(cusHAWB)
		{
		}

		public override void HookEvents()
		{
			base.HookEvents();
			IsEventHooked = true;
		}

		#region RunPreSaveDialogs
		public bool RunPreSaveDialogs_CallBase;
		public bool RunPreSaveDialogsCalled;
		public ContinueWithSave RunPreSaveDialogs_ContinueWithSave;
		public override ContinueWithSave RunPreSaveDialogs(IBusiness topLevelEntity)
		{
			ContinueWithSave result;
			if (RunPreSaveDialogs_CallBase)
			{
				result = base.RunPreSaveDialogs(topLevelEntity);
			}
			else
			{
				RunPreSaveDialogsCalled = true;
				result = RunPreSaveDialogs_ContinueWithSave;
			}

			return result;
		}

		#endregion
		#region Shipment Held Letter Form
		public DialogResult ShipmentHeldLetterFormDialogResult = DialogResult.OK;
		public bool WasShipmentHeldLetterFormShown;
		protected override ShipmentHeldLetterForm NewShipmentHeldLetterForm(ShipmentHeldLetterBusinessObject bizObj, ShipmentHeldLetterRecipient recipient)
		{
			WasShipmentHeldLetterFormShown = true;
			return new TestShipmentHeldLetterForm(ShipmentHeldLetterFormDialogResult, bizObj, recipient);
		}

		class TestShipmentHeldLetterForm : ShipmentHeldLetterForm
		{
			public TestShipmentHeldLetterForm(DialogResult dialogResultAfterShown, ShipmentHeldLetterBusinessObject businessEntity, ShipmentHeldLetterRecipient recipient) : base(businessEntity, recipient)
			{
				ZFormModaliser.ResultToReturnFromShowDialog = dialogResultAfterShown;
			}

			protected override void OnLoad(EventArgs e)
			{
				base.OnLoad(e);
				BusinessEntity.ReasonText = "I want to";
				this.DialogResult = ZFormModaliser.ResultToReturnFromShowDialog;
				OnClosed(EventArgs.Empty);
			}
		}
		#endregion
	}
}
