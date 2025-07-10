using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Engine
{
	public static class ArchiveItemHelper
	{
		public static BusinessObject GetBusinessObjectFromPkAndTableCode(BusinessObjectFactory factory, Guid pk, string tableCode)
			=> factory.Load(BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(tableCode), pk);

		public static ArchiveItem FromBizo(BusinessObject bizo)
			=> new(bizo.PKSchemaColumn, bizo.PK.ToGuid());

		public static readonly HashSet<string> ValidTableCodes = new HashSet<string>()
		{
			JobShipmentSchema.Constants.Prefix,
			JobConsolSchema.Constants.Prefix,
			JobDeclarationSchema.Constants.Prefix,
			ProcessTasksSchema.Constants.Prefix,
			JobContainerSchema.Constants.Prefix,
			DummyBizoSchema.Constants.Prefix,
		};
	}
}
