using System.Collections.ObjectModel;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public class ZNodeCollection<T> : Collection<ZNode<T>>
		where T : class, IBusiness
	{
	}
}
