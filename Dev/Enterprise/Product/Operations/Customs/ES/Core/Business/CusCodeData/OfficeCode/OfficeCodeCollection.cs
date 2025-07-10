using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business
{
	public class OfficeCodeCollection : EuOfficeCodeCollection
	{
		public OfficeCodeCollection(JobDeclaration master) : base(master)
		{
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeof(OfficeCode);

		public new OfficeCode AddNew() => (OfficeCode)base.AddNew();

		public new OfficeCode this[int index] => (OfficeCode)base[index];
	}
}
