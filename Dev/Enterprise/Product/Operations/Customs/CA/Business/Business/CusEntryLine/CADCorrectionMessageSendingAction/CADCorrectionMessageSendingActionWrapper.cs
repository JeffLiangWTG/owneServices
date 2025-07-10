using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.FetchStrategies;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.CA.Business
{
	public class CADCorrectionMessageSendingActionWrapper : NonPersistentBusinessObject, IObsoleteValidation, ICusSupportingInfoTypeSupporter
	{
		#region Schema

		public static class Schema
		{
			public const string SendMessage = "SendMessage";
			public const string SaveWithoutSendMessage = "SaveWithoutSendMessage";
		}

		#endregion

		public CADCorrectionMessageSendingActionWrapper(CusEntryHeader header)
			: base(header.Factory)
		{
			this.CADEntryHeader = header;
		}

		public CusEntryHeader CADEntryHeader { get; }

		#region Properties
		public ZBool ForceSend { get; set; } = false;
		public ZBool OKClicked { get; set; }
		public ZBool CancelClicked { get; set; }

		public ZBool SendMessage
		{
			get { return sendMessage; }
			set
			{
				if (value != SendMessage)
				{
					SetNonPersistentPropertyValue(SendMessageInfo, ref sendMessage, value);
				}

				if (!IsValidationSuspended)
				{
					ValidateSendMessage();
					ValidateSaveWithoutSendMessage();
				}
				this.SendMessageInfo.RefreshBinding();
				this.SaveWithoutSendMessageInfo.RefreshBinding();
			}
		}
		ZBool sendMessage;

		public ZPropertyInfo SendMessageInfo
		{
			get { return GetZPropertyInfo(Schema.SendMessage); }
		}

		public ZBool SaveWithoutSendMessage
		{
			get { return saveWithoutSendMessage; }
			set
			{
				if (value != SaveWithoutSendMessage)
				{
					SendingActions.SetReadOnlyIncludingChildren(value);
					SetNonPersistentPropertyValue(SaveWithoutSendMessageInfo, ref saveWithoutSendMessage, value);
					if (value)
					{
						SendingActions.RemoveAndDeleteAll();
					}
				}

				if (!IsValidationSuspended)
				{
					ValidateSendMessage();
					ValidateSaveWithoutSendMessage();
				}
				this.SendMessageInfo.RefreshBinding();
				this.SaveWithoutSendMessageInfo.RefreshBinding();
			}
		}
		ZBool saveWithoutSendMessage;

		public ZPropertyInfo SaveWithoutSendMessageInfo
		{
			get { return GetZPropertyInfo(Schema.SaveWithoutSendMessage); }
		}

		#endregion

		[ChildEditable(true)]
		public CADCorrectionMessageSendingActionCollection SendingActions
		{
			get
			{
				if (sendingActions == null)
				{
					sendingActions = new CADCorrectionMessageSendingActionCollection(this);
					RegisterEditableChildObject(sendingActions);
				}

				return sendingActions;
			}
		}
		CADCorrectionMessageSendingActionCollection sendingActions;

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			return new Dictionary<ZString, Type>()
			{
				{ Common.CA.CusSupportingInfoTypeList.Codes.CadCorrectionMessageSendingAction, typeof(CADCorrectionMessageSendingAction) },
			};
		}

		public void ClearSendingActions()
		{
			if (SendingActions != null)
			{
				SendingActions.RemoveAndDeleteAll();
			}
		}

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateSendMessage();
			ValidateSaveWithoutSendMessage();
			ValidateSendingActionsCount();
			ValidateReasonAndAppealsProgramCodeCombination();
		}

		public void ValidateSendingActionsCount()
		{
			if (!IsValidationSuspended)
			{
				ClearRowNotifications();
				if (SendMessage && !SendingActions.Any())
				{
					AddRowError(ResString.GetMultilingualString("EC49089A-8BD9-40BA-9366-33D3D15E7ACC", "At least one Amendment Detail is required."));
				}
			}
		}

		public void ValidateSendMessage()
		{
			if (!IsValidationSuspended)
			{
				SendMessageInfo.ClearAllNotifications();
				if (!SendMessage & !SaveWithoutSendMessage)
				{
					SendMessageInfo.AddError(ResString.GetMultilingualString("A5195B29-FC42-4E7C-9CE1-AA103F39F205", "Please select an option before click 'OK'."));
				}
			}
		}

		public void ValidateSaveWithoutSendMessage()
		{
			if (!IsValidationSuspended)
			{
				SaveWithoutSendMessageInfo.ClearAllNotifications();
				if (!SendMessage && !SaveWithoutSendMessage)
				{
					SaveWithoutSendMessageInfo.AddError(ResString.GetMultilingualString("A5195B29-FC42-4E7C-9CE1-AA103F39F205", "Please select an option before click 'OK'."));
				}
			}
		}

		public void ValidateReasonAndAppealsProgramCodeCombination()
		{
			if (!IsValidationSuspended)
			{
				var reasonAndAppeals = SendingActions.Select(x => (x.CSI_Code, x.CSI_SubType)).ToArray().Distinct();
				if (reasonAndAppeals.Count() > 3)
				{
					SendMessageInfo.AddError(ResString.GetMultilingualString("55238EE7-14AA-4DF9-BA14-B365DCA1C445", "Only up to 3 unique reason code and appeals program can be submitted at a time."));
				}
			}
		}

		#endregion
	}
}
