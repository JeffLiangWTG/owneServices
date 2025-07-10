using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class AmendedItemCollection : NonPersistentBusinessObjectCollection<AmendedItem>
	{
		public AmendedItemCollection(IEnumerable<AmendedItem> items, CodeDescriptionPairList dataItemIDList)
		{
			foreach (var item in items)
			{
				item.DataItemDescription = dataItemIDList.GetDescriptionFromCode(item.DataItemID);
				Add(item);
			}
		}
		public AmendedItemCollection(IEnumerable<AmendedItem> items)
		{
			foreach (var item in items)
			{
				Add(item);
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotImplementedException();

		protected override bool AllowNewCore => false;
	}
}
