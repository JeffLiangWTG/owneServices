using System;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;
using UniversalTransactionInfo = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	public class RINGlobalElectronicInvoiceBuilderForTurkey : GlobalElectronicInvoiceBuilderBaseForTurkey
	{
		public RINGlobalElectronicInvoiceBuilderForTurkey(string batchNumber, UniversalTransactionBatch universalTransactionBatch)
			: base(batchNumber, TurkeyEInvoiceAPICommandList.Codes.SendReceivablesInvoice)
		{
			Argument.NotNull(universalTransactionBatch, "universalTransactionBatch");
			if (universalTransactionBatch?.TransactionCollection?.Count != 1)
			{
				throw new ArgumentException("Each transaction batch can have only one transaction in order to generate a Turkey Electronic Invoice.");
			}
			Transaction = universalTransactionBatch.TransactionCollection.First();
			Batch = universalTransactionBatch;
		}

		UniversalTransactionInfo Transaction { get; }

		UniversalTransactionBatch Batch { get; }

		GlbBranch GEIMessageBranch => gEIMessageBranch ?? (gEIMessageBranch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, Transaction.Branch.Code ?? ZString.Empty));
		GlbBranch gEIMessageBranch;

		protected sealed override GlbCompany GEIMessageCompany => GEIMessageBranch?.Company;

		protected sealed override ZString GetPayloadXML(INotifications notifications)
		{
			using (var memoryStream = new MemoryStream())
			{
				new EInvoiceXmlWriter().WriteXmlToStream(Batch, GEIMessageCompany, memoryStream);
				memoryStream.Position = 0;
				using (var reader = new StreamReader(memoryStream, true))
				{
					return reader.ReadToEnd();
				}
			}
		}
	}
}
