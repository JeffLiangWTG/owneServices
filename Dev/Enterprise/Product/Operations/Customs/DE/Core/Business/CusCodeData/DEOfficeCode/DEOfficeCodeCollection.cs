using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class DEOfficeCodeCollection : EuOfficeCodeCollection
	{
		public DEOfficeCodeCollection(BusinessObject master)
			: base(master)
		{
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeof(DEOfficeCode);

		public new DEOfficeCode AddNew() => (DEOfficeCode)base.AddNew();

		public new DEOfficeCode this[int index] => (DEOfficeCode)base[index];
	}
}
