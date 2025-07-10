using System.Drawing;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Notifications
{
	/// <summary>
	/// Represents notification icon layout.
	/// </summary>
	public interface IIconLayout
	{
		/// <summary>
		/// Gets or sets the layout's target.
		/// </summary>
		/// <value>The target control.</value>
		Control Target { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether icon should be rendred as semi transparent.
		/// </summary>
		/// <value><c>true</c> if should be rendred as semi transparent; otherwise, <c>false</c>.</value>
		bool SemiTransparent { get; set; }

		/// <summary>
		/// Resizes the control if neccesary in order to accomodate notification icon.
		/// </summary>
		/// <param name="revert">if set to <c>true</c> then will revert previous resize.</param>
		void ResizeControl(bool revert);

		/// <summary>
		/// Computes the notification icon area.
		/// </summary>
		/// <param name="area">The initial control area (client rectangle).</param>
		/// <param name="clip">The clip.</param>
		/// <returns>Computed notification area rectangle.</returns>
		Rectangle Compute(Rectangle area, Size clip);
	}
}