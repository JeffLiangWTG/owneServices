using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business
{
	public class KeyDataPairCollection : NonPersistentBusinessObjectCollection<KeyDataPair>, IKeyDataPairCollection
	{
		public KeyDataPairCollection(BusinessObjectFactory factory)
			: base(factory) { }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new KeyDataPair();
		}

		IKeyDataPair IBusinessObjectCollection<IKeyDataPair>.this[int index] => base[index];

		public IEnumerator<IKeyDataPair> GetEnumerator()
		{
			foreach (var keydata in (IEnumerable<BusinessObject>)this)
			{
				yield return (IKeyDataPair)keydata;
			}
		}

		IKeyDataPair IBusinessObjectCollection<IKeyDataPair>.AddNew()
		{
			return base.AddNew();
		}
	}
}
