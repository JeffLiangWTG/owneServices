using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common.Collections;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Core.Forms
{
	#region SuppressResourceStringsCheckRegion

	/// <summary>
	/// Provides debugging information about a form at runtime.
	/// </summary>
	public static class FormDebugInfo
	{
		const string IgnoredBindingSeparator = "愛";

		#region CTRL+SHIFT+D/Developer Information Form

		public static string GetActiveControlInfoFromForm(Form form)
		{
			var builder = new ZStringBuilder();
			if (form != null)
			{
				builder.AppendLine(GetFormInfo(form));
				builder.Append(GetControlInfoForControlAndActiveControl(form.ActiveControl));
			}
			builder.AppendLine();
			builder.Append(GetBindingSourcesInfo(form?.GetFrontMostActiveControl()));
			return builder.ToString();
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		internal static string GetFormInfo(Form form)
		{
			var builder = new ZStringBuilder();
			if (form != null)
			{
				builder.AppendLine("Form information");
				builder.AppendLine($"Name: {form.Name}");
				builder.AppendLine($"Type: {form.GetType()}");
			}

			return builder.ToString();
		}

		static string GetControlInfoForControlAndActiveControl(Control control)
		{
			var builder = new ZStringBuilder();
			if (control != null)
			{
				builder.AppendLine(GetControlInfo(control));
				var childInfo = (control is IContainerControl containerControl) ? GetControlInfoForControlAndActiveControl(containerControl.ActiveControl) : string.Empty;
				builder.Append(childInfo);
			}
			return builder.ToString();
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		internal static string GetControlInfo(Control control)
		{
			var builder = new ZStringBuilder();
			builder.AppendLine("Control information");
			if (control != null)
			{
				builder.AppendLine($"Name: {control.Name}");
				builder.AppendLine($"Type: {control.GetType()}");

				var readOnlyProperty = BindableComponentMetaDataPropertyLocator.GetDefaultMetaDataProperty(control.GetType(), MetaDataTypes.ReadOnly);
				if (readOnlyProperty != null)
				{
					builder.AppendLine($"ReadOnly: {readOnlyProperty.GetValue(control)}");
				}

				builder.AppendLine($"TabStop: {control.TabStop}, TabIndex: {control.TabIndex}");
				builder.AppendLine();
				builder.Append(GetBindingInfo(control));
			}
			else
			{
				builder.AppendLine("No active control.");
			}

			return builder.ToString();
		}

		internal static string GetBindingInfo(Control control)
		{
			var builder = new ZStringBuilder();
			if (control != null)
			{
				builder.AppendLine($"Binding information for {control.Name}");
				string bindingInfo;
				if (control.Parent is ZGrid grid)
				{
					bindingInfo = GetGridBindingInfo(grid, control);
				}
				else
				{
					bindingInfo = GetControlBindingsInfo(control);
				}
				builder.Append(string.IsNullOrEmpty(bindingInfo) ? "(none)\r\n" : bindingInfo);
			}

			return builder.ToString();
		}

		static string GetGridBindingInfo(ZGrid grid, Control editControl)
		{
			var builder = new ZStringBuilder();
			foreach (var column in grid.Columns)
			{
				if (!(column.ColumnStyle is ZTextBoxColumnStyle columnStyle) || columnStyle.MappingName.Contains(IgnoredBindingSeparator) || columnStyle.EditControl != editControl)
				{
					continue;
				}

				builder.AppendLine($"BindingMember: {columnStyle.MappingName}");
				var propertySegments = columnStyle.MappingName.Split('.', '+');
				builder.Append(TableForBindingMember(propertySegments));
				break;
			}

			return builder.ToString();
		}

		static string TableForBindingMember(string[] propertySegments)
		{
			var builder = new ZStringBuilder();
			var property = propertySegments[propertySegments.Length - 1];
			var tablePrefix = Schema.GetPrefixFromColumnName(property);
			if (!string.IsNullOrWhiteSpace(tablePrefix))
			{
				var schema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchemaFromColumnNamePrefix(tablePrefix);
				builder.AppendLine(schema?.GetSchemaColumn(property) != null ? $"Table: {schema.TableName}" : "Table: [calculated property]");
			}
			else
			{
				builder.AppendLine("Table: [calculated property]");
			}

			builder.AppendLine();

			return builder.ToString();
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		static string GetControlBindingsInfo(Control control)
		{
			var builder = new ZStringBuilder();
			var dataBindings = control.DataBindings
				.Cast<Binding>()
				.Where(b => !b.BindingMemberInfo.BindingMember.Contains(IgnoredBindingSeparator));
			var configurator = control.GetParent<ZForm>() as IDevToolMessageBuilderMappingPathConfigurator;
			var addMappingPath = configurator?.CanDisplayMessageBuilderMappingPath ?? false;

			foreach (var binding in dataBindings)
			{
				builder.AppendLine($"DataSource type: {binding.DataSource.GetType()}");
				builder.AppendLine($"BindingMember: {binding.BindingMemberInfo.BindingMember}");
				var propertySegments = binding.BindingMemberInfo.BindingMember.Split('.', '+');
				builder.Append(TableForBindingMember(propertySegments));
				var bindingMemberObject = GetBindingMemberObject(binding.DataSource, propertySegments);
				if (bindingMemberObject != null)
				{
					builder.AppendLine($"Property Type: {bindingMemberObject.GetType()}");
					if (addMappingPath)
					{
						AppendMappingPath(builder, binding);
					}
					var macroExample = GetMacroExample(string.Join(".", propertySegments), bindingMemberObject);
					if (macroExample != null)
					{
						builder.Append("Macro example: ");
						builder.Append(macroExample);
					}
				}
			}

			return builder.ToString();
		}

		static void AppendMappingPath(ZStringBuilder builder, Binding binding)
		{
			builder.AppendLine("Mapping Path(s):")
				.AppendLine(ConstructMappingPath(binding));
		}

		static string ConstructMappingPath(Binding binding)
		{
			var props = GetBindingProps(binding);
			return string.Join("/", props);
		}

		static IEnumerable<string> GetBindingProps(Binding binding)
		{
			if (binding != null)
			{
				yield return binding.BindingMemberInfo.BindingMember;
			}
		}

		static string GetMacroExample(string propertyPath, object obj)
		{
			if (obj == null)
			{
				return null;
			}

			do
			{
				var macroExampleTemplate = GetFormat(ref obj);
				if (macroExampleTemplate != null)
				{
					return string.Format(CultureInfo.InvariantCulture, macroExampleTemplate, propertyPath, obj);
				}

				var underlyingObject = GetUnderlyingObject(obj);
				obj = underlyingObject == obj ? null : underlyingObject;
			} while (obj != null);

			return null;
		}

		static object GetUnderlyingObject(object obj)
		{
			if (!(obj is IEnumerable enumberableObject))
			{
				return null;
			}

			var enumerator = enumberableObject.GetEnumerator();
			if (!enumerator.MoveNext())
			{
				return null;
			}

			var underlyingObject = enumerator.Current;
			if (enumerator is IDisposable disposable)
			{
				disposable.Dispose();
			}

			return underlyingObject;
		}

		internal static string GetFormat(ref object obj)
		{
			var type = obj.GetType();

			if (type == typeof(ZInt) || type == typeof(ZByte) || type == typeof(ZDecimal))
			{
				return "{0}>={1}";
			}

			if (type == typeof(ZString))
			{
				return "{0}==\"{1}\"";
			}

			if (type == typeof(ZGuid))
			{
				return "\"<{0}>\"==\"{1}\"";
			}

			if (type == typeof(ZBool))
			{
				obj = (ZBool)obj ? null : "!";
				return "{1}{0}";
			}

			if (type == typeof(ZDate) || type == typeof(ZDateTime) || type == typeof(ZDateTimeOffset))
			{
				var isValid = type == typeof(ZDateTimeOffset) ? ((ZDateTimeOffset)obj).IsValid : type == typeof(ZDateTime) ? ((ZDateTime)obj).IsValid : ((ZDate)obj).IsValid;
				if (!isValid)
				{
					obj = ZDateTime.Now;
				}

				return "{0}.Year>={1:yyyy} && {0}.Month>={1:MM} && {0}.Day>={1:dd}";
			}

			return null;
		}

		static object GetBindingMemberObject(object obj, string[] propertySegments, int currentSegmentIndex = 0)
		{
			var currentSegment = propertySegments.ElementAtOrDefault(currentSegmentIndex);
			if (currentSegment == null)
			{
				return obj;
			}

			var collectionDepth = 0;
			var propertyValue = GetValueForPropertyIfExists(obj, currentSegment, ref collectionDepth);
			if (propertyValue == null)
			{
				return null;
			}

			if (collectionDepth > 0 && currentSegmentIndex > 0)
			{
				propertySegments[currentSegmentIndex - 1] += string.Concat(Enumerable.Repeat("[0]", collectionDepth));
			}

			return GetBindingMemberObject(propertyValue, propertySegments, currentSegmentIndex + 1);
		}

		static object GetValueForPropertyIfExists(object obj, string propertyName, ref int collectionDepth)
		{
			if (obj == null)
			{
				return null;
			}

			var type = obj.GetType();

			// object can contain multiple properties with the same name
			// for instance when the base class has the property and then it is overriden with a 'new' keyword in the derived class
			var propertyInfos = type.GetProperties().Where(property => property.Name == propertyName).ToArray();

			if (propertyInfos.Any())
			{
				var mostSuitablePropertyInfo = propertyInfos.Length > 1 ? GetMostSuitablePropertyInfo(propertyInfos, type) : propertyInfos.Single();
				return mostSuitablePropertyInfo != null ? mostSuitablePropertyInfo.GetValue(obj) : null;
			}

			var underlyingObject = GetUnderlyingObject(obj);

			if (underlyingObject != null && underlyingObject != obj)
			{
				collectionDepth++;
				var objectsInUnderlyingType = GetValueForPropertyIfExists(underlyingObject, propertyName, ref collectionDepth);
				if (objectsInUnderlyingType != null)
				{
					return objectsInUnderlyingType;
				}
			}

			return null;
		}

		static PropertyInfo GetMostSuitablePropertyInfo(PropertyInfo[] propertyInfos, Type type)
		{
			while (type != typeof(object))
			{
				foreach (var propertyInfo in propertyInfos)
				{
					if (propertyInfo.DeclaringType == type)
					{
						return propertyInfo;
					}
				}

				type = type.BaseType;
			}

			return null;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		static string GetBindingSourcesInfo(Control control)
		{
			var builder = new ZStringBuilder();
			builder.AppendLine("BindingSource information");
			var current = control?.Parent;
			while (current != null)
			{
				if (current is ICompositeControlBindingSourceProvider bindingSource && bindingSource.BindingSource != null)
				{
					builder.AppendLine($"{current.Name}: {current.GetType().FullName}");
					builder.AppendLine($"\tDataSource type: {(bindingSource.BindingSource.DataSource == null ? "null" : bindingSource.BindingSource.DataSource.GetType().FullName)}");
					builder.AppendLine($"\tDataMember: {((KBindingSource)bindingSource.BindingSource).DataMember}");
					builder.AppendLine($"\tNumber of bound controls: {((KBindingSource)bindingSource.BindingSource).FullBindingMembers.Count()}");
				}
				current = current.Parent;
			}

			return builder.ToString();
		}

		#endregion

		#region CTRL+SHIFT+R (Usually used for developing DocEngine Template)

		#region GetControlInfoFromParentToChild

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		public static (string ControlInfo, string ListContent, Control TargetControl) GetActiveControlInfoFromFormForDocEngine(Form form)
		{
			var builder = new ZStringBuilder();
			var controlPathBuilder = new ZStringBuilder();
			var targetControl = form.GetFrontMostActiveControl();
			var listContent = string.Empty;
			if (form != null)
			{
				string bindingInfo;
				targetControl = GenerateControlPathAndReturnMostRelevantControlForDocEngine(form, targetControl, controlPathBuilder);

				builder.Append(GetControlInfoForControlAndActiveControlForDocEngine(form.ActiveControl));
				builder.AppendLine("   in " + GetFormInfoForDocEngine(form));
				builder.Prepend(controlPathBuilder + System.Environment.NewLine + System.Environment.NewLine);
				(bindingInfo, listContent) = GetBindingInfoForDocEngine(targetControl);
				builder.Append(bindingInfo);
			}
			return (builder.ToString(), listContent, targetControl);
		}

		static Control GenerateControlPathAndReturnMostRelevantControlForDocEngine(Form form, Control control, ZStringBuilder controlPath)
		{
			var result = control;
			var current = control;
			while (current != null)
			{
				if (current.Parent is ZGrid parentGrid)
				{
					result = current;
					var columnStyle = parentGrid.Columns.FirstOrDefault(c => c.ColumnStyle is ZTextBoxColumnStyle zTextBoxColumnStyle && zTextBoxColumnStyle.EditControl == current)?.ColumnStyle;
					if (!string.IsNullOrEmpty(columnStyle?.HeaderText))
					{
						controlPath.Prepend(FormattableString.Invariant($"> {columnStyle.HeaderText} "));
					}
				}
				else if (current is IResCaptionedControl resCaptionedControl && !string.IsNullOrEmpty(resCaptionedControl.CaptionResourceString.Caption))
				{
					controlPath.Prepend(FormattableString.Invariant($"> {resCaptionedControl.CaptionResourceString.Caption} "));
				}
				else
				{
					var caption = current.GetExtension<ILabelCaptionRenderer>()?.Caption;
					if (!string.IsNullOrEmpty(caption))
					{
						controlPath.Prepend(FormattableString.Invariant($"> {caption} "));
					}
				}

				current = current.Parent;
			}
			controlPath.Prepend(FormattableString.Invariant($"{form.Name} "));

			return result;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		static string GetControlInfoForControlAndActiveControlForDocEngine(Control control)
		{
			var builder = new ZStringBuilder();
			if (control != null)
			{
				var childInfo = (control is IContainerControl containerControl) ? GetControlInfoForControlAndActiveControlForDocEngine(containerControl.ActiveControl) : string.Empty;
				builder.Append(childInfo);
				builder.AppendLine(!string.IsNullOrEmpty(childInfo) ? "   in " + GetControlInfoForDocEngine(control) : GetControlInfoForDocEngine(control));
			}
			return builder.ToString();
		}

		#endregion

		#region GetControlInfoFromChildToParent

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		public static (string ControlInfo, string ListContent) GetActiveControlInfoFromControlForDocEngine(Control control)
		{
			var builder = new ZStringBuilder();
			var controlPathBuilder = new ZStringBuilder();

			var controlAndParentControlInfo = GetControlInfoForControlAndParentControlForDocEngine(control, controlPathBuilder);
			builder.Append(controlAndParentControlInfo);

			var form = control.FindForm();
			if (form != null)
			{
				controlPathBuilder.Prepend(form.Name);
				builder.Append("   in " + GetFormInfoForDocEngine(form));
				builder.Prepend(controlPathBuilder + System.Environment.NewLine);
			}
			string bindingInfo;
			string listContent;

			(bindingInfo, listContent) = GetBindingInfoForDocEngine(control);
			builder.Append(bindingInfo);
			return (builder.ToStringWithNewLineBetweenAppends(), listContent);
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		static string GetControlInfoForControlAndParentControlForDocEngine(Control control, ZStringBuilder controlPathBuilder)
		{
			var builder = new ZStringBuilder();

			if (control != null)
			{
				var info = GetControlInfoForDocEngine(control);
				builder.Append(info);
				controlPathBuilder.Prepend($" > {control.Name}");

				var containerControl = control.Parent?.GetContainerControl();
				var parentInfo = containerControl != null && containerControl != control.FindForm() ? GetControlInfoForControlAndParentControlForDocEngine((Control)containerControl, controlPathBuilder) : string.Empty;

				if (!string.IsNullOrEmpty(parentInfo))
				{
					builder.Append("   in " + parentInfo);
				}
			}

			return builder.ToStringWithNewLineBetweenAppends();
		}

		#endregion

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
#if DEBUG
		internal
#endif
		static string GetFormInfoForDocEngine(Form form)
		{
			var builder = new ZStringBuilder();
			if (form != null)
			{
				builder.AppendLine($"Form: {form.Name} ({form.GetType()})");
			}

			return builder.ToString();
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
#if DEBUG
		internal
#endif
		static string GetControlInfoForDocEngine(Control control)
		{
			var builder = new ZStringBuilder();
			if (control != null)
			{
				builder.Append($"Control: {control.Name} ({control.GetType()})");
			}
			else
			{
				builder.AppendLine("No active control.");
			}

			return builder.ToString();
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
#if DEBUG
		internal
#endif
		static (string, string) GetBindingInfoForDocEngine(Control control)
		{
			var builder = new ZStringBuilder();
			var listContent = string.Empty;
			if (control != null)
			{
				string bindingInfo;
				(bindingInfo, listContent) = GetControlBindingInfoForDocEngine(control);

				builder.Append(string.IsNullOrEmpty(bindingInfo) ? "(none)\r\n" : bindingInfo);
			}

			return (builder.ToString(), listContent);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		static (string, string) GetControlBindingInfoForDocEngine(Control control)
		{
			var builder = new ZStringBuilder();
			var builderForListContent = new ZStringBuilder();
			var bindingWithType = GetControlBindingForDocEngine(control);

			if (bindingWithType.Binding != null)
			{
				var binding = bindingWithType.Binding;
				builder.AppendLine($"DataSource Type: {binding.DataSource.GetType()}");
				builder.AppendLine($"Binding Member: {binding.BindingMemberInfo.BindingMember}{(string.IsNullOrEmpty(bindingWithType.BindingType) ? "" : $" ({bindingWithType.BindingType})")}");

				var listControl = control;
				if (listControl is ZDropCodeBox dropCodeBox)
				{
					listControl = dropCodeBox.Parent;
				}

				if (listControl is ZListUserControl listUserControl)
				{
					using (listUserControl.PreventErrorReportForMissingListAttribute())
					{
						if (listUserControl.List is IList listUserControlList)
						{
							var listType = listUserControlList.GetType().FullName;
							if (listUserControlList.Count > 0)
							{
								builder.AppendLine($"List Type: {listType}");
							}
							AddAdditionalInformationIfNeeded(listUserControlList, builder);
							foreach (ICodeDescription item in listUserControlList)
							{
								builderForListContent.AppendLine($"{item.Code}	{item.Description}");
							}
						}
					}
				}
				else
				{
					AddAdditionalInformationIfNeeded(listControl, builder);
				}

				var propertySegments = binding.BindingMemberInfo.BindingMember.Split('.', '+');

				builder.AppendLine();
				builder.Append(TableForBindingMemberForDocEngine(propertySegments));

				var documentDataContext = GetDocumentDataContext(control);
				if (!string.IsNullOrEmpty(documentDataContext))
				{
					builder.Append($"DataContext: {documentDataContext}");
					builder.AppendLine();
				}

				if (binding.BindingManagerBase is CurrencyManager currencyManager &&
					currencyManager.List is IList currencyManagerList && !(currencyManagerList is BusinessObject))
				{
					if (currencyManager.GetCurrent() is object current && currencyManagerList.Contains(current))
					{
						var position = currencyManager.Position;
						if (propertySegments.Contains(binding.BindingMemberInfo.BindingPath))
						{
							var index = Array.IndexOf(propertySegments, binding.BindingMemberInfo.BindingPath);
							var bindingPath = propertySegments[index];
							propertySegments[index] = bindingPath + $"[{position + 1}]";
						}
					}
				}

				var macro = FormattableString.Invariant($"{string.Join(".", propertySegments)}");
				var isMacroValid = false;
				var businessObjectReflector = ObjectFactory.Get<IBusinessObjectReflector>();
				if (businessObjectReflector != null)
				{
					isMacroValid = businessObjectReflector.IsPropertyAccessible(binding.DataSource.GetType(), macro);
				}
				if (isMacroValid)
				{
					builder.Append("Macro: ");
					builder.AppendLine(FormattableString.Invariant($"<{macro}>"));
				}

				var parentForm = control.GetParent<ZForm>() as IDevToolMessageBuilderMappingPathConfigurator;
				if (parentForm?.CanDisplayMessageBuilderMappingPath ?? false)
				{
					const string XPathSeparator = "/";
					var mappingPaths = GetMappingXPaths(binding).Select(v => XPathSeparator + v);
					builder.AppendLine()
						.Append("Mapping Path(s):")
						.AppendLine()
						.Append(string.Join("\r\n", mappingPaths));
				}
			}

			return (builder.ToString(), builderForListContent.ToString());
		}

		static IEnumerable<string> GetMappingXPaths(Binding binding)
		{
			var dataSourceType = binding.DataSource?.GetType();
			var bindingMemberInfo = binding.BindingMemberInfo;
			var bindingPath = bindingMemberInfo.BindingPath;
			var bindingField = bindingMemberInfo.BindingField.Replace("+", "/");
			if (string.IsNullOrEmpty(bindingPath))
			{
				yield return bindingField;
				yield break;
			}

			var bindingPathParts = bindingMemberInfo.BindingPath.Split('.');
			if (dataSourceType is null)
			{
				yield break;
			}

			var numberOfParts = bindingPathParts.Length;
			Type partDataSource = dataSourceType;
			for (int partCounter = 0; partCounter < numberOfParts; partCounter++)
			{
				var part = bindingPathParts[partCounter];
				var dataSourceInfo = GetDataSourceInfo(part, partDataSource);
				partDataSource = dataSourceInfo.DataSourceType;
				if (partDataSource is null || !dataSourceInfo.IsCollection)
				{
					yield return part;
					continue;
				}

				yield return CreateNormalCollectionPart(part);
				yield return CreateCollectionFilterPart(part);

				if (partCounter == numberOfParts - 1)
				{
					yield return CreateCollectionWithBindingFieldPart(part);
				}
			}

			yield return bindingField;
			string CreateNormalCollectionPart(string part) => FormattableString.Invariant($"{part}[X]");
			string CreateCollectionFilterPart(string part) => FormattableString.Invariant($"{part}[@Property=''][X] or /{part}[@Property=''][3]");
			string CreateCollectionWithBindingFieldPart(string part) => FormattableString.Invariant($"{part}[1]/{bindingField} or /{part}[@Property=''][1]/{bindingField}");
		}

#if DEBUG
		internal
#endif
		static (Type DataSourceType, bool IsCollection) GetDataSourceInfo(string part, Type dataSourceType)
		{
			var propertyInfo = dataSourceType?.GetProperties().FirstOrDefault(p => p.Name == part);
			if (propertyInfo == null)
			{
				return (null, false);
			}

			var propType = propertyInfo.PropertyType;
			var listType = ListUtil.GetListElementType(propType);
			return listType is null
				? (propType, false)
				: (listType, true);
		}

		static void AddAdditionalInformationIfNeeded(object control, ZStringBuilder builder)
		{
			if (control is IAdditionalInformation additionalInformation)
			{
				var information = additionalInformation.AdditionalInformation;
				if (!string.IsNullOrEmpty(information))
				{
					builder.AppendLine(information);
				}
			}
		}

		static string GetDocumentDataContext(Control control)
		{
			if (control.GetTopLevelNonParentedControl() is ZForm zForm && zForm.DataSource is BusinessObject businessObject)
			{
				using (var mapTreePresenter = ObjectFactory.Get<IMapTreePresentationManager>())
				{
					return mapTreePresenter.GetDocumentDataContext(businessObject);
				}
			}

			return string.Empty;
		}

		public static string GetControlBindingMember(Control control)
		{
			var result = string.Empty;
			var binding = GetControlBindingForDocEngine(control);
			if (binding.Binding != null)
			{
				var propertySegments = binding.Binding.BindingMemberInfo.BindingMember.Split('.', '+');
				result = $"{string.Join(".", propertySegments)}";
			}

			return result;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static (Binding Binding, string BindingType) GetControlBindingForDocEngine(Control control)
		{
			Binding binding = null;
			var bindingType = string.Empty;
			if (control is ZGrid controlGrid)
			{
				binding = new Binding(controlGrid.BindTo, controlGrid.DataSource, controlGrid.BindTo);
				bindingType = controlGrid.List?.GetType().Name;
			}
			else if (control.Parent is ZGrid grid)
			{
				var columnStyle = grid.GetCurrentColumnStyle();
				if (columnStyle != null)
				{
					bindingType = columnStyle.PropertyDescriptor?.PropertyType?.Name;

					if (grid.ListManager.Bindings.Count > 0)
					{
						binding = grid.ListManager.Bindings.OfType<Binding>().FirstOrDefault(b => b.BindingMemberInfo.BindingField == columnStyle.MappingName);

						if (binding == null)
						{
							binding = grid.ListManager.Bindings.OfType<Binding>().FirstOrDefault(b =>
								!b.BindingMemberInfo.BindingMember.Contains(IgnoredBindingSeparator) &&
								b.BindingMemberInfo.BindingMember.Contains(FormattableString.Invariant($"{columnStyle.MappingName}")) &&
								!b.BindingMemberInfo.BindingMember.Contains(FormattableString.Invariant($"{columnStyle.MappingName}Info")));
						}
					}

					if (binding == null)
					{
						var prefixOfDataMember = string.Empty;
						if (!string.IsNullOrEmpty(grid.BindTo) && grid.BindTo != ".")
						{
							prefixOfDataMember = $"{grid.BindTo}.";
						}
						else
						{
							var moduleButtonGrid = GetParentZModuleButtonGridForDocEngine(grid);
							if (!string.IsNullOrEmpty(moduleButtonGrid?.BindToGridList))
							{
								prefixOfDataMember = $"{moduleButtonGrid.BindToGridList}.";
							}
						}

						binding = new Binding(columnStyle.MappingName, grid.DataSource, FormattableString.Invariant($"{prefixOfDataMember}{columnStyle.MappingName}"));
					}
				}
			}
			else
			{
				var locator = BindableComponentMetaDataPropertyLocator.GetInstance(control.GetType());
				binding = control.DataBindings[locator?.DefaultBindingProperty?.Name];
			}

			if (string.IsNullOrEmpty(bindingType))
			{
				bindingType = binding?.BindingManagerBase?.GetItemProperties()?[binding.BindingMemberInfo.BindingField]?.PropertyType?.Name;
			}

			return (binding, bindingType);
		}

		static ZModuleButtonGrid GetParentZModuleButtonGridForDocEngine(ZGrid grid)
		{
			if (grid is ZGridWithoutColumnStylesSerialisation gridWithoutColumnStylesSerialisation && gridWithoutColumnStylesSerialisation.Parent?.Parent is ZModuleButtonGrid moduleButtonGrid)
			{
				return moduleButtonGrid;
			}

			return null;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		static string TableForBindingMemberForDocEngine(string[] propertySegments)
		{
			const string tableFieldNameLabel = "Table/Field Name:";
			var builder = new ZStringBuilder();
			var property = propertySegments[propertySegments.Length - 1];
			var tablePrefix = CargoWise.Schema.Schema.GetPrefixFromColumnName(property);
			if (!string.IsNullOrWhiteSpace(tablePrefix))
			{
				var schema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchemaFromColumnNamePrefix(tablePrefix);
				builder.AppendLine(schema?.GetSchemaColumn(property) != null ? $"{tableFieldNameLabel} {schema.TableName}.{propertySegments.Last()}" : $"{tableFieldNameLabel} [calculated property]");
			}
			else
			{
				builder.AppendLine($"{tableFieldNameLabel} [calculated property]");
			}

			builder.AppendLine();

			return builder.ToString();
		}

#endregion
	}

	#endregion
}
