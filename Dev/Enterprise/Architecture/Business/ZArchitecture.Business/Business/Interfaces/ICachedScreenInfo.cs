using System.Collections.Generic;
using Enterprise.Core.Forms;

namespace Enterprise.ZArchitecture.Business
{
	public interface ICachedScreenInfo
	{
		IEnumerable<IPhysicalScreenInfo> Screens { get; }
	}
}
