using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	class TS313MessageProvider : TS313And315MessageProvider, ITS313Header, IDeclaration08
	{
		public TS313MessageProvider(TemporaryStorageMessageSendingObject sendingObject) : base(sendingObject)
		{
			this.header = Argument.NotNull(sendingObject.Header, nameof(header));
		}

		readonly TemporaryStorageHeader header;

		public IDeclaration08 Declaration => this;

		#region IDeclaration08
		public string LRN => header.LRN;
		public string MRN => header.MRN;

		public string MsgType => header.AMA_MessageType == PNTSMessageTypeList.Codes.CombinedTemporaryStorage ? TemporaryStorageDeclarationTypeList.Codes.DeclarationAndPresentationNotification : TemporaryStorageDeclarationTypeList.Codes.Declaration;

		public string SpecificCircumstanceIndicator => null;

		#endregion

		public IReadOnlyCollection<DateTime> PreviousDocument => previousDocument ?? (previousDocument = GetPreviousDocumentCollection());
		IReadOnlyCollection<DateTime> previousDocument;

		IReadOnlyCollection<DateTime> GetPreviousDocumentCollection()
		{
			return new Collection<DateTime>()
			{
				header.AMA_DateAtCustomsOffice.IsValid ? header.AMA_DateAtCustomsOffice.ToDateTime() : default,
			};
		}
	}
}
