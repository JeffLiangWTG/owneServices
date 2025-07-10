using System;
using System.Data;
using System.Globalization;
using System.IO;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MailManager.Business
{
	public class MailAttachment : AutoMailDBAttachments, IDeliveryEmailAttachment
	{
		public MailAttachment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public EncodingEnum EncodingType
		{
			get
			{
				switch (MA_Encoding)
				{
					case "B64":
						return EncodingEnum.Base64;
					default:
						return EncodingEnum.Unknown;
				}
			}
		}

		#region Interface implementations

		#region IDeliveryEmailAttachment

		long IDeliveryEmailAttachment.FileSizeInBytes => MA_Data.Length;

		string IDeliveryEmailAttachment.FileName => MA_FileName;

		bool IDeliveryEmailAttachment.ShouldBeAttached => false;

		#endregion

		public ZString HumanReadableAttachmentSize => AttachmentSize(AttachmentSizeInBytes);

		public double AttachmentSizeInBytes
		{
			get
			{
				if (!attachmentSizeInBytes.HasValue)
				{
					var queryResult = new DynamicBusinessObjectCollection(Factory.GetCachedReadOnlyFactory());
					var query = @"SELECT CAST(LEN(dbo.CLRUncompressAsBytes(" + MailDBAttachmentsSchema.Constants.MA_Data + ")) AS VARCHAR) as AttachmentLength FROM " + MailDBAttachmentsSchema.Constants.SqlSchemaName + "." + MailDBAttachmentsSchema.Constants.TableName + " WHERE " + MailDBAttachmentsSchema.Constants.PK + "= @AttachmentPK";
					var sqlParams = new ZSqlParameterCollection();
					sqlParams.Add("@AttachmentPK", PK, MailDBAttachmentsSchema.PK);

					queryResult.Load(query, sqlParams);

					if (queryResult.Count > 0)
					{
						var lengthStr = (ZString)queryResult[0]["AttachmentLength"];
						attachmentSizeInBytes = Convert.ToDouble(lengthStr.ToString(), CultureInfo.CurrentCulture);
					}
					else
					{
						attachmentSizeInBytes = ((IDeliveryEmailAttachment)this).FileSizeInBytes;
					}
				}
				return attachmentSizeInBytes.Value;
			}
		}

		double? attachmentSizeInBytes;

		static internal ZString AttachmentSize(double len)
		{
			string[] sizes = { "B", "KB", "MB", "GB" };
			int order = 0;
			while (len >= 1024 && order < sizes.Length - 1)
			{
				len = len / 1024;
				++order;
			}
			return string.Format(CultureInfo.InvariantCulture, "{0:0.#}{1}", len, sizes[order]);
		}

		#endregion

		public enum EncodingEnum
		{
			Base64, Unknown
		}

		public void CopyValuesFrom(MailAttachment sourceAttachment)
		{
			base.CopyValuesFrom(sourceAttachment);
		}

		public string SaveTo(string destinationFolder)
		{
			EnsureBlobField(MailDBAttachmentsSchema.MA_Data);
			string destinationFileName = Path.Combine(destinationFolder, MA_FileName.ExcludeChars(@"\/?%*:|""<>"));
			using (var sourceStream = GetMA_DataReader())
			using (var targetStream = ObjectFactory.Get<IUserFileAccess>().OpenFileSave(destinationFileName))
			{
				sourceStream.CopyTo(targetStream);
			}

			return destinationFileName;
		}
	}
}
