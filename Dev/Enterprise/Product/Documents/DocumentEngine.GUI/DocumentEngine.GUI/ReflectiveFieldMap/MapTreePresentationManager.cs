using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using Enterprise.Core.Forms;
using Enterprise.DocumentEngine.ReflectiveFieldMap;
using Enterprise.DocumentEngine.ValueProviders;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Macro;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.ReflectiveFieldMap
{
	public class MapTreePresentationManager : Disposable, IMapTreePresentationManager
	{
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification= "Follows existing implementation")]
		public Type[] ParentTypes { get; set; }
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Follows existing implementation")]
		public string[] VariableNames { get; set; }
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Follows existing implementation")]
		public Type[] VariableParentTypes { get; set; }
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Follows existing implementation")]
		public object[] ParentBusinessObjects { get; set; }
		public Type XmlType { get; set; }
		public Func<string, string> ErrorMessageExtender { get; set; }

		public event MacroSelectedEventHandler MacroSelected;

		public string GetDocumentDataContext(BusinessObject businessObject)
		{
			if (businessObject is IDocumentSupportable documentSupportable)
			{
				var documentSupporter = documentSupportable.DocumentSupporter;

				var businessObjectType = businessObject is IDocumentSupportableOverrideType documentSupportableOverrideType
					? documentSupportableOverrideType.DocumentSupportableType
					: documentSupportable.GetType();

				var dataContextMapList = new DataContextMapList(documentSupporter);
				var documentDataContext = dataContextMapList
					.Find(t =>
						businessObjectType == t.TopLevelDataSourceType
						|| businessObjectType.IsSubclassOf(t.TopLevelDataSourceType)
					)?.DataContextIdentifier;

				return documentDataContext;
			}

			return string.Empty;
		}

		public void ShowPresentationManagerForm(BusinessObject businessObject, Control control)
		{
			_ = businessObject ?? throw new ArgumentNullException(nameof(businessObject));
			_ = control ?? throw new ArgumentNullException(nameof(control));

			form?.Dispose();

			if (businessObject is IDocumentSupportable documentSupportable)
			{
				var documentSupporter = documentSupportable.DocumentSupporter;
				var documentDataContext = GetDocumentDataContext(businessObject);
				if (!string.IsNullOrEmpty(documentDataContext))
				{
					var wrapper = new DataReflectorValueProviderWrapper(
						Array.ConvertAll(new[] { documentSupportable.GetType() },
							baseType => documentDataContext.IsNullOrEmpty()
								? new DocDataProviderReflector(baseType)
								: new DocDataProviderReflector(new DataContextMapList.MapElement(documentDataContext, baseType))),
						new ValueProviderMap(), DataReflectorValueProviderWrapper.Mode.Browse);

					form = new MapTreeForm(wrapper, documentSupporter, documentDataContext);
					ShowMapTreeForm();
				}
			}
			else
			{
				form = new MapTreeForm(GetWrapper(new[] { businessObject.GetType() }), parentBusinessObjects: new[] { businessObject });
				ShowMapTreeForm();
			}

			if (form != null && !form.IsDisposed)
			{
				var controlBindingMember = FormDebugInfo.GetControlBindingMember(control);
				if (!string.IsNullOrEmpty(controlBindingMember))
				{
					form.FindExactly(controlBindingMember);
				}
			}
		}

		public string GetUserSelectionMacro()
		{
			var wrapper = GetWrapper(ParentTypes);
			using (var form = new MapTreeForm(wrapper))
			{
				ZFormModaliser.ShowDialogAndDispose(form);
				return wrapper.SelectedMacro;
			}
		}

		public void ShowPresentationManagerForm(bool showIndex = true, bool shouldEscapeAllSpecialCharacters = false)
		{
			if (form == null || form.IsDisposed)
			{
				if (ShowXmlFields && XmlType == null)
				{
					XmlType = typeof(UniversalDataBuss.DataObjects.Universal.Shipment);
				}
				if (UseMcrEvaluator)
				{
					if (ParentBusinessObjects?.Length > 0)
					{
						if (ParentBusinessObjects[0] is MacroScope scope)
						{
							if (VariableNames == null || VariableNames.Length == 0)
							{
								initializeBaseVariable(scope);
							}
						}
						else
						{
							throw new ArgumentException("Only MacroScope is accepted as ParentBusinessObjects");
						}
					}
					else
					{
						var scope = new MacroScope();
						ParentBusinessObjects = new object[] { scope };
						initializeBaseVariable(scope);
					}
				}
				form = new MapTreeForm(GetWrapper(ParentTypes, VariableNames, VariableParentTypes, showIndex, XmlType), parentBusinessObjects: ParentBusinessObjects, shouldEscapeAllSpecialCharacters: shouldEscapeAllSpecialCharacters, context: Libraries.CreateContext(), errorMessageExtender: ErrorMessageExtender);

				if (DataFieldsOnly)
				{
					form.macrosTab.TabVisible = false;
				}
				else if (MacroFieldsOnly)
				{
					form.dataPropertiesTab.TabVisible = false;
				}

				if (HideDataFields)
				{
					form.dataPropertiesTab.TabVisible = false;
				}

				if (HideMacroFields)
				{
					form.macrosTab.TabVisible = false;
				}

				form.UseMcrEvaluator = UseMcrEvaluator;
				form.ShowEditor = ShowEditField;
				form.MacroSelected += MacroSelected;
				form.MacroMaxLength = MacroMaxLength;
			}
			form.textBoxMacro.Text = InitialText ?? string.Empty;
			form.textBoxMacro.SelectionStart = InitialText != null ? InitialText.Length : 0;

			ShowMapTreeForm();
		}

		void initializeBaseVariable(MacroScope scope)
		{
			var baseVariables = ObjectFactory.Get<IWorkflowMacroContextDecider>().GetBaseVariables();
			foreach (var kvp in baseVariables)
			{
				scope.SetVariable(kvp.Key, kvp.Value.Object);
			}
			VariableNames = baseVariables.Keys.ToArray();
			VariableParentTypes = baseVariables.Values.Select(v => v.Type).ToArray();
		}

		void ShowMapTreeForm()
		{
			if (form != null && !form.IsDisposed)
			{
				if (ModalParent is ZForm parentForm)
				{
					ZFormModaliser.Show(form, parentForm);
				}
				else
				{
					form.Show();
				}
				form.Focus();
			}
		}

		DataReflectorValueProviderWrapper GetWrapper(Type[] parentTypes)
			=> GetWrapper(parentTypes, variableNames: null, variableParentTypes: null, showIndex: true, xmlType: null);

		DataReflectorValueProviderWrapper GetWrapper(Type[] parentTypes, string[] variableNames, Type[] variableParentTypes, bool showIndex, Type xmlType)
		{
			var mode = UseBrowseMode ? DataReflectorValueProviderWrapper.Mode.Browse : DataReflectorValueProviderWrapper.Mode.Select;
			var docFilter = UseMcrEvaluator ? new MCRDataReflectorFilter() : new DocDataReflectorFilter();
			var macroTagType = new MemberDescription.MacroTagTypes(OpeningMacroTag, ClosingMacroTag);
			var xmlWrapper = xmlType != null
				? new DocDataProviderReflector(xmlType, new UniversalXmlDataReflectorFilter(), macroTagType)
				{
					ShowIndex = showIndex,
					DefaultIndex = DefaultCollectionIndex,
				}
				: null;

			var docDataProviderReflectors = Array.ConvertAll(parentTypes, baseType => new DocDataProviderReflector(baseType, docFilter, macroTagType) { ShowIndex = showIndex, DefaultIndex = DefaultCollectionIndex, });
			var variableReflectors = variableParentTypes != null ? Array.ConvertAll(variableParentTypes, baseType => new DocDataProviderReflector(baseType, docFilter, macroTagType) { ShowIndex = showIndex, DefaultIndex = DefaultCollectionIndex, }) : null;

			return new DataReflectorValueProviderWrapper(docDataProviderReflectors,
				variableNames,
				variableReflectors,
				xmlWrapper,
				this.UseMcrEvaluator ? (new LibraryValueProviderMap { Libraries = this.Libraries.ToImmutableArray() }) : new ValueProviderMap(),
				mode);
		}

		protected override void Dispose(bool isDisposing)
		{
			if (form != null && !form.IsDisposed)
			{
				form.Dispose();
			}
		}

		public bool DataFieldsOnly { get; set; }
		public bool MacroFieldsOnly { get; set; }
		public bool ShowEditField { get; set; }
		public object ModalParent { get; set; }
		public string InitialText { get; set; }
		public int MacroMaxLength { get; set; }
		public bool UseBrowseMode { get; set; }

		public string OpeningMacroTag { get; set; } = ZArchitecture.Business.MacroConstants.DefaultMacroOpeningBracket;
		public string ClosingMacroTag { get; set; } = ZArchitecture.Business.MacroConstants.DefaultMacroClosingBracket;
		public bool HideDataFields { get; set; }
		public bool HideMacroFields { get; set; }
		public bool UseMcrEvaluator { get; set; }
		public bool ShowXmlFields { get; set; }
		public int DefaultCollectionIndex { get; set; } = 1;

		IMacroLibrary[] libraries;
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Follows existing implementation")]
		public IMacroLibrary[] Libraries
		{
			get
			{
				if (libraries == null)
				{
					return SelectDefaultLibraryCollection(XmlType != null);
				}
				return libraries;
			}
			set
			{
				libraries = value;
			}
		}
		public static IMacroLibrary[] SelectDefaultLibraryCollection(bool showXmlField)
		{
			return showXmlField ? new IMacroLibrary[] { new CargoWiseOneStandardLibrary(), new UniversalMacroLibrary() } : new IMacroLibrary[] { new CargoWiseOneStandardLibrary() };
		}
		internal MapTreeForm form;
	}
}
