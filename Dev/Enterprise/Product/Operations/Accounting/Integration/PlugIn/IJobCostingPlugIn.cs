using CargoWise.Types;

using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Integration
{
	public interface IJobCostingPlugIn : IGenericJobCostPlugIn
	{
		ZGuid PK { get; }
		ZString JK_UniqueConsignRef { get; }
		RefUNLOCO LoadPort { get; }
		RefUNLOCO DischargePort { get; }
		JobProfitLossCollection ProfitLossContainer { get; }
		decimal ConsolExchangeRate { get; }
		decimal ExchangeRateForCurrency(RefCurrency currency, ZGuid currentJobConsolCostPK);
		RefCurrency ConsolCurrency { get; }
		bool IsMasterCollect { get; }
		OrgHeader ReceivingAgent { get; }
		OrgHeader ReceivingAgentAPInvoicingParty { get; }
		OrgHeader ReceivingAgentARInvoicingParty { get; }
		OrgHeader SendingAgent { get; }
		OrgHeader SendingAgentAPInvoicingParty { get; }
		OrgHeader SendingAgentARInvoicingParty { get; }
		ZString TransportMode { get; }
		ZString ContainerMode { get; }
		ZString Direction { get; }
		ZString ConsolType { get; }
		ZString Module { get; }
		void AddNewToLogs(Event @event, ZString reference);
		ZString GetPrepaidCollect(IJobInvoicingPlugIn apportionableJob);
		CodeDescriptionPairList PrepaidCollectList { get; }
	}
}
