using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsArrivalCargoDescCollection<out T> : INctsCommonCargoDescCollection<T>
		where T : NctsArrivalCargoDesc
	{
		new T this[int index] { get; }
	}

	public class NctsArrivalCargoDescCollection<T> : NctsCommonCargoDescCollection<T>, INctsArrivalCargoDescCollection<T>
		where T : NctsArrivalCargoDesc
	{
		public NctsArrivalCargoDescCollection(NctsBill nctsBill)
			: base(nctsBill)
		{
			if (!nctsBill.Header?.IsArrivalMovement ?? false)
			{
				AdditionalFilter = new ZQuery { IsNoResultQuery = true };
			}
		}

		public NctsArrivalCargoDescCollection(NctsArrivalMovementHeader movementHeader)
			: base(movementHeader)
		{
		}

		protected override void SetDefaultsForNewElementCore(T newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		}
	}
}
