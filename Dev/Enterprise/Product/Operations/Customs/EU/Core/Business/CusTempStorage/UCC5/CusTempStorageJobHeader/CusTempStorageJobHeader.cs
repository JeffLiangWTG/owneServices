using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	[UniversalDataContext(DataContextType.TemporaryStorage)]
	[SingleObjectAroundARow]
	[CodeProperty(CusTempStorageJobHeader.Schema.SJH_JobReference)]
	public class CusTempStorageJobHeader : AutoCusTempStorageJobHeader
		, Integration.Customs.EU.ICusTempStorageJobHeader
		, IWorkflowProvider
		, IJobNumber
		, IRelatedJob
		, IEDocsProvider
	{
		public CusTempStorageJobHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusTempStorageJobHeader.Schema
		{
			public const string SJH_CustomsOfficeDescription = "SJH_CustomsOfficeDescription";
		}

		#region Type Decider

		public static readonly CusTempStorageJobHeaderTypeDecider TypeDecider = new CusTempStorageJobHeaderTypeDecider();

		#endregion

		#region Properties

		[List(nameof(Lookups) + "." + nameof(CusTempStorageJobHeaderLookups.TransportModeList))]
		public override ZString SJH_TransportMode { get => base.SJH_TransportMode; set => base.SJH_TransportMode = value; }

		[List(nameof(Lookups) + "." + nameof(CusTempStorageJobHeaderLookups.TransportMeansList))]
		public override ZString SJH_TransportMeansCode { get => base.SJH_TransportMeansCode; set => base.SJH_TransportMeansCode = value; }

		[List(nameof(Lookups) + "." + nameof(CusTempStorageJobHeaderLookups.PreviousReferenceTypeList))]
		public override ZString SJH_PreviousReferenceType { get => base.SJH_PreviousReferenceType; set => base.SJH_PreviousReferenceType = value; }

		[List(nameof(Lookups) + "." + nameof(CusTempStorageJobHeaderLookups.CustomsOfficeList))]
		public override ZString SJH_CustomsOffice { get => base.SJH_CustomsOffice; set => base.SJH_CustomsOffice = value; }

		[List(nameof(Lookups) + "." + nameof(CusTempStorageJobHeaderLookups.EntryCustomsOfficeList))]
		public override ZString SJH_CustomsOfficeOfEntryIntoEU { get => base.SJH_CustomsOfficeOfEntryIntoEU; set => base.SJH_CustomsOfficeOfEntryIntoEU = value; }

		[List(nameof(Lookups) + "." + nameof(CusTempStorageJobHeaderLookups.OrganizationsFindBoxList))]
		public override ZGuid SJH_OA_Presenter
		{
			get => base.SJH_OA_Presenter;
			set
			{
				base.SJH_OA_Presenter = value;
				SJH_OA_PresenterInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SJH_OA_Presenter_ZAddressInfo => GetWrappedZPropertyInfo(nameof(SJH_OA_Presenter_ZAddress), info => SJH_OA_PresenterInfo);

		[List(nameof(Lookups) + "." + nameof(CusTempStorageJobHeaderLookups.OrganizationsFindBoxList))]
		public override ZGuid SJH_OA_Representative
		{
			get => base.SJH_OA_Representative;
			set
			{
				base.SJH_OA_Representative = value;
				SJH_OA_RepresentativeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SJH_OA_Representative_ZAddressInfo => GetWrappedZPropertyInfo(nameof(SJH_OA_Representative_ZAddress), info => SJH_OA_RepresentativeInfo);

		#endregion

		#region New Properties

		#region Customs Office

		public virtual ZZRefCusCodeListCombined CustomsOffice => Factory.GetValue(ref customsOffice,
			() => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, SJH_CustomsOffice, CountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, EffectivePresentationDate));
		CachedProperty<ZZRefCusCodeListCombined> customsOffice;

		public virtual ZString SJH_CustomsOfficeDescription => CustomsOffice?.ZZD_Description ?? ZString.Empty;

		public ZPropertyInfo SJH_CustomsOfficeDescriptionInfo => GetZPropertyInfo(Schema.SJH_CustomsOfficeDescription);

		public CusGuaranteeHeader GuaranteeHeader => Factory.Load<CusGuaranteeHeader>(SJH_CPH_Guarantee);

		public ZString SJH_GuaranteeDescription => GuaranteeHeader?.Description ?? ZString.Empty;

		public ZString SJH_GuaranteeNumber => GuaranteeHeader?.CPH_Number ?? ZString.Empty;
		#endregion

		#region Customs Office of Entry Info EU

		public virtual ZZRefCusCodeListCombined CustomsOfficeOfEntryIntoEU => Factory.GetValue(ref customsOfficeOfEntryIntoEU, () => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, SJH_CustomsOfficeOfEntryIntoEU, CountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, EffectivePresentationDate));
		CachedProperty<ZZRefCusCodeListCombined> customsOfficeOfEntryIntoEU;

		#endregion

		ZDateTime EffectivePresentationDate => SJH_PresentationDate.IsEmpty ? ZDateTime.Today : SJH_PresentationDate;

		public GlbCompany Company
		{
			get
			{
				var branch = Branch;
				return branch != null ? branch.Company : Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			}
		}

		public RefCountry Country => Company?.Country;

		public ZString CountryCode => Country?.Code ?? ZString.Empty;

		//public virtual ZBool IsReExport => false;

		#endregion

		#region CusTempStorageDecs

		[ChildEditable(true)]
		public CusTempStorageDecCollection CusTempStorageDecs => cusTempStorageDecs ?? (cusTempStorageDecs = GetCusTempStorageDecs());

		CusTempStorageDecCollection GetCusTempStorageDecs()
		{
			var result = CreateNewCusTempStorageDecs();
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		protected virtual CusTempStorageDecCollection CreateNewCusTempStorageDecs() => new CusTempStorageDecCollection<CusTempStorageDec, CusTempStorageJobHeader>(this);

		CusTempStorageDecCollection cusTempStorageDecs;

		#endregion

		#region Validation

		protected override CusTempStorageJobHeaderValidation GetNewValidation()
		{
			return new CusTempStorageJobHeaderValidation(this);
		}

		#endregion

		#region IWorkflowProvider

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		[ActionFieldFollow]
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

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(GetNewProcessTaskCollection);
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		protected virtual ProcessTaskCollection GetNewProcessTaskCollection()
		{
			return new ProcessTaskCollection<CusTempStorageJobHeaderProcessTask, CusTempStorageJobHeader>(this);
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return new ColumnValueRanker();
		}

		ZGuid IWorkflowProviderCore.PK
		{
			get { return PK; }
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return new CusTempStorageJobHeaderWorkflowDescriptor().Code; }
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				this.WorkflowItems.RemoveAndDeleteAll();
				this.DeleteChildren<CusTempStorageDec>(CusTempStorageDecSchema.STH_SJH);
			}
			base.Delete();
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SJH_GB = GlbBranch.CurrentBranch.PK;
		}

		#endregion

		#region IJobNumber

		string IJobNumber.JobNumber => SJH_JobReference;

		#endregion

		#region IRelatedJob Members
		ZString IRelatedJob.JobNumber => SJH_JobReference;

		ZString IRelatedJob.JobDescription => HumanReadableName;

		ZString IRelatedJob.JobStatus => JobStatus;

		protected virtual ZString JobStatus => ZString.Empty;

		ControllerID IControllerIDProvider.ControllerID => ControllerIDs.Customs.TemporaryStorage;

		Guid IControllerIDProvider.BusinessObjectPK => PK.ToGuid();
		#endregion

		#region IEDocsProvider Members 
		public EDocsProviderSupporter GetEDocsProviderSupporter() => new EDocsProviderSupporter(this);

		public DocumentSupporter DocumentSupporter => documentSupporter ?? (documentSupporter = GetDocumentSupporter());
		DocumentSupporter documentSupporter;

		protected virtual DocumentSupporter GetDocumentSupporter() => new CusTempStorageJobHeaderDocumentSupporter(this);

		public DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = new CusTempStorageJobHeaderDocManagerInfo(this));

		public bool IsUcc6 => Configuration.IsUCC6(this);

		public CusTempStorageJobHeaderConfiguration Configuration => configuration ?? (configuration = CusTempStorageJobHeaderConfiguration.GetConfiguration(Factory, CountryCode));
		CusTempStorageJobHeaderConfiguration configuration;

		DocManagerInfo docManagerInfo;
		#endregion

		public CusTempStorageDec CusTempStorageDec
		{
			get
			{
				if (fCusTempStorageDec == null)
				{
					fCusTempStorageDec = LoadCusTempStorageDec();
					RegisterEditableChildObject(fCusTempStorageDec);
				}
				return fCusTempStorageDec;
			}
		}

		CusTempStorageDec fCusTempStorageDec;

		protected virtual CusTempStorageDec LoadCusTempStorageDec() => CusTempStorageDec.Load<CusTempStorageDec>(this);
	}
}
