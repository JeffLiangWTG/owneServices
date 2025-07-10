using System.Windows.Controls;

namespace BuildTools.Test
{
	/// <summary>
	/// add a xaml file here, and add the file path with lower case to safeguard list.
	/// if build result is successful, illustrate that the CheckNewXamlFile could handle the situation that on some developer's computer, the file system is upper or lower case totally.
	/// </summary>
	public partial class TestCheckNewXamlFile : UserControl
	{
		public TestCheckNewXamlFile()
		{
			InitializeComponent();
		}
	}
}
