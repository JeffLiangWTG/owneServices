using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocPackLocationCollection : DocumentWrapperCollection
	{
		#region Constructors & Type Overriding
		public DocPackLocationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocPackLocationCollection(PackLocationCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public static DocPackLocationCollection New(BusinessObjectFactory factory)
		{
			DocPackLocationCollection result = null;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(factory);
			}
			else
			{
				result = new DocPackLocationCollection(factory);
			}
			return result;
		}

		protected delegate DocPackLocationCollection NewDelegate(BusinessObjectFactory factory);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();
		#endregion

		public new DocPackLocation this[int index]
		{
			get
			{
				return (DocPackLocation)base[index];
			}
		}
	}
}
