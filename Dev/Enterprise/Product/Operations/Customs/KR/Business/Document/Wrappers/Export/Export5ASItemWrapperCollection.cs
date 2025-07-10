using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class Export5ASItemWrapperCollection : NonPersistentBusinessObjectCollection<Export5ASItemWrapper>
	{
		public Export5ASItemWrapperCollection(IEnumerable<Export5ASItem> exportAmendItems, BusinessObjectFactory factory)
			: base(factory)
		{
			PopulateElements(factory, exportAmendItems);
		}

		void PopulateElements(BusinessObjectFactory factory, IEnumerable<Export5ASItem> exportAmendItems)
		{
			if (exportAmendItems != null)
			{
				foreach (var item in exportAmendItems)
				{
					Add(new Export5ASItemWrapper(item, factory));
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotImplementedException();

		protected override bool AllowNewCore => false;
	}
}
