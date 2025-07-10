using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine.ReflectiveFieldMap.Testing
{
	public abstract class DocumentWrapperCollectionForTest : IBODocDataProviderCollection
	{
		public abstract DocumentWrapperForTest this[int index] { get; }

		#region IBODocDataProviderCollection Members

		int IBODocDataProviderCollection.Count
		{
			get { throw new NotImplementedException(); }
		}

		ZString IBODocDataProviderCollection.Format(ZString formatString, ZString delimiter, ZString filterString, ZString groupByParameters, ZInt maxItems)
		{
			throw new NotImplementedException();
		}

		object IBODocDataProviderCollection.Total(ZString fieldName, ZString decimalPlaces, ZString filter)
		{
			throw new NotImplementedException();
		}

		IBODocDataProvider IBODocDataProviderCollection.this[int index]
		{
			get { throw new NotImplementedException(); }
		}

		IBODocDataProvider IBODocDataProviderCollection.this[string index]
		{
			get { throw new NotImplementedException(); }
		}

		public BusinessObject Find(ZString match)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
