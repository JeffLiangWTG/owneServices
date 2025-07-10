using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.DocumentWrappers;

namespace Enterprise.Client.ATL.DocWrappers
{
	public class DocATLAPPayment : DocAPPayment
	{
		#region Constructors & Type Overriding
		protected DocATLAPPayment(Payment payment, BusinessObjectFactory factoryToWrap)
			: base(payment, factoryToWrap)
		{
		}

		public new static DocATLAPPayment New(Payment payment, BusinessObjectFactory factoryToWrap)
		{
			return (payment != null) ? new DocATLAPPayment(payment, factoryToWrap) : null;
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(OverriddenNewMethod);
		}

		static DocATLAPPayment OverriddenNewMethod(Payment payment, BusinessObjectFactory factoryToWrap)
		{
			return DocATLAPPayment.New(payment, factoryToWrap);
		}

		#endregion

		#region Wrapper Fields

		public override ZString ChequeAmountAsString
		{
			get
			{
				return Cheque.ChequeAmount.ToString("#,##0.00");
			}
		}

		public override ZString RemittanceAdviceDetailsHeadings
		{
			get
			{
				return AlignTextToTheLeft("DATE", DateWidth)
					+ new ZString(' ', SpacerWidth)
					+ AlignTextToTheLeft("CHEQUE", ChequeWidth)
					+ new ZString(' ', SpacerWidth)
					+ AlignTextToTheLeft("REFERENCE", ReferenceWidth)
					+ new ZString(' ', SpacerWidth)
					+ AlignTextToTheLeft("DETAILS", DetailsWidth)
					+ new ZString(' ', SpacerWidth)
					+ AlignTextToTheRight("TRANSACTION AMOUNTS", TransactionAmountsWidth);
			}
		}

		public override ZString RemittanceAdviceDetails
		{
			get
			{
				ZString result = ZString.Empty;

				if (Invoices.Count > 0 && Invoices.Count <= NumberOfTransactionLinesThatCanFitOnThePage)
				{
					foreach (DocTransactionHeader item in Invoices)
					{
						result += AlignTextToTheLeft(item.InvoiceDate.ToShortDateString(), DateWidth)
									+ new ZString(' ', SpacerWidth)
									+ AlignTextToTheLeft(ChequeNumber, ChequeWidth)
									+ new ZString(' ', SpacerWidth)
									+ AlignTextToTheLeft(item.TransactionType + " " + item.TransactionNumber, ReferenceWidth)
									+ new ZString(' ', SpacerWidth)
									+ AlignTextToTheLeft(item.Desc, DetailsWidth)
									+ new ZString(' ', SpacerWidth)
									+ item.Currency.Code
									+ new ZString(' ', SpacerWidth)
									+ AlignTextToTheRight(item.ApportionedAmount.ToString(2), TransactionAmountsWidth - item.Currency.Code.Length - SpacerWidth) + System.Environment.NewLine;
					}
					ZInt lengthOfColumn = DateWidth + SpacerWidth + ChequeWidth + SpacerWidth + ReferenceWidth + SpacerWidth + DetailsWidth + SpacerWidth + TransactionAmountsWidth;
					result += AlignTextToTheRight("-------------", lengthOfColumn) + System.Environment.NewLine;

					ZString totalAmountAsString = Currency.Code + new ZString(' ', SpacerWidth) + AlignTextToTheRight(ChequeAmountAsString, TransactionAmountsWidth - Currency.Code.Length - SpacerWidth);
					result += AlignTextToTheRight(totalAmountAsString, lengthOfColumn);
				}

				return result;
			}
		}

		#endregion
	}
}
