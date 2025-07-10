using System.Collections.Generic;
using System.Data;
using System.Drawing.Imaging;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.Imaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

namespace Enterprise.DocumentScanning.Business
{
	public class StorageDocs : StorageDocsBase, IStorageDocs
	{
		public StorageDocs(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Testing Only
#if DEBUG
		public static StorageDocs New_DEBUG(BusinessObjectFactory factory)
		{
			return (StorageDocs)factory.New(typeof(StorageDocs));
		}

		public static StorageDocs NewWithParent_DEBUG(NumberedBusinessObjectFactory factory)
		{
			OrgHeader org = factory.NewWithValidTestData<OrgHeader>(); //storage main requires parent. If you want another one - reassign.

			StorageDocs doc = (StorageDocs)factory.NewWithParent(typeof(StorageDocs));
			doc.ParentMain.SM_ParentFK = org.PK;
			doc.ParentMain.SM_DB = factory.DBNumber == 0 ? 1 : factory.DBNumber;
			return doc;
		}

		public static StorageDocs NewWithParentWithoutFK_DEBUG(NumberedBusinessObjectFactory factory)
		{
			var doc = (StorageDocs)factory.NewWithParent(typeof(StorageDocs));
			doc.ParentMain.SM_DB = factory.DBNumber == 0 ? 1 : factory.DBNumber;
			return doc;
		}
#endif
		#endregion

		#region Validation

		public new StorageDocsRealValidation Validation
		{
			get { return (StorageDocsRealValidation)base.Validation; }
		}

		protected override StorageDocsValidation GetNewValidation()
		{
			return new StorageDocsRealValidation(this);
		}

		#endregion

		#region Methods

		public ModuleIdentifier ModuleID
		{
			get { return OwnerAssemblyData.ModuleID; }
		}

		protected override bool ShouldUpdateDescAndPublishedFlagsWhileSettingDocType
		{
			get { return !SC_IsSystemGenerated; }
		}

		public override bool ReadOnly
		{
			get { return base.ReadOnly || SC_IsDeleted; }
			set { base.ReadOnly = value; }
		}

		public override bool IsImageFile
		{
			get { return true; }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "eDoc"; }
		}

		protected override DocTypeCategoryQuery GetDocTypeCategoryQuery()
		{
			ZString refType = (ParentMain != null && AssemblyDataLookup.IsDocManagerCodeValid(ParentMain.SM_Type)) ?
				AssemblyDataLookup.GetReferenceTypeFromDocManagerCode(ParentMain.SM_Type) : Core.Constants.ReferenceTypes.Unallocated;

			return new DocTypeCategoryQuery(MasterFactory, refType);
		}

		public override void SetNewFileName(string fileName)
		{
			SC_FileName = fileName;
		}

		#endregion

		#region Related Business Objects

		#region DocumentOwner

		protected virtual BusinessObject DocumentOwner
		{
			get { return (ParentMain != null && !ParentMain.SM_ParentFK.IsEmpty) ? MasterFactory.Load(OwnerAssemblyData.BusinessObjectType, ParentMain.SM_ParentFK) : null; }
		}

		#endregion

		#endregion

		#region Properties
#if DEBUG
		public
#else
		protected internal
#endif
		override bool SC_IsPublishedReadonlyDefault
		{
			get { return false; }
		}

		#region RefType List

		public CodeDescriptionPairList SC_FormCategory_List => AssemblyDataLookup.DocManagerCodes;

		#endregion

		#region SC_DocType

		public bool SC_DocType_ReadOnly
		{
			get
			{
				return (SC_IsSystemGenerated || ParentMain == null || ParentMain.SM_TypeInfo.HasErrors() || ParentMain.SM_Type.IsEmpty ||
						 ParentMain.SM_Type == Core.Constants.DocManagerCodes.Unallocated);
			}
		}

		#endregion

		#region SC_ImageData

		public override ZBlob SC_ImageData
		{
			get
			{
				return base.SC_ImageData;
			}
			set
			{
				if (value != null && DocManagerRegistry.Instance.ConvertImagesToJpeg.Value)
				{
					using var memoryStream = new MemoryStream(value);
					using var image = AllocateDocumentsManager.AsImage(memoryStream);
					if (image != null
						&& !AllocateDocumentsManager.IsGifImage(image)
						&& !AllocateDocumentsManager.IsJpegImage(image))
					{
						var parameters = new EncoderParameters(1);
						parameters.Param[0] = new EncoderParameter(Encoder.Quality, (long)75);
						var jpegData = TiffToJpegConverter.TryShrinkImageByConvertingToJpeg(image, parameters);
						if (jpegData != null && jpegData.Length < value.Length)
						{
							value = jpegData;
							SC_DataType = Core.Constants.FileFormats.JPG;
						}
					}
				}

				base.SC_ImageData = value;
			}
		}

		protected override bool ShouldDeleteTempFile
		{
			get
			{
				return base.ShouldDeleteTempFile && WasOpenInExternalEditor;
			}
		}

		#endregion

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

		public bool SC_SaveVersions_ReadOnly
		{
			get { return DocType != null && (bool)DocType.RT_SaveVersions; }
		}

#endregion

		#region eDoc Format

		public override ZString EDocFormat
		{
			get
			{
				// Default format is TIF
				var format = Core.Constants.FileFormats.TIF;
				using var stream = new MemoryStream(SC_ImageData);
				using var image = AllocateDocumentsManager.AsImage(stream);

				if (image != null)
				{
					if (ImageFormat.Jpeg.Equals(image.RawFormat))
					{
						format = Core.Constants.FileFormats.JPG;
					}

					if (ImageFormat.Png.Equals(image.RawFormat))
					{
						format = Core.Constants.FileFormats.PNG;
					}

					if (ImageFormat.Gif.Equals(image.RawFormat))
					{
						format = Core.Constants.FileFormats.GIF;
					}

					if (ImageFormat.Bmp.Equals(image.RawFormat))
					{
						format = Core.Constants.FileFormats.BMP;
					}
				}

				return format;
			}
		}

		#endregion

		#region Owner Assembly Data

		public IAssemblyData OwnerAssemblyData
		{
			get
			{
				if (ParentMain == null)
				{
					return AssemblyDataLookup.AllAssemblyData.Unallocated;
				}
				else
				{
					if (ownerAssemblyData == null || ownerAssemblyData.DocManagerCode != ParentMain.SM_Type)
					{
						var newData = AssemblyDataLookup.GetAssemblyDataFromDocManagerCode(ParentMain.SM_Type);

						if (newData != null)
						{
							ownerAssemblyData = newData;
						}
					}
				}
				return ownerAssemblyData;
			}
		}

		IAssemblyData ownerAssemblyData;

		#endregion

		#region Lists

		protected override CodeDescriptionPairList GetDocumentTypeLookupList()
		{
			CodeDescriptionPairList codes = base.GetDocumentTypeLookupList();
			CodeDescriptionPair pairPrv = new CodeDescriptionPair(Core.Constants.RefDocTypes.InternallyCreatedPrivateDocument, Core.Constants.RefDocTypeDescriptions.InternallyCreatedPrivateDocument);
			CodeDescriptionPair pairPub = new CodeDescriptionPair(Core.Constants.RefDocTypes.InternallyCreatedPublicDocument, Core.Constants.RefDocTypeDescriptions.InternallyCreatedPublicDocument);
			if (!codes.Contains(pairPrv))
			{
				codes.AddPair(pairPrv.Code, pairPrv.Description);
			}
			if (!codes.Contains(pairPub))
			{
				codes.AddPair(pairPub.Code, pairPub.Description);
			}
			return codes;
		}

		#endregion

		#region IDeliverable Members

		protected override DeliveryInfo.DeliveryFormats DeliveryFormats => DeliveryInfo.DeliveryFormats.TIFF;

		protected override IEnumerable<string> GetSupportedDeliveryMethodsCore()
		{
			return Core.Constants.ContactNotifyModes.All;
		}

		protected override ZString HackedFileExtensionOnlyForIDeliverable => OrgConstants.AttachmentType.TIF;

		protected override ZString NameCore
		{
			get { return SC_DescMultilingual; }
		}

		public override ZString NameForBinding => SC_FileNameWithExtension;

		#endregion

		#region FillWithValidTestData
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			SetDefaultValueForParentMain();
		}

		protected void SetDefaultValueForParentMain()
		{
			if (ParentMain == null)
			{
				MasterFactory.CreateParentFor(this);
			}

			ParentMain.SM_DB = 1;
		}
#endif
#endregion

		#region WasOpenInExternalEditor

		public ZBool WasOpenInExternalEditor { get; set; }

		#endregion
	}
}
