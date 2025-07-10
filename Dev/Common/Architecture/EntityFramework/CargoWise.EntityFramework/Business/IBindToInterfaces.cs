using CargoWise.ComponentModel.Design;

namespace CargoWise.EntityFramework
{
	public interface IBindToList
	{
		[SmartTagVisible]
		string BindToList { get; set; }
	}

	/// <summary>
	/// Apply this interface to a control to override the resource string key that is used to look up
	/// resource string captions. IBindTo is used as a fallback.
	/// </summary>
	public interface IResourceStringBindingMember
	{
		string ResourceStringBindingMember { get; }
	}
}
