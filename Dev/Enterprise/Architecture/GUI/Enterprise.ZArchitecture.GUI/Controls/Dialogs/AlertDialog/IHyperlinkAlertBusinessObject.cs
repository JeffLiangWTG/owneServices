using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IHyperlinkAlertBusinessObject : IBusiness
	{
		ZString MessageLabel { get; }
		ZString LongMessageText { get; }
	}
}
