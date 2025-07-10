using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Implemented by a control that must handle it's own data binding, for example the DataGrid.
	/// If your control simply binds to a property, apply DefaultBindingPropertyAttribute to the
	/// data bound property instead of implementing this interface.
	/// </summary>
	public interface IDataBoundControl
	{
		/// <summary>
		///	Set the data source and data member simultaneously.
		/// </summary>
		/// <param name="dataSource">The data source, or null to unbind the control.</param>
		/// <param name="dataMember">The path and property name of the data member.</param>
		void SetDataBinding(object dataSource, string dataMember);

		/// <summary>
		/// Get the type of the data source.
		/// </summary>
		Type DataSourceType { get; }

		/// <summary>
		/// Get the top-level data source this control is bound to.
		/// </summary>
		object DataSource { get; }

		/// <summary>
		/// Get the data member this control is bound to.
		/// </summary>
		string DataMember { get; }
	}

	/// <summary>
	/// The default implementation of IDataBoundControl. Use this class to get an
	/// IDataBoundControl implementation for controls that may or may not explicitly
	/// implement IDataBoundControl, or to provide a default implementation when
	/// implementing IDataBoundControl yourself.
	/// </summary>
	public static class DataBoundControl
	{
		public static IDataBoundControl GetDefaultImplementation(Control control)
		{ return new DefaultDataBoundControlImpl(control); }

		public static IDataBoundControl Get(Control control)
		{
			IDataBoundControl dataBoundControl = control as IDataBoundControl;
			return dataBoundControl ?? GetDefaultImplementation(control);
		}

		#region SetDataBindingForMetadataProperties

		public static void SetDataBindingForMetadataProperties(Control control, object dataSource, string dataMember)
		{
			if (dataSource != null)
			{
				BindMetaDataProperties(control, dataSource, dataMember);
			}
			else
			{
				UnbindMetaDataProperties(control);
			}
		}

		static void BindMetaDataProperties(Control control, object dataSource, string dataMember)
		{
			List<Binding> bindingsToUpdate = new List<Binding>();
			KBindingMemberInfo dataMemberInfo = new KBindingMemberInfo(dataMember);

			MetaDataPropertyMapping[] metaDataMappings = GetMetaDataPropertyMappings(control, dataSource, dataMemberInfo);
			foreach (MetaDataPropertyMapping mapping in metaDataMappings)
			{
				try
				{
					BindMetaDataProperty(control, mapping, dataSource, bindingsToUpdate);
				}
				catch (Exception e)
				{
					// To help diagnosing exceptions inside Windows.Forms binding code we need additional information.
					e.Data.Add("ControlTypeDescriptorProviderType", GetTypeDescriptorProviderTypes(control));
					e.Data.Add("ControlPropertyDescriptorCollection", FormatPropertyDescriptorCollection(control));
					e.Data.Add("DataSourceTypeDescriptorProviderType", GetTypeDescriptorProviderTypes(dataSource));

					throw new KDataBindingException(dataMember, e.Message, e);
				}
			}

			// we need to re-read these because the meta-data value doesn't always
			// populate initially when the control is first shown when there is
			// no current on the parent CurrencyManager
			foreach (Binding binding in bindingsToUpdate)
			{
				try
				{
					if (binding.ControlUpdateMode != ControlUpdateMode.Never)
					{
						binding.ReadValue();
					}
				}
				catch (IndexOutOfRangeException)
				{
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
			Justification = "Generic exception handling for retrieving extra diagnostic information within an exception handler")]
		static string GetTypeDescriptorProviderTypes(object target)
		{
			try
			{
				if (target == null)
				{
					return null;
				}

				var providerTypeNames = FindTypeDescriptionProviders(TypeDescriptor.GetProvider(target))
					.Select(provider => provider.GetType().Name);
				return string.Join(", ", providerTypeNames);
			}
			catch (Exception e)
			{
				return string.Format(CultureInfo.InvariantCulture, (NoResString)"Could not retrieve type descriptor providers for {0}. {1}", target?.GetType().Name, e);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
			Justification = "Generic exception handling for retrieving extra diagnostic information within an exception handler")]
		static string FormatPropertyDescriptorCollection(object target)
		{
			try
			{
				var pdc = TypeDescriptor.GetProperties(target);
				var propertyDescriptorInfos = pdc.Cast<PropertyDescriptor>().Select(FormatPropertyDescriptor);
				return $"{{{string.Join(", ", propertyDescriptorInfos)}}}";
			}
			catch (Exception e)
			{
				return string.Format(CultureInfo.InvariantCulture, (NoResString)"Could not retrieve property descriptors for {0}. {1}", target?.GetType().Name, e);
			}
		}

		static string FormatPropertyDescriptor(PropertyDescriptor pd)
			=> pd == null ? "null" : $"{pd.Name} Type={pd.PropertyType.Name} Descriptor={pd.GetType().Name}";

		static List<object> FindTypeDescriptionProviders(object provider)
		{
			// Unfortunately there is no nice way of retrieving the list of registered TypeDescriptionProviders
			// that were registered with TypeDescriptor.AddProvider or using the TypeDescriptionProviderAttribute
			// We use reflection here to extract this list the hard way to provide the extra diagnostic information.
			// https://stackoverflow.com/a/7525792/279098
			var list = new List<object>();

			if (provider == null)
			{
				return list;
			}

			var providerType = provider.GetType();

			if (providerType.Name == "TypeDescriptionNode")
			{
				var nestedProvider = provider.GetField("Provider");
				list.AddRange(FindTypeDescriptionProviders(nestedProvider));
				var next = provider.GetField("Next");
				list.AddRange(FindTypeDescriptionProviders(next));
				return list;
			}

			if (providerType.Name == "DelegatingTypeDescriptionProvider")
			{
				var nestedProvider = provider.GetProperty("Provider");
				list.AddRange(FindTypeDescriptionProviders(nestedProvider));
				return list;
			}

			list.Add(provider);
			return list;
		}

		public static object GetField(this object target, string name)
			=> target?.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(target);

		public static object GetProperty(this object target, string name)
			=> target?.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(target);

		static void BindMetaDataProperty(Control control, MetaDataPropertyMapping mapping, object dataSource, List<Binding> bindingsToUpdate)
		{
			if (mapping.DataSourceMetaDataProperty is ConstantValuePropertyDescriptor)
			{
				SetControlValueUsingTypeConverter(control, mapping.ControlMetaDataProperty, ((ConstantValuePropertyDescriptor)mapping.DataSourceMetaDataProperty).ConstantValue);
			}
			else
			{
				BindableComponentMetaDataPropertyLocator locator = BindableComponentMetaDataPropertyLocator.GetInstance(control.GetType());
				control.DataBindings.RemoveBinding(mapping.ControlMetaDataProperty.Name);

				KBinding binding = new KBinding(mapping.ControlMetaDataProperty.Name, dataSource, mapping.DataSourceMetaDataBindingMember.BindingMember);
				SetBindingOptions(binding, locator.GetBindingOptions(mapping.ControlMetaDataProperty));
				if ((mapping.MetaDataType.IsOneWay || !mapping.DataSourceMetaDataProperty.HasSetter()) &&
					!typeof(IList).IsAssignableFrom(mapping.DataSourceMetaDataProperty.PropertyType))
				{
					binding.DataSourceUpdateMode = DataSourceUpdateMode.Never;
				}

				var listManager = control.BindingContext?.EnsureListManager(binding.DataSource, binding.BindingMemberInfo.BindingPath);
				var shouldSuspendBinding = control.IsHandleCreated && listManager != null && listManager.Count == 0 && !listManager.IsBindingSuspended;
				using (shouldSuspendBinding ? new DisposableAction(listManager.SuspendBinding, listManager.ResumeBinding) : null)
				{
					control.DataBindings.Add(binding);
				}
				bindingsToUpdate.Add(binding);
			}
		}

		static void SetControlValueUsingTypeConverter(Control control, PropertyDescriptor controlProperty, object value)
		{
			object controlValue = value;
			if (!controlProperty.PropertyType.IsInstanceOfType(value))
			{
				if (controlProperty.Converter.CanConvertFrom(value.GetType()))
				{
					controlValue = controlProperty.Converter.ConvertFrom(value);
				}
			}
			controlProperty.SetValue(control, controlValue);
		}

		static void UnbindMetaDataProperties(Control control)
		{
			BindableComponentMetaDataPropertyLocator locator = BindableComponentMetaDataPropertyLocator.GetInstance(control.GetType());
			foreach (PropertyDescriptor controlMetaProperty in locator.DefaultControlMetaDataProperties.Values)
			{
				Binding metaBinding = control.DataBindings[controlMetaProperty.Name];
				if (metaBinding != null)
				{
					control.DataBindings.Remove(metaBinding);
				}
			}
		}

		#endregion

		#region struct MetaDataPropertyMapping

		struct MetaDataPropertyMapping
		{
			public MetaDataPropertyMapping(
				MetaDataType metaDataType, PropertyDescriptor controlMetaDataProperty, PropertyDescriptor dataSourceMetaDataProperty, string dataSourceBindingPath)
			{
				this.MetaDataType = metaDataType;
				this.ControlMetaDataProperty = controlMetaDataProperty;
				this.DataSourceMetaDataProperty = dataSourceMetaDataProperty;
				this.DataSourceMetaDataBindingMember =
					new KBindingMemberInfo(dataSourceBindingPath, DataSourceMetaDataProperty.Name);
			}

			/// <summary>
			/// Get the MetaDataType associated with this mapping.
			/// </summary>
			public readonly MetaDataType MetaDataType;

			/// <summary>
			/// Get the PropertyDescriptor used to access the data on the control.
			/// </summary>
			public readonly PropertyDescriptor ControlMetaDataProperty;

			/// <summary>
			/// Get the PropertyDescriptor used to access the data on the data source.
			/// </summary>
			public readonly PropertyDescriptor DataSourceMetaDataProperty;

			/// <summary>
			/// Get the fully-qualified bind to member names of the meta data property.
			/// </summary>
			public readonly KBindingMemberInfo DataSourceMetaDataBindingMember;
		}

		#endregion

		#region class DefaultDataBoundControlImpl

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Impl")]
		sealed class DefaultDataBoundControlImpl : IDataBoundControl
		{
			public DefaultDataBoundControlImpl(Control control)
			{ this.Control = control; }

			public Control Control { get; private set; }

			#region IDataBoundControl Members

			public void SetDataBinding(object dataSource, string dataMember)
			{
				SetDataBindingForMetadataProperties(Control, dataSource, dataMember);
				BindableComponentMetaDataPropertyLocator locator = BindableComponentMetaDataPropertyLocator.GetInstance(Control.GetType());
				PropertyDescriptor controlValueProperty = locator.DefaultBindingProperty;

				Binding binding = Control.DataBindings[controlValueProperty.Name];
				if (binding != null)
				{
					Control.DataBindings.Remove(binding);
				}
				if (dataSource != null)
				{
					KBinding newBinding = new KBinding(controlValueProperty.Name, dataSource, dataMember);
					SetBindingOptions(newBinding, locator.GetBindingOptions(controlValueProperty));
					Control.DataBindings.Add(newBinding);
				}
				BindControlExtensions(dataSource, dataMember);
			}

			void BindControlExtensions(object dataSource, string dataMember)
			{
				IExtendedControl extendedControl = Control as IExtendedControl;
				if (extendedControl != null)
				{
					extendedControl.Extensions.SetDataBinding(dataSource, dataMember);
				}
			}

			public Type DataSourceType
			{ get { return ValueControlProperty == null ? null : ValueControlProperty.PropertyType; } }

			public object DataSource
			{
				get
				{
					Binding binding = ValueControlProperty == null ? null : Control.DataBindings[ValueControlProperty.Name];
					return (binding == null) ? null : binding.DataSource;
				}
			}

			string IDataBoundControl.DataMember
			{ get { return BindingMemberInfo.BindingMember; } }

			BindingMemberInfo BindingMemberInfo
			{
				get
				{
					Binding binding = ValueControlProperty == null ? null : Control.DataBindings[ValueControlProperty.Name];
					return (binding == null) ? new BindingMemberInfo() : binding.BindingMemberInfo;
				}
			}

			#endregion

			#region Implementation

			PropertyDescriptor ValueControlProperty
			{
				get
				{
					if (valueControlProperty == null)
					{
						valueControlProperty = BindableComponentMetaDataPropertyLocator.GetInstance(Control.GetType()).DefaultBindingProperty;
					}
					return valueControlProperty;
				}
			}
			PropertyDescriptor valueControlProperty;

			#endregion
		}

		#endregion

		#region Implementation

		static MetaDataPropertyMapping[] GetMetaDataPropertyMappings(Control control, object dataSource, KBindingMemberInfo dataMember)
		{
			List<MetaDataPropertyMapping> result = new List<MetaDataPropertyMapping>();
			PropertyDescriptor valueProperty = null;
			if (!string.IsNullOrEmpty(dataMember.BindingMember))
			{
				valueProperty = GetDataPropertyDescriptor(control, dataSource, dataMember);
				if (valueProperty == null)
				{
					throw new KDataBindingException(
						dataMember.BindingMember,
						string.Format(
							CultureInfo.InvariantCulture,
							"Could not find data-member in data-source type '{0}' with binding-path '{1}' and binding-field '{2}' for control-type '{3}'.",
							dataSource?.GetType()?.Name ??
								(dataSource == null
									? "object (NULL value)"
									: "object (NULL GetType)"
								),
							dataMember.BindingPath,
							dataMember.BindingField,
							control?.GetType()?.Name ?? "NULL"));
				}
			}

			// there can only be meta data properties if this interface is implemented
			KPropertyDescriptor valueKProperty = valueProperty as KPropertyDescriptor;
			KPropertyDescriptorCollection collection = valueKProperty == null ? null : valueKProperty.Collection;
			if (collection is PropertyDescriptorCollectionWithMetaData)
			{
				// get all control meta data properties
				IDictionary<string, PropertyDescriptor> dictionary = BindableComponentMetaDataPropertyLocator.GetInstance(control.GetType()).DefaultControlMetaDataProperties;
				foreach (KeyValuePair<string, PropertyDescriptor> entry in dictionary)
				{
					MetaDataType info = MetaDataType.GetMetaDataType(entry.Key);
					PropertyDescriptor metaDataProperty = MetaData.GetMetaDataProperty(valueProperty.ComponentType, valueProperty, info.Id);

					if (info.IsMandatory && metaDataProperty == null)
					{
						throw new InvalidOperationException("Could not find required meta-data " + info.Id + ". You must apply this to property " + valueProperty.Name + ".");
					}
					if (metaDataProperty != null)
					{
						PropertyDescriptor controlMetaDataProperty = entry.Value;
						result.Add(new MetaDataPropertyMapping(
							info, controlMetaDataProperty, metaDataProperty, dataMember.BindingPath));
					}
				}
			}
			return result.ToArray();
		}

		static PropertyDescriptor GetDataPropertyDescriptor(Control control, object dataSource, KBindingMemberInfo dataMember)
		{
			BindingContext bc = control.BindingContext ?? new BindingContext();

			PropertyDescriptor result;
			try
			{
				result = bc.EnsureListManager(dataSource, dataMember.BindingPath).GetItemProperties()[dataMember.BindingField];
			}
			catch (Exception e)
			{
				throw new KDataBindingException(dataMember.BindingMember, e.Message, e);
			}
			return result;
		}

		static void SetBindingOptions(KBinding binding, BindingOptionsAttribute bindingOptions)
		{
			if (bindingOptions.FormattingEnabled)
			{
				binding.FormattingEnabled = bindingOptions.FormattingEnabled;
			}
			if (bindingOptions.UseTypeConverters)
			{
				binding.UseTypeConverters = bindingOptions.UseTypeConverters;
			}
			if (bindingOptions.UpdateDataSourceOnPropertyChange)
			{
				binding.DataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;
			}
		}

		#endregion
	}
}
