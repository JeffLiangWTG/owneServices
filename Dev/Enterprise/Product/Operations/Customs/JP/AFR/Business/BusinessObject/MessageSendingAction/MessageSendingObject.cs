using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class MessageSendingObject : AutoMessageSendingObject
	{
		public MessageSendingObject(JPAFRBills bill, ActionCode actionCode, MessageSendingAction sendingAction)
			: base(bill.Factory)
		{
			this.Bill = bill;
			this.sendingAction = Argument.NotNull(sendingAction, "MessageSendingAction");
			using (((ISingleElementListInternal)this).SuspendListChanged())
			{
				base.AddToFactoryCache();
			}
			UpdateAction(actionCode);
		}
		MessageSendingAction sendingAction;

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory, MessageSendingAction sendingAction)
				: base(factory)
			{
				this.sendingAction = Argument.NotNull(sendingAction, nameof(sendingAction));
			}

			readonly MessageSendingAction sendingAction;

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(MessageSendingObject);
			}

			public MessageSendingObject LoadOrNew(JPAFRBills bill, ActionCode actionCode)
			{
				MessageSendingObject obj = null;

				if (actionCode != ActionCode.Registering || !bill.IsBillAlreadyRegistered)
				{
					obj = Factory.Load<MessageSendingObject>(bill.PK);
					if (obj == null)
					{
						obj = new MessageSendingObject(bill, actionCode, sendingAction);
					}
					else
					{
						obj.sendingAction = sendingAction;
					}
					obj.UpdateAction(actionCode);
				}
				return obj;
			}
		}

		public void UpdateAction(ActionCode actionCode, bool defaultToSend = true, bool defaultToDefaultAction = true)
		{
			var oldActionCode = this.ActionCode;
			this.ActionCode = actionCode;
			JPM_BillOfLadingNumber = Bill.JPB_BillNumber;
			JPM_ReleaseStatus = Bill.JPB_ReleaseStatus;
			JPM_MessageStatus = Bill.JPB_MessageStatus;

			JPM_Calc_ESDT = Bill.JPB_Calc_ESDT;
			JPM_Calc_EFDT = Bill.JPB_Calc_EFDT;
			ShouldValidateDate = null;

			if (oldActionCode != actionCode || defaultToDefaultAction)
			{
				JPM_ActionCode = GetDefaultActionCode(JPM_ReleaseStatus, JPM_ActionCode);
			}
			var shouldSend = defaultToSend || JPM_Send;
			JPM_Send = shouldSend;
			if (shouldSend)
			{
				RegisterBillAsEditableChildObject();
			}
		}

		public override SchemaGuidColumn PKSchemaColumn
		{
			get { return JPAFRBillsSchema.PK; }
		}

		public void UnRegisterBillAsEditableChildObject()
		{
			UnRegisterEditableChildObject(Bill);
		}

		public override ZBool JPM_Send
		{
			get { return base.JPM_Send; }
			set
			{
				var oldValue = JPM_Send;
				base.JPM_Send = value;
				JPM_SendInfo.RefreshBinding();
				if (!IsCopying && oldValue != JPM_Send)
				{
					if (JPM_Send)
					{
						RegisterBillAsEditableChildObject();
					}
					else
					{
						UnRegisterBillAsEditableChildObject();
					}
					RunBillPreSaveValidation();
				}
			}
		}

		protected override bool JPM_Send_ReadOnly
		{
			get { return ActionCodeTool.IsForceSendingInGrid(ActionCode); }
		}

		[List(nameof(ActionCodeList))]
		public override ZString JPM_ActionCode
		{
			get { return base.JPM_ActionCode; }
			set
			{
				base.JPM_ActionCode = value;
				if (JPM_ActionCode != AFRSendingActionCodeList.Codes.Delete)
				{
					JPM_DeleteReasonCode = ZString.Empty;
					JPM_DeleteReasonText = ZString.Empty;
				}
				JPM_ActionCodeInfo.RefreshBinding();
			}
		}

		protected override bool JPM_ActionCode_ReadOnly
		{
			get { return ActionCodeTool.IsLimitingActionInGrid(ActionCode); }
		}

		internal bool HasATDBeenSent
		{
			get { return sendingAction != null && sendingAction.HasATDBeenSent; }
		}

		#region Release Status

		protected override ZString GetJPM_ReleaseStatusDescription()
		{
			return Factory.GetCachedValue<AFRBillCustomsStatusList>().GetDescriptionFromCode(JPM_ReleaseStatus) ?? ZString.Empty;
		}

		#endregion

		#region Message Status

		protected override ZString GetJPM_MessageStatusDescription()
		{
			return Factory.GetCachedValue<MessageStatusList>().GetDescriptionFromCode(JPM_MessageStatus);
		}

		#endregion

		[List(nameof(DeleteReasonCodeList))]
		public override ZString JPM_DeleteReasonCode
		{
			get { return base.JPM_DeleteReasonCode; }
			set
			{
				base.JPM_DeleteReasonCode = value;

				JPM_DeleteReasonText = IsDeleteReasonCodeFreeTextRequired ? string.Empty : DeleteReasonCodeList.GetDescriptionFromCode(JPM_DeleteReasonCode);
				JPM_DeleteReasonCodeInfo.RefreshBinding();
			}
		}

		protected override bool JPM_DeleteReasonCode_ReadOnly => !IsBillSendAndDelete;

		public override ZString JPM_DeleteReasonText
		{
			get { return base.JPM_DeleteReasonText; }
			set
			{
				base.JPM_DeleteReasonText = value;
				JPM_DeleteReasonTextInfo.RefreshBinding();
			}
		}

		protected override bool JPM_DeleteReasonText_ReadOnly => !(IsBillSendAndDelete && IsDeleteReasonCodeFreeTextRequired);

		IEnumerable<ZZRefCusCodeListCombined> DeleteReasons
		{
			get
			{
				return Factory.GetCachedValue("JapanAFRDeleteReasons", () => ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanAFRDeleteReason, ZDateTime.Today));
			}
		}

		public CodeDescriptionPairList DeleteReasonCodeList
		{
			get
			{
				return Factory.GetCachedValue("JapanAFRDeleteReasonCodeList", () =>
				{
					var result = new CodeDescriptionPairList();
					foreach (var deleteReason in DeleteReasons)
					{
						result.AddPair(deleteReason.ZZD_Code, deleteReason.ZZD_Description);
					}
					return result;
				});
			}
		}

		public ZZRefCusCodeListCombined DeleteReasonCode => DeleteReasons.SingleOrDefault(x => x.ZZD_Code == JPM_DeleteReasonCode);

		public ZBool IsDeleteReasonCodeFreeTextRequired => DeleteReasonCode?.HasAttribute(RefCusCodeListAttributeTypes.Codes.FreeTextRequired) ?? ZBool.False;

		public ZBool IsBillSendAndDelete => JPM_Send && JPM_ActionCode == AFRSendingActionCodeList.Codes.Delete;

		public CodeDescriptionPairList ActionCodeList
		{
			get { return AFRSendingActionCodeList.GetActionCodeList(Factory, ActionCode); }
		}

		public bool IsRegisteringActionAndBillAlreadyOnFile
		{
			get { return ActionCode == ActionCode.Registering && IsBillAlreadyRegistered; }
		}

		public bool IsBillAlreadyRegistered
		{
			get { return Bill.IsBillAlreadyRegistered; }
		}

		public bool IsRiskAssessmentReceivedForBill
		{
			get { return Bill.IsRiskAssessmentReceivedForBill; }
		}

		public bool IsMasterBillMarkedAsCompleted
		{
			get { return sendingAction.IsBillRegistrationCompleted; }
		}

		#region Implementation
		internal delegate bool ShouldValidateDateDelegate();
		internal ShouldValidateDateDelegate ShouldValidateDate;

		protected override void AddToFactoryCache()
		{
			// should be called after MoveDetail is set
		}

		protected override ZGuid GetPK()
		{
			return Bill.PK;
		}

		void RegisterBillAsEditableChildObject()
		{
			var bill = Bill;
			if (bill != null)
			{
				RegisterEditableChildObject(bill);
			}
		}

		void RunBillPreSaveValidation()
		{
			var bill = Bill;
			if (bill != null)
			{
				bill.RunPreSaveValidation();
			}
		}

		public ActionCode ActionCode
		{
			get;
			private set;
		}
		public readonly JPAFRBills Bill;

		ZString GetDefaultActionCode(ZString customStatus, ZString actionCode)
		{
			var result = ZString.Empty;
			switch (ActionCode)
			{
				case ActionCode.NewBill:
					result = HasATDBeenSent || sendingAction.IsBillRegistrationCompleted ? AFRSendingActionCodeList.Codes.Add : AFRSendingActionCodeList.Codes.Register;
					break;
				case ActionCode.AmendingDelete:
					result = AFRSendingActionCodeList.Codes.Delete;
					break;
				case ActionCode.AmendingAdd:
				case ActionCode.AmendingUpdate:
					result = AFRBillCustomsStatusList.IsRegisteredType(customStatus) ? AFRSendingActionCodeList.Codes.Update : AFRSendingActionCodeList.Codes.Add;
					break;
				case ActionCode.CorrectMasterInformation:
					result = (actionCode != AFRSendingActionCodeList.Codes.Update && actionCode != AFRSendingActionCodeList.Codes.Delete) ? AFRSendingActionCodeList.Codes.Update : actionCode.ToString();
					break;
				case ActionCode.CorrectVesselInformationByRegistration:
					result = AFRSendingActionCodeList.Codes.Register;
					break;
				case ActionCode.CorrectVesselInformationByAmendment:
					result = AFRSendingActionCodeList.Codes.Add;
					break;
				case ActionCode.ReRegisterMasterAfterATD:
				case ActionCode.ReRegisterMasterBeforeATD:
					result = AFRSendingActionCodeList.Codes.Register;
					break;
				case ActionCode.ChangeDepartureTimeAfterATD:
					result = AFRSendingActionCodeList.Codes.Update;
					break;
			}
			return result;
		}

		#endregion
	}
}
