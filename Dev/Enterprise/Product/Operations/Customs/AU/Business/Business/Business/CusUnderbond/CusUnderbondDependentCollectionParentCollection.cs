using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusUnderbondUnionCollectionParentCollection : Customs.Business.CusUnderbondUnionCollectionParentCollection
	{
		public CusUnderbondUnionCollectionParentCollection(BusinessObjectFactory factory, Type typeOfElements)
			: base(factory, typeOfElements)
		{
		}

		public CusUnderbondUnionCollectionParentCollection(BusinessObjectFactory factory)
			: this(factory, typeof(IAUCusUnderbondUnionCollectionParent))
		{
		}

		public new IAUCusUnderbondUnionCollectionParent this[int index]
		{
			get { return (IAUCusUnderbondUnionCollectionParent)Elements[index]; }
		}
	}
}
