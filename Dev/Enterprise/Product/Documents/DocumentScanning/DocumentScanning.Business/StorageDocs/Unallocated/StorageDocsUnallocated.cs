using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.PreviewableDocument;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Business
{
	[CodeProperty(Schema.Code)]
	public class StorageDocsUnallocated : StorageDocsBase, IStorageDocsUnallocated, IDocManagerSupport
	{
		#region Schema

		public new class Schema : AutoStorageDocs.Schema
		{
			public const string Code = "Code";
		}

		public ZString Code
		{
			get { return "ABC"; }
		}

		#endregion

		public StorageDocsUnallocated(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Validation

		public new StorageDocsUnallocatedValidation Validation
		{
			get { return (StorageDocsUnallocatedValidation)base.Validation; }
		}

		protected override StorageDocsValidation GetNewValidation()
		{
			return new StorageDocsUnallocatedValidation(this);
		}

		#endregion

		#region Properties

		public bool IsAutoAllocatable
		{
			get
			{
				return (!SC_DocType.IsEmpty &&
					(!SC_DataType.IsEmpty && SC_DataType != Core.Constants.ReferenceTypes.Unallocated) &&
					!HasErrors && CanBeAutoAllocated());
			}
		}

		bool CanBeAutoAllocated()
		{
			return SC_ParentID.IsEmpty || (MasterFactory.Load(OwnerAssemblyData.BusinessObjectType, SC_ParentID) != null);
		}

		public bool SC_DocType_ReadOnly
		{
			get
			{
				return (SC_IsSystemGenerated || SC_DataType.IsEmpty ||
						 SC_DataType == Core.Constants.DocManagerCodes.Unallocated || SM_TypeInfo.HasErrors());
			}
		}

		public bool SC_ParentID_ReadOnly
		{
			get
			{
				return SC_IsSystemGenerated || SC_DataType.IsEmpty || SC_DataType == Core.Constants.DocManagerCodes.Unallocated || IsCollectionTypeNotValid;
			}
		}

		public IAssemblyData OwnerAssemblyData
		{
			get
			{
				if (SC_DataType.IsEmpty || SC_DataType == Core.Constants.DocManagerCodes.Unallocated)
				{
					return AssemblyDataLookup.Unallocated;
				}
				else
				{
					if (fOwnerAssemblyData == null || fOwnerAssemblyData.DocManagerCode != SC_DataType)
					{
						IAssemblyData newData = AssemblyDataLookup.GetAssemblyDataFromDocManagerCode(SC_DataType, false);

						if (newData != null)
						{
							fOwnerAssemblyData = newData;
							enteredTypeValid = true;
						}
						else
						{
							enteredTypeValid = false;
						}
					}
				}
				return fOwnerAssemblyData;
			}
		}

		public ModuleIdentifier ModuleID
		{
			get { return OwnerAssemblyData.ModuleID; }
		}

		internal bool enteredTypeValid;

		IAssemblyData fOwnerAssemblyData;

		bool IsPdf => PreviewableDocumentHelper.IsPdf(SC_ImageDataFromDb);

		public override ZString EDocFormat => IsPdf ? Core.Constants.FileFormats.PDF : Core.Constants.FileFormats.TIF;

		protected override ZString HackedFileExtensionOnlyForIDeliverable => EDocFormat;

		public override CodeDescriptionPairList SC_DocType_List
		{
			get
			{
				if (docTypeList == null || SC_DataType != listSM_Type)
				{
					listSM_Type = SC_DataType;
					docTypeList = BuildSC_DocType_List();
				}

				return docTypeList;
			}
		}

		CodeDescriptionPairList docTypeList;
		ZString listSM_Type;

		[MaxLength(3)]
		public override ZString SM_Type
		{
			get
			{
				return SC_DataType;
			}
			set
			{
				if (SC_DataType != value)
				{
					SC_ParentID = ZGuid.Empty;
				}

				SC_DataType = value;
				if (SC_DataType == Core.Constants.DocManagerCodes.Unallocated)
				{
					SC_DocType = ZString.Empty;
				}
				else
				{
					UpdateDescriptionIfRequired();
				}

				Validation.ValidateSC_DocType();
				Validation.ValidateSC_Desc();
			}
		}

		public override ZPropertyInfo SM_TypeInfo
		{
			get { return GetZPropertyInfo("SC_DataType"); }
		}

		public override StorageMain ParentMain
		{
			get { return null; }
		}

		public override bool IsAllocated
		{
			get { return !SC_ParentID.IsEmpty; }
		}

		public override ZGuid SC_SM
		{
			get
			{
				return ZGuid.Empty;
			}
			set
			{ }
		}

		public override ZBool SC_IsDeleted
		{
			get { return base.SC_IsDeleted; }
			set
			{
				base.SC_IsDeleted = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateSC_DocType();
					Validation.ValidateSC_Desc();
					Validation.ValidateSC_DataType();
				}
			}
		}

		#region DocsBarcode

#if DEBUG
		public
#else

		internal
#endif
		StorageDocsBarcode StorageDocsBarcode
		{
			get
			{
				return MasterFactory.LoadTop1<StorageDocsBarcode>(new ZQuery(StorageDocsBarcodeSchema.SCB_SC, PK));
			}
		}

		#endregion

		#region ScannedBarcodeValue

		[MaxLength(StorageDocsBarcode.Schema.SCB_BarcodeMaxLength)]
		public ZString ScannedBarcodeValue
		{
			get
			{
				return StorageDocsBarcode == null ? ZString.Empty : StorageDocsBarcode.SCB_Barcode;
			}
			set
			{
				if (StorageDocsBarcode == null)
				{
					if (!string.IsNullOrEmpty(value))
					{
						var docsBarcode = Factory.New<StorageDocsBarcode>();
						docsBarcode.SCB_SC = PK;
						docsBarcode.SCB_Barcode = value;
					}
				}
				else
				{
					if (string.IsNullOrEmpty(value))
					{
						StorageDocsBarcode.Delete();
					}
					else
					{
						StorageDocsBarcode.SCB_Barcode = value;
					}
				}
				ScannedBarcodeValueInfo.RefreshBinding();
			}
		}

		public bool ScannedBarcodeValue_ReadOnly
		{
			get { return true; }
		}

		public ZPropertyInfo ScannedBarcodeValueInfo
		{
			get { return GetZPropertyInfo(nameof(ScannedBarcodeValue)); }
		}

		#endregion

		#region RefType List

		public CodeDescriptionPairList SC_FormCategory_List => SC_IsDeleted ? AssemblyDataLookup.DocManagerCodes : AssemblyDataLookup.DocManagerCodesForAllocation;

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.ReferenceTypes.Unallocated);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		public override bool CompanyCode_ReadOnly => false;
		public override bool BranchCode_ReadOnly => false;
		public override bool DepartmentCode_ReadOnly => false;

		#endregion

		#region Methods

		public string SetAllocateStatus(DocumentResult scannedDocument)
		{
			var barcode = scannedDocument as BaseBarcode;

			if (IsAutoAllocatable && barcode != null && !barcode.RefPK.IsEmpty)
			{
				AllocationStatus = Res.GetString("282f043f-36dc-4878-9161-14a1f1cb116b", "Allocatable");
			}
			else if (barcode == null)
			{
				AllocationStatus = Res.GetString("622298be-0ae1-4a74-a38f-8446c33a1135", "No suitable barcode detected");
			}
			else if (string.IsNullOrWhiteSpace(barcode.FullBarcodeText))
			{
				AllocationStatus = Res.GetString("a111c04e-a126-4425-a4e1-702903c481ab", "Barcode text missing/not found");
			}
			else if (!BarcodeHelper.IsValidBarcodeType(barcode.FullBarcodeText))
			{
				AllocationStatus = Res.GetString("aec49e03-220b-4a46-a660-e958ad44abb9", "Barcode type unrecognized (not Shipment or Document Type)");
			}
			else if (barcode.DocType.IsEmpty)
			{
				AllocationStatus = Res.GetString("62ba1645-09a2-4080-8bf8-a228a84d6737", "Barcode lacks a Document Type");
			}
			else if (barcode.DocManagerCode.IsEmpty)
			{
				AllocationStatus = Res.GetString("be200a11-76a1-487a-9cd8-4d90a78542b9", "Barcode lacks a Document Manager Code");
			}
			else if (barcode.RefPK.IsEmpty)
			{
				AllocationStatus = Res.GetString("e82b6255-f679-4f61-9d71-ea60201e861e", "Could not find related Job for Barcode");
			}
			else if (SM_Type.IsEmpty)
			{
				AllocationStatus = Res.GetString("0d61c086-a5e7-454b-b27d-4bf5657f7e1f", "eDocs Storage lacks a type");
			}
			else if (SM_Type == Core.Constants.ReferenceTypes.Unallocated)
			{
				AllocationStatus = Res.GetString("cc29bcda-5b27-49c7-8a69-c63be08dff6b", "eDocs Storage's type is Unallocated");
			}
			else if (SC_DescMultilingual.IsEmpty)
			{
				AllocationStatus = Res.GetString("db946308-03d9-4b9b-b6d4-0df9de42a0b9", "Description is empty");
			}
			else if (HasErrors)
			{
				AllocationStatus = Res.GetString("52118c0b-add1-45b9-b22b-028d11b75529", "Validation errors prevent allocation");
			}

			return AllocationStatus;
		}

#if DEBUG
		public
#else
		internal
#endif
		class HiddenStorageDocsNote : HiddenTextNote
		{
			public HiddenStorageDocsNote(IStmNoteParent parent, string description = null) : base(parent, description) { }

			protected override void Factory_Saving(BusinessObjectFactory factory)
			{
				//do not delete if empty automatically - creates odd concurrency problems if two people are editing Allocate EDocs at once.
				//deleting will be done when the StorageDocs is allocated
			}
		}

		public ZString AllocationStatus
		{
			get
			{
				return AllocationStatusNote.Text;
			}
			set
			{
				AllocationStatusNote.Text = value;
			}
		}

#if DEBUG
		public
#else
		internal
#endif
		HiddenStorageDocsNote AllocationStatusNote
		{
			get
			{
				return allocationStatusNote ?? (allocationStatusNote = new HiddenStorageDocsNote(this, (NoResString)"Allocation Status"));
			}
		}
		HiddenStorageDocsNote allocationStatusNote;

		public ZString AllocationNotes
		{
			get
			{
				return AllocationNotesNote.Text;
			}
			set
			{
				AllocationNotesNote.Text = value;
			}
		}

#if DEBUG
		public
#else
		internal
#endif
		HiddenStorageDocsNote AllocationNotesNote
		{
			get
			{
				return allocationNotesNote ?? (allocationNotesNote = new HiddenStorageDocsNote(this, (NoResString)"Allocation Notes"));
			}
		}
		HiddenStorageDocsNote allocationNotesNote;

		public override void Delete()
		{
			AllocationStatusNote.Delete();
			AllocationNotesNote.Delete();
			if (StorageDocsBarcode != null)
			{
				StorageDocsBarcode.Delete();
			}
			base.Delete();
		}

		protected override DocTypeCategoryQuery GetDocTypeCategoryQuery()
		{
			ZString refType = (AssemblyDataLookup.IsDocManagerCodeValid(SC_DataType, false)) ?
				AssemblyDataLookup.GetReferenceTypeFromDocManagerCode(SC_DataType) : Core.Constants.ReferenceTypes.Unallocated;

			return new DocTypeCategoryQuery(MasterFactory, refType);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SM_Type = Core.Constants.DocManagerCodes.Unallocated;
		}

		#endregion

		#region Lists

		#region SC_ParentID_List

		public IBusinessObjectCollection SC_ParentID_List
		{
			get
			{
				if (!IsCollectionTypeNotValid)
				{
					return collection;
				}
				else
				{
					return new NonPersistentUnallocatedObjectCollection(MasterFactory);
				}
			}
		}

		IBusinessObjectCollection collection;
		bool IsCollectionTypeNotValid
		{
			get
			{
				if (OwnerAssemblyData == null || !enteredTypeValid)
				{
					return true;
				}
				collection = OwnerAssemblyData.GetBusinessObjectCollection(MasterFactory);
				return collection == null;
			}
		}

#if DEBUG
		public
#else
		protected internal
#endif
		override bool SC_IsPublishedReadonlyDefault => false;

		public override bool IsImageFile => !IsPdf;

		protected override DeliveryInfo.DeliveryFormats DeliveryFormats => IsImageFile ? DeliveryInfo.DeliveryFormats.TIFF : DeliveryInfo.DeliveryFormats.File;

		protected override ZString NameCore => Res.GetString("3ADD81B2-1188-40B1-A929-953841013F02", "Unallocated eDoc");

		#endregion

		#endregion
		public override void SetNewFileName(string fileName)
		{
			// This method is only called from EditPropertiesFileForm which doesn't support unallocated documents.
			throw new NotImplementedException();
		}

		protected override IEnumerable<string> GetSupportedDeliveryMethodsCore()
		{
			return Core.Constants.ContactNotifyModes.All;
		}
	}
}
