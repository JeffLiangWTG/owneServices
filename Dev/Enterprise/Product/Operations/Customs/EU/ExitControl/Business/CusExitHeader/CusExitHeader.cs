using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.EUExitControl;
using CusEntryHeader = Enterprise.Customs.EU.Business.Declaration.CusEntryHeader;
using TransportModeTranslator = Enterprise.Customs.EU.Business.TransportModeTranslator;

namespace Enterprise.Customs.EU.ExitControl.Business;

[CodeProperty(CusExitHeader.Schema.CXH_JobReference), DescriptionProperty(CusExitHeader.Schema.CXH_JobReference)]
public class CusExitHeader(BusinessObjectFactory factory, DataRow row) : ExitControlBase.Business.CusExitHeader(factory, row)
		, ICusExitHeader
		, IDocManagerSupport
		, IRelatedJob
		, IValidationModesSupporter
		, IWorkflowProvider
		, IUcc6ValueProvider
		, IDocumentSupportable
		, IWorkflowTriggerEventSource
{
	public new class Schema : AutoCusExitHeader.Schema
	{
		public const string CarrierCode = nameof(CusExitHeader.CarrierCode);
		public const string CarrierName = nameof(CusExitHeader.CarrierName);
		public const string ExporterCode = nameof(CusExitHeader.ExporterCode);
		public const string ExporterName = nameof(CusExitHeader.ExporterName);
		public const string BranchCode = nameof(CusExitHeader.BranchCode);
		public const string BranchName = nameof(CusExitHeader.BranchName);
		public const string CustomsAgentName = nameof(CusExitHeader.CustomsAgentName);
		public const string Broker = nameof(CusExitHeader.Broker);
		public const string BrokerName = nameof(CusExitHeader.BrokerName);
		public const string ReferenceNumber = nameof(CusExitHeader.ReferenceNumber);
		public const string UniqueConsignmentReference = nameof(CusExitHeader.UniqueConsignmentReference);
		public const string Consignment = nameof(CusExitHeader.Consignment);
		public const string Status = nameof(CusExitHeader.Status);
		public const string StatusDescription = nameof(CusExitHeader.StatusDescription);
		public const string MessageStatus = nameof(CusExitHeader.MessageStatus);
		public const string MessageStatusDescription = nameof(CusExitHeader.MessageStatusDescription);
	}

	public new class Loader(BusinessObjectFactory factory) : BusinessObject.Loader(factory), ICusExitHeaderLoader
	{
		public (ZQuery mainQuery, ZQuery secondaryQuery) GetLoadQuery(ZGuid parentId, ZString parentTableCode, ZInt? clusterKey = null)
		{
			var mainQuery = new ZQuery(CusExitHeaderSchema.CXH_ParentTableCode, parentTableCode);
			var secondaryQuery = new ZQuery(CusExitHeaderSchema.CXH_ParentID, parentId);
			secondaryQuery.AddToFilter(CusExitHeaderSchema.CXH_ApplicationCode, Common.CusExitHeaderApplicationCodeList.Codes.ExitControl);
			if (clusterKey.HasValue)
			{
				secondaryQuery.AddToFilter(CusExitHeaderSchema.CXH_ClusterKey, clusterKey.Value);
			}
			return (mainQuery, secondaryQuery);
		}

		public ICusExitHeader[] Load(bool fetchOnlyFromLocalCache, ZGuid parentId, ZString parentTableCode, ZInt? clusterKey = null)
		{
			(var mainQuery, var secondaryQuery) = GetLoadQuery(parentId, parentTableCode, clusterKey);
			mainQuery.AddToFilter(secondaryQuery);
			mainQuery.FetchOnlyFromLocalCache = fetchOnlyFromLocalCache;
			return Factory.Load<ICusExitHeader>(mainQuery);
		}

		protected override Type GetTypeOfBusinessObjectToLoad() => typeof(CusExitHeader);
	}

	public static readonly CusExitHeaderTypeDecider TypeDecider = new CusExitHeaderTypeDecider();

	public new CusExitHeaderLookups Lookups => (CusExitHeaderLookups)base.Lookups;

	public new CusExitHeaderValidation Validation => (CusExitHeaderValidation)base.Validation;

	protected override ExitControlBase.Business.CusExitHeaderLookups GetNewLookups() => new CusExitHeaderLookups(this);

	protected override ExitControlBase.Business.CusExitHeaderValidation GetNewValidation() => new CusExitHeaderValidation(this);

	public new ICusExitReportCollection<CusExitReport> CusExitReports => (ICusExitReportCollection<CusExitReport>)base.CusExitReports;

	public new ICusExitContainerCollection<CusExitContainer> CusExitContainers => (ICusExitContainerCollection<CusExitContainer>)base.CusExitContainers;

	public new ICusExitConsignmentCollection<CusExitConsignment> CusExitConsignments => (ICusExitConsignmentCollection<CusExitConsignment>)base.CusExitConsignments;

	public new ICusExitConsignmentPackageCollection<CusExitConsignmentPackage> CusExitConsignmentPackages => (ICusExitConsignmentPackageCollection<CusExitConsignmentPackage>)base.CusExitConsignmentPackages;

	protected override ICusExitReportCollection<ExitControlBase.Business.CusExitReport> CreateNewCusExitReportCollection() => new CusExitReportCollection<CusExitReport>(this);

	protected override ICusExitContainerCollection<ExitControlBase.Business.CusExitContainer> CreateNewCusExitContainerCollection() => new CusExitContainerCollection<CusExitContainer>(this);

	protected override ICusExitConsignmentCollection<ExitControlBase.Business.CusExitConsignment> CreateNewCusExitConsignmentCollection() => new CusExitConsignmentCollection<CusExitConsignment>(this);

	protected override ICusExitConsignmentPackageCollection<ExitControlBase.Business.CusExitConsignmentPackage> CreateNewCusExitConsignmentPackageCollection() => new CusExitConsignmentPackageCollection<CusExitConsignmentPackage>(this);

	IActiveBusinessObjectCollection<ICusExitReport> ICusExitHeader.CusExitReports => CusExitReports;

	public BusinessObject Parent
	{
		get
		{
			if (parent == null || parent.IsDeleted || parent.PK != CXH_ParentID)
			{
				parent = !CXH_ParentID.IsEmpty && !CXH_ParentTableCode.IsEmpty
					? Factory.Load(CXH_ParentTableCode, CXH_ParentID)
					: null;
			}
			return parent;
		}
		set
		{
			parent = value;
			if (parent != null && (CXH_ParentID != parent.PK || CXH_ParentTableCode != parent.TablePrefix))
			{
				CXH_ParentID = parent.PK;
				CXH_ParentTableCode = parent.TablePrefix;
				CusExitReports.MarkAsNeedingValidation();
			}
		}
	}
	BusinessObject parent;

	public JobDeclaration Declaration => Parent as JobDeclaration;

	public ForwardingShipment Shipment => Parent as ForwardingShipment;

	public ZString CountryCode => Company?.GC_RN_NKCountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

	[ReadOnlyMember(nameof(IsPluggedIntoParent))]
	[ResourceStringData("638B93E0-4038-4F9B-BE14-6BABFF4DECA3", Caption = "Job Number")]
	public override ZString CXH_JobReference { get => base.CXH_JobReference; set => base.CXH_JobReference = value; }

	public bool IsPluggedIntoParent => Parent != null;

	public TransportModeTranslator TransportModeTranslator => transportModeTranslator ?? (transportModeTranslator = GetTransportModeTranslator());
	TransportModeTranslator transportModeTranslator;

	protected virtual TransportModeTranslator GetTransportModeTranslator() => new TransportModeTranslator();

	public override ZGuid CXH_GC_Company
	{
		get => base.CXH_GC_Company;
		set
		{
			var oldValue = CXH_GC_Company;
			base.CXH_GC_Company = value;
			if (!IsCopying && oldValue != CXH_GC_Company)
			{
				foreach (var report in CusExitReports)
				{
					report.AlternativeEvidences.MarkAsNeedingValidation();
				}
			}
		}
	}

	[ResourceStringData("820D98EB-0C3C-428E-85CD-59DE1601D50E", Caption = "Carrier")]
	[List(nameof(Lookups) + "." + nameof(CusExitHeaderLookups.OrganizationsFindBoxList))]
	public override ZGuid CXH_OA_Carrier { get => base.CXH_OA_Carrier; set => base.CXH_OA_Carrier = value; }

	protected override ZAddress GetNewCXH_OA_Carrier_ZAddress()
	{
		var zAddress = new ZAddress(CXH_OA_CarrierInfo);
		zAddress.DefaultAddressType = AddressType.OFC;
		return zAddress;
	}

	[ResourceStringData("F61D2382-0FFC-44A0-B358-607A1799B46E", Caption = "Carrier")]
	public ZString CarrierCode => Carrier?.Header.OH_Code ?? ZString.Empty;

	[ResourceStringData("0d38dd40-0655-42d9-9f5a-74af7991a14e", Caption = "Carrier Name")]
	public ZString CarrierName => Carrier?.CompanyName ?? ZString.Empty;

	[ResourceStringData("24307ED0-5EA4-40E0-99D2-0AD355C06537", Caption = "Exporter")]
	public ZString ExporterCode => Exporter?.OH_Code ?? ZString.Empty;

	[ResourceStringData("26810993-8773-47FC-85AF-827EB9C3452E", Caption = "Exporter Name")]
	public ZString ExporterName => Exporter?.MainAddress.CompanyName ?? ZString.Empty;

	[ResourceStringData("430cc5e0-d9bb-4f66-8321-90b83388283a", Caption = "Branch")]
	public ZString BranchCode => Branch?.GB_Code ?? ZString.Empty;

	[ResourceStringData("b449ef26-204e-424c-b0db-6e1943d2b1a4", Caption = "Branch Name")]
	public ZString BranchName => Branch?.GB_BranchName ?? ZString.Empty;

	[ResourceStringData("35044958-48DB-47D6-81EE-70E7AE6226F3", Caption = "Customs Agent Name")]
	public ZString CustomsAgentName => CustomsAgent?.GS_FullName ?? ZString.Empty;

	[ResourceStringData("382FCD78-4FFD-4642-9BA2-A282495939B8", Caption = "Broker", MediumCaption = "Broker", ShortCaption = "Broker")]
	public ZString Broker { get => CXH_GS_NKCustomsAgent; }

	public ZPropertyInfo BrokerInfo { get => GetZPropertyInfo(nameof(Broker)); }

	[ResourceStringData("9D2DDD60-73D6-4912-A2C6-B5C9F8435605", Caption = "Broker Name", MediumCaption = "Broker Name", ShortCaption = "Broker Name")]
	public ZString BrokerName { get => CustomsAgentName; }

	public ZPropertyInfo BrokerNameInfo => GetZPropertyInfo(nameof(BrokerName));

	IActiveBusinessObjectCollection<ICusExitConsignment> ICusExitHeader.CusExitConsignments => CusExitConsignments;

	public ZAddressWithContact CarrierZAddressWithContact
	{
		get
		{
			if (fCarrierZAddressWithContact == null)
			{
				fCarrierZAddressWithContact = GetCarrierZAddressWithContact();
			}

			return fCarrierZAddressWithContact;
		}
	}
	ZAddressWithContact fCarrierZAddressWithContact;

	ZAddressWithContact GetCarrierZAddressWithContact()
	{
		var result = new ZAddressWithContact(CXH_OC_CarrierContactInfo, CXH_OA_CarrierInfo);
		return result;
	}

	public IDictionary<ZShort, ZShort> ContainersSequenceDictionary => Factory.GetValue(ref containersSequenceDictionaryCached, () => CusExitContainers
		.Select(x => x.CXN_Sequence).Where(x => x > ZShort.Zero)
		.GroupBy(x => x).ToDictionary(x => x.Key, y => (ZShort)y.Count()));
	CachedProperty<IDictionary<ZShort, ZShort>> containersSequenceDictionaryCached;

	protected override AutologState AutoLoggingState => AutologState.AutoLogged;

	protected override ZString HumanReadableNameCore => Res.GetString("ECD98599-87B5-47B7-8471-61782065892C", "Exit Control {0}", CXH_JobReference);

	public ZBool ShouldHaveSeqNumInContainersOrEquipmentsAndSeals => ShouldHaveSeqNumInContainersOrEquipmentsAndSealsCore;

	protected virtual bool ShouldHaveSeqNumInContainersOrEquipmentsAndSealsCore => false;

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();

		var currentBranch = GlbBranch.CurrentBranch;
		CXH_GB_Branch = currentBranch.PK;
		CXH_GC_Company = currentBranch.GB_GC;
		CXH_ApplicationCode = Common.CusExitHeaderApplicationCodeList.Codes.ExitControl;
	}

	public override void OnSaving()
	{
		base.OnSaving();
		PopulateCXH_JobReferenceIfNeeded();
	}

	void PopulateCXH_JobReferenceIfNeeded()
	{
		if (!IsInDatabase && CXH_JobReference.IsEmpty)
		{
			var parent = Parent;
			if (parent != null)
			{
				if (parent is JobDeclaration declaration)
				{
					declaration.PopulateJE_DeclarationReferenceIfNeeded();
					CXH_JobReference = declaration.JE_DeclarationReference;
				}
				else if (parent is ForwardingShipment shipment)
				{
					shipment.PopulateBillAndShipmentNumberIfNeeded();
					CXH_JobReference = shipment.JS_UniqueConsignRef;
				}
				else if (parent is ForwardingConsol consol)
				{
					consol.PopulateJK_UniqueConsignRefIfNeeded();
					CXH_JobReference = consol.JK_UniqueConsignRef;
				}
			}
			else
			{
				PopulateNumberPropertyIfRequired(CXH_JobReferenceInfo, objectFactory => GetNewJobReference(objectFactory), true);
			}
		}
	}

	ZString GetNewJobReference(BusinessObjectFactory factory)
	{
		var numberGeneratorTarget = new ExitControlNumberGeneratorTarget();
		var generator = new NumberGenerator
		{
			Factory = factory,
			Context = GetNewNumberGeneratorContext(),
			BaseFountain = Env.NumberFountains.ExitControlJobNumber,
			FountainGetter = Env.NumberFountains.GetExitControlJobNumberGeneratorFountain,
			PrimaryTarget = numberGeneratorTarget
		};
		generator.ValueProviders.AddRange(new StandardValueSource());
		generator.Generate();
		generator.EnforceMaxLengths();
		return numberGeneratorTarget.Value.ToUpper();
	}

	NumberGeneratorContext GetNewNumberGeneratorContext()
	{
		return CXH_GB_Branch.IsEmpty ? new NumberGeneratorContext() : new NumberGeneratorContext(GlbCompany.CurrentCompany.PK, CXH_GB_Branch, GlbDepartment.CurrentDepartment.PK);
	}

	public DocManagerInfo DocManagerInfo => docManagerInfo ??= new CusExitHeaderDocManagerInfo(this);
	DocManagerInfo docManagerInfo;

	public void DefaultDataFromParent(bool includeHeaderData)
	{
		var parent = Parent;
		var shipment = parent as ForwardingShipment;
		var shipmentIsNotNull = shipment != null;
		var declaration = parent as JobDeclaration ?? shipment?.DeclarationForDocuments as JobDeclaration;
		if (declaration != null)
		{
			if (includeHeaderData)
			{
				DefaultJobReferenceFromParent();
				DefaultHeaderDataFromDeclaration(declaration);
			}

			foreach (var entryHeader in declaration.CustomsEntryHeaders)
			{
				CreateConsignmentFromEntryHeader(entryHeader);
			}
		}
		else if (shipmentIsNotNull)
		{
			if (includeHeaderData)
			{
				DefaultJobReferenceFromParent();
				DefaultHeaderDataFromShipment(shipment);
			}
			CreateConsignmentFromShipment(shipment);
		}

		void DefaultJobReferenceFromParent()
		{
			if (shipmentIsNotNull)
			{
				CXH_JobReference = shipment.JS_UniqueConsignRef;
			}
			else
			{
				CXH_JobReference = declaration.JE_DeclarationReference;
			}
		}
	}

	protected virtual CusExitConsignment CreateConsignmentFromEntryHeader(CusEntryHeader entryHeader) => CreateConsignmentFromMRN(entryHeader.MovementReferenceNumber);

	protected CusExitConsignment CreateConsignmentFromMRN(ZString mrn)
	{
		CusExitConsignment consignment = null;
		if (!mrn.IsEmpty && !CusExitConsignments.Any(x => x.CXC_MovementReference == mrn))
		{
			consignment = CusExitConsignments.AddNew();
			consignment.CXC_MovementReference = mrn;
		}
		return consignment;
	}

	protected virtual void DefaultHeaderDataFromDeclaration(JobDeclaration declaration)
	{
		CXH_GB_Branch = declaration.JE_GB;
		CXH_OH_Exporter = declaration.SupplierDocumentaryAddress?.OrganisationPK ?? ZGuid.Empty;
		CXH_OA_Carrier = declaration.ShippingLine?.MainAddress?.PK ?? ZGuid.Empty;
	}

	protected virtual void DefaultHeaderDataFromShipment(ForwardingShipment shipment)
	{
	}

	protected virtual CusExitConsignment CreateConsignmentFromShipment(ForwardingShipment shipment) => null;

	[ChildEditableTestExclude]
	public ICusExitReportCollection<CusExitReport> CusExitReportStatus => cusExitReportStatus ?? (cusExitReportStatus = new CusExitReportCollection<CusExitReport>(this));
	ICusExitReportCollection<CusExitReport> cusExitReportStatus;

	#region IRelatedJob

	ZString IRelatedJob.JobNumber => CXH_JobReference;

	ZString IRelatedJob.JobDescription => HumanReadableName;

	ZString IRelatedJob.JobStatus => ZString.Empty;

	ControllerID IControllerIDProvider.ControllerID => ControllerIDs.Customs.EU.ExitControl;

	Guid IControllerIDProvider.BusinessObjectPK => PK.ToGuid();

	#endregion

	#region IValidationModesSupporter

	public void RecalculateValidationModesOnAllLevel()
	{
		CusExitReports.ForEach(x => x.ValidationModesCalculator.RecalculateValidationModes());
		ValidationModesCalculator.RecalculateValidationModes();
	}

	public ValidationModes ValidationModes
	{
		get
		{
			if (!fValidationModes.HasValue)
			{
				ValidationModesCalculator.RecalculateValidationModes();
			}
			return fValidationModes.Value;
		}
		set
		{
			fValidationModes = value;
		}
	}
	ValidationModes? fValidationModes;

	public ExitHeaderValidationModesCalculator ValidationModesCalculator => validationModesCalculator ?? (validationModesCalculator = CreateNewValidationModesCalculator());
	ExitHeaderValidationModesCalculator validationModesCalculator;

	protected virtual ExitHeaderValidationModesCalculator CreateNewValidationModesCalculator() => new ExitHeaderValidationModesCalculator(this);

	public bool IsOriginalValidationMode
	{
		get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.Original); }
	}

	#endregion

	#region IWorkflowInformationProvider

	public IWorkflowInformationProvider GetWorkflowInformationProvider() => WorkflowInformationProvider;

	IWorkflowInformationProvider WorkflowInformationProvider
	{
		get
		{
			if (workflowInformationProvider == null)
			{
				workflowInformationProvider = new CusExitHeaderWorkflowInformationProvider(this);
			}
			return workflowInformationProvider;
		}
	}
	IWorkflowInformationProvider workflowInformationProvider;

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

	public ProcessTaskCollection WorkflowItems
	{
		get
		{
			var result = workflowItems;
			var relatedDeclaration = Declaration;
			var relatedShipment = Shipment;
			if (relatedDeclaration != null)
			{
				result = relatedDeclaration.WorkflowItems;
			}
			else if (relatedShipment != null)
			{
				result = relatedShipment.WorkflowItems;
			}
			else if (result == null)
			{
				result = this.GetOrCreateProcessTaskCollection(() => new CusExitHeaderProcessTaskCollection(this));
				workflowItems = result;
			}
			return result;
		}
	}
	ProcessTaskCollection workflowItems;

	public IColumnValueRanker GetTemplateSelectionCriteria()
	{
		return new ColumnValueRanker();
	}

	public ZString WorkflowType => WorkflowDescriptors.CusExitHeaderWorkflowDescriptorCode;

	ProcessTaskCollection IWorkflowProvider.WorkflowItems => WorkflowItems;

	#endregion

	#region IWorkflowTriggerEventSource

	public IReadOnlyList<IWorkflowProviderCore> ParentWorkflowProviders
	{
		get
		{
			var list = new List<IWorkflowProviderCore>();
			var shipment = Shipment;
			if (shipment != null)
			{
				list.Add(shipment);
			}

			var declaration = Declaration;
			if (declaration != null)
			{
				list.Add(declaration);
			}

			return list;
		}
	}

	public IGlbCompany JobHeaderCompany => Company;

	#endregion

	#region Display Properties For Module Grid

	[ResourceStringData("21e4866a-8749-4829-a638-acb8fbf97cc7", Caption = "Registration Number (ext.)", MediumCaption = "Reg. Number (ext.)", ShortCaption = "Reg. No. (ext.)")]
	public ZString ReferenceNumber => CusExitConsignments
		.Select(e => e.CXC_ReferenceNumber).GetSingleValueOrManyText(e => e, CommonEntryStatusList.Descriptions.MultipleEntryStatus);

	[ResourceStringData("52e246ac-cdc3-4bb6-8f86-7648706bb4e7", Caption = "Reference Number UCR", MediumCaption = "Ref. Number UCR", ShortCaption = "Ref. No. UCR")]
	public ZString UniqueConsignmentReference => CusExitConsignments
		.Select(e => e.CXC_UniqueConsignmentReference).GetSingleValueOrManyText(e => e, CommonEntryStatusList.Descriptions.MultipleEntryStatus);

	[ResourceStringData("0b581dc1-d0d4-4123-980f-e7a50b51fa3f", Caption = "Entry/Consignment", MediumCaption = "Ent./Cons.", ShortCaption = "E./C.")]
	public ZString Consignment =>
		CusExitReports.Select(e => e.Consignment?.MessageDescriptionForEdocs ?? "").GetSingleValueOrManyText(e => e, CommonEntryStatusList.Descriptions.MultipleEntryStatus);

	[ResourceStringData("75a2d970-4c49-4b67-9e34-2b4e12136493", Caption = "Status", MediumCaption = "Stat.", ShortCaption = "St. ")]
	public ZString Status => CusExitReports.GetSingleValueOrManyText(e => e.CER_Status, CommonEntryStatusList.Codes.MultipleEntryStatus);

	[ResourceStringData("49d2bd07-0b03-4596-9793-e9e043d3393a", Caption = "Status Description", MediumCaption = "Status Desc.", ShortCaption = "Status")]
	public ZString StatusDescription => CusExitReports.GetSingleValueOrManyText(e => e.StatusDescription, e => e.CER_Status, CommonEntryStatusList.Descriptions.MultipleEntryStatus);

	[ResourceStringData("39aec5ac-1ac1-4c4b-b7de-b0a452726407", Caption = "Message Status", MediumCaption = "Msg. Status", ShortCaption = "Msg. St.")]
	public ZString MessageStatus => CusExitReports.GetSingleValueOrManyText(e => e.CER_MessageStatus, CommonEntryStatusList.Codes.MultipleEntryStatus);

	[ResourceStringData("45cbdc7f-f74a-4c2b-9613-ce7485b6088b", Caption = "Message Status Description",
		MediumCaption = "Msg. Status Desc.", ShortCaption = "Msg. St. Desc.")]
	public ZString MessageStatusDescription =>
		CusExitReports.GetSingleValueOrManyText(e => e.MessageStatusDescription, e => e.CER_MessageStatus, CommonEntryStatusList.Descriptions.MultipleEntryStatus);

	#endregion

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

	protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
	{
		get
		{
			var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
			result.AddRange(CusExitReports);
			return result.ToArray();
		}
	}

	public bool IsUCC6 => IsUCC6Core;
	protected virtual bool IsUCC6Core => false;

	public ExitControlConfiguration Configuration => ExitControlConfiguration.GetConfiguration(Factory, CountryCode);

	public DocumentSupporter DocumentSupporter => documentSupporter ??= CreateNewDocumentSupporter();

	DocumentSupporter documentSupporter;

	protected virtual DocumentSupporter CreateNewDocumentSupporter() => new CusExitHeaderDocumentSupporter(this);
}
