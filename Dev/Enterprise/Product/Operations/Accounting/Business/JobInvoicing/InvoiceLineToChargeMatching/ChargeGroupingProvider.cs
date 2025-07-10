using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.JobInvoicing.InvoiceLineToChargeMatching
{
	public abstract class ChargeGroupingProvider
	{
		protected ChargeGroupingProvider(IEnumerable<RecordedCharges> charges)
		{
			Charges = charges;
		}

		IEnumerable<RecordedCharges> Charges { get; }
		public ZInt Weight => Constants.InvoiceLineToChargeMatchingFactor.ChargeGroupFactor * GetWeight();
		protected abstract ZInt GetWeight();

		IReadOnlyList<ChargeGroup> GetGroups()
		{
			return Charges.GroupBy(x => GetGroupingKey(x))
				.Select(group => new ChargeGroup(group)).ToList();
		}
		protected abstract GroupingKey GetGroupingKey(RecordedCharges x);

		public IReadOnlyList<ChargeGroup> Groups => groups ?? (groups = GetGroups());
		IReadOnlyList<ChargeGroup> groups;
	}

	public class ChargeGroupingByJobAndChargeCodeProvider : ChargeGroupingProvider
	{
		public ChargeGroupingByJobAndChargeCodeProvider(IEnumerable<RecordedCharges> charges)
			: base(charges)
		{
		}
		protected override ZInt GetWeight() => Constants.InvoiceLineToChargeMatchingGroupWeight.JobAndChargeUsedWeight;
		protected override GroupingKey GetGroupingKey(RecordedCharges x) => new GroupingKey(x.AC_Code, x.JH_JobNum, x.JK_UniqueConsignRef, x.E6_PK, x.Currency, x.OH_Code, x.OrgType);
	}

	public class ChargeGroupingByJobProvider : ChargeGroupingProvider
	{
		public ChargeGroupingByJobProvider(IEnumerable<RecordedCharges> charges)
			: base(charges)
		{
		}

		protected override ZInt GetWeight() => Constants.InvoiceLineToChargeMatchingGroupWeight.JobUsedWeight;
		protected override GroupingKey GetGroupingKey(RecordedCharges x) => new GroupingKey(x.JH_JobNum, x.JK_UniqueConsignRef, x.E6_PK, x.Currency, x.OH_Code, x.OrgType);
	}
}
