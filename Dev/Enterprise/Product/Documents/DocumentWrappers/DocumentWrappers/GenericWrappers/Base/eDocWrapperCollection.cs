using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentWrappers.GenericWrappers.Base
{
	public class eDocWrapperCollection : GenericWrapperCollection<eDocWrapper>
	{
		internal static eDocWrapperCollection New(IDocManagerSupport eDocsParent, BusinessObjectFactory factory)
		{
			if (eDocsParent != null)
			{
				var eDocs = eDocsParent.DocManagerInfo.Documents;
				if (eDocs != null)
				{
					return new eDocWrapperCollection(eDocs, factory);
				}
			}

			return new eDocWrapperCollection(factory);
		}

		public eDocWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public eDocWrapperCollection(IStorageDocsBaseCollection parentCollection, BusinessObjectFactory factory)
			: base(factory)
		{
			Argument.NotNull(parentCollection, "parentCollection");
			Load(parentCollection);
			EDocsInternal = parentCollection;
		}
		readonly IStorageDocsBaseCollection EDocsInternal;

		protected override DocumentWrapper WrapObject(object objectToWrap)
		{
			return new eDocWrapper((IeDoc)objectToWrap, Factory);
		}

		protected override IBODocDataProvider GetRow(ZString index)
		{
			if (index.Length is 3 or 4 && EDocsInternal != null)
			{
				var eDoc = EDocsInternal.GetMostRecentEDoc(index);
				if (eDoc != null)
				{
					return new eDocWrapper(eDoc, Factory);
				}
			}

			return base.GetRow(index);
		}
	}
}
