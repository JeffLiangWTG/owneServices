using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.Freight.Integration;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgMatchings.MatchProduct
{
	class EntityToPartConverter : PartConverter, IConverter<IEntity, IPart>
	{
		internal EntityToPartConverter(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region SuppressResourceStringsCheckRegion
		public IPart Convert(IEntity orderline)
		{
			var part = new Part();

			if (orderline.Properties.Any(p => p.Name == "Partno"))
			{
				part.PartNum = (string)orderline["Partno"];
			}

			if (orderline.Properties.Any(p => p.Name == "Description"))
			{
				part.Description = (string)orderline["Description"];
			}

			var order = orderline.Parent;
			var buyer = FindBuyer(order);
			var supplier = FindSupplier(order);

			part.Buyer = buyer;
			part.Supplier = supplier;
			return part;
		}

		#endregion
	}
}
