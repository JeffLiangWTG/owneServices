using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.CH.Business;

public abstract class DocumentWrapperCollectionWithLanguage<TObject, TWrappedObject> : DocumentWrapperCollection<TWrappedObject> where TWrappedObject : DocumentWrapper
{
	protected DocumentWrapperCollectionWithLanguage(IEnumerable<TObject> collection, BusinessObjectFactory factory, ZString documentLanguage)
		: base(collection.Select(item => (item, documentLanguage)), factory)
	{
	}

	protected sealed override DocumentWrapper WrapObject(object objectToWrap)
	{
		var tuple = (ValueTuple<TObject, ZString>)objectToWrap;
		return WrapObject(tuple.Item1, tuple.Item2);
	}

	protected abstract DocumentWrapper WrapObject(TObject objectToWrap, ZString documentLanguage);
}
