using System.Collections.ObjectModel;
using CargoWise.Customs.FR.MessageDefinitions.DCG.Response;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public interface IDCGResponseDataProvider
	{
		ZString DateFrom { get; }
		ZString DateTo { get; }
		Collection<TRefDsidse> Entries { get; }
		ZString EntryNum { get; }
		ZString DCGReference { get; }
		Collection<TTaxationDetail> Taxes { get; }
		ZString TransactionID { get; }
		Collection<TReponseErreur> Errors { get; }
		Collection<TAnomalie> Anomalies { get; }
		ZBool HasTaxes { get; }
		ZBool HasAnomalies { get; }
		ZBool HasErrors { get; }
		ZBool HasEntries { get; }
		ZString GetMessageInterpretation();
		ZString GetReadableTaxList();
		ZString GetReadableAnomalyList();
		ZString GetReadableErrorList();
		ZDate PeriodStartDate { get; }
		ZDate PeriodEndDate { get; }
		ZString Direction { get; }
		ZString Frequency { get; }
		ZString MessageStatusDescription { get; }
	}
}
