using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.CN.Business
{
	public class EntryInstructionAttachment : NonPersistentBusinessObject
	{
		public EntryInstructionAttachment(EntryInstructionAttachmentCollection parent)
		{
			ParentCollection = Argument.NotNull(parent, nameof(parent));
			EntryInstruction = parent.EntryInstruction;
		}

		public EntryInstructionAttachment(EntryInstructionAttachmentCollection parent, CusStorageDocPivot cusStorageDocPivot) : this(parent)
		{
			this.CusStorageDocPivot = cusStorageDocPivot;
			if (cusStorageDocPivot != null)
			{
				fAttachmentType = cusStorageDocPivot.CSD_DocType.Left(AttachmentTypeInfo.MaxLength);
			}
		}

		public EntryInstructionAttachment(EntryInstructionAttachmentCollection parent, CusAttachment cusAttachment) : this(parent)
		{
			this.cusAttachment = cusAttachment;
			if (cusAttachment != null)
			{
				fAttachmentType = cusAttachment.CY_Code.Left(AttachmentTypeInfo.MaxLength);
			}
		}

		EntryInstructionAttachmentCollection ParentCollection { get; }
		public CusEntryInstruction EntryInstruction { get; }
		public CusStorageDocPivot CusStorageDocPivot { get; private set; }
		CusAttachment cusAttachment;

		#region Properties
		[MaxLength(8)]
		[List(nameof(Lookups) + "." + nameof(EntryInstructionAttachmentLookups.AttachmentTypeList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.EntryInstructionAttachment|AttachmentType", Caption = "Attachment Type", ShortCaption = "Type")]
		public ZString AttachmentType
		{
			get => fAttachmentType;
			set
			{
				if (fAttachmentType != value)
				{
					SetNonPersistentPropertyValue(AttachmentTypeInfo, ref fAttachmentType, value);

					if (RequireNumbersOnly(value))
					{
						if (cusAttachment == null)
						{
							cusAttachment = ParentCollection.CreateNewCusAttachment();
						}

						if (CusStorageDocPivot != null)
						{
							ParentCollection.RemoveAndDeleteCusStorageDocPivot(CusStorageDocPivot);
							CusStorageDocPivot = null;
						}
						cusAttachment.CY_Code = value;
					}
					else
					{
						if (CusStorageDocPivot == null)
						{
							CusStorageDocPivot = ParentCollection.CreateNewCusStorageDocPivot();
						}

						if (cusAttachment != null)
						{
							ParentCollection.RemoveAndDeleteCusAttachment(cusAttachment);
							cusAttachment = null;
						}
						CusStorageDocPivot.CSD_DocType = value;
					}

					Validation.ValidateAttachmentType();
					AttachmentTypeInfo.RefreshBinding();
				}
			}
		}

		static bool RequireNumbersOnly(ZString value)
		{
			return value == CSDDocTypeList.Codes._10000001 || value == CSDDocTypeList.Codes._10000002 || value == CSDDocTypeList.Codes._10000003 || value == CSDDocTypeList.Codes._10000004;
		}

		public void RemoveAndDeleteFromParent()
		{
			if (CusStorageDocPivot != null)
			{
				ParentCollection.RemoveAndDeleteCusStorageDocPivot(CusStorageDocPivot);
				CusStorageDocPivot = null;
			}
			if (cusAttachment != null)
			{
				ParentCollection.RemoveAndDeleteCusAttachment(cusAttachment);
				cusAttachment = null;
			}
		}

		ZString fAttachmentType;

		public ZPropertyInfo AttachmentTypeInfo => GetZPropertyInfo(nameof(AttachmentType));

		[ResourceStringData("Enterprise.Customs.CN.Business.EntryInstructionAttachment|AttachmentTypeDescription", Caption = "Attachment Type Description", MediumCaption = "Type Description", ShortCaption = "Type Desc.")]
		public ZString AttachmentTypeDescription => Lookups.AttachmentTypeList.GetDescriptionFromCode(AttachmentType);

		public ZPropertyInfo AttachmentTypeDescriptionInfo => GetZPropertyInfo(nameof(AttachmentTypeDescription));

		[MaxLength(255)]
		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(AttachmentNumberReadonly))]
		[ResourceStringData("Enterprise.Customs.CN.Business.EntryInstructionAttachment|AttachmentNumber", Caption = "Attachment Number", MediumCaption = "Number", ShortCaption = "Num.")]
		public ZString AttachmentNumber
		{
			get
			{
				if (RequireNumbersOnly(AttachmentType))
				{
					return cusAttachment?.CY_Data ?? ZString.Empty;
				}
				return ZString.Empty;
			}
			set
			{
				if (RequireNumbersOnly(AttachmentType))
				{
					if (cusAttachment == null)
					{
						cusAttachment = ParentCollection.CreateNewCusAttachment();
					}
					cusAttachment.CY_Data = value;
					Validation.ValidateAttachmentNumber();
					AttachmentNumberInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo AttachmentNumberInfo => GetZPropertyInfo(nameof(AttachmentNumber));

		public bool AttachmentNumberReadonly => !RequireNumbersOnly(AttachmentType);

		[List(nameof(Lookups) + "." + nameof(EntryInstructionAttachmentLookups.EDocList))]
		[ReadOnlyMember(nameof(EDocReadonly))]
		[ResourceStringData("Enterprise.Customs.CN.Business.EntryInstructionAttachment|EDoc", Caption = "eDoc")]
		public ZGuid EDoc
		{
			get
			{
				if (!RequireNumbersOnly(AttachmentType))
				{
					return CusStorageDocPivot?.CSD_StorageDocReference ?? ZGuid.Empty;
				}
				return ZGuid.Empty;
			}
			set
			{
				if (!RequireNumbersOnly(AttachmentType))
				{
					if (CusStorageDocPivot == null)
					{
						CusStorageDocPivot = ParentCollection.CreateNewCusStorageDocPivot();
					}
					CusStorageDocPivot.CSD_StorageDocReference = value;
					Validation.ValidateEDoc();
					EDocInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo EDocInfo => GetZPropertyInfo(nameof(EDoc));

		public bool EDocReadonly => RequireNumbersOnly(AttachmentType);

		#endregion

		#region Lookups

		public EntryInstructionAttachmentLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = new EntryInstructionAttachmentLookups(this);
				}

				return fLookups;
			}
		}

		EntryInstructionAttachmentLookups fLookups;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public EntryInstructionAttachmentValidation Validation => new EntryInstructionAttachmentValidation(this);

		#endregion

		#region Link To Invoice Line

		public bool CanLinkToInvoiceLine => CSDDocTypeList.CanLinkToInvoiceLine(AttachmentType);

		public ZShort[] GetLinkedEntryLineNumbers()
		{
			ZShort[] result = null;

			if (CanLinkToInvoiceLine && CusStorageDocPivot != null)
			{
				result = CusStorageDocPivot.InvoiceLineLinks.Select(x => x.Relation2Object).Cast<JobComInvoiceLine>()
					.Select(l => l.CusEntryLine).WhereNotNull().Distinct().Select(n => n.CL_LineNumber).OrderBy(n => n).ToArray();
			}

			return result ?? Array.Empty<ZShort>();
		}

		#endregion

		public IeDoc Document
		{
			get
			{
				if (document == null)
				{
					var docRef = EDoc;
					if (docRef.IsValid)
					{
						document = EntryInstruction.EDocCollections.Select(x => x.GetFromUniqueKey(docRef.ToGuid())).FirstOrDefault(x => x != null);
					}
				}
				return document;
			}
		}
		IeDoc document;

		public ZString FileName => Document?.FileName ?? ZString.Empty;

		public override bool HasChanges
		{
			get => (CusStorageDocPivot?.HasChanges ?? false) || (cusAttachment?.HasChanges ?? false);
			set => base.HasChanges = value;
		}
	}
}
