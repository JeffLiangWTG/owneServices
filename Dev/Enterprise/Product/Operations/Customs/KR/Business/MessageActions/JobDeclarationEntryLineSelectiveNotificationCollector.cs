using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class JobDeclarationEntryLineSelectiveNotificationCollector : JobDeclarationMessageSendingNotificationCollector
	{
		public JobDeclarationEntryLineSelectiveNotificationCollector(JobDeclaration business, CusEntryLine entryLine) : base(business, new[] { entryLine.Header })
		{
			this.entryLine = entryLine;
		}

		readonly CusEntryLine entryLine;

		protected override bool ShouldIncludeNotificationsFromObject(BusinessObject businessObject)
		{
			var isRelevant = businessObject switch
			{
				CusEntryLine entryLine => entryLine.PK == this.entryLine.PK,
				CusEntryHeader entry => entry.PK == entryLine.CL_CH,
				CusEntryInstruction instruction => instruction.PK == entryLine.Header.CH_CEI_Instruction,
				JobComInvoiceLine invoiceLine => invoiceLine.JI_CL == entryLine.PK,
				JobComInvoiceHeader invoice => entryLine.InvoiceLines.Select(x => x.JI_JZ).Distinct().Contains(invoice.PK),
				_ => true
			};
			return isRelevant && base.ShouldIncludeNotificationsFromObject(businessObject);
		}
	}
}
