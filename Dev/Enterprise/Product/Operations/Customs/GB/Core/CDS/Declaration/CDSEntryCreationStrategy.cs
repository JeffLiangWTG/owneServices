using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public class CDSEntryCreationStrategy : C88CreationStrategy
	{
		public CDSEntryCreationStrategy(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override void ProcessInvoiceHeaderAdditionalInfosForHeaderCore(Customs.Business.MergeKey mergeKey, Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
		}

		public override Customs.Business.MergeKey GetKeyForLine(Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			var result = base.GetKeyForLine(invoiceLine);
			var line = (JobComInvoiceLine)invoiceLine;
			result.Add(line.ZG_MethodOfPayment);
			var header = line.InvoiceHeader;
			result.Add(header.JZ_OA_SellerAddress);
			result.Add(header.JZ_OA_BuyerAddress);
			return result;
		}

		protected override void ProcessAdditionalInfosForLineMergeKey(Customs.Business.MergeKey mergeKey, Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			JobComInvoiceLine line = (JobComInvoiceLine)invoiceLine;

			// Each additional info can either be header only, header or line, or line only.
			// header or line are all currently sent at the line level so get added together for the merge key
			List<BusinessObject> lineAdditionalInfos = new List<BusinessObject>();
			foreach (BusinessObject bo in line.InvoiceHeader.AdditionalInfos)
			{
				lineAdditionalInfos.Add((EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo)bo);
			}
			lineAdditionalInfos.AddRange(line.AdditionalInfos);
			AddResults(mergeKey, lineAdditionalInfos, GetAdditionalInfoKeys());
		}

		protected override void AfterCreateOrGetEntryHeader(Customs.Business.CusEntryHeader entryHeader, Customs.Business.BaseJobComInvoiceLine baseInvoiceLine)
		{
			base.AfterCreateOrGetEntryHeader(entryHeader, baseInvoiceLine);
			entryHeader.CH_CEI_Instruction = baseInvoiceLine.JI_CEI;
		}
	}
}
