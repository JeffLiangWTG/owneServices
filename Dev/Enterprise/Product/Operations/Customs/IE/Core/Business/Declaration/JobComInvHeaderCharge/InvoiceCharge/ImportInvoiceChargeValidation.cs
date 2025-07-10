using System.Linq;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ImportInvoiceChargeValidation : EU.Business.Declaration.InvoiceChargeValidation
	{
		public ImportInvoiceChargeValidation(InvoiceCharge parent) : base(parent)
		{
		}

		protected new InvoiceCharge Parent => (InvoiceCharge)base.Parent;

		protected override void CheckJ7_ChargeType()
		{
			base.CheckJ7_ChargeType();
			CheckRuleBR4010();
		}

		void CheckRuleBR4010()
		{
			var parent = Parent;
			if (parent.Invoice is JobComInvoiceHeader invoice && invoice.IsRuleBR4010Active)
			{
				var charges = invoice.Charges.Cast<InvoiceCharge>();
				var groupCharges = invoice.GroupCharges.Cast<InvoiceApportionCharge>();
				var chargeType = parent.J7_ChargeType;

				if (_1XChargeButNoBA() || BAChargeButNo1XOrDifferentAmounts())
				{
					parent.J7_ChargeTypeInfo.AddMessageError(Res.GetString("787B9CB6-6DBE-4179-8BA8-17F004AEC9F1", "[BR4010] In all circumstances the respective amount declared for 1X must equal the respective amount declared for BA."));
				}

				bool _1XChargeButNoBA()
				{
					return chargeType == AISChargeCodeList.Codes._1X && !charges.Any(x => x.J7_ChargeType == AISChargeCodeList.Codes.BA) && !groupCharges.Any(x => x.J7_ChargeType == AISChargeCodeList.Codes.BA) && !invoice.GroupHeader.Charges.Any(x => x.J7_ChargeType == AISChargeCodeList.Codes.BA);
				}

				bool BAChargeButNo1XOrDifferentAmounts()
				{
					if (chargeType == AISChargeCodeList.Codes.BA)
					{
						var charges1x = charges.Where(x => x.J7_ChargeType == AISChargeCodeList.Codes._1X).Select(x => x.J7_Amount)
							.Concat(groupCharges.Where(x => x.J7_ChargeType == AISChargeCodeList.Codes._1X).Select(x => x.J7_Amount));

						if (charges1x.Any())
						{
							var sum1X = charges1x.Sum(x => x);
							var sumBA = charges.Where(x => x.J7_ChargeType == AISChargeCodeList.Codes.BA).Sum(x => x.J7_Amount)
								+ groupCharges.Where(x => x.J7_ChargeType == AISChargeCodeList.Codes.BA).Sum(x => x.J7_Amount);
							return sumBA != sum1X;
						}
						else
						{
							return !(invoice.GroupHeader.Charges.Where(x => x.J7_ChargeType == AISChargeCodeList.Codes._1X).Sum(x => x.J7_Amount) == 0m && parent.J7_Amount == 0m);
						}
					}
					else
					{
						return false;
					}
				}
			}
		}
	}
}
