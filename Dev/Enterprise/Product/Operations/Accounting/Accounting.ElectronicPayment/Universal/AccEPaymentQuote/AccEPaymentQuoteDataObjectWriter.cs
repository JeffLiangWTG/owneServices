using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.ElectronicPayment.Universal
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This class will be used in futures Work Items")]
	public class AccEPaymentQuoteDataObjectWriter : TopLevelDataObjectWriter<AccEPaymentQuote, UniversalTransaction>
	{
		public AccEPaymentQuoteDataObjectWriter(IDataWritingManager manager) : base(manager) { }

		protected override ZString GetEDIMessageSubType() => EDIMessageSubTypeList.Codes.XmlUniversalTransaction;

		protected override DataContextType GetTopLevelDataContextType() => DataContextType.AccEPaymentQuote;

		protected override void PopulateDataObject(AccEPaymentQuote sourceBO, UniversalTransaction dataObject)
		{
			new AccEPaymentQuoteExporter().PopulateUniversalTransaction(sourceBO, dataObject);
		}
	}
}
