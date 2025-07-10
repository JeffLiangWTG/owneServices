
namespace Enterprise.DocumentEngine
{
	public enum MenuEditingMode
	{
		AllowAll,
		NotAllowEditingOfSystemOrClientMenus,
		AllowEditingOfSystemDefinedOnly,
		AllowEditingOfClientSpecificOnly
	}

	public interface IMenuEditable
	{
		MenuEditingMode EditingMode { get; set; }
	}
}
