using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Customs.AU.Module.NEXDOC;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	/// <summary>
	/// Module for NEXDOCNotification.
	/// </summary>
	public class NEXDOCNotificationModule : ZFilterGridModule
	{
		public NEXDOCNotificationModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.AU.NexDocNotifications; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.AU.NEXDOCNotificationController);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new NEXDOCNotificationFilterStripControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new QuarantineNexDocNotificationCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new NEXDOCNotificationFilterStripBusinessObject();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());

			var acknowledgeMenuItem = new ZMenuItem(Res.GetString("82BD6C19-F9CC-45B2-A96B-1FB306A0FFAF", "Acknowledge Forward/Transfer Request"));
			acknowledgeMenuItem.Click += AcknowledgeMenuItem_Click;
			result.Add(acknowledgeMenuItem);

			return result.ToArray();
		}

		void AcknowledgeMenuItem_Click(object sender, EventArgs e)
		{
			if (DisplayGrid == null || DisplayGrid.SelectedElements.Length < 1)
			{
				Globals.Message.ShowInformation(Res.GetString("45367C32-1AE7-43DA-8CD7-8F3FD1085F3A", "Please select a notification to acknowledge."));
			}
			else if (DisplayGrid.SelectedElements.Length > 1)
			{
				Globals.Message.ShowInformation(Res.GetString("F6858480-2B66-43B5-9D0C-004D4BA11AAB", "Please select only one notification."));
			}
			else
			{
				var notification = (QuarantineNexDocNotification)DisplayGrid.SelectedElements[0];
				if (!notification.IsAcknowledgedStatusForSendingMessage)
				{
					Globals.Message.ShowInformation(notification.WrongAcknowledgedStatusForSendingMessage);
				}
				else
				{
					using (var form = new NEXDOCAcknowledgeForm(notification))
					{
						ZFormModaliser.ShowDialogAndDispose(form);
					}
				}
			}
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.NEXDOCReferenceFile; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Broker; }
		}

		protected override IModuleDecisionProvider CreateDefaultModuleDecisionProvider()
		{
			return new NEXDOCNotificationModuleDecisionProvider(this);
		}

		public override bool AllowView => true;

		public override bool AllowDelete => false;

		public override bool AllowEdit => false;

		public override bool AllowNew => false;

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}

		class NEXDOCNotificationModuleDecisionProvider : DefaultModuleDecisionProvider
		{
			public NEXDOCNotificationModuleDecisionProvider(ZFilterGridModule module)
				: base(module)
			{
			}

			public override bool AllowExcelExport
			{
				get { return false; }
			}
		}
	}
}
