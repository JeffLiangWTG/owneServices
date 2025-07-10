using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageBuilders;
using baseCustoms = Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	class CusAWBMultiMessageManagerForFRC<T> : baseCustoms.MultiMessageManager
		where T : BusinessObject, ICcsukCusAwb
	{
		public CusAWBMultiMessageManagerForFRC(GetCusAWBDelegate getCusMAWBDelegate)
			: base()
		{
			this.getCusAWBDelegate = getCusMAWBDelegate;
		}

		protected override baseCustoms.SingleMessageManager[] GetAllMessageManagers()
		{
			var managers = new List<baseCustoms.SingleMessageManager>();
			if (AWB != null)
			{
				managers.Add(new CusAWBSingleMessageManagerForFRC<T>(AWB)); // MAWB before HAWBs to that FRI acts on MAWB before HAWB (otherwise HAWB FRI message comes first and may be rejected with 'master not yet created')
				var mawb = AWB as CusMAWB;
				if (mawb != null)
				{
					foreach (CusHAWB house in mawb.ChildBills)
					{
						managers.Add(new CusAWBSingleMessageManagerForFRC<CusHAWB>(house));
					}
				}
				if (AWB.HasSplits)
				{
					foreach (SplitConsignment split in AWB.Splits)
					{
						var splitManager = new CusAWBSingleMessageManagerForFRC<SplitConsignment>(split);
						splitManager.ForceAmendment = !split.IsInDatabase && split.NumberOfPiecesReceived > 0;  // If the awb is whole and has some (but not all) pieces, and user splits and assigns those received pieces to a (new) split, force an FRC for the new split (after FCS) to receive that piece. 
						managers.Add(splitManager);
					}
				}
			}
			return managers.ToArray();
		}

		#region Boring stuff

		public delegate T GetCusAWBDelegate();

		public override baseCustoms.IMessageManageableBizObj TopLevelBizObjToManage
		{
			get { return AWB; }
		}

		public T AWB
		{
			get { return getCusAWBDelegate == null ? null : getCusAWBDelegate(); }
		}
		readonly GetCusAWBDelegate getCusAWBDelegate;

		protected override bool SendWheneverPossibleOnceMessagingActive
		{
			get { return true; }
		}

		protected override bool ShowNotificationsAfterSaveCore
		{
			get { return true; }
		}

		#endregion
	}

	public class CusAWBSingleMessageManagerForFRC<T> : baseCustoms.SingleMessageManager
		where T : BusinessObject, ICcsukCusAwb
	{
		public CusAWBSingleMessageManagerForFRC(T aWB)
		{
			this.aWB = aWB;
		}

		protected override bool RequiresAmendmentCore()
		{
			return aWB.Messages.Count > 0  // no need to amend if have not sent FRI
				&& aWB.PresenceOnNetworkStatus != PresenceOnNetworkList.Codes.ArchivedOnCcsuk  // FRCs should not be sent for ARC-hived awbs
				&& (!aWB.ProfileInfo.HasErrors() && (LicenceAndPimaHelper.IsFullShed(aWB) || LicenceAndPimaHelper.IsFallbackShed(aWB)))  // no need to amend if FRC is not allowed (i.e. not a shed)
				&& (ForceAmendment || base.RequiresAmendmentCore()); // no need to send if relevant FRI message content is not changing
		}

		internal bool ForceAmendment { get; set; }

		protected override EDIMessage[] GenerateOriginalMessagesCore(BusinessObject bizo)
		{
			var result = new List<EDIMessage>();
			var builder = new CcsukInventoryMessageBuilder(bizo, new CcsukTransmissionMessageFunction.CUSCAR.FRI());  // FRI - new message
			var messageBuilderResult = builder.PopulateMessages();
			foreach (IBuilderResult builderResult in messageBuilderResult.GetBuilderResults())
			{
				if (messageBuilderResult.IsSuccess)
				{
					result.Add(builderResult.Message);
				}
				else
				{
					builderResult.Message.Delete();   // If generation of the message resulting in a red ErrorCollector item, do not save message
				}
			}
			return result.ToArray();
		}

		#region Boring stuff

		public override BusinessObject BusinessObject
		{
			get { return aWB; }
		}

		public override bool CanSendOriginal
		{
			get { return false; }
		}

		public override bool CanSendWithdrawal
		{
			get { return false; }
		}

		protected override EDIMessage[] GenerateAmendmentMessagesCore(BusinessObject bizo)
		{
			return new CcsukAwbAmendmentMessagesGenerator<T>((T)bizo).GenerateAmendmentMessageSet();
		}

		protected override EDIMessage[] GenerateWithdrawalMessagesCore(BusinessObject bizo)
		{
			throw new NotSupportedException("Cannot auto-create FRX messages");
		}

		public override bool IsWaitingForResponse
		{
			get { return false; } // no responses from CCSUK!
		}

		public override string MessageFriendlyName
		{
			get { return "CUSCAR message for " + aWB.HumanReadableName; }
		}

		public override bool ShouldSendOriginalOnSave
		{
			get { return !aWB.IsInDatabase && !(aWB is SplitConsignment); }  // Send FRI if not yet in DB, except for splits
		}

		readonly T aWB;

		#endregion
	}

	class CcsukAwbAmendmentMessagesGenerator<T> : baseCustoms.AmendmentMessagesGenerator
		where T : BusinessObject, ICcsukCusAwb
	{
		public CcsukAwbAmendmentMessagesGenerator(T aWB)
			: base(aWB)
		{
			this.aWB = aWB;
		}

		protected override EDIMessage GenerateAmendmentMessageCore()
		{
			var builder = new CcsukInventoryMessageBuilder(aWB, new CcsukTransmissionMessageFunction.CUSCAR.FRC()); // FRC - amendment			
			var messageBuilderResult = builder.PopulateMessages();
			foreach (IBuilderResult bR in messageBuilderResult.GetBuilderResults())
			{
				// No need to call AfterFullSuccess to get SYS-CAR. Saving will do that. If you call it too early then amendment detection falsely sees changes (due to different CARs each time).
				if (messageBuilderResult.IsSuccess)
				{
					return bR.Message;
				}
				else
				{
					bR.Message.Delete();  // If generation of the message resulting in a red ErrorCollector item, do not save message
				}
			}
			return null;
		}

		#region Boring stuff

		protected override EDIMessage GenerateOriginalMessageCore()
		{
			return null;
		}

		protected override EDIMessage GenerateWithdrawalMessageCore(BusinessObject bizo, BusinessObject bizoInNewFactory)
		{
			return null;
		}

		protected override ZPropertyInfo[] UniqueIdentifierInfos
		{
			get { return Array.Empty<ZPropertyInfo>(); } // A list of properties which, when changed, should trigger delete-then-insert instead of amend. Not implemented, validation already handles this... kinda.
		}

		readonly T aWB;

		#endregion
	}
}
