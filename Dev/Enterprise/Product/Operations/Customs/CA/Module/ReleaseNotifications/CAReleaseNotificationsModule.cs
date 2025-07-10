using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.GUI;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.Messaging.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	[SuppressFormsLocalizedTest]
	public class CAReleaseNotificationsModule : EDIMessageModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.CA.CAReleaseNotifications; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.CA.CAReleaseNotifications);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CAReleaseNotificationsCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CAReleaseNotificationsFilterBusinessObject();
		}

		protected override bool ShowRequeuingMenu
		{
			get { return false; }
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CAReleaseNotificationsFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());
			if (CACustomsDataRegistry.Instance.RNSActive.Value)
			{
				var statusQuery = new ZMenuItem(ResString.GetMultilingualString("bfe2c0fb-ff44-429d-b5db-4cb30c6d72fc", "Send Release Status Query"), StatusQuery_Click);
				result.Add(statusQuery);
				var arrivalMessage = new ZMenuItem(ResString.GetMultilingualString("37ed367a-e8d5-4cdf-a28a-28bbf9f7e2ce", "Arrival Certification Message"), ArrivalMessage_Click);
				result.Add(arrivalMessage);
			}
			return result.ToArray();
		}

#if DEBUG
		internal MenuItem[] NewActionMenuItemsInternal => GetNewActionMenuItems();
#endif

		void StatusQuery_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(
				new RNSRequestForm(
					new RNSRequestBO(RNSMessageTypes.Codes.StatusQuery, Factory)));
		}

		void ArrivalMessage_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(
				new RNSRequestForm(
					new RNSRequestBO(RNSMessageTypes.Codes.ArrivalCertification, Factory)));
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.ReleaseNotificationSystem; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CAReleaseNotifications; }
		}
	}
}
