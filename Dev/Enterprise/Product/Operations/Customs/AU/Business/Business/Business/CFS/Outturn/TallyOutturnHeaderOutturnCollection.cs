using System;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class TallyOutturnHeaderOutturnCollection : CusOutturnHeaderDepotCusOutturnCollection
	{
		public TallyOutturnHeaderOutturnCollection(TallyOutturnHeader header)
			: base(header)
		{
		}

		public new TallyOutturn this[int index]
		{
			get { return (TallyOutturn)base[index]; }
		}

		public new TallyOutturn AddNew()
		{
			return (TallyOutturn)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pk)
		{
			return typeof(TallyOutturn);
		}
	}
}
