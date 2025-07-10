
namespace Enterprise.ZArchitecture.GUI
{
	using System.Windows.Forms;

	/// <summary>
	///		Defines members for managing the default binding for <see cref="ZBindingSource"/>.
	/// </summary>
	public interface IDefaultBindingSettings
	{
		/// <summary>
		///		Notifies the <see cref="ZBindingSource"/> to exclude <paramref name="control"/> from initialization with default binding.
		/// </summary>
		/// <param name="control"></param>
		void ExcludeFromDefaultBinding(Control control);
	}
}
