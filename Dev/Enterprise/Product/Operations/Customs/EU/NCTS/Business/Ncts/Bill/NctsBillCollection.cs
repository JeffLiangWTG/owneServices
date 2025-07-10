using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsBillCollection<out T> : IActiveBusinessObjectCollection<T>, ISequenceNumberHeader, ICusInBondBillCollection
		where T : NctsBill
	{
		new T this[int index] { get; }

		void ApplySort(string propertyName, ListSortDirection direction);
		void MarkAsNeedingValidation();
		void RefreshBinding();
		void RefreshMaxCountValidation();
		ShortSequenceNumberGenerator SequenceGenerator { get; }
		event CollectionCountChangedEventHandler CollectionCountChange;
	}

	public class NctsBillCollection<T> : CusInBondBillCollection<T>, INctsBillCollection<T>
		where T : NctsBill
	{
		public NctsBillCollection(NctsHeader nctsHeader)
			: base(nctsHeader)
		{
			this.nctsHeader = nctsHeader;
			EnableMaxCountValidation();
		}

		protected readonly NctsHeader nctsHeader;

		protected virtual int MaxCount => nctsHeader.IsInPhase5TransitionPeriod
			? (nctsHeader.IsArrivalMovement ? 1 + this.Count(x => x.MovementDetail.B9_UnloadedState == NctsUnloadedStateList.Codes.MIS) : 1)
			: 99;

		protected override bool AllowNew => !nctsHeader.ArrivalMovementHeader?.UnloadingDifferenceDataReadOnly ?? base.AllowNew;

		void EnableMaxCountValidation()
		{
			if (nctsHeader.IsRuleActive(x => x.IsRuleE1406Active))
			{
				var maxCount = MaxCount;
				this.EnableMaxCountValidationWithMessageError(maxCount, warnAtHalfway: false, Res.GetString("D747F5A9-BA39-4528-A9AB-891A39425FD3", "You may enter a maximum of {0} House Consignments.", maxCount));
			}
		}

		public void RefreshMaxCountValidation() => this.EnableMaxCountValidation();

		protected override object[] GetCollectionState()
		{
			return new object[] { SequenceGenerator };
		}

		public ShortSequenceNumberGenerator SequenceGenerator => sequenceNumberCalculator ?? (sequenceNumberCalculator = new ShortSequenceNumberGenerator(this));
		ShortSequenceNumberGenerator sequenceNumberCalculator;

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => this;
	}
}
