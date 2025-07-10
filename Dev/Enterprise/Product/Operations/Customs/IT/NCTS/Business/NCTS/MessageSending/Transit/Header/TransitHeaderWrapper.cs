using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business;

public class TransitHeaderWrapper : NctsSADHeaderCommonWrapper
{
	public TransitHeaderWrapper(NctsHeader nctsHeader) : base(nctsHeader)
	{
	}

	protected override IETHeaderMeansOfTransportCrossingBorder MeansOfTransportCrossingBorderCore => new TransitHeaderMeansOfTransportCrossingBorderWrapper(NctsMovementHeader.BM_TOLCarrierCode, NctsMovementHeader.BM_TOLCarrierID);

	protected override IEnumerable<IETHeaderTransitCustomsOffice> TransitCustomsOfficesCore => NctsHeader
		.CustomsOffices
		.GetElementsHaving(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit)
		.Select(customsOffice => new TransitHeaderCustomsOfficeOfTransit(customsOffice));

	protected override IEnumerable<IETHeaderGuarantee> GuaranteesCore => NctsHeader
		.Guarantees
		.Cast<NctsGuarantee>()
		.Select(guarantee => new TransitHeaderGuarantee(guarantee));

	protected override ZString AuthorizationCINCore => NctsHeader.Authorization.Right(1);

	protected override ZString AuthorizationNoCore
	{
		get
		{
			var authorisationNumber = NctsHeader.Authorization;
			return authorisationNumber.Left(authorisationNumber.Length - 1);
		}
	}

	protected override IDeclaration DeclarationCore => new TransitHeaderDeclarationWrapper(NctsMovementHeader);

	protected override IETHeaderPrincipalTrader PrincipalTraderCore => new TransitHeaderPrincipalTraderWrapper(NctsHeader.Principal, NctsMovementHeader.Representative);

	protected override ITransactionData TransactionDataCore => new TransitHeaderTransactionDataWrapper();
}
