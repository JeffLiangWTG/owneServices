using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusHAWBBaseTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var mawb = factory.Load<Customs.Business.CusMAWB>(new ZGuid(row[CusHAWB.Schema.CS_CM]));
			if (mawb == null && GetApplicationCode(row) == Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages)
			{
				return typeof(CusHAWB);
			}
			if (mawb is CusMAWB)
			{
				return typeof(CusHAWB);
			}
			if (mawb is CTOCusMAWB)
			{
				return typeof(CTOCusHAWB);
			}

			return typeof(Customs.Business.CusHAWB);
		}

		string GetApplicationCode(DataRow row) => row[CusHAWB.Schema.CS_ApplicationCode].ToString();

		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}
	}
}
