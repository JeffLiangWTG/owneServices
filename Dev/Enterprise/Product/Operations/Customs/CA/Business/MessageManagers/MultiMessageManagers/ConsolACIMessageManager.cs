using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Security;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	public class ConsolACIMessageManager : MultiMessageManager
	{
		public ConsolACIMessageManager(GetCusSCAOceanBillDelegate getCusSCAOceanBillDelegate)
		{
			this.getCusSCAOceanBillDelegate = getCusSCAOceanBillDelegate;
		}
		readonly GetCusSCAOceanBillDelegate getCusSCAOceanBillDelegate;

		public delegate CusSCAOceanBill GetCusSCAOceanBillDelegate();

		public override IMessageManageableBizObj TopLevelBizObjToManage
		{
			get { return OceanBill; }
		}

		public CusSCAOceanBill OceanBill
		{
			get { return getCusSCAOceanBillDelegate == null ? null : getCusSCAOceanBillDelegate(); }
		}

		public void RefreshAll(IConsolSendsMessagesToCustoms sender)
		{
			if (sender != null)
			{
				SingleMessageManager[] messageManagersToReset = sender.WhichMessagesShouldWeRefresh(AllMessageManagers);
				if (messageManagersToReset.Length > 0)
				{
					if (sender.ShowUserConfirmation(RefreshDataComments, RefreshAllCaption, RefreshAllConfirmationMessage + " ", "REFRESH"))
					{
						OceanBill.EnableAndSynchronise(true);
						foreach (SupplementaryCargoReportMessageManager messageManager in messageManagersToReset)
						{
							messageManager.RefreshDetails();
						}
					}
				}
			}
		}

		static string RefreshDataComments
		{
			get
			{
				return Res.GetString("D4C25AB3-02AA-49BF-99D4-F45178245B81", @"Warning - You are about to refresh ACI data!
Data entered on the ACI tab for selected shipment(s) will be refreshed from original shipment data. Any data that has been overridden on the ACI tab may be lost. You should check the data on the ACI tab of selected shipments. If Supplementary Cargo Reports have already been submitted to Customs then you should re-send messages, after the refresh, to ensure that correct data has been reported.");
			}
		}

		static string RefreshAllCaption
		{
			get { return Res.GetString("7CCB1D1F-D563-4535-9799-2DC32ADE00B2", "Warning - Refresh data?"); }
		}

		static string RefreshAllConfirmationMessage
		{
			get { return Res.GetString("BE668FF1-D618-486B-9C9D-756416E6EB4E", "If you are sure you want to refresh ACI data, please type:"); }
		}

		internal static string ShipmentCannotSaveWhenWaitingForResponse
		{
			get { return Res.GetString("5719C752-1A31-48C3-BAF8-3CF314DBD4DE", "There are messages waiting for responses and the system has detected you have made changes that affect Customs messages. If you wish to re-send now then open the individual shipment(s) and re-send from the ACI menu of each shipment."); }
		}

		#region Implementation

		protected override string OriginalMessageTypeUsedInConfirmation
		{
			get { return string.Empty; }
		}

		protected override string AmendmentMessageTypeUsedInConfirmation
		{
			get { return string.Empty; }
		}

		protected override string WithdrawalMessageTypeUsedInConfirmation
		{
			get { return Res.GetString("f122280f-862e-4509-93e4-6366c3fd79c6", "cancel"); }
		}

		protected override bool SendWheneverPossibleOnceMessagingActive
		{
			get { return false; }
		}

		protected override SingleMessageManager[] GetAllMessageManagers()
		{
			var result = new List<SingleMessageManager>();
			if (OceanBill != null)
			{
				foreach (CusSCAHouse house in OceanBill.HouseBills)
				{
					var singleManager = new SupplementaryCargoReportMessageManager(house, null);
					singleManager.PendingMessagesError = ShipmentCannotSaveWhenWaitingForResponse;
					result.Add(singleManager);
				}
			}
			return result.ToArray();
		}

		protected override SecurityCheckpoint SendWithMessageErrorsSecurityCheckpoint
		{
			get
			{
				return Env.Security.ConsolCAeManifestSendWithMessageErrors;
			}
		}

		protected override SecurityCheckpoint ResetToOriginalSecurityCheckpoint
		{
			get
			{
				return Env.Security.ConsolCAeManifestResetToOriginal;
			}
		}

		#endregion
	}
}
