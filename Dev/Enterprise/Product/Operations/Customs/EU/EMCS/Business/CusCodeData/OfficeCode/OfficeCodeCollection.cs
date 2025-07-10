using System;
using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class OfficeCodeCollection<T> : OfficeCodeCollection where T : OfficeCode
	{
		public OfficeCodeCollection(EMCSJobDeclaration master)
			: base(master)
		{
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeof(T);

		public new T AddNew() => (T)base.AddNew();

		public new T this[int index] => (T)base[index];
	}
}
