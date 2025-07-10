#if DEBUG
using System;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.DocBuilder.BulkTemplateUpdating
{
	public class UpdateTemplateCommandCollection : NonPersistentBusinessObjectCollection<UpdateTemplateCommand>
	{
		public UpdateTemplateCommandCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override bool AllowSort
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}
	}
}
#endif
