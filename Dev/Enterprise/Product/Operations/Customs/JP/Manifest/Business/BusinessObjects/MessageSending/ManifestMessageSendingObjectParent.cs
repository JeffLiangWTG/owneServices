using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class ManifestMessageSendingObjectParent : BaseMessageSendingObjectParent<ManifestMessageSendingObject>, IMessageVisualObjectParentProvider
	{
		public ManifestMessageSendingObjectParent(AsycudaManifestHeader header) : base(header.Factory)
		{
			this.header = header;
			Context = header.MessageSendingContext ?? new MessageSendingContext();

			header.RunPreSaveValidation();
			UseVisualData = false;

			UpdateAllowSendWithErrorReadOnly();
			BizObjValidationMessageErrorsInfo.ValueChanged += BizObjValidationMessageErrors_ValueChanged;

			DefaultValuesAfterInitializeSendingObjectsCollection();
		}

		public readonly AsycudaManifestHeader header;

		public void DefaultValuesAfterInitializeSendingObjectsCollection()
		{
			foreach (ManifestMessageSendingObject sendingObject in SendingObjectsCollection)
			{
				sendingObject.ShouldSend = !IsSendingNVC01BondedLocationAmendment && sendingObject.Bill.ABL_BillStatus == ZString.Empty;
			}
		}

		#region CurrentProcedureCode & MessageTypes

		public ZString CurrentProcedureCode => Context.ProcedureCode;

		public bool NeedsSorting => IsHDF01 || (IsSendingNVC01Message && !IsSendingNVC01BondedLocationAmendment);

		public bool IsHDF01 => CurrentProcedureCode == JPProcedureCodeList.Codes.HDF01;

		public bool IsHCH01 => CurrentProcedureCode == JPProcedureCodeList.Codes.HCH01;

		public bool IsCHA => CurrentProcedureCode == JPProcedureCodeList.Codes.CHA;

		public bool IsSendingNVC01Message => CurrentProcedureCode == JPProcedureCodeList.Codes.NVC01;

		public bool IsSendingNVC01BondedLocationAmendment => header.IsNVC01BondedLocationAmendmentSendingInProgress;

		#endregion

		public ZBool AllowExportMessage => (SelectedSendingObjects.Any() || IsSendingNVC01BondedLocationAmendment) && (BizObjValidationMessageErrorsInfo.Value.IsEmpty || AllowSendWithError) && !ExportPath.IsEmpty;

		public ZBool AllowSendMessage => (SelectedSendingObjects.Any() || IsSendingNVC01BondedLocationAmendment) && (BizObjValidationMessageErrorsInfo.Value.IsEmpty || AllowSendWithError) && !JPRegistry.Instance.IsMailboxAndRemoteWebPrintClientCredentialsEmpty && JPNACCSMailboxCredentialChecker.HasSetupNACCSMailbox(header.Branch?.Company);

		public IEnumerable<INotification> WarningsMessage => ManifestMessageSendingNotificationCollector.GetWarnings();

		#region DummySendObject

		public ManifestMessageSendingObject DummySendingObjectForNVC01BondedLocationAmendment => DummySendObjectProvider.DummySendingObjectForNVC01BondedLocationAmendment;

		public DummySendObjectProvider DummySendObjectProvider => dummySendObjectProvider ??= new DummySendObjectProvider(this);
		DummySendObjectProvider dummySendObjectProvider;

		#endregion

		#region Export Path

		[ResourceStringData("Enterprise.Customs.JP.Manifest.Business.ManifestMessageSendingObjectParent|ExportPath", Caption = "Export To", FullDescription = "The folder the message will be exported to. This can be set in Registry -> Customs -> Country or Region Specific -> Japan -> NACCS Messaging -> Default Folder for Exporting Messages")]
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

		#endregion

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

		#endregion

		#region EndSendMessage

		public ZBool EndSendMessage
		{
			get => endSendMessage;
			set
			{
				SetNonPersistentPropertyValue(EndSendMessageInfo, ref endSendMessage, value);
				header.MessageSendingContext.EndSendMessage = endSendMessage;
				Validation.ValidateEndSendMessage();
				SendingObjectsCollection.Cast<ManifestMessageSendingObject>().ForEach(x => x.Validation.ValidateShouldSend());
			}
		}

		ZBool endSendMessage;

		public ZPropertyInfo EndSendMessageInfo => GetZPropertyInfo(nameof(EndSendMessage));

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

		public ManifestMessageSendingNotificationCollector ManifestMessageSendingNotificationCollector => new ManifestMessageSendingNotificationCollector(this, SelectedSendingObjects.Cast<ManifestMessageSendingObject>().Select(x => x.Bill), true);

		#region IMessageVisualObjectParentProvider

		public bool UseVisualData { get; set; }

		public IMessageSendingContext Context { get; }

		public MessageVisualObjectParent VisualObjectParent => visualObjectParent ??= new MessageVisualObjectParent(Factory);
		MessageVisualObjectParent visualObjectParent;

		public IEnumerable<IMessageContentProvider> GetContentProviders()
		{
			if (IsSendingNVC01BondedLocationAmendment)
			{
				return [new ManifestMessageContentProvider(Factory, [DummySendingObjectForNVC01BondedLocationAmendment])];
			}

			var maxBillNumberForOneMessage = CurrentProcedureCode.ToString() switch
			{
				JPProcedureCodeList.Codes.HCH01 => Constants.Message.MaxNumberOfBillsForHCH01,
				JPProcedureCodeList.Codes.CHA => Constants.Message.MaxNumberOfBillsForHCH01,
				JPProcedureCodeList.Codes.HDF01 => Constants.Message.MaxNumberOfBillsForHDF01,
				JPProcedureCodeList.Codes.NVC01 => Constants.Message.MaxNumberOfBillsForNVC01,
				JPProcedureCodeList.Codes.HDE => Constants.Message.MaxNumberOfBillsForHDE,
				_ => throw new DeveloperNotificationException($"Invalid Procedure Code: {CurrentProcedureCode}"),
			};

			var selectedSendingObjects = SelectedSendingObjects.Cast<ManifestMessageSendingObject>();
			if (IsSendingNVC01Message)
			{
				var res = new List<ManifestMessageContentProvider>();
				var groupedSendingObjectsCollection = selectedSendingObjects.GroupBy(x => x.Action);
				foreach (var groupedSendingObjects in groupedSendingObjectsCollection)
				{
					var batchedSendingObjectsCollection = groupedSendingObjects.Batch(maxBillNumberForOneMessage);
					res.AddRange(batchedSendingObjectsCollection.Select(x => new ManifestMessageContentProvider(Factory, x)));
				}
				return res;
			}

			if (IsHCH01 && EndSendMessage)
			{
				selectedSendingObjects = selectedSendingObjects.Union([DummySendObjectProvider.DummySendingObjectForHCH01End]);
			}

			return selectedSendingObjects
				.Cast<ManifestMessageSendingObject>()
				.Batch(maxBillNumberForOneMessage)
				.Select(x => new ManifestMessageContentProvider(Factory, x));
		}

		#endregion

		#region Overrides & New

		public override BusinessObject TopLevelBusinessObject => header;

		public override bool AllowEmptyDeclaration => IsSendingNVC01Message;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			exportPath = JPRegistry.Instance.DefaultFolderForExportingMessages.Value;
		}

		protected override NonPersistentBusinessObjectCollection<ManifestMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			var result = new ManifestMessageSendingObjectCollection(header.Factory);
			if (CurrentProcedureCode == JPProcedureCodeList.Codes.HDE)
			{
				result.Add(new ManifestMessageSendingObject(header.MasterBill, this));
			}
			else
			{
				foreach (var bill in header.Bills)
				{
					result.Add(new ManifestMessageSendingObject(bill, this));
				}
			}

			return result;
		}

		protected override ZString GetBizObjValidationMessageErrors() => (SelectedSendingObjects.Any() || IsSendingNVC01BondedLocationAmendment) ? Regex.Replace(MessageSendingValidation.CheckBusinessObjectLevelValidation().NotificationsAsString(), "(?<!\r)\n", "\r\n") : string.Empty;

		protected override ZString GetAdditionalWarningsCore() => GetNotificationsMessage(WarningsMessage.ToUniqueMessageListString);

		ZString GetNotificationsMessage(Func<string> getNotificationsMessage) => (SelectedSendingObjects.Any() || IsSendingNVC01BondedLocationAmendment) ? Regex.Replace(getNotificationsMessage(), "(?<!\r)\n", "\r\n") : string.Empty;

		public override Security.SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.GlobalManifestSendWithMessageErrors;

		public ManifestMessageSendingObjectParentValidation Validation => new(this);

		protected override MessageSendingValidation GetNewMessageSendingValidation() => MessageSendingValidation.New(TopLevelBusinessObject, GetNewMessageErrorCollector(), SecurityCheckpointToSendWithMessageError ?? Env.Security.CustomsDeclarationSendWithMessageErrors, false);

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		protected override IEnumerable<INotification> GetNewMessageErrorCollector() => ManifestMessageSendingNotificationCollector.GetMessageErrors();

		#endregion
	}
}
