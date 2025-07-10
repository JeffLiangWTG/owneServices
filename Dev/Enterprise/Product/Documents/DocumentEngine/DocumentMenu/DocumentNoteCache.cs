using System.Collections.Generic;
using CargoWise.Definitions;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.SDF;

namespace Enterprise.DocumentEngine
{
	public class UserControlProviderListCacheItem
	{
		public UserControlProviderListCacheItem(StmSystemDefinedFieldCollection systemDefinedFieldCollection, UserControlProviderList userDefined)
		{
			SystemDefinedFieldCollection = systemDefinedFieldCollection;
			UserDefined = userDefined;
		}

		public StmSystemDefinedFieldCollection SystemDefinedFieldCollection { get; private set; }
		public UserControlProviderList UserDefined { get; private set; }
	}

	public class DocumentNoteCache : Dictionary<BusinessContext, UserControlProviderListCacheItem>
	{
	}
}
