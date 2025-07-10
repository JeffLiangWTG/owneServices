using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.Business
{
	public class WriteOffResultCollection : NonPersistentBusinessObjectCollection<WriteOffResult>
	{
		public WriteOffResultCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		protected override bool AllowNewCore => false;
	}
}
