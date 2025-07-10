using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.DocumentWrappers;

namespace Enterprise.Client.TIP.DocWrappers
{
	public class DocTIPAPPayment : DocAPPayment
	{
		#region Constructors
		protected DocTIPAPPayment(Payment payment, BusinessObjectFactory factoryToWrap)
			: base(payment, factoryToWrap)
		{
		}

		public new static DocTIPAPPayment New(Payment payment, BusinessObjectFactory factoryToWrap)
		{
			return (payment != null) ? new DocTIPAPPayment(payment, factoryToWrap) : null;
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(OverriddenNewMethod);
		}

		static DocTIPAPPayment OverriddenNewMethod(Payment payment, BusinessObjectFactory factoryToWrap)
		{
			return DocTIPAPPayment.New(payment, factoryToWrap);
		}

		#endregion

		#region Menu Filter Fields
		public override ZBool PrintStandard
		{
			get { return !PrintClientSpecific; }
		}

		public override ZBool PrintClientSpecific
		{
			get { return ZBool.True; }
		}
		#endregion

		public new DocMatchLink MatchLink
		{
			get
			{
				if (fMatchLink == null)
				{
					foreach (DocTransactionHeader docTransaction in ReceiptMatches)
					{
						if (docTransaction.TransactionPK != TransactionPK)
						{
							fMatchLink = docTransaction.MatchLink;
							break;
						}
					}
				}
				return fMatchLink;
			}
		}
		DocMatchLink fMatchLink;

		#region ChargeLines
		public DocTIPHeaderLineTransactionCollection ChargeLines
		{
			get
			{
				if (fChargeLines == null)
				{
					fChargeLines = new DocTIPHeaderLineTransactionCollection(Factory);
					foreach (DocTransactionHeader item in Payments)
					{
						TransactionHeaderWithLines itemWithLines = item.WrappedObject as TransactionHeaderWithLines;
						bool shouldAddEmptyLine = true;
						if (itemWithLines != null && itemWithLines.Lines.Count > 0)
						{
							shouldAddEmptyLine = false;
							foreach (DependentTransactionLine line in itemWithLines.Lines)
							{
								fChargeLines.Add(DocTIPHeaderLineTransaction.New(item, line, Factory));
							}
						}
						if (shouldAddEmptyLine)
						{
							fChargeLines.Add(DocTIPHeaderLineTransaction.New(item, null, Factory));
						}
					}
				}
				return fChargeLines;
			}
		}
		DocTIPHeaderLineTransactionCollection fChargeLines;
		#endregion
	}
}
