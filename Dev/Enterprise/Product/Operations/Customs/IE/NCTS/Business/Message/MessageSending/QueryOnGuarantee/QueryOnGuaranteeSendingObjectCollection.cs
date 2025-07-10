using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.NCTS.Business;
using NctsGuarantee = Enterprise.Customs.IE.NCTS.Business.NctsGuarantee;
using NctsHeader = Enterprise.Customs.IE.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.IE.Business
{
	public class QueryOnGuaranteeSendingObjectCollection : NonPersistentBusinessObjectCollection<QueryOnGuaranteeSendingObject>
	{
		public QueryOnGuaranteeSendingObjectCollection(NctsHeader nctsHeader) : base(nctsHeader.Factory)
		{
			header = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		}

		readonly NctsHeader header;

		public void PopulateElements()
		{
			RemoveAndDeleteAll();
			foreach (var guarantee in header.MovementHeader.Guarantees.Cast<NctsGuarantee>())
			{
				Add(new QueryOnGuaranteeSendingObject(guarantee));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotSupportedException("Allow new is false so shouldn't get called");

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
