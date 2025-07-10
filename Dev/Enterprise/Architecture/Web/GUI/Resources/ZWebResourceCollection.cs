using System.Collections;

namespace Enterprise.ZArchitecture.Web.GUI
{
	/// <summary>
	/// Collection of WebResources
	/// </summary>
	public class ZWebResourceCollection : CollectionBase
	{
		public virtual void Add(ZWebResource newResource)
		{
			this.List.Add(newResource);
		}

		public virtual ZWebResource this[int index]
		{
			get { return (ZWebResource)this.List[index]; }
		}
	}
}
