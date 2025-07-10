using CargoWise.EntityFramework;
using Enterprise.ResourceStrings.Business;

namespace Enterprise.DocumentEngine
{
	public abstract class DocumentMacroUsageCollection : NonPersistentBusinessObjectCollection<DocumentMacroUsage>, IDocBuilderUsageCollection
	{
		public new IDocBuilderUsage this[int i]
		{
			get
			{
				return base[i];
			}
		}

		public override bool ReadOnly
		{
			get
			{
				return true;
			}
		}
	}
}
