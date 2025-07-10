using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonTransitOperationMRNWrapper : INCTSCommonTransitOperationMRN
	{
		public NCTS5CommonTransitOperationMRNWrapper(NctsHeader header)
		{
			nctsHeader = Argument.NotNull(header, nameof(header));
		}
		protected NctsHeader nctsHeader;

		public ZString MRN => nctsHeader.MovementReferenceNumber;
	}
}
