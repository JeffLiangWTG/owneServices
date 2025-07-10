using System;
using System.Collections;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public sealed class BufferedItemBufferPenetrationViewModelCollection : NonPersistentBusinessObjectCollection<BufferedItemBufferPenetrationViewModel>
	{
		public BufferedItemBufferPenetrationViewModelCollection(IBuffer buffer)
		{
			foreach (var item in buffer.RelatedBufferedItems)
			{
				Add(new BufferedItemBufferPenetrationViewModel(buffer, item));
			}
		}

		public BufferedItemBufferPenetrationViewModelCollection(IBufferedItem bufferedItem)
		{
			foreach (var buffer in bufferedItem.GetRelatedBuffers())
			{
				Add(new BufferedItemBufferPenetrationViewModel(buffer, bufferedItem));
			}
		}

		#region NonPersistentBusinessObjectCollection Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			return property.Name == "PenetrationPercent" ? new PercentageStringComparer(property, direction) : base.GetComparerForSort(property, direction);
		}

		#endregion
	}
}
