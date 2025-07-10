using System;
using CargoWise.Data;
using CargoWise.Organizations.CodeGeneration;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Converters;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgCodeGenerations
{
	public class OrgCodeInfoConverter : IEntityConverter<IOrgCodeInfo>
	{
		public OrgCodeInfoConverter(DbConnection connection, AncillaryImportServices sessionServices)
		{
			converter = new ERConverter(connection, sessionServices);
		}
		readonly ERConverter converter;

		public IOrgCodeInfo Convert(IEntity entity)
		{
			return Convert(entity, null);
		}

		public IOrgCodeInfo Convert(IEntity entity, IEntityContext context)
		{
			if (entity.EntityName != "OrgHeader")
			{
				throw new ArgumentException("Only OrgHeader Entity could Convert to IOrgCodeInfo");
			}
			var cacheAlwaysUseInternalPK = false;
			if (context != null)
			{
				cacheAlwaysUseInternalPK = context.AlwaysUseInternalPK;
				context.AlwaysUseInternalPK = true;
			}

			var row = converter.Convert(entity, context);

			if (context != null)
			{
				context.AlwaysUseInternalPK = cacheAlwaysUseInternalPK;
			}

			return new OrgCodeDataProvider(entity.InternalPK, row);
		}
	}
}
