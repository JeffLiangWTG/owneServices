using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.JP.Business
{
	public class MSXMessageSendingObjectAttachment : NonPersistentBusinessObject
	{
		public MSXMessageSendingObjectAttachment(MSXMessageSendingObject parent)
		{
			Parent = Argument.NotNull(parent, nameof(parent));
		}

		public MSXMessageSendingObject Parent { get; }

		[ResourceStringData("1FAC390C-AD3D-4E02-BBC9-A8D7F6CB4F37", Caption = "File")]
		[List(nameof(Lookups) + "." + nameof(MSXMessageSendingObjectAttachmentLookups.FileList))]
		public ZGuid File
		{
			get => file;
			set
			{
				SetNonPersistentPropertyValue(FileInfo, ref file, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateFile();
					Validation.ValidateFileSize();
				}
			}
		}

		ZGuid file;

		public ZPropertyInfo FileInfo => GetZPropertyInfo(nameof(File));

		[ResourceStringData("E7220013-D1C9-4F38-AB77-894BDEADD182", Caption = "File Size (KB)")]
		public ZDecimal FileSize => Document?.FileSizeInMB * 1024 ?? ZDecimal.Zero;

		public ZPropertyInfo FileSizeInfo => GetZPropertyInfo(nameof(FileSize));

		public IeDoc Document
		{
			get
			{
				IeDoc document = null;

				var docRef = File;
				if (docRef.IsValid)
				{
					document = Parent.EDocCollections.Select(x => x.GetFromUniqueKey(docRef.ToGuid())).FirstOrDefault(x => x != null);
				}

				return document;
			}
		}

		[ResourceStringData("E53DC31F-711B-423E-AC02-E0772914D544", Caption = "Type")]
		[List(nameof(Lookups) + "." + nameof(MSXMessageSendingObjectAttachmentLookups.TypeList))]
		public ZString Type
		{
			get => type;
			set
			{
				SetNonPersistentPropertyValue(TypeInfo, ref type, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateType();
				}
			}
		}

		ZString type;

		public ZPropertyInfo TypeInfo => GetZPropertyInfo(nameof(Type));

		[ResourceStringData("E47C5D95-A6F9-401A-948F-640130603812", Caption = "Type Description")]
		public ZString TypeDescription => Lookups.TypeList.GetDescriptionFromCode(Type);

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#region Lookups

		public MSXMessageSendingObjectAttachmentLookups Lookups
		{
			get
			{
				if (lookups == null || !IsLookupsCachedInBase)
				{
					lookups = new MSXMessageSendingObjectAttachmentLookups(this);
				}

				return lookups;
			}
		}

		MSXMessageSendingObjectAttachmentLookups lookups;

		#endregion

		#region Validation

		public MSXMessageSendingObjectAttachmentValidation Validation => new MSXMessageSendingObjectAttachmentValidation(this);

		#endregion
	}
}
