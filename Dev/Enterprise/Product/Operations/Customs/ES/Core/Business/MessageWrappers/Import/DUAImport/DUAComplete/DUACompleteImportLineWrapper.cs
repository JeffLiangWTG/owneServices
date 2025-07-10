using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DUACompleteImportLineWrapper : DUAImportCommonLineWrapper, IDUACompleteImportLine
	{
		public DUACompleteImportLineWrapper(CusEntryLine cusEntryLine, ZBool isCanary) : base(cusEntryLine, isCanary)
		{
		}

		public ZString REACode => entryLine.RandomLine.ZG_REAProductCode;

		public ZDecimal PositiveAdjustment
		{
			get
			{
				if (positiveAdjustment == null)
				{
					positiveAdjustment = new CachedProperty<ZDecimal>(entryLine.Factory, () =>
					{
						var amount = ZDecimal.Zero;
						amount += entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(line => line.JI_PosAdj);

						return amount;
					});
				}
				return positiveAdjustment.Value;
			}
		}
		CachedProperty<ZDecimal> positiveAdjustment;

		public ZDecimal NegativeAdjustment
		{
			get
			{
				if (negativeAdjustment == null)
				{
					negativeAdjustment = new CachedProperty<ZDecimal>(entryLine.Factory, () =>
					{
						var amount = ZDecimal.Zero;
						amount += entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(line => line.JI_NegAdj);

						return -amount;
					});
				}
				return negativeAdjustment.Value;
			}
		}
		CachedProperty<ZDecimal> negativeAdjustment;

		public ZDecimal StatisticalValue => entryLine.CL_StatisticalValue;
	}
}
