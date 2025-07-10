using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Integration.MasterFiles;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	public enum MacroType
	{
		DocEngine = 0,
		Antlr
	}

	[Testing.SuppressCheckControlLookupList]
	[Testing.SuppressCheckControlModuleId]
	public class ZMacrosFindBoxColumnStyleInfo : ZCodeFindBoxColumnStyleInfo
	{
		public ZMacrosFindBoxColumnStyleInfo()
		{
			CharacterCasing = CharacterCasing.Normal;
		}
		public bool ShowIndex { get; set; } = true;

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(ZMacrosFindBoxColumnStyle); }
		}

		[DefaultValue(CharacterCasing.Normal)]
		public override CharacterCasing CharacterCasing
		{
			get { return base.CharacterCasing; }
			set { base.CharacterCasing = value; }
		}

		[DefaultValue(false)]
		public bool DataFieldsOnly { get; set; }

		[DefaultValue(MacroType.DocEngine)]
		public MacroType MacroType { get; set; }

		[DefaultValue(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool UsePredefinedRoots { get; set; }
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Type[] RootTypes { get; set; }
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public BusinessObject[] Roots { get; set; }

		[DefaultValue(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsUsedForExpressions { get; set; }

		[DefaultValue(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool UseFieldChangeMacroEvaluatorForPreview { get; set; }

		[DefaultValue(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IncludeParentJobInRoots { get; set; } = true;

		[DefaultValue(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ShouldEscapeAllSpecialCharacters { get; set; }
	}

	public class ZMacrosFindBoxColumnStyle : ZCodeFindBoxColumnStyle
	{
		public ZMacrosFindBoxColumnStyle(ZMacrosFindBoxColumnStyleInfo columnInfo) : this(() =>
			new ZMacrosFindBox(columnInfo.MacroType)
			{
				UseFieldChangeMacroEvaluatorForPreview = columnInfo.UseFieldChangeMacroEvaluatorForPreview,
				ShouldEscapeAllSpecialCharacters = columnInfo.ShouldEscapeAllSpecialCharacters,
				XmlType = columnInfo.XmlType,
				HideDataFields = columnInfo.HideDataFields,
				HideMacroFields = columnInfo.HideMacroFields,
				UseMcrEvaluator = columnInfo.MacroType == MacroType.Antlr || columnInfo.UseMcrEvaluator,
				DefaultCollectionIndex = columnInfo.MacroType == MacroType.Antlr ? 0 : columnInfo.DefaultCollectionIndex,
				OpeningMacroTag = columnInfo.MacroType == MacroType.Antlr ? string.Empty : columnInfo.MacroOpeningBracket,
				ClosingMacroTag = columnInfo.MacroType == MacroType.Antlr ? string.Empty : columnInfo.MacroClosingBracket,
			}, columnInfo)
		{ }

		protected ZMacrosFindBoxColumnStyle(Func<ZMacrosFindBox> gridFindBox, ZMacrosFindBoxColumnStyleInfo columnInfo) : base(gridFindBox, columnInfo) { }

		protected new ZMacrosFindBoxColumnStyleInfo ColumnInfo
		{
			get { return (ZMacrosFindBoxColumnStyleInfo)base.ColumnInfo; }
		}

		protected new ZMacrosFindBox FindBox
		{
			get
			{
				var box = (ZMacrosFindBox)base.FindBox;
				box.ShowIndex = this.ColumnInfo.ShowIndex;
				return box;
			}
		}

		protected override void Edit(CurrencyManager source, int rowNum, System.Drawing.Rectangle bounds, bool readOnly, string instantText, bool cellVisible)
		{
			if (!IsEditing)
			{
				if (ColumnInfo.UsePredefinedRoots)
				{
					FindBox.RootTypes = ColumnInfo.RootTypes;
					FindBox.Roots = ColumnInfo.Roots;
				}
				else
				{
					object parentJob = null;

					if (ColumnInfo.IncludeParentJobInRoots && GetTopLevelForm() is ZForm form)
					{
						parentJob = form.DataSource;
					}

					FindBox.PrepareControl(source.List[rowNum], parentJob, PropertyDescriptor);
				}

				FindBox.AllowMultipleMacroses = ColumnInfo.AllowMultipleMacroses;
				FindBox.DataFieldsOnly = ColumnInfo.DataFieldsOnly;
				FindBox.IsUsedForExpressions = ColumnInfo.IsUsedForExpressions;
				FindBox.PrepareContextMenu(source, MappingName);
			}

			base.Edit(source, rowNum, bounds, readOnly, instantText, cellVisible);
		}
	}

	public class ZMacrosFindBox : ZGridFindBox, IMacroBox
	{
		public ZMacrosFindBox()
		{
		}

		public bool ShowIndex { get; set; } = true;

		public ZMacrosFindBox(MacroType macroType)
		: this()
		{
			this.macroType = macroType;

			if (macroType == MacroType.Antlr)
			{
				UseMcrEvaluator = true;
				OpeningMacroTag = ClosingMacroTag = string.Empty;
				DefaultCollectionIndex = 0;
			}
		}

		readonly MacroType macroType;
		public bool AllowMultipleMacroses { get; set; }
		public bool UseFieldChangeMacroEvaluatorForPreview { get; set; }
		public bool ShouldEscapeAllSpecialCharacters { get; set; }
		public Type[] RootTypes { get; set; }
		public BusinessObject[] Roots { get; set; }
		public BusinessObject Current { get; set; }
		public PropertyDescriptor PropertyDescriptor { get; set; }
		public bool HideDataFields { get; set; }
		public bool HideMacroFields { get; set; }
		public bool ShowXmlFields { get; set; }
		public bool UseMcrEvaluator { get; set; }
		public int DefaultCollectionIndex { get; set; } = 1;
		public string OpeningMacroTag { get; set; } = "<";
		public string ClosingMacroTag { get; set; } = ">";
		public Type XmlType { get; set; }
		public IMacroLibrary[] Libraries { get; set; }
		public object Context { get; set; }
		public bool IsUsedForExpressions
		{
			get { return ((IFindBoxUserControl)this).AutoCompleteDisabled; }
			set { ((IFindBoxUserControl)this).AutoCompleteDisabled = value; }
		}
		
		internal void PrepareControl(object current, object parentJob, PropertyDescriptor descriptor)
		{
			var bizo = Current = current as BusinessObject;
			PropertyDescriptor = descriptor;

			var types = new List<Type>();
			var values = new List<BusinessObject>();

			var (attributeObj, rootTypeAttribute) = GetRootTypeAttribute(current, descriptor);
			if (rootTypeAttribute != null)
			{
				var attType = attributeObj.GetType();
				var getTypesMethod = attType.GetMethod(rootTypeAttribute.TypeProviderFunctionName);
				var getRootsMethod = attType.GetMethod(rootTypeAttribute.RootProviderFunctionName);

				types.AddRange((Type[])getTypesMethod.Invoke(attributeObj, Array.Empty<object>()));
				values.AddRange((BusinessObject[])getRootsMethod.Invoke(attributeObj, Array.Empty<object>()));
			}
			else
			{
				if (current is IRootTypeProvider rootTypeProvider)
				{
					types = rootTypeProvider.RootTypes.ToList();

					if (rootTypeProvider.Roots != null)
					{
						values = rootTypeProvider.Roots.ToList();
					}
				}
				else
				{
					if (bizo != null)
					{
						types.Add(bizo.GetType());
						values.Add(bizo);
					}
				}

				var parentBusinessObject = parentJob as BusinessObject;
				if (parentBusinessObject != null)
				{
					var parentType = parentBusinessObject.GetType();
					if (!types.Contains(parentType))
					{
						types.Add(parentType);
						values.Add(parentBusinessObject);
					}
				}
			}

			RootTypes = types.ToArray();
			Roots = values.ToArray();
		}

		static (object objWithAttribute, RootTypeProviderAttribute attribute) GetRootTypeAttribute(object current, PropertyDescriptor descriptor)
		{
			var currentType = current.GetType();
			var subProperties = descriptor.Name.Split('+');
			for (int i = 0; i < subProperties.Length - 1; i++)
			{
				var property = currentType.GetProperty(subProperties[i]);
				if (property == null)
				{
					return default;
				}
				current = property.GetValue(current);
				if (current == null)
				{
					return default;
				}
				currentType = current.GetType();
			}

			var rootTypeAttribute = currentType.GetProperty(subProperties[subProperties.Length - 1])?
					.GetCustomAttributes(typeof(RootTypeProviderAttribute), true)
					.Cast<RootTypeProviderAttribute>()
					.FirstOrDefault();

			return (current, rootTypeAttribute);
		}

		internal void PrepareContextMenu(CurrencyManager source, string mappingName)
		{
			if (CodeBox.contextMenuManager != null)
			{
				CodeBox.contextMenuManager.MacroBox = this;
				CodeBox.contextMenuManager.TextTemplatesFactory.Bind(source, mappingName);
			}
		}

		#region IMacroBox

		bool IMacroBox.IsMacroControl => true;

		ResourceStringData IMacroBox.InsertMacroCaption => DataFieldsOnly ? Res.GetData("a0e8ef62-8c28-4401-9694-bb9a2381a59e", "Select Field") : null;

		string IMacroBox.MacroOpeningBracket => OpeningMacroTag;

		string IMacroBox.MacroClosingBracket => ClosingMacroTag;

		TextBox IMacroBox.TextBox => CodeBox;

		bool IMacroBox.AllowMultipleMacros => true;

		public bool DataFieldsOnly { get; set; }

		public bool MacroFieldsOnly { get; set; }

		object IMacroBox.DataSource => DataSource;

		EventHandler IMacroBox.InsertMacroHandler => InsertMacro;

		EventHandler IMacroBox.PreviewMacroHandler => Roots != null && Roots.Length > 0 ? PreviewMacro : null;

		IMapTreePresentationManager IMacroBox.MacroManager { get; set; }

		#endregion

		void InsertMacro(object sender, EventArgs e)
		{
			SelectFromPopupForm();
		}

		void SetupMapTreePresenter()
		{
			if (mapTreePresenter == null)
			{
				mapTreePresenter = ObjectFactory.Get<IMapTreePresentationManager>();
				mapTreePresenter.DataFieldsOnly = DataFieldsOnly;
				mapTreePresenter.ShowEditField = AllowMultipleMacroses;
				mapTreePresenter.ShowXmlFields = ShowXmlFields;
				mapTreePresenter.ModalParent = FindForm();
				mapTreePresenter.MacroSelected += MacroSelected;
				mapTreePresenter.MacroMaxLength = CodeBox.MaxLength;
				mapTreePresenter.HideDataFields = HideDataFields;
				mapTreePresenter.HideMacroFields = HideMacroFields;
				mapTreePresenter.DefaultCollectionIndex = DefaultCollectionIndex;
				mapTreePresenter.UseMcrEvaluator = UseMcrEvaluator;
				mapTreePresenter.OpeningMacroTag = OpeningMacroTag;
				mapTreePresenter.ClosingMacroTag = ClosingMacroTag;
			}

			mapTreePresenter.XmlType = XmlType;
			mapTreePresenter.InitialText = CodeBox.Text;
		}

		public override void SelectFromPopupForm(bool autoSelect = false)
		{
			if (macroType == MacroType.Antlr)
			{
				var contextProvider = GetContextProvider();
				if (contextProvider != null)
				{
					var context = contextProvider.GetSampleContext();
					var variables = context?.Variables;
					SetupMapTreePresenter();
					mapTreePresenter.Libraries = context?.Libraries;
					mapTreePresenter.ParentTypes = new Type[] { context?.ParentType };
					mapTreePresenter.ParentBusinessObjects = new Object[] { context?.Scope };
					mapTreePresenter.VariableParentTypes = variables?.Select(variable => variable.Value.Item2).ToArray();
					mapTreePresenter.VariableNames = variables?.Keys?.ToArray();
					mapTreePresenter.ErrorMessageExtender = context.ErrorMessageExtender;
					mapTreePresenter.ShowPresentationManagerForm(ShowIndex, ShouldEscapeAllSpecialCharacters);
				}
				else
				{
					antlrMacroForm = ObjectFactory.Get<IMacroEvaluationTool>().Run(ParentForm);
				}
			}
			else if (macroType == MacroType.DocEngine)
			{
				if (RootTypes != null && RootTypes.Length > 0)
				{
					SetupMapTreePresenter();
					mapTreePresenter.ParentTypes = DataFieldsOnly ? RootTypesHelper.GetDataFieldsOnlyRootTypes(RootTypes) : RootTypes;
					mapTreePresenter.ParentBusinessObjects = Roots;
					mapTreePresenter.ShowPresentationManagerForm(ShowIndex, ShouldEscapeAllSpecialCharacters);
				}
			}

			IAntlrMacroContextProvider GetContextProvider()
			{
				var currentDescriptor = PropertyDescriptor;

				while (currentDescriptor != null)
				{
					if (typeof(IAntlrMacroContextProvider).IsAssignableFrom(currentDescriptor.PropertyType))
					{
						return (IAntlrMacroContextProvider)currentDescriptor.GetValue(Current);
					}

					currentDescriptor  = currentDescriptor is WrappingPropertyDescriptor wrappingPropertyDescriptor ? wrappingPropertyDescriptor.Outer : null;
				}

				return Current as IAntlrMacroContextProvider;
			}
		}

		void PreviewMacro(object sender, EventArgs e)
		{
			if (Roots != null && Roots.Length > 0)
			{
				if (UseFieldChangeMacroEvaluatorForPreview)
				{
					ZFormModaliser.Show(new TextTemplatePreviewForm { PreviewText = ObjectFactory.Get<IWorkflowMacroEvaluator>().EvaluateMacros(Roots, CodeBox.Text).result?.ToString() }, FindForm());
				}
				else
				{
					ZFormModaliser.Show(new TextTemplatePreviewForm { PreviewText = ObjectFactory.Get<ITextMacroProcessor>().Replace(CodeBox.Text, Roots, shouldEscapeAllSpecialCharacters: ShouldEscapeAllSpecialCharacters) }, FindForm());
				}
			}
		}

		internal Form antlrMacroForm;
		internal IMapTreePresentationManager mapTreePresenter;

#if DEBUG
		internal
#endif
		void MacroSelected(string macro)
		{
			if (macro.Length > CodeBox.MaxLength)
			{
				Globals.Message.Show(Res.GetString("9D5329B8-B47E-472C-AFA0-3DCB09BBBEE1", "Macro length cannot be greater than {0} symbols, the excess is removed.", CodeBox.MaxLength));
				CodeBox.Text = macro.Substring(0, CodeBox.MaxLength);
			}
			else
			{
				CodeBox.Text = macro;
			}
		}

		protected override System.Collections.IList GetList(object dataSource, string listMember, string dataMemberForErrorReporting)
		{
			return null;
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			base.Dispose(isNotFinalizing);
			if (mapTreePresenter != null)
			{
				mapTreePresenter.Dispose();
				mapTreePresenter = null;
			}
		}

		void IMacroBox.StartedEditing(TextBox t) => throw new NotImplementedException();
	}
}
