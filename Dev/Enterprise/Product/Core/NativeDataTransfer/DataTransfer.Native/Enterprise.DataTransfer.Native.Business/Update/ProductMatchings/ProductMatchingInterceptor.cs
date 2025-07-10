using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business.ProductMatching;
using Enterprise.DataTransfer.Native.Business.Update.OrgMatchings.MatchProduct;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Interceptors;

namespace Enterprise.DataTransfer.Native.Business.Update.ProductMatchings
{
	public class ProductMatchingInterceptor : BaseInterceptor
	{
		public ProductMatchingInterceptor(ProductMatchingSetting setting, AncillaryImportServices sessionServices)
			: base(setting, sessionServices)
		{
			factory = setting.Context.ObjectFactory;
			partConverter = new EntityToPartConverter(factory);
		}
		readonly BusinessObjectFactory factory;
		readonly EntityToPartConverter partConverter;

		public override void Invoke(IEntitySet entitySet)
		{
			var order = entitySet.Root;

			var orderItems = FindOrderItems(order);
			foreach (var orderItem in orderItems)
			{
				CreateMissingProduct(orderItem);
			}

			Function(entitySet);
		}

		#region SuppressResourceStringsCheckRegion

		static IEnumerable<IEntity> FindOrderItems(IEntity order)
		{
			return order.Children.Where(p => p.EntityName == "JobOrderLine");
		}

		#endregion

		public void CreateMissingProduct(IEntity orderline)
		{
			var part = partConverter.Convert(orderline);
			new ProductMatcher(factory).Match(part, out _, out _);
		}
	}
}
