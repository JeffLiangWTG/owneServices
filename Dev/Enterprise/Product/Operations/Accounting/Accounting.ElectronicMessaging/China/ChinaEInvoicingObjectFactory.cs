using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.ElectronicMessaging.Common.Universal.AccountingInvoiceDataContextManager;
using static Enterprise.Core.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.China
{
	public class ChinaEInvoicingObjectFactory : CountryEInvoicingObjectFactory
	{
		protected override ZString CountryCode => CountryCodes.China;

		protected override ITransactionBatchToPayloadWriter GetTransactionBatchToPayloadWriter() =>
			new ChinaPayloadWriter();

		protected override BaseEInvoicingDataValidator GetDataValidator(GlbCompany company) =>
			new EInvoicingDataValidatorForChina(company);

		protected override ZString GetMessageType(TransactionBatch transactionBatch, AccEInvoicingBatch eInvoicingBatch)
			=> ChinaEInvoiceAPICommandList.GetMessageType(((AccEInvoicingTransactionPivot)eInvoicingBatch?.TransactionPivots.FirstOrDefault())?.AIP_ActionType ?? ZString.Empty);

		protected override ZQuery GetInvoiceEventMessageTargetQuery(UniversalEvent xmlEvent)
		{
			var result = new ZQuery();

			var invoicePKContext = xmlEvent.ContextCollection.FirstOrDefault(x =>
								x.Type == EventDataConstants.Context_InvoicePK &&
								x.Value.HasValue);
			if (invoicePKContext != null)
			{
				var splitedInvoicePKs = invoicePKContext.Value?.Split("|");
				splitedInvoicePKs = splitedInvoicePKs.Take(splitedInvoicePKs.Length - 1).ToArray();
				var pKs = new List<ZGuid>();
				foreach (var invoicePK in splitedInvoicePKs)
				{
					if (ZGuid.TryParse(invoicePK, out ZGuid transactionPK))
					{
						pKs.Add(transactionPK);
					}
				}

				result = new ZQuery(AccTransactionHeaderSchema.PK, pKs);
			}

			return result;
		}

		protected override IGlobalXUEFunctionalityProvider GetGlobalXUEFunctionalityProvider
			=> new ChinaGlobalXUEFunctionalityProvider(this);
	}
}
