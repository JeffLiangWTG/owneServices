using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsDeparturePayInfoTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type bizOType = null;

			var moveHeaderPK = (row != null) ? new ZGuid(row[Customs.Business.AutoCusInBondPayInfo.Schema.BPI_BM]) : ZGuid.Empty;
			if (!moveHeaderPK.IsEmpty)
			{
				var moveHeader = factory.Load<NctsDepartureMovementHeader>(moveHeaderPK);
				bizOType = moveHeader?.PayInfoType;
			}

			if (bizOType == null)
			{
				ErrorReporter.ReportOnce("NctsDeparturePayInfo type is unknown", "Cannot determine the NctsDeparturePayInfo object, because parent business object is unknown");
			}

			return bizOType;
		}

		public override Type GetTypeForNew() => null;
	}
}
