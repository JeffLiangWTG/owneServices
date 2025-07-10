using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business;

public class TIRHeaderWrapper : NctsSADHeaderCommonWrapper
{
	public TIRHeaderWrapper(NctsHeader nctsHeader) : base(nctsHeader)
	{
	}

	protected override IETHeaderMeansOfTransportCrossingBorder MeansOfTransportCrossingBorderCore => new TIRHeaderEmptyMeansOfTransportCrossingBorderWrapper();

	protected override IEnumerable<IETHeaderTransitCustomsOffice> TransitCustomsOfficesCore => Enumerable.Empty<IETHeaderTransitCustomsOffice>();

	protected override IEnumerable<IETHeaderGuarantee> GuaranteesCore => Enumerable.Empty<IETHeaderGuarantee>();

	protected override ZString AuthorizationCINCore => ZString.Empty;

	protected override ZString AuthorizationNoCore => ZString.Empty;

	protected override IDeclaration DeclarationCore => new TIRHeaderDeclarationWrapper(NctsMovementHeader);

	protected override IETHeaderPrincipalTrader PrincipalTraderCore => new TIRHeaderPrincipalTraderWrapper(NctsHeader.Principal);

	protected override ITransactionData TransactionDataCore => new TIRHeaderEmptyTransactionDataWrapper();
}
