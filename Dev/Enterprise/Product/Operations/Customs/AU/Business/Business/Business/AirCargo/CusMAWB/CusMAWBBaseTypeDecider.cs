using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusMAWBBaseTypeDecider : TypeDecider, ICusMAWBTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			if (new ZBool(row[CusMAWB.Schema.CM_IsCTOMAWB]))
			{
				return typeof(CTOCusMAWB);
			}
			else
			{
				return DefaultType;
			}
		}

		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}

		public Type DefaultType => typeof(CusMAWB);
	}
}
