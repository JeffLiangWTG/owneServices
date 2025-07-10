namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public interface IPostbackConditionControl : IAutoPostbackControl
	{
		string PostbackCondition { get; set; }
	}
}
