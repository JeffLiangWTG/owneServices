using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.JobInvoicing.InvoiceLineToChargeMatching
{
	public abstract class InvoiceLineGroupingProvider
	{
		protected InvoiceLineGroupingProvider(IEnumerable<UniversalTransactionLineWrapper> universalTransactionLineWrappers)
		{
			UniversalTransactionLineWrappers = universalTransactionLineWrappers;
		}

		IEnumerable<UniversalTransactionLineWrapper> UniversalTransactionLineWrappers { get; }

		public ZInt Weight => Constants.InvoiceLineToChargeMatchingFactor.InvoiceLineGroupFactor * GetWeight();
		protected abstract ZInt GetWeight();
		IReadOnlyList<InvoiceLineGroup> GetGroups() =>
			UniversalTransactionLineWrappers.Cast<UniversalTransactionLineWrapper>()
				.GroupBy(x => GetGroupingKey(x))
				.Select(group => new InvoiceLineGroup(group)).ToList();
		protected abstract GroupingKey GetGroupingKey(UniversalTransactionLineWrapper x);

		public IReadOnlyList<InvoiceLineGroup> Groups => groups ?? (groups = GetGroups());
		IReadOnlyList<InvoiceLineGroup> groups;
	}

	public class LineGroupingByJobAndChargeCodeProvider : InvoiceLineGroupingProvider
	{
		public LineGroupingByJobAndChargeCodeProvider(IEnumerable<UniversalTransactionLineWrapper> invoiceLineWrapperCollection)
			: base(invoiceLineWrapperCollection)
		{
		}
		protected override ZInt GetWeight() => Constants.InvoiceLineToChargeMatchingGroupWeight.JobAndChargeUsedWeight;

		protected override GroupingKey GetGroupingKey(UniversalTransactionLineWrapper x) => new GroupingKey(x.ChargeCodeSource, x.JobHeader?.JH_JobNum ?? ZString.Empty, x.JobConsol?.JK_UniqueConsignRef ?? ZString.Empty, x.OSCurrency);
	}

	public class LineGroupingByJobProvider : InvoiceLineGroupingProvider
	{
		public LineGroupingByJobProvider(IEnumerable<UniversalTransactionLineWrapper> invoiceLineWrapperCollection)
			: base(invoiceLineWrapperCollection)
		{
		}

		protected override ZInt GetWeight() => Constants.InvoiceLineToChargeMatchingGroupWeight.JobUsedWeight;

		protected override GroupingKey GetGroupingKey(UniversalTransactionLineWrapper x) => new GroupingKey(x.JobHeader?.JH_JobNum ?? ZString.Empty, x.JobConsol?.JK_UniqueConsignRef ?? ZString.Empty, x.OSCurrency);
	}
}
