using System;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class AttachedDocumentProvider : IAttachedDocument
	{
		public AttachedDocumentProvider(ZString type, ZString identifier, ZDateTime date)
		{
			Type = type;
			Identifier = identifier;
			Date = DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(date);
		}

		public string Type { get; }
		public string Identifier { get; }
		public DateTime Date { get; }
	}
}
