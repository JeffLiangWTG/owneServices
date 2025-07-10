using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DocumentCommonWrapper : IDocumentsCommon
	{
		public DocumentCommonWrapper(ZString code, ZString referenceNumber)
		{
			Name = code;
			Number = referenceNumber;
		}

		public ZString Name { get; }

		public ZString Number { get; }
	}
}
