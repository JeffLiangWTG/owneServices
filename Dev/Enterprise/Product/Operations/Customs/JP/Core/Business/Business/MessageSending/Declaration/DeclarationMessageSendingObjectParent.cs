using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.Environment;
using NotificationType = CargoWise.EntityFramework.NotificationType;

namespace Enterprise.Customs.JP.Business
{
	public class DeclarationMessageSendingObjectParent : JobDeclarationMessageSendingObjectParent<MessageSendingObject>, IMessageVisualObjectParentProvider
	{
		public DeclarationMessageSendingObjectParent(JobDeclaration declaration)
			: base(declaration)
		{
			Context = declaration.MessageSendingContext ?? new MessageSendingContext();

			using (declaration.UnRegisterEditableChildObjectsForMessageValidation())
			{
				declaration.LoadChildEditableObjects();
				declaration.MarkAsNeedingValidationIncludingChildren();
				declaration.RunPreSaveValidation();
			}

			UpdateAllowSendWithErrorReadOnly();

			BizObjValidationMessageErrorsInfo.ValueChanged += BizObjValidationMessageErrors_ValueChanged;
		}

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		ICodeDescriptionPairList AllProcedureCodeList { get; } = new JPProcedureCodeList();

		protected override JobDeclarationMessageSendingObject CreateNewJobDeclarationMessageSendingObject(Customs.Business.CusEntryHeader header)
		{
			var result = new MessageSendingObject((CusEntryHeader)header);

			if (AllProcedureCodeList.ContainsCode(Context.ProcedureCode))
			{
				result.ProcedureCode = Context.ProcedureCode;
			}

			if (Context.Action == ActionList.Codes.One)
			{
				result.Action = Context.Action;
				result.Action_ReadOnly = true;
				result.ShouldSend = true;
				InitializeVisualObjects(result);
			}

			return result;
		}

		void InitializeVisualObjects(MessageSendingObject sendingObj)
		{
			VisualObjectParent.InitializeVisualObjects(new[] { new MessageContentProvider(Factory, sendingObj) });
		}

		protected override IEnumerable<Customs.Business.CusEntryHeader> GetEntryHeadersToBeSent()
		{
			if (Context.ProcedureCode == JPProcedureCodeList.Codes.ECR && Context.Action == ActionList.Codes.One && Context.EntryHeadersToBeSent != null)
			{
				return Context.EntryHeadersToBeSent;
			}

			return base.GetEntryHeadersToBeSent();
		}

		protected override ZString GetAdditionalWarningsCore() => GetNotificationsMessage(WarningsMessage.ToUniqueMessageListString);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			exportPath = JPRegistry.Instance.DefaultFolderForExportingMessages.Value;
		}

		ZString GetNotificationsMessage(Func<string> getNotificationsMessage)
		{
			var result = string.Empty;
			var selectedSendingObjects = SelectedSendingObjects.Cast<MessageSendingObject>();

			if (selectedSendingObjects.Any())
			{
				if (selectedSendingObjects.Any(c =>
						(c.ProcedureCode == JPProcedureCodeList.Codes.IDC || c.ProcedureCode == JPProcedureCodeList.Codes.EDC || c.ProcedureCode == JPProcedureCodeList.Codes.IDE || c.ProcedureCode == JPProcedureCodeList.Codes.EDE)
						&& !(c.Header?.EntryInstruction?.CEI_DateForDuty.IsToday ?? false)))
				{
					result += Res.GetString("CCB2A8E5-D6EC-4788-9973-FB7D4AEA4F1D", "Scheduled Declaration Date is different from the current date.");
					result += "\r\n";
				}
				result += getNotificationsMessage();
				result = Regex.Replace(result, "(?<!\r)\n", "\r\n");
			}
			return result;
		}

		protected override void HookMessageSendingObjectEvents(BaseMessageSendingObject bo)
		{
			base.HookMessageSendingObjectEvents(bo);
			if (bo is MessageSendingObject sendingObject)
			{
				sendingObject.ProcedureCodeInfo.ValueChanged += (s, e) => ResetValidationMessages();
			}
		}

		IEnumerable<INotification> WarningsMessage => CollectNotifications(NotificationType.Warning);

		#region AllowSendWithError

		[ReadOnlyMember(nameof(AllowSendWithErrorReadOnly))]
		public ZBool AllowSendWithError
		{
			get => allowSendWithError;
			set => SetNonPersistentPropertyValue(AllowSendWithErrorInfo, ref allowSendWithError, value);
		}

		ZBool allowSendWithError;

		public ZPropertyInfo AllowSendWithErrorInfo => GetZPropertyInfo(nameof(AllowSendWithError));

		ZBool AllowSendWithErrorReadOnly => string.IsNullOrWhiteSpace(BizObjValidationMessageErrors);

		public ZBool AllowSendMessage => SelectedSendingObjects.Any() && (BizObjValidationMessageErrorsInfo.Value.IsEmpty || AllowSendWithError) && ParentDeclaration.IsReadyForSending;

		#endregion

		#region Export Path

		[ResourceStringData("Enterprise.Customs.JP.Business.DeclarationMessageSendingObjectParent|ExportPath", Caption = "Export To", FullDescription = "The folder the message will be exported to. This can be set in Registry -> Customs -> Country or Region Specific -> Japan -> NACCS Messaging -> Default Folder for Exporting Messages")]
		public ZString ExportPath
		{
			get => exportPath;
			set
			{
				if (exportPath != value)
				{
					SetNonPersistentPropertyValue(ExportPathInfo, ref exportPath, value);
					Validation.ValidateExportPath();
				}
			}
		}

		ZString exportPath;

		public ZPropertyInfo ExportPathInfo => GetZPropertyInfo(nameof(ExportPath));

		public ZBool AllowExportMessage => SelectedSendingObjects.Any() && (BizObjValidationMessageErrorsInfo.Value.IsEmpty || AllowSendWithError) && !ExportPath.IsEmpty;

		#endregion

		#region Validation

		protected override MessageSendingValidation GetNewMessageSendingValidation() => MessageSendingValidation.New(TopLevelBusinessObject, GetNewMessageErrorCollector(), SecurityCheckpointToSendWithMessageError ?? Env.Security.CustomsDeclarationSendWithMessageErrors, false);

		protected override IEnumerable<INotification> GetNewMessageErrorCollector() => CollectNotifications(NotificationType.MessageError);

		public DeclarationMessageSendingObjectParentValidation Validation => new(this);

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		public IEnumerable<INotification> CollectNotifications(INotificationType notificationType)
		{
			var result = Enumerable.Empty<INotification>();
			var selectedSendingObjects = SelectedSendingObjects.Cast<MessageSendingObject>();

			switch (Context.ProcedureCode)
			{
				case JPProcedureCodeList.Codes.ECR:
					result = new ECRMessageSendingNotificationCollector(this, selectedSendingObjects.Select(x => x.Header), true).GetNotifications(notificationType);
					break;
				default:
					result = new JobDeclarationMessageSendingNotificationCollector(ParentDeclaration, selectedSendingObjects.Select(x => x.Header)).GetNotifications(notificationType);
					break;
			}
			return result;
		}

		#endregion

		void BizObjValidationMessageErrors_ValueChanged(object sender, EventArgs e)
		{
			UpdateAllowSendWithErrorReadOnly();
		}

		void UpdateAllowSendWithErrorReadOnly()
		{
			if (BizObjValidationMessageErrors.IsEmpty)
			{
				AllowSendWithError = false;
			}
		}

		#region IMessageVisualObjectParentProvider

		public bool UseVisualData { get; set; }

		public IMessageSendingContext Context { get; }

		public MessageVisualObjectParent VisualObjectParent => visualObjectParent ??= new MessageVisualObjectParent(Factory);
		MessageVisualObjectParent visualObjectParent;

		public IEnumerable<IMessageContentProvider> GetContentProviders()
		{
			return SelectedSendingObjects
				.Cast<MessageSendingObject>()
				.Select(x => new MessageContentProvider(Factory, x));
		}

		#endregion
	}
}
