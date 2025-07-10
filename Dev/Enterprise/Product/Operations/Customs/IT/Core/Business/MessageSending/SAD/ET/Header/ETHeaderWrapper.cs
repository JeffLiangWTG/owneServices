using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class ETHeaderWrapper : SADHeaderCommonWrapper, IETHeader
{
	public ETHeaderWrapper(CusEntryHeader entryHeader) : base(entryHeader)
	{
	}

	protected override ZString CountryOfDispatchCore => DispatchCountryCodeResolver.GetDispatchCountryCodeForHeader();

	public ZBool HeaderDataDeclaredOnItems => EntryInstruction.IsBuyersConsol;

	public ZBool? SecurityData => null;

	public IMeansOfTransport MeansOfTransportAtDeparture => new SADMeansOfTransportWrapper(JobDeclaration.ZG_Box18TransportNationality, JobDeclaration.ZG_Box18TransportID);

	public IETHeaderMeansOfTransportCrossingBorder MeansOfTransportCrossingBorder => new ETHeaderMeansOfTransportCrossingBorderWrapper(JobDeclaration.TransportNationality?.Code ?? ZString.Empty, JobDeclaration.MeansOfTransportCrossingBorderIdentity);

	public ZString DialogLanguageIndicatorAtDeparture => ZString.Empty;

	public IETHeaderSecurityBlock SecurityBlock => new ETHeaderSecurityBlockWrapper(EntryHeader);

	public ZString ExitCustomsOffice => JobDeclaration.CustomsOffices?.Find(office => office.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfExit).FirstOrDefault()?.CY_Data ?? ZString.Empty;

	public IETHeaderAgreedLocationOfGoods AgreedLocationOfGoods => new ETHeaderAgreedLocationOfGoodsWrapper(EntryHeader);

	public IETHeaderPrincipalTrader PrincipalTrader => new ETHeaderPrincipalTraderWrapper();

	public IEnumerable<IETHeaderTransitCustomsOffice> TransitCustomsOffices => Enumerable.Empty<IETHeaderTransitCustomsOffice>();

	public IEnumerable<IETHeaderGuarantee> Guarantees => Enumerable.Empty<IETHeaderGuarantee>();

	public ZString DestinationCustomsOffice => ZString.Empty;

	public IEnumerable<ZString> Seals => EntryInstruction.GetEffectiveSealNumbers();

	public IETHeaderControlResult ControlResult => new ETHeaderControlResultWrapper();

	protected override IDeclaration DeclarationCore => new ETDeclarationWrapper(JobDeclaration, EntryInstruction);

	protected override ITermOfDeliveryGroup TermsOfDeliveryCore => new SADTermsOfDeliveryWrapper(EntryHeader);

	protected override ITransactionData TransactionDataCore => new SADTransactionDataWrapper(EntryHeader);

	protected override ZString CountryOfDestinationCore => JobDeclaration.CountryOfDestinationCode;

	protected override ZBool? IsContainerizedTransportCore => CustomsRulesProvider.ConvertContainerModeFromCargoWiseToIT(JobDeclaration.ContainerMode);

	protected override ZString TransportModeAtBorderCore => JobDeclaration.TransportModeTranslator.TranslateToWCOCode(JobDeclaration.JE_TransportMode);

	protected override ZString InlandTransportModeCore => JobDeclaration.TransportModeTranslator.TranslateToWCOCode(JobDeclaration.JE_TransportModeInland);

	protected override IWarehouseIdentification WarehouseIdentificationCore => new SADEmptyWarehouseIdentificationWrapper();

	DispatchCountryCodeResolver DispatchCountryCodeResolver => dispatchCountryCodeResolver ?? (dispatchCountryCodeResolver = new DispatchCountryCodeResolver(EntryHeader));

	DispatchCountryCodeResolver dispatchCountryCodeResolver;
}
