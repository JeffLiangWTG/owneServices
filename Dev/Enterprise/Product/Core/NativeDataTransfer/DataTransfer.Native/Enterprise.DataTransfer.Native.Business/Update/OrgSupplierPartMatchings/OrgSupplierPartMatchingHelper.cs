using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Business.Update.OrgMatchings.MatchProduct;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Utils.Models;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgSupplierPartMatchings
{
	public interface IOrgSupplierPartMatchingHelper
	{
		IPart ConvertPartEntityToPart(IEntity partEntity, IOrgHeader supplier, IOrgHeader buyer);
		IEnumerable<IEntity> FindSupplierPartEntities(IEntity order);
		IOrgHeader FindBuyer(IEntity order);
		IOrgHeader FindSupplier(IEntity order);
	}

	internal class OrgSupplierPartMatchingHelper : PartConverter, IOrgSupplierPartMatchingHelper
	{
		internal OrgSupplierPartMatchingHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public IPart ConvertPartEntityToPart(IEntity partEntity, IOrgHeader supplier, IOrgHeader buyer)
		{
			var part = new Part();
			part.PartNum = (string)partEntity["PartNum"];
			part.Buyer = supplier;
			part.Supplier = buyer;
			return part;
		}

		public IEnumerable<IEntity> FindSupplierPartEntities(IEntity order)
		{
			return order.Relatives().Where(e => e.EntityName == "OrgSupplierPart");
		}
	}
}