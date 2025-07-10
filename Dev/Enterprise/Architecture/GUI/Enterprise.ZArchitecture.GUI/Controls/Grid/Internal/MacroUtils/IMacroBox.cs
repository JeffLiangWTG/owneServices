using System;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration.DocumentEngine;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IMacroBox
	{
		bool IsMacroControl { get; }
		ResourceStringData InsertMacroCaption { get; }
		string MacroOpeningBracket { get; }
		string MacroClosingBracket { get; }
		TextBox TextBox { get; }
		bool AllowMultipleMacros { get; }
		bool DataFieldsOnly { get; set; }
		bool MacroFieldsOnly { get; set; }
		void StartedEditing(TextBox t);
		object DataSource { get; }
		bool ShouldEscapeAllSpecialCharacters { get; set; }
		bool HideMacroFields { get; }
		bool HideDataFields { get; }
		bool ShowXmlFields { get; }
		int DefaultCollectionIndex { get; }
		Type XmlType { get; }
		bool UseMcrEvaluator { get; }

		EventHandler InsertMacroHandler { get; }
		EventHandler PreviewMacroHandler { get; }

		IMapTreePresentationManager MacroManager { get; set; }
	}
}
