using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public enum AmendedItemType
	{
		All,
		Header,
		Line
	}
	public class LocalExportAmendItemWrapperCollection : NonPersistentBusinessObjectCollection<LocalExportAmendItemWrapper>
	{
		public LocalExportAmendItemWrapperCollection(IEnumerable<ILocalExportAmendItem> amendedItems, AmendedItemType amendedType, BusinessObjectFactory factory)
			: base(factory)
		{
			if (amendedItems == null)
			{
				return;
			}
			foreach (var amendedItem in amendedItems)
			{
				if (amendedType == AmendedItemType.All
					|| amendedType == AmendedItemType.Header && IsHeaderAmendItem(amendedItem)
					|| amendedType == AmendedItemType.Line && !IsHeaderAmendItem(amendedItem))
				{
					this.Add(new LocalExportAmendItemWrapper(amendedItem, factory));
				}
			}
		}
		static bool IsHeaderAmendItem(ILocalExportAmendItem amendItem)
		{
			return amendItem.ItemSequenceNumber == 0;
		}
		protected override bool AllowNewCore => false;
		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotSupportedException("The method is not supported.");
	}
}
