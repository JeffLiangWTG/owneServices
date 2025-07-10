using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Design;
using CargoWise.Design.TypeIntellisense;
using CargoWise.Windows.UI;

namespace CargoWise.ComponentModel.Design
{
	/// <summary>
	/// An editor for selecting a Type object in a designer using  intellisense.
	/// To use this editor:<br/>
	/// <br/>
	/// - Apply [Editor(typeof(TypeValueIntellisenseEditor))] to the property this editor is applied to.<br/>
	/// - Apply [TypeConverter(typeof(DesignTimeTypeTypeConverter))] to the property this editor is applied to, if the
	///   property's return type is Type and not String.<br/>
	/// - Apply [DesignerSerializer(typeof(TypeFixCodeDomSerializer), typeof(CodeDomSerializer))]<br/>
	///   to the class that contains the property this editor is applied to, if you need the Type<br/>
	///   object serialized to the component.<br/>
	/// - Apply [TypeValueIntellisenseEditorSubtypeFilter(typeof(Type1), typeof(Type2)..)]<br/>
	///   to the property this editor is applied to if you want to filter out which types you want<br/>
	///   to include in the auto-list. The types are a list of base types or interfaces for which the<br/>
	///   auto-list is generated for.<br/>
	/// <br/>
	/// Types are populated into the auto list by using:<br/>
	/// - EnvDTE.Reference classes are used to determine the references for the current projects.<br/>
	/// - The ITypeResolutionService, if available is used for types outside the current solution.<br/>
	///   If ITypeResolutionService isn't available, normal .net types are loaded directly, which<br/>
	///   will cause assemblies to be loaded.<br/>
	/// - EnvDTE is used to get type information for types within the current solution. Normal .net<br/>
	///   types are not used in this case because the developer may have uncompiled changes.<br/>
	/// </summary>
	public class TypeValueIntellisenseEditor : UITypeEditorWithAutoList
	{
		public TypeValueIntellisenseEditor()
		{
			// HACK: This class is loaded from a ITypeResolutionService. CargoWise.ComponentModel must be
			// loaded early on (here), because later the assembly no longer loads correctly (it looks in
			// the wrong place or something inside an IEnumerable when enumerating intellisensed types).
			object typeLoaded = typeof(TypeValueIntellisenseEditorSubtypeFilterAttribute);
		}

		// Related helper classes:
		//
		// Designer Classes
		// ----------------
		// UITypeEditorWithAutoList - Base class of the editor. This is an abstract editor that shows
		//                            an 'auto-list' to the user when they start typing.
		// TypeValueIntellisenseEditorSubtypeFilterAttribute - Applied to the same property as the editor to
		//                                                     specify base types to list sub-classes of.
		// TypeTypeConverter - Helper class that converts a Type object to a string and vice-versa. When
		//                     converting from a string to a Type, a FakeType object is created if the
		//                     Type cannot be found.
		//
		// Auto-List Source
		// ----------------
		// IAutoListSource - Interface implemented by the auto-list provider. This interface allows for the
		//                   auto-list of anything, not just types.
		// ListBoxAutoListSource - Base class for an auto-list provider that uses a ListBox to show the
		//                         items in the list to the user.
		// TypeIntellisenseAutoListSource - Auto-list provider that produces .net types in a ListBox.
		// IntellisenseDataSource - Generates a list of .net types asynchronously.
		// DesignTimeTypeEnumerable - Enumerable for all types in a project, and it's referenced projects.
		// VSCodeType - Inherits from System.Type and represents an EnvDTE.CodeType object.

		public override void PaintValue(PaintValueEventArgs e)
		{
			try
			{
				CheckAppliedCorrectly(e.Context.PropertyDescriptor);
				base.PaintValue(e);
			}
			catch (Exception f) when (!f.IsCriticalException())
			{
				e.Graphics.DrawString(f.Message, new Font("", 10), Brushes.Black, new PointF(0, 0));
			}
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			CheckAppliedCorrectly(context.PropertyDescriptor);
			return base.EditValue(context, provider, value);
		}

		/// <summary>
		/// Ensure this editor is correctly applied to the given property. Throws an exception if not.
		/// </summary>
		public static void CheckAppliedCorrectly(PropertyDescriptor property)
		{
			if (property == null)
			{
				throw new ArgumentNullException(nameof(property), "You must specify a property to apply the editor to");
			}
			UITypeEditor editor = (UITypeEditor)property.GetEditor(typeof(UITypeEditor));
			if (!(editor is TypeValueIntellisenseEditor))
			{
				throw new ArgumentException(
					"[Editor(typeof(" + typeof(TypeValueIntellisenseEditor).FullName +
					"), typeof(UITypeEditor)] not applied to property '" + property.Name +
					"', or the editor type could not be resolved.");
			}
			if (property.PropertyType != typeof(Type) && property.PropertyType != typeof(string))
			{
				throw new ArgumentException("Property must return System.Type or System.String.");
			}
			TypeConverterAttribute converterAttr = (TypeConverterAttribute)property.Attributes[typeof(TypeConverterAttribute)];
			Type converterType = null;
			if (converterAttr != null && !string.IsNullOrEmpty(converterAttr.ConverterTypeName))
			{
				converterType = Type.GetType(converterAttr.ConverterTypeName);
			}
			if (property.PropertyType == typeof(Type) &&
				(converterType == null || !typeof(TypeTypeConverter).IsAssignableFrom(converterType)))
			{
				throw new ArgumentException(
					"If you apply editor " + typeof(TypeIntellisenseAutoListSource).FullName +
					" to a property of type System.Type, you must apply also apply [TypeConverter(" +
					typeof(TypeTypeConverter).FullName + ")] to that property.");
			}
			if (typeof(IComponent).IsAssignableFrom(property.ComponentType))
			{
				TypeFixCodeDomSerializer.CheckAppliedCorrectly(property.ComponentType);
			}
			TypeValueIntellisenseEditorSubtypeFilterAttribute.CheckAppliedCorrectly(property);
		}

		protected override IAutoListSource NewAutoListSource(ITypeDescriptorContext context)
		{
			TextBox.GotFocus -= new EventHandler(TextBox_GotFocus);
			TextBox.GotFocus += new EventHandler(TextBox_GotFocus);
			return new TypeIntellisenseAutoListSource(context);
		}

		void TextBox_GotFocus(object sender, EventArgs e)
		{
			EnableTypeTypeConverterPerformanceEnhancer();
		}

		#region EnableTypeTypeConverterPerformanceEnhancer / DisableTypeTypeConverterPerformanceEnhancer

		IDisposable typeTypeConverterPerformanceEnhancer;

		void EnableTypeTypeConverterPerformanceEnhancer()
		{
			if (typeTypeConverterPerformanceEnhancer == null && TextBox != null)
			{
				typeTypeConverterPerformanceEnhancer = TypeTypeConverter.TemporarilyResolveTypeOnlyFromCurrentlyLoadedAssemblies();
				TextBox.LostFocus += new EventHandler(TextBox_LostFocusOrDisposed_DisablePerfEnhancement);
				TextBox.Disposed += new EventHandler(TextBox_LostFocusOrDisposed_DisablePerfEnhancement);
			}
		}

		void DisableTypeTypeConverterPerformanceEnhancer()
		{
			if (typeTypeConverterPerformanceEnhancer != null)
			{
				typeTypeConverterPerformanceEnhancer.Dispose();
				typeTypeConverterPerformanceEnhancer = null;
			}
		}

		void TextBox_LostFocusOrDisposed_DisablePerfEnhancement(object sender, EventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			textBox.LostFocus -= new EventHandler(TextBox_LostFocusOrDisposed_DisablePerfEnhancement);
			textBox.Disposed -= new EventHandler(TextBox_LostFocusOrDisposed_DisablePerfEnhancement);
			DisableTypeTypeConverterPerformanceEnhancer();
		}

		#endregion
	}
}
