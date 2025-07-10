using System;
using System.Collections.Generic;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentWrappers.Mapping
{
	public class DocumentWrapperMapper : Mapper, IDocumentWrapperMapper
	{
		public DocumentWrapperMapper(Type wrapperTypeToMap, bool includeChildrenAndRelatedObjects, bool includeIBODocDataProviders)
			: base(wrapperTypeToMap, includeChildrenAndRelatedObjects, includeIBODocDataProviders, typeof(DocumentWrapper), typeof(DocumentWrapperCollection), new List<Type>(new Type[] { typeof(DocBaseWrapper), typeof(DocumentWrappersCore.DocBaseWrapperBaseWithImageSupport) }), null, null)
		{
		}

		#region IDocumentWrapperMapper Members

		string IDocumentWrapperMapper.GetMapAsText()
		{
			return GetMapAsText();
		}

		#endregion
	}
}
