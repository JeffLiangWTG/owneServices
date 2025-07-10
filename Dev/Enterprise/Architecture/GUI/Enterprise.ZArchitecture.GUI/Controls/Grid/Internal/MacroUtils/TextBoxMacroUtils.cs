using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	static class TextBoxMacroUtils
	{
		public static Type[] GetRootTypes(this IMacroBox macroBox)
		{
			switch (macroBox.DataSource)
			{
				case IRootTypeProvider provider:
					return provider.RootTypes;
				case object source:
					return new[] { source.GetType() };
				default:
					return null;
			}
		}

		public static BusinessObject[] GetRoots(this IMacroBox macroBox)
		{
			switch (macroBox.DataSource)
			{
				case IRootTypeProvider provider:
					return provider.Roots;
				case BusinessObject bizo:
					return new[] { bizo };
				default:
					return null;
			}
		}

		public static EventHandler GetInsertMacroHandler(this IMacroBox macroBox)
		{
			if (macroBox.IsMacroControl)
			{
				var rootTypes = macroBox.GetRootTypes();
				return rootTypes != null && rootTypes.Length > 0 ? (s, e) => macroBox.InsertMacro() : null;
			}
			return null;
		}

		public static EventHandler GetPreviewMacroHandler(this IMacroBox macroBox)
		{
			if (macroBox.IsMacroControl)
			{
				var roots = macroBox.GetRoots();
				return roots != null && roots.Length > 0 ? (s, e) => PreviewMacro(macroBox) : null;
			}
			return null;
		}

		public static void PreviewMacro(this IMacroBox macroBox)
		{
			ZFormModaliser.Show(new TextTemplatePreviewForm { PreviewText = macroBox.GetPreviewText() }, macroBox.TextBox.FindForm());
		}

		public static void InsertMacro(this IMacroBox macroBox)
		{
			var rootTypes = GetRootTypes(macroBox);
			if (rootTypes != null && rootTypes.Length > 0)
			{
				if (macroBox.MacroManager == null)
				{
					macroBox.MacroManager = ObjectFactory.Get<IMapTreePresentationManager>();
					macroBox.MacroManager.ModalParent = macroBox.TextBox.FindForm();
					macroBox.MacroManager.MacroSelected += macro => MacroSelected(macroBox, macro);
					macroBox.MacroManager.DataFieldsOnly = macroBox.DataFieldsOnly;
					macroBox.MacroManager.MacroFieldsOnly = macroBox.MacroFieldsOnly;
				}
				macroBox.MacroManager.ParentTypes = rootTypes;
				macroBox.MacroManager.ShowPresentationManagerForm();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "System constant")]
		public static string GetPreviewText(this IMacroBox macroBox)
		{
			var text = (ZString)macroBox.TextBox.Text;
			var roots = GetRoots(macroBox);

			if (roots != null && roots.Length > 0)
			{
				const string tempOpeningBraket = "||&lt;||";
				const string tempClosingBraket = "||&gt;||";

				if (macroBox.MacroOpeningBracket != MacroConstants.DefaultMacroOpeningBracket)
				{
					text = text.Replace(MacroConstants.DefaultMacroOpeningBracket, tempOpeningBraket);
					text = text.Replace(macroBox.MacroOpeningBracket, MacroConstants.DefaultMacroOpeningBracket);
				}
				if (macroBox.MacroClosingBracket != MacroConstants.DefaultMacroClosingBracket)
				{
					text = text.Replace(MacroConstants.DefaultMacroClosingBracket, tempClosingBraket);
					text = text.Replace(macroBox.MacroClosingBracket, MacroConstants.DefaultMacroClosingBracket);
				}

				text = text.ReplaceIgnoringCase(MacroConstants.HtmlNewLineTag, MacroConstants.NewLineTag);
				text = ObjectFactory.Get<ITextMacroProcessor>().Replace(text, roots);

				if (macroBox.MacroOpeningBracket != MacroConstants.DefaultMacroOpeningBracket)
				{
					text = text.Replace(tempOpeningBraket, MacroConstants.DefaultMacroOpeningBracket);
				}
				if (macroBox.MacroClosingBracket != MacroConstants.DefaultMacroClosingBracket)
				{
					text = text.Replace(tempClosingBraket, MacroConstants.DefaultMacroClosingBracket);
				}
			}
			return text;
		}

		public static void MacroSelected(this IMacroBox macroBox, string macro)
		{
			var textBox = macroBox.TextBox;
			macroBox.StartedEditing(textBox);

			if (macroBox.MacroOpeningBracket != MacroConstants.DefaultMacroOpeningBracket)
			{
				macro = macro.Replace(MacroConstants.DefaultMacroOpeningBracket, macroBox.MacroOpeningBracket);
			}
			if (macroBox.MacroClosingBracket != MacroConstants.DefaultMacroClosingBracket)
			{
				macro = macro.Replace(MacroConstants.DefaultMacroClosingBracket, macroBox.MacroClosingBracket);
			}

			if (macroBox.AllowMultipleMacros)
			{
				textBox.SelectedText = macro;
			}
			else
			{
				textBox.Text = macro;
			}
		}
	}
}
