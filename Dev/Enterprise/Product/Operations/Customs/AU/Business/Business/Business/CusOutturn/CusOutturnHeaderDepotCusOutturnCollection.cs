using System;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusOutturnHeaderDepotCusOutturnCollection : Customs.Business.CusOutturnHeaderCusOutturnCollection
	{
		public CusOutturnHeaderDepotCusOutturnCollection(CusOutturnHeader parent)
			: base(parent)
		{
		}

		public new DepotCusOutturn this[int index]
		{
			get { return (DepotCusOutturn)base[index]; }
		}

		public new DepotCusOutturn AddNew()
		{
			return (DepotCusOutturn)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(DepotCusOutturn);
		}
	}
}
