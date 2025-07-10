using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class SupportingDocumentWrappersCollection : NonPersistentBusinessObjectCollection<SupportingDocumentWrapper>
	{
		public SupportingDocumentWrappersCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("SupportingDocumentWrapper cannot be created by users in grid.");
		}
	}
}
