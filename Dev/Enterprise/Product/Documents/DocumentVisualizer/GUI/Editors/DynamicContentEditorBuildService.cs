using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class DynamicContentEditorBuildService : IEditorBuildService
	{
		public DynamicContentEditorBuildService()
		{
			controlProviders = new Lazy<Dictionary<Type, Func<IMacroBusinessObjectProperty, Control>>>(GetControlProviders);
		}

		readonly Lazy<Dictionary<Type, Func<IMacroBusinessObjectProperty, Control>>> controlProviders;

		public IEditorView Build(IDynamicContentLayoutElement content, IEditorPresenter editorPresenter, IMacroBusinessObjectProperty[] properties, float scale)
		{
			Argument.NotNull(content, nameof(content));
			Argument.NotNull(editorPresenter, nameof(editorPresenter));

			if (properties == null || !properties.Any())
			{
				return null;
			}

			if (properties.Length == 1 && properties[0].PropertyType == typeof(ZBool))
			{
				return new CheckBoxEditorView(content, properties[0]);
			}

			IEditorView result = null;

			var controls = new List<Control>();
			var boundProperties = new List<IMacroBusinessObjectProperty>();

			foreach (var property in properties)
			{
				var control = GetControl(property);

				if (control == null)
				{
					continue;
				}

				boundProperties.Add(property);

				controls.Add(control);
				control.SetBindingMember(property.PropertyName);

				var renderer = control.GetExtension<LabelCaptionRenderer>();
				renderer.Caption = property.DisplayName;
			}

			if (controls.Count == 1)
			{
				result = new InPlaceDynamicEditorView(content, editorPresenter, controls[0], boundProperties[0], scale);
			}
			else
			{
				result = new PopupDynamicEditorView(content, editorPresenter, controls.ToArray(), boundProperties.ToArray(), scale);
			}

			return result;
		}

		#region Implementation

		Dictionary<Type, Func<IMacroBusinessObjectProperty, Control>> GetControlProviders()
		{
			return new Dictionary<Type, Func<IMacroBusinessObjectProperty, Control>>
			{
				{ typeof(ZString), GetTextEditor },
				{ typeof(ZBool), GetCheckBox },
				{ typeof(DateTime), GetDateEditor },
				{ typeof(ZDateTime), GetDateEditor },
				{ typeof(ZDate), GetDateEditor },
				{ typeof(ZByte), property => GetNumberEditor(property, 0) },
				{ typeof(ZShort), property => GetNumberEditor(property, 0) },
				{ typeof(ZInt), property => GetNumberEditor(property, 0) },
				{ typeof(ZLong), property => GetNumberEditor(property, 0) },
				{ typeof(ZDecimal), property => GetNumberEditor(property, 2) },
				{ typeof(ZDateTimeOffset), GetDateOffsetEditor },
			};
		}

		Control GetControl(IMacroBusinessObjectProperty property)
		{
			Control control = null;

			if (property.PropertyType != null && controlProviders.Value.ContainsKey(property.PropertyType))
			{
				var provider = controlProviders.Value[property.PropertyType];
				control = provider(property);
			}

			return control;
		}

		Control GetTextEditor(IMacroBusinessObjectProperty property)
		{
			Control control = null;

			var listProperty = property.MetaData
				.FirstOrDefault(m => m.Id == MetaDataTypes.ListDataSource);

			if (listProperty != null)
			{
				if (listProperty.Value is IFindBoxListProvider findBoxListProvider)
				{
					var customFindBoxMetaData = property.MetaData
						.FirstOrDefault(m => m.Id == CustomFindBoxMetaData.Identifier);

					IFindBoxPopup GetCustomFindBoxPopup()
					{
						if (customFindBoxMetaData is CustomFindBoxMetaData metaData
							&& metaData.CustomFindBoxProvider?.Invoke() is IFindBoxPopup findBoxPopup)
						{
							return findBoxPopup;
						}

						return null;
					}

					var showDescription = true;
					if (customFindBoxMetaData is CustomFindBoxMetaData customMetaData)
					{
						showDescription = customMetaData.ShowDescription;
					}

					var codeBox = new VisualizerCodeFindBox(GetCustomFindBoxPopup)
					{
						ShowDescriptionBox = showDescription,
					};

					codeBox.CodeBox.AcceptsReturn = true;
					codeBox.CodeBox.CharacterCasing = CharacterCasing.Normal;

					var disableModifiableMetaData = property.MetaData.FirstOrDefault(m => m.Id == MetaDataTypes.DisableModifiable);
					if (disableModifiableMetaData != null && (bool)disableModifiableMetaData.Value)
					{
						codeBox.CodeBox.ReadOnly = true;
					}

					control = codeBox;
				}
				else
				{
					var dropEdit = new VisualizerDropEdit();
					dropEdit.CodeBox.AcceptsReturn = true;
					dropEdit.CharacterCasing = CharacterCasing.Normal;

					var list = (listProperty.Value as CodeDescriptionPairList) ??
						(listProperty.Value as ICodeDescriptionPairListProvider)?.CodeDescriptionPairList;

					if (list != null && list.ToArray().All(x => x.Description.IsNullOrEmpty()))
					{
						dropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
					}

					control = dropEdit;
				}
			}

			return control ?? GetPlainTextBox();
		}

		Control GetCheckBox(IMacroBusinessObjectProperty property)
		{
			var control = new ZCheckBox
			{
				AutoSize = true,
				FlatStyle = FlatStyle.System
			};

			return control;
		}

		Control GetNumberEditor(IMacroBusinessObjectProperty property, int defaultNumberOfDecimals)
		{
			var decimalsMetaData = property.MetaData
					.FirstOrDefault(m => m.Id == MetaDataTypes.DecimalPlaces);

			var numberOfDecimals = decimalsMetaData != null
				? (int)decimalsMetaData.Value
				: defaultNumberOfDecimals;

			var control = new ZCalcEdit
			{
				AcceptsReturn = true,
				DecimalPlaces = numberOfDecimals
			};

			return control;
		}

		Control GetPlainTextBox()
		{
			return new VisualizerTextBox
			{
				AcceptsReturn = true,
				CharacterCasing = CharacterCasing.Normal,
				Multiline = true,
				WordWrap = false
			};
		}

		Control GetDateEditor(IMacroBusinessObjectProperty property)
		{
			var dateTimeFormatMetaData = property.MetaData
					.FirstOrDefault(m => m.Id == MetaDataTypes.DateTimeFormat);

			var dateTimeFormat = dateTimeFormatMetaData?.Value is KDateTimeFormat kDateTimeFormat
				? kDateTimeFormat
				: KDateTimeFormat.Short;

			var control = new ZDateEdit();
			control.DateTextBox.AcceptsReturn = true;

			switch (dateTimeFormat)
			{
				case KDateTimeFormat.Long:
					control.DateTimeFormat = ZDateTimePickerFormat.Long;
					break;

				case KDateTimeFormat.Short:
					control.DateTimeFormat = ZDateTimePickerFormat.Short;
					break;

				case KDateTimeFormat.Time:
					control.DateTimeFormat = ZDateTimePickerFormat.Time;
					break;
			}

			return control;
		}

		Control GetDateOffsetEditor(IMacroBusinessObjectProperty property)
		{
			var dateTimeFormatMetaData = property.MetaData
					.FirstOrDefault(m => m.Id == MetaDataTypes.DateTimeFormat);

			var dateTimeFormat = dateTimeFormatMetaData?.Value is KDateTimeFormat kDateTimeFormat
				? kDateTimeFormat
				: KDateTimeFormat.Short;

			var control = new ZDateTimeOffsetEdit();
			control.DateTextBox.AcceptsReturn = true;

			switch (dateTimeFormat)
			{
				case KDateTimeFormat.Long:
					control.DateTimeFormat = ZDateTimePickerFormat.Long;
					break;

				case KDateTimeFormat.Short:
					control.DateTimeFormat = ZDateTimePickerFormat.Short;
					break;

				case KDateTimeFormat.Time:
					control.DateTimeFormat = ZDateTimePickerFormat.Time;
					break;
			}

			return control;
		}

		#endregion
	}
}
