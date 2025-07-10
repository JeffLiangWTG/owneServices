using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusUnderbondCollection : Customs.Business.CusUnderbondCollection
	{
		public CusUnderbondCollection(ICusUnderbondDependentCollectionParent parent)
			: base(parent)
		{
		}

		public new CusUnderbond this[int index]
		{
			get { return (CusUnderbond)Elements[index]; }
		}

		public new CusUnderbond AddNew() => (CusUnderbond)base.AddNew();
		public new CusUnderbond AddNew(Type businessObjectType) => (CusUnderbond)base.AddNew(businessObjectType);
		public override Type GetTypeOfElementsFromPK(ZGuid pK) => typeof(CusUnderbond);
	}
}
