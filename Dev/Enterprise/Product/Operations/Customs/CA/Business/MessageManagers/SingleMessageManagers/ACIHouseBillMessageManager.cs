//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.Common;
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.Business;
	using Enterprise.Customs.Business.MessageManagers;
	using Enterprise.Customs.CA.Business.MessageBuilders;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.Messaging.MessageBuilders;

	public class ACIHouseBillMessageManager : CAEManifestForwarderMessageManager
	{
		public ACIHouseBillMessageManager(IACIHouseBillProvider houseBillProvider, IUserNotification notification)
			: base(houseBillProvider, new ACIEManifestForwaderStatusCalculator(MessageTypeList.Descriptions.ACIHouseBill), notification)
		{
		}

		#region Refresh Details

		public override void RefreshDetails()
		{
			base.RefreshDetails();
			if (HouseBill != null)
			{
				HouseBill.EnableAndSynchronise(true);
			}
		}

		CusCAeMHHouse HouseBill
		{
			get { return DataWrapper.TopLevelBusinessObject as CusCAeMHHouse; }
		}

		#endregion

		#region Overrides

		public override string MessageFriendlyName
		{
			get { return Res.GetString("C41E1F79-E449-4B7B-907A-F1A1FB893478", "ACI eManifest House Bill Report for CCN {0}", DataWrapper.HouseCCN); }
		}

		public ZString HouseBillCCN
		{
			get { return DataWrapper.HouseCCN; }
		}

		public bool IsHouseBillInCustomsStatusActive
		{
			get { return HouseBill.IsCustomsStatusActive; }
		}

		protected override void UpdateMessageDetailsCore(Enterprise.Messaging.Business.EDIMessage message)
		{
			message.EM_ApplicationReference = HouseBillCCN.Replace(" ", "");
		}

		#region CanSendThisMessage

		protected override bool CanSendThisMessage(MessageSubTypes actionCode, out ZString messageText)
		{
			_ = base.CanSendThisMessage(actionCode, out messageText);
			if (messageText.IsEmpty)
			{
				if (!DataWrapper.Lines.Any())
				{
					messageText = Res.GetString("C0D03F42-36D2-48BA-9409-E11AD1072D8E", "there are no valid Packing Lines entered for this job, please check the packing lines.");
				}
				else if (actionCode == MessageSubTypes.Withdraw && !CanSendWithdrawal)
				{
					messageText = Res.GetString("C74C6AB3-F4E6-47C3-8BB8-5F4AA2DDD708", "the House Bill has not been reported yet.");
				}
			}
			return messageText.IsEmpty;
		}

		#endregion

		protected override IMessageBuilder GetMessageBuilder(MessageSubTypes actionCode)
		{
			return new ACIHouseBillMessageBuilder(DataWrapper, actionCode);
		}

		new IACIHouseBillProvider DataWrapper
		{
			get { return (IACIHouseBillProvider)base.DataWrapper; }
		}

		protected override MessageSendingNotificationCollection GetCommonNotificationsForSending()
		{
			var result = base.GetCommonNotificationsForSending();

			if (this.HouseBill.BW_Weight.IsEmpty)
			{
				result.AddError(CusCAeMHHouseValidation.WeightIsMandatory);
			}

			return result;
		}

		public override MessageSendingNotificationCollection GetNotificationsForSendingAWithdrawal()
		{
			var result = base.GetNotificationsForSendingAWithdrawal();

			if (this.HouseBill.IsCloseReported)
			{
				result.AddWarning(Res.GetString("01d85ff7-a726-40b0-b1d9-7da650da29a1", "The House Bill CCN: {0} is already marked as closed in a close report, are you sure you want to include this CCN in the close report?", HouseBillCCN));
			}

			return result;
		}

		protected override IEnumerable<INotification> GetMessageErrors()
		{
			var notification = base.GetMessageErrors().ToList();
			var master = HouseBill?.MasterBill;
			if (master != null)
			{
				var collector = new CustomsNotificationCollector(master, false, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors();
				collector.ForEach(x => notification.Add(x));
			}
			return notification;
		}

		#endregion
	}
}
