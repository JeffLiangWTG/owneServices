using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class ExportCustomsManifestHeaderTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return defaultType;
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			if (((string)row[ExportCustomsManifestHeader.Schema.ED_ManifestType]) == AirManifestTypeList.Codes.CtoReceivalRemovalStandAlone)
			{
				return airCTOType;
			}

			return defaultType;
		}

		public override Type GetTypeForNew()
		{
			return defaultType;
		}

		readonly Type defaultType = typeof(ExportCustomsManifestHeader);
		readonly Type airCTOType = typeof(AirCTOExportCustomsManifestHeader);
	}
}
