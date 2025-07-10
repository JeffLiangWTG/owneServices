using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	public class RCNGlobalElectronicInvoiceBuilderForTurkey : GlobalElectronicInvoiceRequestByIdBuilderForTurkey
	{
		public RCNGlobalElectronicInvoiceBuilderForTurkey(string batchNumber, GlbCompany company, string invoiceId, ZGuid reversalCreditNotePk)
			: base(batchNumber, TurkeyEInvoiceAPICommandList.Codes.CancelReceivablesInvoice, company, invoiceId)
		{
			Argument.NotNullOrEmpty(!reversalCreditNotePk.IsValid ? null : reversalCreditNotePk.ToString(), nameof(reversalCreditNotePk));

			ReversalCreditNotePk = reversalCreditNotePk;
		}

		ZGuid ReversalCreditNotePk { get; }

		protected sealed override ZString GetPayloadXML(INotifications notifications)
		{
			var result = ZString.Empty;

			var reversalCreditNote = Factory.Load<ARCreditNote>(ReversalCreditNotePk);
			if (reversalCreditNote == null)
			{
				notifications.AddError(Res.GetString("b6810401-b313-43c2-b8ac-9b606427c64b", "Cannot load AR Credit Note by PK {0}.", ReversalCreditNotePk));
			}
			else if (reversalCreditNote.AH_ComplianceSubType != TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN)
			{
				notifications.AddError(Res.GetString("f443835f-70cc-44ce-a4c6-9582d4721ca3", "Reversal AR Credit Note should have '{0}' compliance subtype but was '{1}'.", TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN, reversalCreditNote.AH_ComplianceSubType));
			}
			else if (!reversalCreditNote.AH_PostDate.IsValid)
			{
				notifications.AddError(Res.GetString("e6c6a221-105d-470a-a152-0e5b5193247b", "Invalid Post Date on reversal AR Credit Note."));
			}
			else
			{
				using (var memoryStream = new MemoryStream())
				{
					new RCNXmlWriter().WriteXmlToStream(InvoiceId, reversalCreditNote.AH_PostDate.ToDateTime(), memoryStream);

					memoryStream.Position = 0;
					using (var reader = new StreamReader(memoryStream, true))
					{
						result = reader.ReadToEnd();
					}
				}
			}

			return result;
		}
	}
}
