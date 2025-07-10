using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.JP.Common;
using static Enterprise.Customs.JP.Common.JPMessageActionList;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class ManifestMessageSendingObjectValidation : ZValidation
	{
		public ManifestMessageSendingObjectValidation(ManifestMessageSendingObject parent) : base(parent)
		{
			this.parent = parent;
		}

		readonly ManifestMessageSendingObject parent;

		public override Type AutoValidationType => typeof(ManifestMessageSendingObjectValidation);

		public override void ValidateAll()
		{
			ValidateMessageType();
			ValidateAction();
			ValidateShouldSend();
			ValidateReason();
		}

		public void ValidateReason()
		{
			ValidateCalculatedProperty(parent.ReasonInfo);
		}

		protected void CheckReason()
		{
			if ((parent.Parent?.IsCHA ?? false) && parent.ShouldSend)
			{
				var info = parent.ReasonInfo;
				MandatoryValidation.CheckEntered(info);

				var bill = parent.Bill;
				var reason = parent.Reason;

				if ((bill.ABL_BillStatus == JPCustomsStatusList.Codes.AWR || string.IsNullOrEmpty(bill.ABL_BillStatus))
						&& reason != ReasonList.Codes.ADD)
				{
					var reasonDesc = parent.Lookups.ReasonList.GetDescriptionFromCode(reason);
					info.AddWarning(Res.GetString("3D5DAEC8-29F3-405C-8446-7BA24CDD3902", "The reason '{0} - {1}' should only be applied once the bill has been registered. However, this bill has not been registered.", reason, reasonDesc));
				}

				if (!string.IsNullOrEmpty(bill.ABL_BillStatus) && reason == ReasonList.Codes.ADD)
				{
					info.AddWarning(Res.GetString("FFB4A9E2-9818-4514-A0BF-0C23EB851F54", "The reason 'ADD - Addition' should only be applied prior to the bill’s registration. However, this bill has already been registered."));
				}
			}
		}

		public void ValidateMessageType()
		{
			ValidateCalculatedProperty(parent.MessageTypeInfo);
		}

		public void ValidateAction()
		{
			ValidateCalculatedProperty(parent.ActionInfo);
		}

		public void ValidateShouldSend()
		{
			ValidateCalculatedProperty(parent.ShouldSendInfo);
		}

		protected void CheckMessageType()
		{
			ListValidation.MessageErrorIfInvalidCode(parent.MessageTypeInfo);
		}

		protected void CheckAction()
		{
			ListValidation.MessageErrorIfInvalidCode(parent.ActionInfo);
			if (JPProcedureCodeList.Codes.NVC01.Equals(parent.Parent?.CurrentProcedureCode) && string.IsNullOrEmpty(parent.Action) && parent.ShouldSend)
			{
				parent.ActionInfo.AddError(Res.GetString("D8156661-9E14-42F9-BAEE-F2B010F1D779", "Please enter a Message Action."));
			}

			var customsStatus = parent.Bill.ABL_BillStatus;
			if ((parent.Parent?.IsHDF01 ?? false) && parent.Action != HDF01MessageActionList.Codes.X &&
					(customsStatus == JPCustomsStatusList.Codes.AWC || customsStatus == JPCustomsStatusList.Codes.AWD || customsStatus == JPCustomsStatusList.Codes.AWR))
			{
				parent.ActionInfo.AddMessageError(Res.GetString("1F02E54E-0696-4B4B-BA21-8064E810B266", "This bill is currently awaiting a response from NACCS."));
			}
		}

		protected void CheckShouldSend()
		{
			var billStatus = parent.Bill.ABL_BillStatus;
			var info = parent.ShouldSendInfo;

			if (parent.ShouldSend)
			{
				if (billStatus == JPCustomsStatusList.Codes.AWA || billStatus == JPCustomsStatusList.Codes.AWR || billStatus == JPCustomsStatusList.Codes.AWD)
				{
					info.AddMessageError(Res.GetString("E4483E8D-8644-4BB4-A46B-4D27521B0DEF", "You've already sent a message to NACCS and are currently waiting for a response."));
				}

				var sendingObjectParent = parent.Parent;
				if (sendingObjectParent != null)
				{
					switch (sendingObjectParent.CurrentProcedureCode)
					{
						case JPProcedureCodeList.Codes.NVC01:
							AddNVC01Notifications(billStatus, info);
							break;
						case JPProcedureCodeList.Codes.HDE:
							AddHDENotifications(billStatus, info);
							break;
					}

					var shouldCheckMaxLengthForHCH = false;
					var shouldSendCount = sendingObjectParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().Count(x => x.ShouldSend);

					if (parent.Parent.IsHCH01)
					{
						if (sendingObjectParent.EndSendMessage)
						{
							if (shouldSendCount > MaxNumberOfBillsForHCH01End && sendingObjectParent.SendingObjectsCollection.Skip(MaxNumberOfBillsForHCH01End).Contains(parent))
							{
								info.AddError(Res.GetString("D34350D5-0150-438E-B4FC-C5EF855F3BCC", "A maximum of 19 HAWBs can be included in the message when it is marked as END."));
							}
						}
						else
						{
							shouldCheckMaxLengthForHCH = true;
						}
					}

					shouldCheckMaxLengthForHCH = shouldCheckMaxLengthForHCH || sendingObjectParent.IsCHA;
					if (shouldCheckMaxLengthForHCH && shouldSendCount > MaxNumberOfBillsForHCH01 && sendingObjectParent.SendingObjectsCollection.Skip(MaxNumberOfBillsForHCH01).Contains(parent))
					{
						info.AddError(Res.GetString("4CC7A227-43FB-4190-B279-7F1A4264E066", "A maximum of 20 HAWBs can be included in the message."));
					}
				}
			}
			else if (billStatus != JPCustomsStatusList.Codes.ACK)
			{
				info.AddMessageError(Res.GetString("153F9419-F3B1-4701-A753-ED75DCA7B3A6", "When the END checkbox is ticked, all the Send? checkboxes where not ACK should be ticked."));
			}
		}

		void AddNVC01Notifications(ZString billStatus, ZPropertyInfo targetInfo)
		{
			switch (parent.Action)
			{
				case NVC01MessageActionList.Codes.Nine:
					if (billStatus == JPCustomsStatusList.Codes.REG || billStatus == JPCustomsStatusList.Codes.AMD)
					{
						targetInfo.AddWarning(Res.GetString("DD6E1B01-0F0D-48DF-BD5C-9A2518775078", "The Customs Status of selected bill is 'REG' or 'AMD', do you still want to send this message?"));
					}
					break;
				case NVC01MessageActionList.Codes.Five:
				case NVC01MessageActionList.Codes.One:
					if (billStatus == JPCustomsStatusList.Codes.DEL)
					{
						targetInfo.AddWarning(Res.GetString("70444DFC-16D3-4619-A9C5-25144ABDA68A", "The Customs Status of selected bill is 'DEL', do you still want to send this message?"));
					}
					if (string.IsNullOrWhiteSpace(billStatus))
					{
						targetInfo.AddWarning(Res.GetString("032714C5-F1B7-469C-B28C-90D878795B2E", "The selected bill does not contain Customs Status, do you still you want to send this message?"));
					}
					break;
			}
		}

		void AddHDENotifications(ZString billStatus, ZPropertyInfo targetInfo)
		{
			if (billStatus == JPMasterBillStatusList.Codes.END)
			{
				targetInfo.AddMessageError(Res.GetString("D680BB75-A2F1-43E7-97E3-300E8E658DF5", "Master Bill {0} has already been registered as completed (END – All House Bills Sent).", parent.Header.AMA_MasterBill));
			}
		}

		const int MaxNumberOfBillsForHCH01 = 20;

		const int MaxNumberOfBillsForHCH01End = 19;
	}
}
