using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.CN.Business
{
	[CodeProperty(CusEntryInstruction.Schema.CEI_Style), DescriptionProperty(Schema.DescriptionWithBillOfLading)]
	[SystemDefinedValues]
	public class CusEntryInstruction : AutoCNCusEntryInstruction,
		Integration.Customs.CN.ICusEntryInstruction,
		ICusCodeDataTypeSupporter,
		ICusAddInfoTypeSupporter,
		ICusStorageDocPivotTypeSupporter
	{
		#region Schema

		public new abstract class Schema : AutoCNCusEntryInstruction.Schema
		{
			public const string DescriptionWithBillOfLading = "DescriptionWithBillOfLading";
			public const string BillOfLading = "BillOfLading";
			public const string BillOfLadingDate = "BillOfLadingDate";
			public const string SpecialBusinessIdentifiersAsString = "SpecialBusinessIdentifiersAsString";
			public const string CustomsMessageRemarks = "CustomsMessageRemarks";
			public const string CEI_StyleDescription = "CEI_StyleDescription";
			public const string EntryTypeDescription = "EntryTypeDescription";
			public const string OtherPackagesAsString = "OtherPackagesAsString";
			public const string OperationMattersAsString = "OperationMattersAsString";
		}

		#endregion

		public CusEntryInstruction(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		#region Related BusinessObjects

		public new JobDeclaration JobDeclaration => base.JobDeclaration as JobDeclaration;

		public new CusEntryInstructionValidation Validation => (CusEntryInstructionValidation)base.Validation;

		#endregion

		#region New Properties

		public new CusEntryInstructionLookups Lookups => (CusEntryInstructionLookups)base.Lookups;

		#region Bool Properties

		public ZBool WillGenerateEnteringEntry => (JobDeclaration?.WillGenerateBothEntries ?? false) ? (ZBool)!IsChild : JobDeclaration?.IsImport ?? false;

		public ZBool WillGenerateExitingEntry => (JobDeclaration?.WillGenerateBothEntries ?? false) ? IsChild : JobDeclaration?.IsExport ?? false;

		public ZBool WillGenerateCustomsEntry => EntryType == EntryTypeList.Codes.CustomsEntry;

		public ZBool WillGenerateRecordListing => EntryType == EntryTypeList.Codes.RecordListing;

		public ZBool IsGeneralTrade => CEI_Style == CNRefCusProcedure.Codes._0110 || CEI_Style == CNRefCusProcedure.Codes._3339;

		#endregion

		#region CEI_StyleDescription

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|CEI_StyleDescription", Caption = "Desc.", FullDescription = "Customs Procedure Description", MediumCaption = "Procedure Description", ShortCaption = "Description")]
		public ZString CEI_StyleDescription => Lookups.StyleList.GetDescriptionFromCode(CEI_Style);

		public ZPropertyInfo CEI_StyleDescriptionInfo => GetZPropertyInfo(Schema.CEI_StyleDescription);

		#endregion

		#region DescriptionWithBillOfLading

		public ZString DescriptionWithBillOfLading =>
			ZString.Format(!CEI_Description.IsEmpty && !BillOfLading.IsEmpty ? "{0} - {1}" : "{0}{1}", CEI_Description, BillOfLading);

		public ZPropertyInfo DescriptionWithBillOfLadingInfo => GetZPropertyInfo(Schema.DescriptionWithBillOfLading);

		#endregion

		#region EnterpriseQualifications

		[ChildEditable(true)]
		public EnterpriseQualificationCollection EnterpriseQualifications
		{
			get
			{
				if (fEnterpriseQualifications == null)
				{
					fEnterpriseQualifications = new EnterpriseQualificationCollection(this);
					fEnterpriseQualifications.Load();
					RegisterEditableChildObject(fEnterpriseQualifications);
					fEnterpriseQualifications.CountChanged += CIQRelatedCollection_CountChanged;
				}
				return fEnterpriseQualifications;
			}
		}
		EnterpriseQualificationCollection fEnterpriseQualifications;

		#endregion

		#region Special Business Identifiers

		[ChildEditable(true)]
		public SpecialBusinessIdentifierCollection SpecialBusinessIdentifiers
		{
			get
			{
				if (specialBusinessIdentifiers == null)
				{
					specialBusinessIdentifiers = new SpecialBusinessIdentifierCollection(this);
					specialBusinessIdentifiers.Load();
					RegisterEditableChildObject(specialBusinessIdentifiers);
					specialBusinessIdentifiers.CountChanged += CIQRelatedCollection_CountChanged;
				}
				return specialBusinessIdentifiers;
			}
		}

		void CIQRelatedCollection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded && IsCIQRequiresEditableAndFalse)
			{
				CEI_CIQRequires = true;
			}
		}

		public bool IsCIQRequiresEditableAndFalse => !CEI_CIQRequiresInfo.ReadOnly && !CEI_CIQRequires;

		SpecialBusinessIdentifierCollection specialBusinessIdentifiers;

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|SpecialBusinessIdentifiersAsString", Caption = "Special Business")]
		public ZString SpecialBusinessIdentifiersAsString => SpecialBusinessIdentifiers.GetSelectedOptionDescAsString();

		public ZPropertyInfo SpecialBusinessIdentifiersAsStringInfo => GetZPropertyInfo(Schema.SpecialBusinessIdentifiersAsString);

		#endregion

		#region Other Packages

		OtherPackageCollection otherPackages;

		[ChildEditable(true)]
		public OtherPackageCollection OtherPackages
		{
			get
			{
				if (otherPackages == null)
				{
					otherPackages = new OtherPackageCollection(this);
					otherPackages.Load();
					RegisterEditableChildObject(otherPackages);
				}
				return otherPackages;
			}
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|OtherPackagesAsString", Caption = "Other Packages")]
		public ZString OtherPackagesAsString => OtherPackages.GetSelectedOptionDescAsString();

		public ZPropertyInfo OtherPackagesAsStringInfo => GetZPropertyInfo(Schema.OtherPackagesAsString);

		#endregion

		#region Operation Matters

		OperationMatterCollection operationMatters;

		[ChildEditable(true)]
		public OperationMatterCollection OperationMatters
		{
			get
			{
				if (operationMatters == null)
				{
					operationMatters = new OperationMatterCollection(this);
					operationMatters.Load();
					RegisterEditableChildObject(operationMatters);
				}
				return operationMatters;
			}
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|OperationMattersAsString", Caption = "Operation Matters")]
		public ZString OperationMattersAsString => OperationMatters.GetSelectedOptionDescAsString();

		public ZPropertyInfo OperationMattersAsStringInfo => GetZPropertyInfo(Schema.OperationMattersAsString);

		#endregion

		#region CIQRequiredDocuments

		[ChildEditable(true)]
		public CIQRequiredDocumentCollection CIQRequiredDocuments
		{
			get
			{
				if (ciqRequiredDocuments == null)
				{
					ciqRequiredDocuments = new CIQRequiredDocumentCollection(this);
					ciqRequiredDocuments.Load();
					RegisterEditableChildObject(ciqRequiredDocuments);
					ciqRequiredDocuments.CountChanged += CIQRelatedCollection_CountChanged;
				}
				return ciqRequiredDocuments;
			}
		}
		CIQRequiredDocumentCollection ciqRequiredDocuments;

		#endregion

		#region Entry Type

		public ZString EntryType => GetEntryType(JobDeclaration, IsChild);

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|EntryTypeDescription", Caption = "Entry Type Description", ShortCaption = "Type", MediumCaption = "Entry Type")]
		public ZString EntryTypeDescription => GetEntryTypeDescription(JobDeclaration, IsChild);

		public ZPropertyInfo EntryTypeDescriptionInfo => GetZPropertyInfo(Schema.EntryTypeDescription);

		public static ZString GetEntryType(JobDeclaration declaration, bool isChild)
		{
			ZString result = ZString.Empty;
			if (declaration != null)
			{
				if (declaration.WillGenerateBothEntries)
				{
					result = declaration.IsImport ? (isChild ? EntryTypeList.Codes.RecordListing : EntryTypeList.Codes.CustomsEntry)
																				: (isChild ? EntryTypeList.Codes.CustomsEntry : EntryTypeList.Codes.RecordListing);
				}
				else
				{
					result = declaration.JE_MessageSubType;
				}
			}
			return result;
		}

		public static ZString GetEntryTypeDescription(JobDeclaration declaration, bool isChild)
		{
			ZString result = ZString.Empty;
			var entryType = GetEntryType(declaration, isChild);

			if (!entryType.IsEmpty)
			{
				var isEntering = declaration.WillGenerateBothEntries ? !isChild : (bool)declaration.IsImport;
				var entryTypeList = DecTypeList.GetDecTypeList(declaration.Factory, isEntering, !isEntering);

				result = entryTypeList.GetDescriptionFromCode(entryType);
			}

			return result;
		}

		#endregion

		#region Attachments

		[ChildEditable(true)]
		public CusStorageDocPivotCollection CusStorageDocPivots
		{
			get
			{
				if (fCusStorageDocPivots == null)
				{
					fCusStorageDocPivots = new CusStorageDocPivotCollection(this);
					fCusStorageDocPivots.Load();
					RegisterEditableChildObject(fCusStorageDocPivots);
				}

				return fCusStorageDocPivots;
			}
		}
		CusStorageDocPivotCollection fCusStorageDocPivots;

		bool IsCusStorageDocPivotsLoaded => fCusStorageDocPivots != null && fCusStorageDocPivots.IsLoaded;

		internal void ReloadCusStorageDocPivots()
		{
			if (IsCusStorageDocPivotsLoaded)
			{
				CusStorageDocPivots.Reload(true);
			}
		}

		[ChildEditable(true)]
		public CusAttachmentCollection CusAttachments
		{
			get
			{
				if (fCusAttachments == null)
				{
					fCusAttachments = new CusAttachmentCollection(this);
					fCusAttachments.Load();
					RegisterEditableChildObject(fCusAttachments);
				}
				return fCusAttachments;
			}
		}
		CusAttachmentCollection fCusAttachments;

		EntryInstructionAttachmentCollection fAttachments;

		[ChildEditable(true)]
		public EntryInstructionAttachmentCollection Attachments
		{
			get
			{
				if (fAttachments == null)
				{
					fAttachments = new EntryInstructionAttachmentCollection(this);
					fAttachments.Load();
					RegisterEditableChildObject(fAttachments);
				}
				return fAttachments;
			}
		}

		Type ICusStorageDocPivotTypeSupporter.CusStorageDocPivotType => typeof(CusStorageDocPivot);

		void ICusStorageDocPivotTypeSupporter.ReloadCollection()
		{
			if (fAttachments != null)
			{
				ReloadCusStorageDocPivots();
				Attachments.Load();
			}
		}

		ZString ICusStorageDocPivotTypeSupporter.HumanReadableName => Res.GetString("9be07bcb-fc70-4667-b861-ba198a10b5e8", "Entry Instruction with CPC {0}", CEI_Style);

		public IEnumerable<IStorageDocsBaseCollection> EDocCollections
		{
			get
			{
				foreach (var eDocCollection in EDocsHelper.GetEDocCollections((CusEntryHeader)EntryHeader))
				{
					yield return eDocCollection;
				}

				var declaration = JobDeclaration;
				foreach (var eDocCollection in EDocsHelper.GetEDocCollections(declaration, declaration?.Shipment))
				{
					yield return eDocCollection;
				}
			}
		}

		#endregion

		#region LinkedFormalEntryHeader

		public CusEntryHeader LinkedFormalEntryHeader => JobDeclaration?.ActiveEntryHeaders?.OfType<CusEntryHeader>()?.Where(x => x.CH_CEI_Instruction == PK && x.IsFormalEntry).FirstOrDefault();

		#endregion

		#endregion

		#region Override Properties

		#region CEI_Style

		[MaxLength(4)]
		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|CEI_Style", Caption = "CPC", FullDescription = "Customs Procedure Code", MediumCaption = "Customs Procedure", ShortCaption = "Cus. Procedure")]
		public override ZString CEI_Style
		{
			get => base.CEI_Style;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(CusEntryInstruction.Schema.CEI_Style))
				{
					var oldValue = CEI_Style;
					base.CEI_Style = value;
					if (!IsCopying && oldValue != CEI_Style)
					{
						DefaultLevyTypeIfNeeded();
					}
				}
			}
		}

		void DefaultLevyTypeIfNeeded()
		{
			var declaration = JobDeclaration;
			if (declaration != null)
			{
				if (declaration.WillGenerateBothEntries && WillGenerateRecordListing || LevyTypeList.GetProcedureAllowEmptyLevyTypes(CEI_Style, declaration.IsImport))
				{
					CEI_LevyType = ZString.Empty;
				}
				else
				{
					var supportedLevyTypes = LevyTypeList.GetSupportedLevyTypesByProcedureCode(Factory, CEI_Style, JobDeclaration.IsImport);
					if (supportedLevyTypes.Any() && !supportedLevyTypes.Contains(CEI_LevyType.ToString()))
					{
						CEI_LevyType = supportedLevyTypes.First();
					}
				}
			}
		}

		#endregion

		#region Packages

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|XC_Packages", Caption = "No of Packages", ShortCaption = "No. Packs")]
		public override ZInt CEI_Packages
		{
			get => base.CEI_Packages;
			set
			{
				var oldValue = CEI_Packages;
				base.CEI_Packages = value;
				if (!IsCopying && oldValue != value)
				{
					if (!value.IsEmpty)
					{
						var relatedInstruction = ParentInstruction ?? ChildInstruction;
						if (relatedInstruction != null && relatedInstruction.CEI_Packages.IsEmpty)
						{
							relatedInstruction.CEI_Packages = value;
						}
					}
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|XC_PackageUQ", Caption = "Pack Type")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.PackageTypeList))]
		public override ZString CEI_PackageUQ
		{
			get => base.CEI_PackageUQ;
			set
			{
				var oldValue = CEI_PackageUQ;
				base.CEI_PackageUQ = value;
				if (!IsCopying && oldValue != value)
				{
					if (!value.IsEmpty)
					{
						var relatedInstruction = ParentInstruction ?? ChildInstruction;
						if (relatedInstruction != null && relatedInstruction.CEI_PackageUQ.IsEmpty)
						{
							relatedInstruction.CEI_PackageUQ = value;
						}
					}
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|XC_PackageUQDescription", Caption = "Package Type Description")]
		public ZString XC_PackageUQDescription => Lookups.PackageTypeList.GetDescriptionFromCode(CEI_PackageUQ);

		#endregion

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|CEI_Description", Caption = "Description")]
		public override ZString CEI_Description
		{
			get => base.CEI_Description;
			set => base.CEI_Description = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|XC_ManualNo", Caption = "Manual No.")]
		public override ZString CEI_ManualNo
		{
			get => base.CEI_ManualNo;
			set
			{
				var oldValue = CEI_ManualNo;
				base.CEI_ManualNo = value;
				if (!IsCopying && value != oldValue)
				{
					var relatedInstruction = ParentInstruction ?? ChildInstruction;
					if (relatedInstruction != null)
					{
						relatedInstruction.CEI_RelatedManualNo = value;
					}
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|XC_RelatedMRN", Caption = "Related Entry No.")]
		public override ZString CEI_RelatedMRN
		{
			get => base.CEI_RelatedMRN;
			set => base.CEI_RelatedMRN = value;
		}

		bool CEI_RelatedManualNoReadonly => JobDeclaration != null && JobDeclaration.WillGenerateBothEntries;

		[ReadOnlyMember(nameof(CEI_RelatedManualNoReadonly))]
		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|XC_RelatedManualNo", Caption = "Related Manual No.")]
		public override ZString CEI_RelatedManualNo
		{
			get => base.CEI_RelatedManualNo;
			set => base.CEI_RelatedManualNo = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|XC_CIQRelatedNum", Caption = "Related CIQ Entry")]
		public override ZString CEI_CIQRelatedNum
		{
			get => base.CEI_CIQRelatedNum;
			set
			{
				base.CEI_CIQRelatedNum = value;
				if (!IsCopying && !CEI_CIQRelatedNum.IsEmpty && IsCIQRequiresEditableAndFalse)
				{
					CEI_CIQRequires = true;
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|XC_CIQRelatedReason", Caption = "Related Reason")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.CIQRelations))]
		public override ZString CEI_CIQRelatedReason
		{
			get => base.CEI_CIQRelatedReason;
			set
			{
				base.CEI_CIQRelatedReason = value;
				if (!CEI_CIQRelatedReason.IsEmpty && IsCIQRequiresEditableAndFalse)
				{
					CEI_CIQRequires = true;
				}
			}
		}

		public override ZDateTime CEI_DateForDuty
		{
			get => base.CEI_DateForDuty;
			set
			{
				var oldValue = CEI_DateForDuty;
				base.CEI_DateForDuty = value;
				if (!IsCopying && oldValue != CEI_DateForDuty)
				{
					JobDeclaration?.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|CNE_ApplyForCombinedInspections", Caption = "Apply for Combined Inspections Across Customs Districts")]
		public override ZBool CNE_ApplyForCombinedInspections { get => base.CNE_ApplyForCombinedInspections; set => base.CNE_ApplyForCombinedInspections = value; }

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|CNE_ApplyForConditionalPickup", Caption = "Apply for Conditional Pick-up and Release of Goods")]
		public override ZBool CNE_ApplyForConditionalPickup { get => base.CNE_ApplyForConditionalPickup; set => base.CNE_ApplyForConditionalPickup = value; }

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|CNE_ApplyForTransition", Caption = "Apply for Transition")]
		public override ZBool CNE_ApplyForTransition { get => base.CNE_ApplyForTransition; set => base.CNE_ApplyForTransition = value; }

		[List(nameof(AddInfoChildLookups) + "." + nameof(CusCNEntryInstructionLookups.TransitionSiteList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|CNE_TransitionSite", Caption = "Transition Site")]
		public override ZString CNE_TransitionSite { get => base.CNE_TransitionSite; set => base.CNE_TransitionSite = value; }

		#region XC_LevyType

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|XC_LevyType", Caption = "Levy Type")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.LevyTypes))]
		public override ZString CEI_LevyType
		{
			get => base.CEI_LevyType;
			set
			{
				var oldValue = base.CEI_LevyType;
				base.CEI_LevyType = value;
				if (!IsCopying && CEI_LevyType != oldValue)
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateCEI_ManualNo();
					}
				}
			}
		}

		#endregion

		#region CEI_CEI_Parent

		[RelatedBusinessObject(nameof(ParentInstruction))]
		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|XC_CEI_Parent", Caption = "Parent")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.Parents))]
		public override ZGuid CEI_CEI_Parent
		{
			get => base.CEI_CEI_Parent;
			set
			{
				var oldParent = ParentInstruction;
				base.CEI_CEI_Parent = value;
				var newParent = ParentInstruction;

				if (!IsCopying && oldParent != newParent)
				{
					JobDeclaration?.CustomsEntryInstructions.Cast<CusEntryInstruction>().Where(x => x.PK != PK && x.CEI_CEI_Parent == CEI_CEI_Parent).ForEach(x => x.CEI_CEI_Parent = ZGuid.Empty);

					if (!newParent?.CEI_CEI_Parent.IsEmpty ?? false)
					{
						newParent.CEI_CEI_Parent = ZGuid.Empty;
					}

					if (IsChild)
					{
						if (newParent != null)
						{
							CEI_RelatedManualNo = newParent.CEI_ManualNo;
							newParent.CEI_RelatedManualNo = CEI_ManualNo;

							if (!CEI_Packages.IsEmpty && newParent.CEI_Packages.IsEmpty)
							{
								newParent.CEI_Packages = CEI_Packages;
							}
							else if (CEI_Packages.IsEmpty)
							{
								CEI_Packages = newParent.CEI_Packages;
							}

							if (!CEI_PackageUQ.IsEmpty && newParent.CEI_PackageUQ.IsEmpty)
							{
								newParent.CEI_PackageUQ = CEI_PackageUQ;
							}
							else if (CEI_PackageUQ.IsEmpty)
							{
								CEI_PackageUQ = newParent.CEI_PackageUQ;
							}
						}

						foreach (var invoiceLine in InvoiceLines)
						{
							using (invoiceLine.GetValidationSuspender())
							{
								invoiceLine.JI_CEI = ZGuid.Empty;
							}
						}
					}
					ChangeCIQRequiredIfNeeded();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Read only parameter")]
		bool CEI_CEI_Parent_ReadOnly => !(JobDeclaration?.WillGenerateBothEntries ?? false) || IsParent;

		public ZBool IsParent => ChildInstruction != null;

		public ZBool IsChild => ParentInstruction != null;

		public CusEntryInstruction ParentInstruction
		{
			get
			{
				if (parentInstructionCached == null)
				{
					parentInstructionCached = new CachedProperty<CusEntryInstruction>(Factory, () =>
					{
						return CEI_CEI_Parent.IsEmpty ? null : (CusEntryInstruction)JobDeclaration?.CustomsEntryInstructions.FindByPK(CEI_CEI_Parent);
					});
				}
				return parentInstructionCached.Value;
			}
		}
		CachedProperty<CusEntryInstruction> parentInstructionCached;

		public CusEntryInstruction ChildInstruction
		{
			get
			{
				if (childInstructionCached == null)
				{
					childInstructionCached = new CachedProperty<CusEntryInstruction>(Factory, () =>
					{
						return JobDeclaration?.CustomsEntryInstructions.Cast<CusEntryInstruction>().FirstOrDefault(x => x.CEI_CEI_Parent == PK);
					});
				}
				return childInstructionCached.Value;
			}
		}
		CachedProperty<CusEntryInstruction> childInstructionCached;

		[List(nameof(JobDeclaration) + "." + nameof(CN.Business.JobDeclaration.CustomsEntryInstructions))]
		public ZGuid ChildInstructionPK => ChildInstruction?.PK ?? ZGuid.Empty;

		#endregion

		#region CEI_JE

		public override ZGuid CEI_JE
		{
			get => base.CEI_JE;
			set
			{
				var oldValue = CEI_JE;
				var oldChildInstruction = ChildInstruction;
				base.CEI_JE = value;
				if (!IsCopying && oldValue != CEI_JE)
				{
					JobDeclaration?.MarkAsNeedingValidationIncludingChildren();
					if (oldChildInstruction != null)
					{
						oldChildInstruction.CEI_CEI_Parent = ZGuid.Empty;
					}
				}
			}
		}

		#endregion

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.IntelligentDeclarationTypeList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|CEI_SubStyle", Caption = "Intelligent Declaration Type", MediumCaption = "Intelligent Dec. Type", ShortCaption = "INT Dec. Type", FullDescription = "Intelligent Assisted Declaration Type")]
		public override ZString CEI_SubStyle { get => base.CEI_SubStyle; set => base.CEI_SubStyle = value; }

		bool LinkedEntryHasBeenLodgedAtCustomsOrIsWaitingForResponse
		{
			get
			{
				var result = false;
				var entryHeaders = JobDeclaration?.ActiveEntryHeaders.OfType<CusEntryHeader>().Where(x => x.CH_CEI_Instruction == this.PK);
				if (entryHeaders != null)
				{
					result = entryHeaders.Any(x => x.HasBeenLodgedAtCustoms) || entryHeaders.Any(x => x.IsWaitingForResponse);
				}
				return result;
			}
		}

		public override bool CanDelete => base.CanDelete && !LinkedEntryHasBeenLodgedAtCustomsOrIsWaitingForResponse;

		public override MultilingualString ReasonForNotAbleToDelete => LinkedEntryHasBeenLodgedAtCustomsOrIsWaitingForResponse
			? ResString.GetMultilingualString(
					"140a4365-f1ea-441f-8d32-a96ab9b48897",
					"Entry Instruction with CPC {0} is being used by an Entry Header which has been lodged at Customs waiting for response and cannot be deleted.",
					CEI_Style)
			: base.ReasonForNotAbleToDelete;

		#region CEI_CIQRequires

		[ReadOnlyMember(nameof(CEI_CIQRequiresReadonly))]
		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|XC_CIQRequires", Caption = "Requires CIQ")]
		public override ZBool CEI_CIQRequires { get => base.CEI_CIQRequires; set => base.CEI_CIQRequires = value; }

		public bool CEI_CIQRequiresReadonly => EntryHeader?.HasBeenLodgedAtCustoms ?? false;

		public void ChangeCIQRequiredIfNeeded()
		{
			if (!CEI_CIQRequiresReadonly && !IsCIQDataAllowed)
			{
				CEI_CIQRequires = false;
			}
		}

		[ResourceStringData("dda89927-8c4d-43c6-a798-f34a0fc4045f", Caption = "Doc. Submission", FullDescription = "Document Submission Type")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.EntryDocumentSubmissionType))]
		public override ZString CEI_DocumentSubmissionType
		{
			get => base.CEI_DocumentSubmissionType; set
			{
				var oldValue = base.CEI_DocumentSubmissionType;
				base.CEI_DocumentSubmissionType = value;
				if (!IsCopying && oldValue != CEI_DocumentSubmissionType)
				{
					JobDeclaration?.MarkAsNeedingValidation();
					JobDeclaration?.ActiveEntryHeaders?.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|XC_EnterprisePromised", Caption = "Enterprise Promise: Enterprise holds the qualification guarantee label and other certification declaration materials required by the customs, knows the content of relevant materials, guarantees compliance with the requirements of laws and regulations, and keeps the documents.")]
		public override ZBool CEI_EnterprisePromised { get => base.CEI_EnterprisePromised; set => base.CEI_EnterprisePromised = value; }

		public ZBool IsCIQDataAllowed => JobDeclaration == null || !JobDeclaration.WillGenerateBothEntries || WillGenerateCustomsEntry;
		public ZPropertyInfo IsCIQDataAllowedInfo => GetZPropertyInfo(nameof(IsCIQDataAllowed));

		public ZBool TwoStageAccessApplicable => CEI_CIQRequires && WillGenerateEnteringEntry;

		void ClearCIQDatasIfNotRequired()
		{
			if (!CEI_CIQRequires)
			{
				CEI_CIQRelatedNum = CEI_CIQRelatedReason = ZString.Empty;
				EnterpriseQualifications.RemoveAndDeleteAll();
				SpecialBusinessIdentifiers.RemoveAndDeleteAll();
				CIQRequiredDocuments.RemoveAndDeleteAll();
			}

			if (!TwoStageAccessApplicable)
			{
				CNE_ApplyForCombinedInspections = false;
				CNE_ApplyForConditionalPickup = false;
				CNE_ApplyForTransition = false;
				CNE_TransitionSite = ZString.Empty;
			}
		}

		#endregion

		#endregion

		#region Override Methods

		protected override Customs.Business.CusEntryInstructionValidation GetNewValidation()
		{
			return new CusEntryInstructionValidation(this);
		}

		protected override Customs.Business.CusEntryInstructionLookups GetNewLookups()
		{
			return new CusEntryInstructionLookups(this);
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override void ResetValuesAfterCloneCore()
		{
			base.ResetValuesAfterCloneCore();
			CEI_CEI_Parent = ZGuid.Empty;
		}

		public override void Delete()
		{
			base.Delete();
			this.DeleteHiddenNotes();
			BillOfLadingNumber?.Delete();
			Attachments.RemoveAndDeleteAll();
			EnterpriseQualifications.RemoveAndDeleteAll();
			SpecialBusinessIdentifiers.RemoveAndDeleteAll();

			CIQRequiredDocuments.RemoveAndDeleteAll();
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			ClearCIQDatasIfNotRequired();
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new FetchStrategies.CusEntryInstructionFetchStrategy(this);
		}

		#endregion

		#region CusEntryNumbers

		CusEntryNumber BillOfLadingNumber
		{
			get
			{
				if (billOfLadingNumber == null || billOfLadingNumber.IsDeleted)
				{
					billOfLadingNumber = CusEntryNumber.Load(this, CusEntryNumberTypes.China.BillOfLading, JobDeclaration?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					if (billOfLadingNumber != null)
					{
						RegisterEditableChildObject(billOfLadingNumber);
					}
				}
				return billOfLadingNumber;
			}
		}

		CusEntryNumber billOfLadingNumber;

		[MaxLength(32)]
		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|BillOfLading", Caption = "B/L No.")]
		public ZString BillOfLading
		{
			get => BillOfLadingNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				if (BillOfLading != value)
				{
					if (BillOfLadingNumber == null)
					{
						billOfLadingNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.China.BillOfLading, JobDeclaration?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
						RegisterEditableChildObject(billOfLadingNumber);
					}

					CheckMaximumLength(BillOfLadingInfo, value);
					BillOfLadingNumber.CE_EntryNum = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateBillOfLading();
					}
					BillOfLadingInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo BillOfLadingInfo => GetZPropertyInfo(Schema.BillOfLading);

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|BillOfLadingDate", Caption = "Issue Date")]
		public ZDateTime BillOfLadingDate
		{
			get => BillOfLadingNumber?.CE_IssueDate ?? ZDateTime.Empty;
			set
			{
				if (BillOfLadingDate != value)
				{
					if (BillOfLadingNumber == null)
					{
						billOfLadingNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.China.BillOfLading, JobDeclaration?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
						RegisterEditableChildObject(billOfLadingNumber);
					}
					BillOfLadingNumber.CE_IssueDate = value;
					BillOfLadingDateInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo BillOfLadingDateInfo =>
			BillOfLadingNumber == null ? GetZPropertyInfo(Schema.BillOfLadingDate) : GetWrappedZPropertyInfo(Schema.BillOfLadingDate, x => BillOfLadingNumber.CE_IssueDateInfo);

		#endregion

		#region StmNote

		[MaxLength(70)]
		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|CustomsMessageRemarks", Caption = "Remarks")]
		public ZString CustomsMessageRemarks
		{
			get => CustomsMessageRemarksNote.Text;
			set => CustomsMessageRemarksNote.SetNoteText(this, CustomsMessageRemarksInfo, value);
		}

		HiddenTextNote CustomsMessageRemarksNote => customsMessageRemarksNote ?? (customsMessageRemarksNote = new HiddenTextNote(this, PredefinedNoteTypes.Instance.CustomsMessageRemarks.Description));
		HiddenTextNote customsMessageRemarksNote;

		public ZPropertyInfo CustomsMessageRemarksInfo => GetZPropertyInfo(Schema.CustomsMessageRemarks);

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(Constants.CusCodeDataTypes.Codes.EnterpriseQualification, typeof(EnterpriseQualification));
			result.Add(Constants.CusCodeDataTypes.Codes.SpecialBusinessIdentifier, typeof(SpecialBusinessIdentifier));
			result.Add(Constants.CusCodeDataTypes.Codes.Package, typeof(OtherPackage));
			result.Add(Constants.CusCodeDataTypes.Codes.OperationMatter, typeof(OperationMatter));
			result.Add(Constants.CusCodeDataTypes.Codes.CusAttachment, typeof(CusAttachment));
			return result;
		}

		#endregion

		#region Implementation of IAdditionalBusinessObjectFetchStrategyProvider

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
			yield return new CusAddInfoTypeSupporterFetchStrategy(this);
		}

		#endregion

		#region Implementation of ICusAddInfoTypeSupporter

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.CIQRequiredDocument, typeof(CIQRequiredDocument));
			return result;
		}

		#endregion

		#region CusSupportingDocuments

		internal IEnumerable<CusSupportingDocument> CusSupportingDocuments
		{
			get
			{
				return InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.CusSupportingDocuments.OfType<CusSupportingDocument>());
			}
		}

		#endregion

		#region IAddInfoChildSupporter Members

		protected override BusinessObject GetAddInfoChild() => AddInfoChild;

		protected override SchemaGuidColumn GetChildForeignKeyColumn() => CusCNEntryInstructionSchema.CNE_CEI;

		#endregion
	}
}
