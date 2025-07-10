namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public interface IDropDownListSupport : IBindToListSupport
	{
		string ValueFieldName { get; }
		string TextFieldName { get; }
	}
}
