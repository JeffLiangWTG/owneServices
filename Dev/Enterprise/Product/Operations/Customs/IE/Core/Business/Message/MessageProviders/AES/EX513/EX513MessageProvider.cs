using System;
using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES
{
	public class EX513MessageProvider : EntryHeaderMessageProvider, IEX513Header
	{
		public EX513MessageProvider(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		public IIE513ExportOperation ExportOperation => null;

		public string PresentationOffice => ZString.Empty;

		public string ExportOffice => ZString.Empty;

		public string ExitOffice => ZString.Empty;

		public string SupervisingOffice => ZString.Empty;

		public IParty Exporter => null;

		public IReadOnlyCollection<IAuthorisation> Authorisations => Array.Empty<IAuthorisation>();

		public string DeferredPayment => ZString.Empty;

		public IIE513GoodsShipment GoodsShipment => null;

		public IParty Declarant => null;

		public IRepresentative Representative => null;

		public bool IsInTransitionPeriod => false;
	}
}
