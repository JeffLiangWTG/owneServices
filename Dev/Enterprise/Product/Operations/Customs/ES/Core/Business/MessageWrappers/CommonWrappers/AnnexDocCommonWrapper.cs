using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class AnnexDocCommonWrapper : IAnnexDocCommon
	{
		public AnnexDocCommonWrapper(IeDoc document, ZString docDescription)
		{
			this.document = Argument.NotNull(document, "StorageDocs cannot be null");
			if (!ESConstants.AcceptedDocumentExtensions.DocumentExtensions.Contains(document.DataType))
			{
				throw new NotSupportedException("File type not accepted");
			}

			Description = docDescription;
		}
		protected readonly IeDoc document;

		public ZString Description { get; }

		public ZString ReferenceNumber => ReferenceNumberCore;

		protected virtual ZString ReferenceNumberCore => document.FileName;

		public ZBlob Image => document.ImageData;

		public ZString Extension => document.DataType;
	}
}
