using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	public class Declaration07Provider : IDeclaration07
	{
		public Declaration07Provider(TemporaryStorageHeader header)
		{
			this.header = Argument.NotNull(header, nameof(header));
		}
		protected readonly TemporaryStorageHeader header;

		public static Declaration07Provider New(TemporaryStorageHeader header) => header == null ? null : new Declaration07Provider(header);

		public string MsgType => header.AMA_MessageType == PNTSMessageTypeList.Codes.CombinedTemporaryStorage ? TemporaryStorageDeclarationTypeList.Codes.DeclarationAndPresentationNotification : TemporaryStorageDeclarationTypeList.Codes.Declaration;

		public string SpecificCircumstanceIndicator => null;

		public IReadOnlyCollection<DateTime> PreviousDocument => previousDocument ?? (previousDocument = GetPreviousDocumentCollection());
		IReadOnlyCollection<DateTime> previousDocument;

		IReadOnlyCollection<DateTime> GetPreviousDocumentCollection()
		{
			return new Collection<DateTime>()
			{
				header.AMA_DateAtCustomsOffice.IsValid ? header.AMA_DateAtCustomsOffice.ToDateTime() : default,
			};
		}

		public string LRN => header.LRN;
	}
}
