using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsDepartureCargoDescTypeDecider : TypeDecider
	{
		public override Type GetTypeForNew() => null;
		public override Type GetTypeForBinding() => null;
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory) => new Customs.Business.CusInBondCargoDescTypeDecider().GetTypeForLoad(row, factory);
	}
}
