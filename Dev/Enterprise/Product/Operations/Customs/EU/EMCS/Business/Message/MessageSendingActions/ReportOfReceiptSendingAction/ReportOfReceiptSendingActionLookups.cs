using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class ReportOfReceiptSendingActionLookups : ZLookups
	{
		public ReportOfReceiptSendingActionLookups(ReportOfReceiptSendingAction parent)
			: base(parent)
		{
		}

		public new ReportOfReceiptSendingAction Parent => (ReportOfReceiptSendingAction)base.Parent;

		public CodeDescriptionPairList ReceiptResultList
		{
			get
			{
				var declaration = Parent.JobDeclaration;

				var hasRefusedQuantity = declaration.InvoiceLines.Cast<EMCSJobComInvoiceLine>().Any(x => x.Outturn.C5_RejectedQuantity > 0);

				return Factory.GetCachedValue(string.Join("|", "ReceiptResultList|Refused", hasRefusedQuantity), () =>
				{
					var result = new CodeDescriptionPairList();

					if (hasRefusedQuantity)
					{
						result.AddPair(EMCSReceiptResultList.Codes.ReceiptPartiallyRefused, EMCSReceiptResultList.Descriptions.ReceiptPartiallyRefused);
					}
					else
					{
						result = new EMCSReceiptResultList();
					}
					return result;
				});
			}
		}
	}
}
