using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public abstract class SADHeaderCommonWrapper : IHeaderCommon
{
	public SADHeaderCommonWrapper(CusEntryHeader entryHeader)
	{
		EntryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		JobDeclaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		EntryInstruction = Argument.NotNull(entryHeader.EntryInstruction, nameof(entryHeader.EntryInstruction));
		Argument.NotNull(entryHeader.MergedLines, nameof(entryHeader.MergedLines));
	}
	protected CusEntryHeader EntryHeader { get; }
	protected JobDeclaration JobDeclaration { get; }
	protected CusEntryInstruction EntryInstruction { get; }

	public ZString AnnualProgressiveNumber => Enterprise.Messaging.Business.EDIMessage.MessageNumberPlaceHolder;

	public ZString AuthorizationNo
	{
		get
		{
			var authorisationNumber = JobDeclaration.ZG_AuthorisationNumber;
			var authorisationNumberLenght = authorisationNumber.Length;
			return authorisationNumberLenght > 1 ? authorisationNumber.Left(authorisationNumberLenght - 1) : ZString.Empty;
		}
	}

	public ZString AuthorizationCIN => JobDeclaration.ZG_AuthorisationNumber.Right(1);

	public ZInt TotalItems => EntryHeader.MergedLines.Count;

	public ZDate AcceptanceDate => EntryInstruction.CEI_DateForDuty.Date;

	public IDeclaration Declaration => DeclarationCore;
	protected abstract IDeclaration DeclarationCore { get; }

	public ITrader Consignor => ConsignorCore;
	protected virtual ITrader ConsignorCore => new SADTraderWrapper(JobDeclaration.SupplierDocumentaryAddress);

	public ITrader Consignee => new SADTraderWrapper(JobDeclaration.ImporterDeliveryAddress);

	public IDeclarantTrader DeclarantTrader => new SADDeclarantTraderWrapper(JobDeclaration);

	public ZString CountryOfDispatch => CountryOfDispatchCore;
	protected virtual ZString CountryOfDispatchCore => JobDeclaration.JE_GoodsOrigin;

	public ITermOfDeliveryGroup TermsOfDelivery => TermsOfDeliveryCore;
	protected abstract ITermOfDeliveryGroup TermsOfDeliveryCore { get; }

	public ITransactionData TransactionData => TransactionDataCore;
	protected abstract ITransactionData TransactionDataCore { get; }

	public IDeferredPayment DeferredPayment => new SADDeferredPaymentWrapper(JobDeclaration);

	public IWarehouseIdentification WarehouseIdentification => WarehouseIdentificationCore;
	protected abstract IWarehouseIdentification WarehouseIdentificationCore { get; }

	public ZString CountryOfDestination => CountryOfDestinationCore;
	protected abstract ZString CountryOfDestinationCore { get; }

	public ZBool? IsContainerizedTransport => IsContainerizedTransportCore;
	protected abstract ZBool? IsContainerizedTransportCore { get; }

	public ZString TransportModeAtBorder => TransportModeAtBorderCore;
	protected abstract ZString TransportModeAtBorderCore { get; }

	public ZString InlandTransportMode => InlandTransportModeCore;
	protected abstract ZString InlandTransportModeCore { get; }

	public ZDate DateLimitOfTemporaryOperation => EntryInstruction.ZG_TempProcLimitDate.Date;
}
