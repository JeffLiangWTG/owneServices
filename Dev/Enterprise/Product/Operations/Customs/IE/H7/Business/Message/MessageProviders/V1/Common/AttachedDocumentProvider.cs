using System.Globalization;
using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class AttachedDocumentProvider : IAttachedDocument
	{
		public AttachedDocumentProvider(ZString type, ZString identifier, ZDateTime date)
		{
			DocumentType = type;
			DocumentIdentifier = identifier;
			DocumentDate = !date.IsEmpty ? DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(date).ToString("yyyyMMdd", CultureInfo.InvariantCulture) : string.Empty;
		}

		public string DocumentType { get; }

		public string DocumentIdentifier { get; }

		public string DocumentDate { get; }
	}
}
