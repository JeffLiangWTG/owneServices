namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Interface to provide focus information for composite controls
	/// </summary>
	public interface IFocusCompositeControl
	{
		string ChildControlIDForFocus { get; }
	}
}
