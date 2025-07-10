using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Macros;

namespace Enterprise.Integration.DocumentEngine
{
	public delegate void MacroSelectedEventHandler(string macro);

	public interface IMapTreePresentationManager : IDisposable
	{
		string GetUserSelectionMacro();
		void ShowPresentationManagerForm(bool showIndex = true, bool shouldEscapeAllSpecialCharacters = false);
		event MacroSelectedEventHandler MacroSelected;
		Type[] ParentTypes { get; set; }
		string[] VariableNames { get; set; }
		Type[] VariableParentTypes { get; set; }
		bool HideDataFields { get; set; }
		bool HideMacroFields { get; set; }
		bool DataFieldsOnly { get; set; }
		bool MacroFieldsOnly { get; set; }
		bool ShowEditField { get; set; }
		object ModalParent { get; set; }
		string InitialText { get; set; }
		int MacroMaxLength { get; set; }
		bool UseBrowseMode { get; set; }
		bool UseMcrEvaluator { get; set; }
		int DefaultCollectionIndex { get; set; }
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		object[] ParentBusinessObjects { get; set; }

		string GetDocumentDataContext(BusinessObject businessObject);
		void ShowPresentationManagerForm(BusinessObject businessObject, Control control);

		/// <summary>
		/// Type used for XML data transfer. Will show separately to ParentTypes.
		/// </summary>
		bool ShowXmlFields { get; set; }

		/// <summary>
		/// Type used for XML data transfer. Will show separately to ParentTypes.
		/// </summary>
		Type XmlType { get; set; }

		/// <summary>
		/// Opening tag. Defaults to "<".
		/// </summary>
		string OpeningMacroTag { get; set; }

		/// <summary>
		/// Closing tag. Defaults to ">".
		/// </summary>
		string ClosingMacroTag { get; set; }

		public IMacroLibrary[] Libraries { get; set; }

		Func<string, string> ErrorMessageExtender { get; set; }
	}
}
