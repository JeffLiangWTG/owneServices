using System;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IOverridableTextBox : IDisposable
	{
		string Text { get; set; }
		event EventHandler TextChanged;
		bool Checked { get; set; }
		event EventHandler CheckedChanged;
		int MaxLength { get; set; }
		event EventHandler MaxLengthChanged;
		string TextOverride { get; set; }
		event EventHandler TextOverrideChanged;
		string PlaceholderText { get; set; }
		event EventHandler PlaceholderTextChanged;
		bool TextIsOverridden { get; set; }
		event EventHandler TextIsOverriddenChanged;
		UpdateTextOnCheckedMode UpdateTextOnChecked { get; set; }
		event EventHandler UpdatesTextOnCheckedChanged;
	}
}
