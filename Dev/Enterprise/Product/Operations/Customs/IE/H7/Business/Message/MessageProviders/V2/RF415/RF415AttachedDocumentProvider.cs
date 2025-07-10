using System;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.H7.Business
{
	public class RF415AttachedDocumentProvider : IAttachedDocument
	{
		public RF415AttachedDocumentProvider(ZString type, ZString identifier, ZDateTime date)
		{
			Type = type;
			Identifier = identifier;
			Date = DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(date, true);
		}

		public string Type { get; }

		public string Identifier { get; }

		public DateTime Date { get; }
	}
}
