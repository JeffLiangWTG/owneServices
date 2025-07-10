using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	[CodeProperty(CusPermitHeaderSchema.Constants.CPH_JobNumber)]
	[DescriptionProperty(CusPermitHeaderSchema.Constants.CPH_Number)]
	public class CusLPCOHeader : CommonCusPermitHeader, Integration.Customs.BR.ICusLPCOHeader, IControllerIDProvider, IWorkflowProvider, IJobInvoicingPlugIn, IMessageAttachee
	{
		public CusLPCOHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZString ShortName => Res.GetString("efd1cc2b-dc43-4f00-b88d-b42394ccc218", "LPCO");

		protected override ZString HumanReadableNameCore => ShortName;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Operational;
			CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			CPH_GC_Company = GlbCompany.CurrentCompany.PK;
			CPH_Type = PermitTypeList.Codes.LPC;
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			CPH_OH_PermitHolder = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			CPH_StartDate = ZDate.BrettsBirthday;
		}
#endif
		ZGuid IMessageAttachee.BranchPK => Company?.FirstActiveBranch?.PK ?? ZGuid.Empty;

		public ControllerID ControllerID => ControllerIDs.Customs.BR.LPCO;
		public Guid BusinessObjectPK => PK.ToGuid();

		protected override bool SupportsCloneCore() => true;

		#region Properties

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusLPCOHeader|CPH_JobNumber", Caption = "Job Number")]
		public override ZString CPH_JobNumber { get => base.CPH_JobNumber; set => base.CPH_JobNumber = value; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusLPCOHeader|CPH_Number", Caption = "LPCO Number")]
		public override ZString CPH_Number { get => base.CPH_Number; set => base.CPH_Number = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.CusLPCOHeader|CPH_OH_PermitHolder", Caption = "LPCO Holder")]
		public override ZGuid CPH_OH_PermitHolder { get => base.CPH_OH_PermitHolder; set => base.CPH_OH_PermitHolder = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.CusLPCOHeader|CPH_StartDate", Caption = "Start Date")]
		public override ZDate CPH_StartDate { get => base.CPH_StartDate; set => base.CPH_StartDate = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.CusLPCOHeader|CPH_EndDate", Caption = "End Date")]
		public override ZDate CPH_EndDate { get => base.CPH_EndDate; set => base.CPH_EndDate = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.CusLPCOHeader|CPH_RetroactiveDate", Caption = "Reference Date", FullDescription = "The Reference Date for issuing the Permit. If not informed, the current date will be used when sending messages.")]
		public override ZDateTimeOffset CPH_RetroactiveDate { get => base.CPH_RetroactiveDate; set => base.CPH_RetroactiveDate = value; }

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(CusLPCOHeaderLookups.MessageStatusList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusLPCOHeader|CPH_MessageStatus", Caption = "Message Status", FullDescription = "The Message Status related to the latest message.")]
		public override ZString CPH_MessageStatus { get => base.CPH_MessageStatus; set => base.CPH_MessageStatus = value; }

		#endregion

		#region Lookups

		protected override CusPermitHeaderLookups GetNewLookups() => new CusLPCOHeaderLookups(this);

		public new CusLPCOHeaderLookups Lookups => (CusLPCOHeaderLookups)base.Lookups;

		#endregion

		#region Validation

		public new CusLPCOHeaderValidation Validation
		{
			get { return (CusLPCOHeaderValidation)base.Validation; }
		}

		protected override CusPermitHeaderValidation GetNewValidation()
		{
			return new CusLPCOHeaderValidation(this);
		}
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
					workflowItems = this.GetOrCreateProcessTaskCollection(GetNewCusLPCOHeaderTaskCollection);
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		ProcessTaskCollection GetNewCusLPCOHeaderTaskCollection()
		{
			return new ProcessTaskCollection<CusLPCOHeaderProcessTask, CusLPCOHeader>(this);
		}

		ZString IWorkflowProviderCore.WorkflowType => WorkflowDescriptors.CusBRLPCOHeaderWorkflowDescriptorCode;

		public IWorkflowInformationProvider GetWorkflowInformationProvider() => null;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, CPH_OH_PermitHolder, ZGuid.Empty);
			return result;
		}

		#endregion

		#region Workflow

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		#endregion

		#region Override

		public override void Delete()
		{
			((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion

		#region EDIMessage

		[ChildEditable(true)]
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

		public void PopulateFromInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			if (invoiceLine != null)
			{
				CPH_OH_PermitHolder = invoiceLine.Declaration?.JE_OH_Supplier ?? ZGuid.Empty;
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateNumberPropertyIfRequired(CPH_JobNumberInfo, GetNewLPCOJobNumber);
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded && !IsInDatabase)
			{
				CPH_JobNumber = ZString.Empty;
			}
			base.OnSaved(saveSucceeded);
		}

		ZString GetNewLPCOJobNumber(BusinessObjectFactory factory)
		{
			var target = new LPCOJobNumberGeneratorTarget();
			var generator = new NumberGenerator
			{
				Factory = factory,
				Context = new NumberGeneratorContext(),
				BaseFountain = Env.NumberFountains.BRLPCOJobNumber,
				FountainGetter = Env.NumberFountains.GetBRLPCOJobNumberGeneratorFountain,
				PrimaryTarget = target
			};
			generator.ValueProviders.AddRange(new StandardValueSource());
			generator.Generate();
			generator.EnforceMaxLengths();
			return target.Value.ToUpper();
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			var result = new List<string>(base.GetPropertiesToExcludeFromCloning())
			{
				Schema.CPH_JobNumber,
				Schema.CPH_CustomsStatus,
				Schema.CPH_MessageStatus,
			};

			return result;
		}

		#region AddCustomsUpdateLog

		public StmALog AddCustomsUpdateLog(ZDateTimeOffset date, string customsReferenceNumber = null, string status = null, string reason = null, string action = null, string freeText = null)
		{
			var parameters = new Dictionary<string, string>();

			if (customsReferenceNumber != null)
			{
				parameters.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber, customsReferenceNumber);
			}
			if (status != null)
			{
				parameters.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Status, status);
			}
			if (reason != null)
			{
				parameters.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, reason);
			}
			if (action != null)
			{
				parameters.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Action, action);
			}

			var log = Logs.AddNew(Events.CustomsUpdate, date, parameters.ToArray());

			if (freeText != null)
			{
				using (((IUpdateFieldsLock)log).LockForUpdatingKeyFields())
				{
					log.ReferenceFreeText = freeText;
				}
			}

			return log;
		}

		#endregion

		#region Loader

		public new class Loader : Customs.Business.CusEntryHeader.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public CusLPCOHeader GetLPCOByNumber(ZString permitNumber)
			{
				var query = GetPermitByPermitNumberQuery(permitNumber);
				return Factory.LoadTop1<CusLPCOHeader>(query);
			}

			ZQuery GetPermitByPermitNumberQuery(ZString permitNumber)
			{
				return permitNumber.IsEmpty ? ZQuery.NoResultQuery
					: new ZQuery(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Operational)
						.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Core.Constants.CountryCodes.Brazil)
						.AddToFilter(CusPermitHeaderSchema.CPH_Type, PermitTypeList.Codes.LPC)
						.AddToFilter(CusPermitHeaderSchema.CPH_Number, permitNumber);
			}
		}

		#endregion

		#region IJobInvoicingPlugIn

		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return fInvoicingSupporter ?? (fInvoicingSupporter = new CusLPCOHeaderInvoicingSupporter(this)); }
		}
		IJobInvoicingSupporter fInvoicingSupporter;

		public bool AllowInvoiceDeletion => true;

		public string JobNumber => CPH_JobNumber;

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
		}

		#endregion
	}
}
