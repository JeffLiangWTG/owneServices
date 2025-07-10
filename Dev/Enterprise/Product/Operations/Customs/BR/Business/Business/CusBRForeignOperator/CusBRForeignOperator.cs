using System;
using System.ComponentModel;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Customs.BR.MessageDefinitions.ProductCatalog;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.BR.Business
{
	[CodeProperty(Schema.ForeignOperatorCode), DescriptionProperty(Schema.ForeignOperatorName)]
	public class CusBRForeignOperator : AutoCusBRForeignOperator, IMessageAttachee, Integration.Customs.BR.ICusBRForeignOperator, IWorkflowProvider, IBackDoorSavingSupportableBizObj, ITemplateCopyable
	{
		public CusBRForeignOperator(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusBRForeignOperator.Schema
		{
			public const string ForeignOperatorCode = nameof(CusBRForeignOperator.ForeignOperatorCode);
			public const string ForeignOperatorName = nameof(CusBRForeignOperator.ForeignOperatorName);
			public const string ForeignOperatorCountry = nameof(CusBRForeignOperator.ForeignOperatorCountry);
		}

		protected override ZString HumanReadableNameCore => Res.GetString("40287D7F-DE0E-4F20-9AF9-303BBE72A771", "Foreign Operator");

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public bool IsMessageAwaitingResponse => BFR_MessageStatus == BRMessageStatusList.Codes.AwaitingResponse;

		ZGuid IMessageAttachee.BranchPK => GlbBranch.CurrentBranch.PK;

		[ResourceStringData("Enterprise.Customs.BR.Business.CusBRForeignOperator|BFR_OH_Owner", Caption = "Owner")]
		public override ZGuid BFR_OH_Owner { get => base.BFR_OH_Owner; set => base.BFR_OH_Owner = value; }

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(CusBRForeignOperatorLookups.MessageStatusList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusBRForeignOperator|BFR_MessageStatus", Caption = "Message Status")]
		public override ZString BFR_MessageStatus { get => base.BFR_MessageStatus; set => base.BFR_MessageStatus = value; }

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(CusBRForeignOperatorLookups.CustomsStatusTypeList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusBRForeignOperator|BFR_CustomsStatus", Caption = "Customs Status")]
		public override ZString BFR_CustomsStatus { get => base.BFR_CustomsStatus; set => base.BFR_CustomsStatus = value; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusBRForeignOperator|BFR_AuthorityIdentifier", Caption = "Authority Identifier")]
		public override ZString BFR_AuthorityIdentifier { get => base.BFR_AuthorityIdentifier; set => base.BFR_AuthorityIdentifier = value; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusBRForeignOperator|BFR_AuthorityVersion", Caption = "Version")]
		public override ZString BFR_AuthorityVersion { get => base.BFR_AuthorityVersion; set => base.BFR_AuthorityVersion = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.CusBRForeignOperator|BFR_OH_ForeignOperator", Caption = "Foreign Operator")]
		public override ZGuid BFR_OH_ForeignOperator { get => base.BFR_OH_ForeignOperator; set => base.BFR_OH_ForeignOperator = value; }

		public ZString ForeignOperatorCode => ForeignOperator?.OH_Code ?? ZString.Empty;

		[ResourceStringData("Enterprise.Customs.BR.Business.CusBRForeignOperator|ForeignOperatorName", Caption = "Name")]
		public ZString ForeignOperatorName => ForeignOperator?.OH_FullName ?? BFR_Name;

		[ResourceStringData("Enterprise.Customs.BR.Business.CusBRForeignOperator|ForeignOperatorCountry", Caption = "Country")]
		public ZString ForeignOperatorCountry
		{
			get
			{
				var country = GetForeignOperatorCountry();
				return country != null ? string.Join(" - ", country.Code, country.Description) : ZString.Empty;
			}
		}

		public ZString ForeignOperatorCountryCode => GetForeignOperatorCountry()?.Code ?? ZString.Empty;

		RefCountry GetForeignOperatorCountry() => ForeignOperator?.MainAddress?.Country ?? Country;

		[ResourceStringData("Enterprise.Customs.BR.Business.CusBRForeignOperator|ForeignOperatorTIN", Caption = "TIN")]
		public ZString ForeignOperatorTIN => ForeignOperator?.GetTinCode() ?? ForeignOperatorDTO?.tin;

		[ResourceStringData("Enterprise.Customs.BR.Business.CusBRForeignOperator|ForeignOperatorInternalCode", Caption = "Internal Code")]
		public ZString ForeignOperatorInternalCode => ForeignOperator?.GetInternalCode() ?? ForeignOperatorDTO?.codigoInterno;

		[ResourceStringData("Enterprise.Customs.BR.Business.CusBRForeignOperator|ForeignOperatorInternalEmail", Caption = "Email")]
		public ZString ForeignOperatorEmail => ForeignOperator?.GetForeignOperatorEmail() ?? ForeignOperatorDTO?.email;

		OperadorEstrangeiroIntegracaoDTO ForeignOperatorDTO
		{
			get
			{
				if (foreignOperatorDTO == null)
				{
					var query = new ZQuery();
					query.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.CAT);
					query.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.OperatorZipFile);
					query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, PK);
					query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodes.BRCustoms);
					query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
					query.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + " DESC";

					var lastMessage = Factory.LoadTop1<EDIMessage>(query);

					if (lastMessage != null)
					{
						foreignOperatorDTO = BRMessageHelper.DeserializeObject<OperadorEstrangeiroIntegracaoDTO>(lastMessage.EM_MessageText, throwExceptionIfOccurs: false);
					}
				}
				return foreignOperatorDTO;
			}
		}
		OperadorEstrangeiroIntegracaoDTO foreignOperatorDTO;

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public override void Delete()
		{
			((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			args.AddExcludedColumns(new[] { CusBRForeignOperator.Schema.BFR_MessageStatus });
			args.AddExcludedColumns(new[] { CusBRForeignOperator.Schema.BFR_CustomsStatus });
			args.AddExcludedColumns(new[] { CusBRForeignOperator.Schema.BFR_AuthorityIdentifier });
			args.AddExcludedColumns(new[] { CusBRForeignOperator.Schema.BFR_AuthorityVersion });
			return base.CloneInternal(args);
		}

		#region Update Message Status on Saving

		internal bool IsUpdateMessageStatusOnSavingSuspended => messageStatusSuspenderIndex > 0;
		int messageStatusSuspenderIndex;

		public IDisposable SuspendUpdateMessageStatusOnSaving() => new DisposableAction(() => messageStatusSuspenderIndex++, () => messageStatusSuspenderIndex--);

		public override void OnSaving()
		{
			base.OnSaving();
			if (!BFR_OH_ForeignOperator.IsEmpty)
			{
				BFR_Name = ZString.Empty;
				BFR_City = ZString.Empty;
				BFR_RN_NKCountryCode = ZString.Empty;
			}
			UpdateMessageStatusIfAnyChangesAffectMessage();
		}

		void UpdateMessageStatusIfAnyChangesAffectMessage()
		{
			if (!IsUpdateMessageStatusOnSavingSuspended && IsInDatabase
				&& (BFR_MessageStatus == BRMessageStatusList.Codes.Accepted || BFR_MessageStatus == BRMessageStatusList.Codes.Rejected)
				&& !BFR_MessageStatusInfo.HasChanges)
			{
				var foreignOperatorInDatabase = new BusinessObjectFactory().Load<CusBRForeignOperator>(PK);
				if (foreignOperatorInDatabase != null && foreignOperatorInDatabase.ForeignOperator != null)
				{
					var messageForForeignOperatorInDB = new ForeignOperatorMessageSendingObject(foreignOperatorInDatabase).GetMessageText();
					var messageForForeignOperator = new ForeignOperatorMessageSendingObject(this).GetMessageText();
					if (messageForForeignOperatorInDB != messageForForeignOperator)
					{
						BFR_MessageStatus = BRMessageStatusList.Codes.NotSent;
					}
				}
			}
		}

		public void SuspendUpdateMessageStatusOnSavingUntilSaved()
		{
			var disposableMessageStatus = SuspendUpdateMessageStatusOnSaving();

			Factory.Saved += OnFactorySaved;

			void OnFactorySaved(BusinessObjectFactory factory, bool savedSuccessfully)
			{
				factory.Saved -= OnFactorySaved;
				disposableMessageStatus?.Dispose();
				disposableMessageStatus = null;
			}
		}

		#endregion

		#region EDIMessageCollection

		[ChildEditable]
		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(this, Factory);
					fMessages.Load();
					fMessages.IsManagedForDataRefresh = true;
					fMessages.SetReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(fMessages);
				}
				return fMessages;
			}
		}

		EDIMessageCollection fMessages;

		#endregion

		#region IWorkflowProvider

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}
				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(GetNewCusBRForeignOperatorTaskCollection);
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		ProcessTaskCollection GetNewCusBRForeignOperatorTaskCollection() => new ProcessTaskCollection<CusBRForeignOperatorProcessTask, CusBRForeignOperator>(this);

		ZString IWorkflowProviderCore.WorkflowType => WorkflowDescriptors.ForeignOperatorWorkflowDescriptorCode;

		public IWorkflowInformationProvider GetWorkflowInformationProvider() => null;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria() => GetTemplateSelectionCriteria();

		public IColumnValueRanker GetTemplateSelectionCriteria() => new ColumnValueRanker();

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CusBRForeignOperatorFetchStrategy(this);
		}

		class CusBRForeignOperatorFetchStrategy : EnterpriseBusinessObjectFetchStrategy
		{
			public CusBRForeignOperatorFetchStrategy(CusBRForeignOperator statementHeader)
				: base(statementHeader)
			{
			}

			CusBRForeignOperator ForeignOperator => (CusBRForeignOperator)BusinessObject;

			protected override void FetchForViewCore(TableColumn[] columns)
			{
				base.FetchForViewCore(columns);
				foreach (var tableColumn in columns)
				{
					switch (tableColumn.ColumnName)
					{
						case Schema.ForeignOperatorName:
						case Schema.ForeignOperatorCountry:
							Factory.AddFetchHint(OrgHeaderSchema.PK, ForeignOperator.BFR_OH_ForeignOperator);
							break;
					}
				}
			}
		}

		#endregion

		IMessageManager IMessageManageableBizObj.GetMessageManagerForAmendmentDetection()
		{
			return new ForeignOperatorMultiMessageManager(new ForeignOperatorMessageSendingObject(this));
		}

		ContinueWithDetection IMessageManageableBizObj.ProcessBeforeDetectingAmendmentAndContinue()
		{
			return ContinueWithDetection.Yes;
		}

		public bool IsInAStatusAmendmentSendable => IsMessageAwaitingResponse;

		#region IBackDoorSavingSupportableBizObj

		bool IBackDoorSavingSupportableBizObj.SupportBackDoorForSavingWhenAmendmentDetected => true;

		AmendmentWithdrawalReason IBackDoorSavingSupportableBizObj.GetAmendmentWithdrawalReason()
		{
			return new AmendmentWithdrawalReason();
		}

		#endregion

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(CusBRForeignOperator);

			public CusBRForeignOperator LoadByOwnerAndAuthorityIdentifier(OrgHeader owner, ZString identifier)
			{
				return owner == null || identifier.IsEmpty ? null : Factory.LoadTop1<CusBRForeignOperator>(GetFilter(owner.PK, identifier, null));
			}

			public CusBRForeignOperator LoadByOwnerAndForeignOperator(ZGuid ownerPK, ZGuid foreignOperatorPK)
			{
				return ownerPK.IsValid && foreignOperatorPK.IsValid ? Factory.LoadTop1<CusBRForeignOperator>(GetFilter(ownerPK, null, foreignOperatorPK)) : null;
			}

			ZQuery GetFilter(ZGuid ownerPK, ZString? identifier, ZGuid? foreignOperatorPK)
			{
				if (!ownerPK.IsValid)
				{
					return ZQuery.NoResultQuery;
				}

				var query = new ZQuery(CusBRForeignOperatorSchema.BFR_OH_Owner, ownerPK);
				if (identifier.HasValue)
				{
					query.AddToFilter(CusBRForeignOperatorSchema.BFR_AuthorityIdentifier, identifier);
				}
				if (foreignOperatorPK.HasValue)
				{
					query.AddToFilter(CusBRForeignOperatorSchema.BFR_OH_ForeignOperator, foreignOperatorPK);
				}
				query.OrderBy = CusBRForeignOperatorSchema.BFR_SystemCreateTimeUtc.Name + " DESC";
				return query;
			}
		}

		#region ITemplateCopyable Members

		protected override bool SupportsCloneCore() => true;

		IBusiness ITemplateCopyable.TemplateCopy() => Clone();

		#endregion
	}
}
