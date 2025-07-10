using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusNctsContainerGenPivotTypeDecider : TypeDecider, Integration.Customs.EU.NCTS.ICusNctsContainerGenPivotTypeDecider
	{
		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForNew() => null;

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var relation1TableCode = new ZString(row[GenPivotSchema.Constants.XX_Relation1TableCode]);
			return relation1TableCode.EqualsIgnoringCase(CusInvPackSchema.Constants.Prefix) ? typeof(NctsCusInBondContainerPackageGenPivot) : typeof(GenPivot);
		}
	}
}
