using System;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusUnderbondCusOutturnCollection : Customs.Business.CusUnderbondCusOutturnCollection
	{
		public CusUnderbondCusOutturnCollection(CusUnderbond underbond, Type typeOfElements)
			: base(underbond)
		{
			fTypeOfElements = typeOfElements;
		}

		public new CusUnderbond Master => (CusUnderbond)base.Master;

		public new CusOutturn this[int index]
		{
			get { return (CusOutturn)Elements[index]; }
		}

		public new CusOutturn AddNew()
		{
			return (CusOutturn)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return fTypeOfElements;
		}
		readonly Type fTypeOfElements;
	}
}
