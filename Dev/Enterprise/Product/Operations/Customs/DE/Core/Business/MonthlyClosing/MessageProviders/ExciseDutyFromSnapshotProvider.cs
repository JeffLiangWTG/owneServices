using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	public sealed class ExciseDutyFromSnapshotProvider : IExciseDuty
	{
		public ExciseDutyFromSnapshotProvider(DEMonthlyClosingEntryLineSnapshotExciseDuty exciseDuty)
		{
			this.exciseDuty = Argument.NotNull(exciseDuty, nameof(exciseDuty));
		}
		readonly DEMonthlyClosingEntryLineSnapshotExciseDuty exciseDuty;

		public string Code => exciseDuty.Code;

		public decimal DegreePercentage => exciseDuty.DegreePercentage;

		public decimal Value => exciseDuty.Value;

		public IAmount Amount => CachedValueHelper.GetValue(ref amount, () => AmountFromSnapshotProvider.NewOrNull(exciseDuty.Amount));
		CachedValue<IAmount> amount;
	}
}
