namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsArrivalAndUnloadingCargoDescCollection<out T> : INctsCommonCargoDescCollection<T>
		where T : NctsArrivalAndUnloadingCargoDesc
	{
		new T this[int index] { get; }

		void SetArrivalGoodsItemsReadOnly();
	}

	public class NctsArrivalAndUnloadingCargoDescCollection<T> : NctsCommonCargoDescCollection<T>, INctsArrivalAndUnloadingCargoDescCollection<T>
		where T : NctsArrivalAndUnloadingCargoDesc
	{
		public NctsArrivalAndUnloadingCargoDescCollection(NctsCommonMovementHeader movementHeader)
			: base(movementHeader)
		{
			SetArrivalGoodsItemsReadOnly();
		}

		protected internal NctsArrivalAndUnloadingCargoDescCollection(NctsContainer nctsContainer)
			: base(nctsContainer)
		{
		}

		protected override void SetDefaultsForNewElementCore(T newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			if (Parent is NctsCommonMovementHeader movementHeader && movementHeader.IsUnloadingMovementHeader)
			{
				newElement.IsNew = true;
			}
		}

		public void SetArrivalGoodsItemsReadOnly()
		{
			if (Parent is NctsArrivalMovementHeader movementHeader)
			{
				var header = movementHeader?.Header;
				if (header != null)
				{
					if (!header.IsPhase5 && (header.Configuration.ReceiveIE043UnloadingPermissionDetailsMessage || movementHeader.AutoPopulatedArrivalGoodsItems))
					{
						SetReadOnlyIncludingChildren(true);
					}
					else
					{
						SetReadOnlyIncludingChildren(false);
					}
				}
			}
		}
	}
}
