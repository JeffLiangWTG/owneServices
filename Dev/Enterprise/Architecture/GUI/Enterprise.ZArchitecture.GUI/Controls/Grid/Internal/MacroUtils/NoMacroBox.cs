using System;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration.DocumentEngine;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// Document macro boilerplate for GUI controls with no macro support.
	/// </summary>
	class NoMacroBox : IMacroBox
	{
		bool IMacroBox.IsMacroControl => false;
		ResourceStringData IMacroBox.InsertMacroCaption => null;
		string IMacroBox.MacroOpeningBracket => string.Empty;
		string IMacroBox.MacroClosingBracket => string.Empty;
		TextBox IMacroBox.TextBox => null;
		bool IMacroBox.AllowMultipleMacros => false;
		bool IMacroBox.DataFieldsOnly { get; set; }
		bool IMacroBox.MacroFieldsOnly { get; set; }
		object IMacroBox.DataSource => null;
		EventHandler IMacroBox.InsertMacroHandler => null;
		EventHandler IMacroBox.PreviewMacroHandler => null;
		IMapTreePresentationManager IMacroBox.MacroManager { get; set; }
		void IMacroBox.StartedEditing(TextBox t) { }
		bool IMacroBox.ShouldEscapeAllSpecialCharacters { get; set; }
		public bool HideMacroFields => false;
		public bool HideDataFields => false;
		public bool ShowXmlFields => false;
		public int DefaultCollectionIndex => 1;
		public Type XmlType => null;
		public bool UseMcrEvaluator => false;
	}
}
