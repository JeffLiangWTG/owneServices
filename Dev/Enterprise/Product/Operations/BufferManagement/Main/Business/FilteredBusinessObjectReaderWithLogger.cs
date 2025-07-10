using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public class FilteredBusinessObjectReaderWithLogger : FilteredBusinessObjectReader
	{
		public FilteredBusinessObjectReaderWithLogger(BusinessObjectFactoryProvider factoryProvider, ZQuery objectFilter, Type businessObjectType, BatchLogger batchLogger)
			: base(factoryProvider, objectFilter, businessObjectType)
		{
			AllowTableValuedParameters = true;
			this.batchLogger = batchLogger;
		}

		readonly BatchLogger batchLogger;

		protected override List<ZGuid> GetObjectPKsCore()
		{
			using (batchLogger?.BatchLoadStarting())
			{
				return base.GetObjectPKsCore();
			}
		}

		protected override BusinessObject[] LoadWithCacheCore(int skipCount, SchemaGuidColumn pkColumn)
		{
			using (batchLogger?.BatchLoadStarting())
			{
				return base.LoadWithCacheCore(skipCount, pkColumn);
			}
		}
	}
}
