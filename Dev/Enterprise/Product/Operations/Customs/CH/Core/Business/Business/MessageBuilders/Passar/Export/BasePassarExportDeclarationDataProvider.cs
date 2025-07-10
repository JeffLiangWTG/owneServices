using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.Business;

public abstract class BasePassarExportDeclarationDataProvider : BasePassarDeclarationMessageDataProvider
{
	public BasePassarExportDeclarationDataProvider(DeclarationMessageSendingObject sendingObject) : base(sendingObject)
	{
	}

	public IIntendedUse IntendedUse => intendedUse ??= IntendedUseDataProvider.New(entryHeader.EntryInstruction);
	IIntendedUse intendedUse;

	public IExportOperation ExportOperation => exportOperation ??= ExportOperationDataProvider.New(entryHeader);
	IExportOperation exportOperation;

	public IPerson Representative => representative ??= PersonDataProvider.New(declaration.DeclarantAddress?.Header, declaration.CusAgent);
	IPerson representative;

	public IFinanceData FinanceData => financeData ??= FinanceDataProvider.New(entryHeader.InvoiceHeaders?.FirstOrDefault());
	IFinanceData financeData;

	public IConsignment Consignment => consignment ??= ConsignmentDataProvider.New(entryHeader);
	IConsignment consignment;
}
