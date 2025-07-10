using System;
using System.IO;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business
{
	public abstract class CommonDocumentSendingObjectValidation : SupportingDocSendingObjectValidation
	{
		protected CommonDocumentSendingObjectValidation(AutoSupportingDocSendingObject parent) : base(parent)
		{
		}

		protected override void CheckDocumentType() { }

		protected override void CheckLocalReferenceNumber() { }

		protected override void CheckEDoc()
		{
			base.CheckEDoc();
			var document = Parent.Document;
			if (document != null)
			{
				var fileName = document.FileName;
				var fileTypeValidation = new FileTypeValidation();
				var fileInfo = new FileInfo(fileName);

				if (fileTypeValidation.IsDangerousFile(fileName) || extensionMatchesBlockedExtension(fileInfo.Extension))
				{
					Parent.EDocInfo.AddError(Res.GetString("21967A5F-86E7-48DA-8602-25E16934D69A", "{0} is not supported.", fileInfo.Extension));
				}

				if (Parent is DocumentSendingObject parent
					&& parent.ParentSendingObject is AdditionalInfoSendingObject additionalInfoSendingObject)
				{
					var currentEDoc = parent.EDoc;
					var parentPK = parent.PK;
					foreach (DocumentSendingObject eDoc in additionalInfoSendingObject.EDocsCollection)
					{
						if (eDoc.EDoc == currentEDoc && eDoc.PK != parentPK)
						{
							Parent.EDocInfo.AddError(Res.GetString("37615608-997C-46B7-B1C6-CB29CF9B4D7E", "You have already selected this eDoc."));
						}
					}
				}
			}
		}

		bool extensionMatchesBlockedExtension(string ext)
		{
			ext = ext.ToUpperInvariant();
			if (!ext.StartsWith(".", StringComparison.Ordinal))
			{
				ext = "." + ext;
			}

			var result = false;

			foreach (var nextExt in blockedFileExtensions)
			{
				var match = nextExt.Length == ext.Length;
				if (match)
				{
					for (int i = 0; i < nextExt.Length; i++)
					{
						if (nextExt[i] != '?' && nextExt[i] != ext[i])
						{
							match = false;
						}
					}
				}
				if (match)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		readonly string[] blockedFileExtensions = new string[]
		{
			".APK",
			".APPX",
			".APPXBUNDLE",
			".CAB",
			".DLL",
			".DMG",
			".ISP",
			".ISO",
			".JAR",
			".LIB",
			".MSIX",
			".MSIXBUNDLE",
			".NSH",
			".PS1",
			".SYS",
			".VXD"
		};
	}
}
