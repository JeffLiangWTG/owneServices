using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgMatchings.MatchProduct
{
	abstract class PartConverter
	{
		internal PartConverter(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public IOrgHeader FindBuyer(IEntity order)
		{
			return FindOrgHeader(order, "Buyer");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public IOrgHeader FindSupplier(IEntity order)
		{
			return FindOrgHeader(order, "Supplier");
		}

		IOrgHeader FindOrgHeader(IEntity order, string type)
		{
			if (order.Parents.Any(p => p.EntityName == type))
			{
				var entity = order.Parents.First(p => p.EntityName == type);
				return factory.Load<OrgHeader>(entity.InternalPK);
			}

			return null;
		}
	}
}
