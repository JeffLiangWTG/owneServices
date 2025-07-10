using Enterprise.DataTransfer.Business.ProductMatching;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgSupplierPartMatchings
{
	class OrgSupplierPartMatchingInterceptor : BaseInterceptor
	{
		public OrgSupplierPartMatchingInterceptor(IInterceptorSetting setting, AncillaryImportServices sessionServices)
			: base(setting, sessionServices)
		{
			var factory = setting.Context.ObjectFactory;
			Helper = new OrgSupplierPartMatchingHelper(factory);
			ProductMatcher = new ProductMatcher(factory);
		}

		#region Dependency

		internal IOrgSupplierPartMatchingHelper Helper { get; set; }
		internal ProductMatcher ProductMatcher { get; private set; }

		#endregion

		public override void Invoke(IEntitySet entitySet)
		{
			var order = entitySet.Root;
			var supplier = Helper.FindSupplier(order);
			var buyer = Helper.FindBuyer(order);
			MatchParts(order, supplier, buyer);
			Function(entitySet);
		}

		void MatchParts(IEntity order, IOrgHeader supplier, IOrgHeader buyer)
		{
			var partEntities = Helper.FindSupplierPartEntities(order);
			foreach (var partEntity in partEntities)
			{
				var part = Helper.ConvertPartEntityToPart(partEntity, supplier, buyer);
				ProductMatcher.Match(part, out var matchedPart, out _);
				if (matchedPart != null)
				{
					partEntity.InternalPK = matchedPart.PK.ToGuid();
				}
			}
		}
	}
}
