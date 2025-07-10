using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture
{
	public interface IZGridColourCustomiseForm : IZForm
	{
		ZFilterStripCommonControl StripControl { get; }
		DialogResult ShowDialog();
		IButtonControl CancelButton { get; }
	}
}

#if DEBUG
namespace Enterprise.ZArchitecture.Testing
{
	public interface IZGridColourCustomiseFormForTest : IZGridColourCustomiseForm
	{
		ZTabControl RulesTabControl { get; }
		void ClickRemoveRuleButton();
	}
}
#endif