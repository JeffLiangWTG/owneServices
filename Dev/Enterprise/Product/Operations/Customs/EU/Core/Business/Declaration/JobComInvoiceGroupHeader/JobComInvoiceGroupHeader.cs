using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobComInvoiceGroupHeader : TypeSafeJobComInvoiceGroupHeader
		, Integration.Customs.EU.IJobComInvoiceGroupHeader
		, IChargeHolder
	{
		public JobComInvoiceGroupHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new static readonly JobComInvoiceGroupHeaderTypeDecider TypeDecider = new JobComInvoiceGroupHeaderTypeDecider();

		ZString IChargeHolder.GetDefaultCurrencyCode(ICustomsChargeCode chargeCode, JobComInvCharge charge)
		{
			var parent = charge.Parent;
			var result = ZString.Empty;

			if (parent is JobComInvoiceGroupHeader groupHeader)
			{
				var charges = groupHeader.Charges;
				var header = groupHeader.JobComInvoiceHeaders.FirstOrDefault();

				if (charges.Cast<JobComInvCharge>().ElementInFrontOf(charge)?.J7_RX_NKCurrency.IsEmpty ?? true)
				{
					result = header?.JZ_RX_NKInvoice_Currency ?? ZString.Empty;
				}
				else
				{
					result = charges.Cast<JobComInvCharge>().ElementInFrontOf(charge).J7_RX_NKCurrency;
				}
			}

			return result;
		}

		[ResourceStringData("EU.JobComInvoiceGroupHeader|ChargesAddDeductTotal", Caption = "Add/Deduct Stat. Value")]
		public ZDecimal ChargesAddDeductTotal => ZDecimal.Zero;

		[ResourceStringData("EU.JobComInvoiceGroupHeader|TotalInvoiceAmount", Caption = "Total Invoice Value")]
		public ZDecimal TotalInvoiceAmount => JZ_InvoiceAmount;
	}
}
