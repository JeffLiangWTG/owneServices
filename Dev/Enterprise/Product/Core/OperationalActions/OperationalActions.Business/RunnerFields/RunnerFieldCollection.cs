using System;
using CargoWise.EntityFramework;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class RunnerFieldCollection : NonPersistentBusinessObjectCollection<RunnerField>
	{
		public RunnerFieldCollection(BusinessObjectFactory factory)
			: base(factory) { }

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
	}
}
