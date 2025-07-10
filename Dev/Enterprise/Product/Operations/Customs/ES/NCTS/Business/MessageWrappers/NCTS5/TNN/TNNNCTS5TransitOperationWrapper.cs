using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class TNNNCTS5TransitOperationWrapper : NCTS5CommonTransitOperationMRNWrapper, ITNNNCTSTransitOperation
	{
		public TNNNCTS5TransitOperationWrapper(NctsHeader header) : base(header)
		{
			Argument.NotNull(nctsHeader.MovementHeader, nameof(nctsHeader.MovementHeader));
		}

		public INCTSCommonTransitOperation CommonTransitOperation => commonTransitOperation ?? (commonTransitOperation = new NCTS5CommonTransitOperationWrapper(nctsHeader));
		NCTS5CommonTransitOperationWrapper commonTransitOperation;

		public ZDateTime DeclarationAcceptanceDate => nctsHeader.AcceptanceDate;

		public ZDateTime ReleaseDate => nctsHeader.ClearanceDate;
	}
}
