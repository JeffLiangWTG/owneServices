using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5SGHeader
	{
		ZString ApplicationNumber { get; }
		ZInt SequenceNo { get; }
		ZString DeclarationCustomsOffice { get; }
		ZString UnipassDeclarantID { get; }
		IEnumerable<IImport5SGEntry> Entries { get; }
	}
}
