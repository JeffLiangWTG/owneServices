using System;

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class IReversingCollection : ITransactionCollection<IReversingImplicitlyImplementedWrapperForBinding>
	{
		public IReversingCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("Adding to this collection is not supported.");
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		public override void Load(ZQuery alternativeAdditionalFilter)
		{
			throw new NotSupportedException("Loading to this collection is not supported.");
		}
	}
}
