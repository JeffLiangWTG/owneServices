using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsCommonCargoDescCollection<out T> : Customs.Business.ICusInBondCargoDescCollection<T>
		where T : NctsCommonCargoDesc
	{
		new T this[int index] { get; }
	}

	public class NctsCommonCargoDescCollection<T> : Customs.Business.CusInBondCargoDescCollection<T>, INctsCommonCargoDescCollection<T>
		where T : NctsCommonCargoDesc
	{
		public NctsCommonCargoDescCollection(BusinessObject master)
			: base(master)
		{
		}

		protected override void SetDefaultsForNewElementCore(T newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			if (Parent is NctsCommonMovementHeader movementHeader)
			{
				newElement.BY_RX_NKCurrency = movementHeader.Header?.LocalCurrency ?? ZString.Empty;
			}
			else if (Parent is NctsBill nctsBill)
			{
				var header = nctsBill.Header;
				newElement.BY_RX_NKCurrency = header?.LocalCurrency ?? ZString.Empty;
			}

			newElement.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
			newElement.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
		}

		protected override bool AllowNew => true;

		protected BusinessObject Parent => Relationship.Master.IsDeleted ? null : Relationship.Master;
	}
}
