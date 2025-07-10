using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusUnderbondUBMREQManager : CMRMessageManager
	{
		public CusUnderbondUBMREQManager(CusUnderbond underbond, ZString ownerCompanyABN)
		{
			this.underbond = underbond;
			OwnerCompanyABN = ownerCompanyABN;
		}

		public CusUnderbondUBMREQManager(CusUnderbond underbond) : this(underbond, ZString.Empty)
		{
		}

		public ZString OwnerCompanyABN { get; }

		internal override ICalculatedCusStatusCalculator[] StatusCalculators => new ICalculatedCusStatusCalculator[] { underbond.Calculator };

		internal override string GetStatus() => underbond.UnderbondStatus.Code;

		public override BusinessObject BusinessObject => underbond;

		public override string MessageFriendlyName => "Underbond Request for: " + underbond.C4_SendersMessageReference + " (" + underbond.LinkedObjectStringRepresentation + ")";

		internal override CMRMessageBuilder[] GetBuilder(BusinessObject bizo)
		{
			var header = GetHeader(bizo as CusUnderbond);
			return header == null ? Array.Empty<CMRMessageBuilder>() : new CMRMessageBuilder[] { new UBMREQMessageBuilder(header, OwnerCompanyABN) };
		}

		internal override EDIMessageCollection GetMessages(BusinessObject bizo) => (bizo as CusUnderbond).Messages;

		protected override EDIMessage[] GenerateAmendmentMessagesCore(BusinessObject bizo)
		{
			var result = new List<EDIMessage>();
			CusUnderbond underbond = bizo as CusUnderbond;
			CMRMessageBuilder[] builders = GetBuilder(underbond);
			foreach (CMRMessageBuilder builder in builders)
			{
				builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
				builder.Messages = GetMessages(underbond);
				result.Add(builder.PopulateMessagesReturningResult());
			}

			return result.ToArray();
		}

		internal override CMRAmendmentGenerator GetAmendmentManager(BusinessObject bizo) => throw new NotSupportedException("This override was not need becase we implemented GetAmendmentMessages ourselves");

		protected override Customs.Business.MessageSendingNotificationCollection GetCommonNotificationsForSending()
		{
			Customs.Business.MessageSendingNotificationCollection result = base.GetCommonNotificationsForSending();
			if (underbond.LinkedObject == null)
			{
				result.AddError("You can't send this underbond because it is not linked to anything.");
			}
			else if (GetHeader(underbond) == null)
			{
				result.AddError("You can't send this underbond because it is not linked to a valid entity. (It is linked to '" + underbond.LinkedObject.HumanReadableName + "')");
			}

			return result;
		}

		protected override bool RequiresAmendmentCore()
		{
			var awb = underbond.LinkedObject as CusHAWBBase;
			return (awb != null && awb.IsMasterBillChangedFromEmpty) || base.RequiresAmendmentCore();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		protected override Customs.Business.MessageSendingQueryCollection GetQueriesForSendingCore()
		{
			Customs.Business.MessageSendingQueryCollection result = base.GetQueriesForSendingCore();
			if (underbond.LinkedObject != null && !underbond.LinkedObject.CanSendWithoutDelay && (bool)Env.Registry.RawRegistry.AUCAutoSendUnderbondOnCARST.Value)
			{
				if (underbond.IsUnderbondForSeaShipment && underbond.OceanBill != null)
				{
					var scaHouse = underbond.Factory.LoadTop1<CusSCAHouse>(new ZQuery(CusSCAHouseSchema.CA_CB, underbond.OceanBill.PK));
					if (scaHouse != null)
					{
						if (scaHouse != null && scaHouse.CanSendWithoutDelay)
						{
							return result;
						}

						var scaPivot = underbond.Factory.LoadTop1<CusSCAPivot>(new ZQuery(CusSCAPivotSchema.CV_CA, scaHouse.PK));
						if (scaPivot != null && ((ICusUnderbondDependentCollectionParent)scaPivot).CanSendWithoutDelay)
						{
							return result;
						}

						ICusUnderbondDependentCollectionParent[] scaHouseProviders = ((ICusUnderbondUnionCollectionParent)scaHouse).GetAllPossibleCollectionProviders();
						foreach (var houseProvider in scaHouseProviders)
						{
							if (houseProvider.CanSendWithoutDelay)
							{
								return result;
							}
						}
					}
				}

				Customs.Business.MessageSendingQuery query = new Customs.Business.MessageSendingQuery();
				query.Question = "No CARST messages were found to permit the acceptance of this underbond. \r\n\r\nDo you want to delay sending it and have CargoWise One send it automatically when a CARST arrives?";
				query.Caption = "Delay sending of Underbond - " + underbond.C4_SendersMessageReference + "?";
				query.Delegate = new Customs.Business.MessageSendingQueryDelegate(DelayAction);
				result.Add(query);
			}

			return result;
		}

		protected override void OnOriginalSentCore()
		{
			base.OnOriginalSentCore();
			if (underbond.LinkedObject != null)
			{
				var mawb = underbond.LinkedObject as CusMAWB;
				if (mawb != null && mawb.Consol != null)
				{
					mawb.Consol.GetLogs().AddNew(Events.UnderbondRequest);
				}
			}
		}

		internal void DelayAction(bool delay)
		{
			fPreventSend = delay;
			if (delay)
			{
				underbond.C4_Status = CMRUnderbondStatuses.Codes.UnderbondSendingDelayed;
			}
		}

		protected override bool GetPreventSendCore => fPreventSend;
		bool fPreventSend;

		protected override bool ShouldValidateCurrentCompanyLocalBusNumCharacters => true;

		protected virtual IUnderbondMovementRequestHeader GetHeader(CusUnderbond underbond)
		{
			ICusUnderbondDependentCollectionParent linkedObject = underbond.LinkedObject;

			if (linkedObject is IUnderbondMovementRequestHeaderProvider)
			{
				return ((IUnderbondMovementRequestHeaderProvider)linkedObject).GetHeader(underbond);
			}
			else
			{
				return null;
			}
		}

		readonly CusUnderbond underbond;
	}
}
