using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public interface IWebControlWithPostbackNewValue
	{
		ZString NewValue
		{
			get;
		}
	}
}