using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class LocalExportStevedoreCollectionWrapper : NonPersistentBusinessObjectCollection<LocalExportStevedoreWrapper>
	{
		public LocalExportStevedoreCollectionWrapper(IEnumerable<ILocalExportStevedore> stevedores, BusinessObjectFactory factory)
			: base(factory)
		{
			PopulateElements(factory, stevedores);
		}

		void PopulateElements(BusinessObjectFactory factory, IEnumerable<ILocalExportStevedore> stevedores)
		{
			if (stevedores != null)
			{
				foreach (var item in stevedores)
				{
					Add(new LocalExportStevedoreWrapper(item, factory));
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotImplementedException();

		protected override bool AllowNewCore => false;
	}
}
