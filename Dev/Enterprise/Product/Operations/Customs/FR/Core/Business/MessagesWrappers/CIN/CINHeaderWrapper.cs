using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Messaging.Interfaces.CIN;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.CIN
{
	public class CINHeaderWrapper : ICINHeader
	{
		public CINHeaderWrapper(CusTempStorageJobHeader header)
		{
			this.header = Argument.NotNull(header, "Job Header cannot be null");
		}

		public ZDateTime MovementTime => ZDateTime.UtcNow;

		public ZString CurrentLocation => Line.TSL_LocationOfGoods;

		public ZString NewLocation => Line.TSL_DestinationPlace;

		public ZString CustomsStatus => Line.TSL_UnionStatus;

		public IEnumerable<ICINCustomsDocument> CustomsDocuments => null;

		public ZString CustomsReference => Line.TSL_ReferenceNumber;

		public IEnumerable<ICINLine> Bills => header.CusTempStorageDec.CusTempStorageLines.Select(x => new CINLineWrapper(x as CusTempStorageLine));

		readonly CusTempStorageJobHeader header;

		public CusTempStorageLine Line => line ?? (line = header.CusTempStorageDec.CusTempStorageLines.Cast<CusTempStorageLine>().FirstOrDefault(x => x.TSL_OwnerReferenceType == OwnerReferenceTypeList.Codes.ORT_AWB));

		public IMessageEnvelope MessageEnvelope => messageEnvelope ?? (messageEnvelope = new CINHeaderEnvelopeWrapper(header));
		IMessageEnvelope messageEnvelope;

		CusTempStorageLine line;
	}
}
