using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUJobComInvoiceHeaderLookups : JobComInvoiceHeaderLookups
	{
		public AUJobComInvoiceHeaderLookups(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		JobComInvoiceHeader InvoiceHeader
		{
			get { return (JobComInvoiceHeader)base.Parent; }
		}

		#region List

		public override CodeDescriptionPairList MessageTypes
		{
			get
			{
				var messageTyppes = base.MessageTypes;
				var invoice = InvoiceHeader;

				if (!invoice.IsAttachedToPersistentDeclaration)
				{
					messageTyppes = invoice.Factory.GetCachedValue("AUCommercialInvoiceHeaderMessageTypes", delegate
					{
						var result = new CodeDescriptionPairList();

						result.AddRange(base.MessageTypes);

						if (!messageTyppes.ContainsCode(AUJobMessageTypeList.Codes.Quarantine))
						{
							result.AddPair(AUJobMessageTypeList.Codes.Quarantine, AUJobMessageTypeList.Descriptions.Quarantine);
						}

						return result;
					});
				}

				return messageTyppes;
			}
		}

		public CodeDescriptionPairList JZ_CommissionType_List
		{
			get { return new CodeDescriptionPairListCustomsCommissionType(); }
		}

		public override CodeDescriptionPairList JZ_IncoTerm_List
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				if (InvoiceHeader.JobDeclaration != null)
				{
					if (InvoiceHeader.JobDeclaration.IsImport && !InvoiceHeader.JobDeclaration.IsImportCMR)
					{
						result = new EdificeIncoTermList();
					}
					else
					{
						result = base.JZ_IncoTerm_List;
					}
				}
				return result;
			}
		}

		#endregion

	}
}
