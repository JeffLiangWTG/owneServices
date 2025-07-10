using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Business
{
	public class MSXMessageSendingObjectAttachmentValidation : ZValidation
	{
		public MSXMessageSendingObjectAttachmentValidation(MSXMessageSendingObjectAttachment parent) : base(parent)
		{
			Parent = parent;
		}
		protected readonly MSXMessageSendingObjectAttachment Parent;

		public override Type AutoValidationType => typeof(MSXMessageSendingObjectAttachmentValidation);

		public override void ValidateAll()
		{
			ValidateFile();
			ValidateType();
			ValidateFileSize();
		}

		public void ValidateFile()
		{
			ValidateCalculatedProperty(Parent.FileInfo);
		}

		protected void CheckFile()
		{
			ListValidation.ErrorIfInvalidPK(Parent.FileInfo);

			if (Parent.Document != null)
			{
				var filename = Parent.Document.FileName;

				if (Parent.Parent.Attachments.Cast<MSXMessageSendingObjectAttachment>().Count(x => x.Document != null && x.Document.FileName == filename) > 1)
				{
					Parent.FileInfo.AddError(Res.GetString("AF64DB83-C96B-43B9-A62E-13C660FA95D5", "Every file name must be unique. If the same file name already exists, please choose a different one."));
				}

				var fileExtension = Path.GetExtension(filename).Replace(".", "");
				var avaliableList = new List<ZString>() { "TXT", "DOC", "DOCX", "PPT", "PPTX", "XML", "HTM", "HTML", "RTF", "JTD", "XLS", "XLSX", "CSV", "JPEG", "JPE", "JPG", "TIF", "TIFF", "BMP", "GIF", "PNG", "PDF", "JET" };

				if (!avaliableList.Any(x => x.EqualsIgnoringCase(fileExtension)))
				{
					Parent.FileInfo.AddError(Res.GetString("778AE4F8-0E41-4FF7-9C0F-9D283FC12474", "Only {0} are allowed.", avaliableList.ToStringWithSeparator(", ")));
				}

				if (filename.Split('.').Length > 2)
				{
					Parent.FileInfo.AddError(Res.GetString("F500B7F4-F08D-4A21-87F3-55787F3D71F7", "Period is only allowed in file extension."));
				}

				var fileNameToBytes = JPMessageUtils.ConvertStringToMessage(filename);

				if (fileNameToBytes.Length > 50)
				{
					Parent.FileInfo.AddError(Res.GetString("771FEC7F-4ECC-4BE8-B4A1-A2C3E0621B21", "Max length exceeded. Please enter a shorter file name."));
				}

				if (filename != JPMessageUtils.ConvertMessageToString(fileNameToBytes))
				{
					Parent.FileInfo.AddError(Res.GetString("4CFF2364-B533-4F09-A683-AC9D239AAB4E", "There are unsupported characters in filename, please change the filename."));
				}

				var unsupportedCharacters = new List<string>();
				foreach (Match match in Regex.Matches(filename, @"[\x00-\x1F\x21-\x2C\x2F\x3A-\x40\x5B-\x5E\x60\x7B-\x7F]"))
				{
					unsupportedCharacters.Add(match.Value);
				}

				if (unsupportedCharacters.Count > 0)
				{
					Parent.FileInfo.AddError(Res.GetString("F1FC3D72-B4A5-4BA5-9F3E-2FDD5179AD7F", "The following characters are not allowed: {0}", unsupportedCharacters.Distinct().ToStringWithSeparator()));
				}
			}
		}

		public void ValidateType()
		{
			ValidateCalculatedProperty(Parent.TypeInfo);
		}

		protected void CheckType()
		{
			ListValidation.ErrorIfInvalidCode(Parent.TypeInfo);
			MandatoryValidation.CheckEntered(Parent.TypeInfo);
		}

		public void ValidateFileSize()
		{
			ValidateCalculatedProperty(Parent.FileSizeInfo);
		}

		protected void CheckFileSize()
		{
			if (Parent.Document != null && Parent.FileSize > 10240)
			{
				Parent.FileSizeInfo.AddError(Res.GetString("267CF09F-16DE-477A-99CE-95FA44111BF1", "The maximum file size allowed is 10 MB."));
			}
		}
	}
}
