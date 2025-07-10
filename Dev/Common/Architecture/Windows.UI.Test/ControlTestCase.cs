using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class ControlTestCase<T> : TestCase where T : Control, new()
	{
		#region SouldHaveDefaultValue
#if !WINZOR
		public void TestSouldHaveDefaultValue()
		{
			var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
				.Where(p => p.PropertyType.IsValueType && ((DesignerSerializationVisibilityAttribute)p.GetCustomAttribute(typeof(DesignerSerializationVisibilityAttribute), false))?.Visibility != DesignerSerializationVisibility.Hidden);

			if (properties.Any())
			{
				var typeName = typeof(T).FullName;
				CombineAssertions($"All properties for Type {typeName} should have default value", () =>
				{
					foreach (var property in properties)
					{
						var defaultValueAttr = property.GetCustomAttribute(typeof(DefaultValueAttribute), true);
						var propertyName = property.Name;
						MethodInfo method = GetShouldSerializeMethod(typeof(T), propertyName);
						AssertEquals($"Property [{propertyName}] in {typeName} should have [DefaultValue] Attribute OR method ShouldSerialize{propertyName}", true, defaultValueAttr != null || method != null);
					}
				});
			}
			else
			{
				Assert(true);
			}
		}
#endif
		#endregion

		#region ShouldSerializeText

		public void TestShouldSerializeText()
		{
			PropertyInfo property = typeof(T).GetProperty("Text");
			MethodInfo method = typeof(T).GetMethod("ShouldSerializeText", BindingFlags.NonPublic | BindingFlags.Instance);
			AssertEquals(
				"The ShouldSerializeText method must have protected accessibility",
				false, GetShouldSerializeMethod(typeof(T), "Text") != null && method == null);
		}

		static MethodInfo GetShouldSerializeMethod(Type type, string propertyName)
		{
			Type current = type;
			while (current != null)
			{
				MethodInfo method = current.GetMethod("ShouldSerialize" + propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
				if (method != null)
				{
					return method;
				}
				current = current.BaseType;
			}
			return null;
		}

		#endregion

		#region Binding ReadOnly

		[RequiresSTA]
		public void TestReadOnlyForBinding()
		{
			if (ReadOnlyForBindingProperty != null)
			{
				AssertEquals("Default value", false, ReadOnlyForBinding);

				ReadOnlyForBinding = true;
				AssertEquals("ReadOnly/!Enabled=true when ReadOnlyForBinding=true", true, ReadOnlyOrNotEnabled);

				ReadOnlyForBinding = false;
				AssertEquals("ReadOnly/!Enabled=false when ReadOnlyForBinding=false", false, ReadOnlyOrNotEnabled);

				ReadOnlyForBinding = true;
				AssertEquals("ReadOnly/!Enabled=true when ReadOnlyForBinding=true", true, ReadOnlyOrNotEnabled);

				AssertEquals("Has ReadOnlyForBinding meta-data control property", "ReadOnlyForBinding", BindableComponentMetaDataPropertyLocator.GetDefaultMetaDataProperty(typeof(T), MetaDataTypes.ReadOnly).Name);
			}
			Assert(true);
		}

		public void TestReadOnlyForBinding_ReadOnlyMetaDataCoexistsWithControlReadOnlySetter()
		{
			if (ReadOnlyForBindingProperty != null)
			{
				AssertEquals("Default value", false, ReadOnlyOrNotEnabled);

				ReadOnlyOrNotEnabled = true;
				AssertEquals(true, ReadOnlyOrNotEnabled);
				ReadOnlyOrNotEnabled = false;
				AssertEquals(false, ReadOnlyOrNotEnabled);
				ReadOnlyOrNotEnabled = true;
				AssertEquals(true, ReadOnlyOrNotEnabled);

				ReadOnlyOrNotEnabled = true;
				ReadOnlyForBinding = true;
				ReadOnlyForBinding = false;
				AssertEquals(true, ReadOnlyOrNotEnabled);

				ReadOnlyOrNotEnabled = false;
				ReadOnlyForBinding = true;
				ReadOnlyForBinding = false;
				AssertEquals(false, ReadOnlyOrNotEnabled);
			}
			Assert(true);
		}

		[RequiresSTA]
		public void TestReadOnlyForBinding_Attributes()
		{
			if (ReadOnlyForBindingProperty != null)
			{
				AssertNotBrowsableOrSerializable(ReadOnlyForBindingProperty);
				AssertNotBrowsableOrSerializable(ReadOnlyForBindingIsNullProperty);
			}
			Assert(true);
		}

		[RequiresSTA]
		public void TestReadOnlyForBindingIsNull()
		{
			if (ReadOnlyForBindingProperty != null)
			{
				AssertNotNull("public bool ReadOnlyForBindingIsNull - property must exist", ReadOnlyForBindingIsNullProperty);
				ReadOnlyForBindingProperty.SetValue(Control, false);
				AssertEquals("ReadOnlyForBinding", false, ReadOnlyForBindingProperty.GetValue(Control));
				ReadOnlyForBindingIsNullProperty.SetValue(Control, true);
				AssertEquals("ReadOnlyForBinding set to true if ReadOnlyForBindingIsNull is set to true", true, ReadOnlyForBindingProperty.GetValue(Control));
			}
			Assert(true);
		}

		void AssertNotBrowsableOrSerializable(PropertyDescriptor property)
		{
			BrowsableAttribute browsableAttr = (BrowsableAttribute)property.Attributes[typeof(BrowsableAttribute)];
			AssertEquals("BrowsableAttribute", false, browsableAttr.Browsable);

			DesignerSerializationVisibilityAttribute designerVisibilityAttr = (DesignerSerializationVisibilityAttribute)property.Attributes[typeof(DesignerSerializationVisibilityAttribute)];
			AssertEquals("DesignerSerializationVisibilityAttribute", DesignerSerializationVisibility.Hidden, designerVisibilityAttr.Visibility);

			EditorBrowsableAttribute editorBrowsableAttr = (EditorBrowsableAttribute)property.Attributes[typeof(EditorBrowsableAttribute)];
			AssertEquals("EditorBrowsableState", EditorBrowsableState.Never, editorBrowsableAttr.State);
		}

		#endregion

		#region Implementation

		protected T Control
		{
			get { return control ?? (control = GetNewControl()); }
		}
		T control;

		protected Type ControlType
		{
			get { return Control.GetType(); }
		}

		protected virtual T GetNewControl()
		{
			return new T();
		}

		bool ReadOnlyOrNotEnabled
		{
			get { return ReadOnlyProperty != null ? ReadOnly : !Control.Enabled; }
			set
			{
				if (ReadOnlyProperty != null)
				{
					ReadOnly = value;
				}
				else
				{
					Control.Enabled = !value;
				}
			}
		}

		bool ReadOnly
		{
			get { return (bool)ReadOnlyProperty.GetValue(Control); }
			set { ReadOnlyProperty.SetValue(Control, value); }
		}

		bool ReadOnlyForBinding
		{
			get { return (bool)ReadOnlyForBindingProperty.GetValue(Control); }
			set { ReadOnlyForBindingProperty.SetValue(Control, value); }
		}

		protected virtual PropertyDescriptor ReadOnlyProperty
		{ get { return readOnlyProperty ?? (readOnlyProperty = GetControlProperty("ReadOnly")); } }
		PropertyDescriptor readOnlyProperty;

		PropertyDescriptor ReadOnlyForBindingProperty
		{ get { return readOnlyForBindingProperty ?? (readOnlyForBindingProperty = GetControlProperty("ReadOnlyForBinding")); } }
		PropertyDescriptor readOnlyForBindingProperty;

		PropertyDescriptor ReadOnlyForBindingIsNullProperty
		{ get { return readOnlyForBindingIsNullProperty ?? (readOnlyForBindingIsNullProperty = GetControlProperty("ReadOnlyForBindingIsNull")); } }
		PropertyDescriptor readOnlyForBindingIsNullProperty;

		PropertyDescriptor GetControlProperty(string name)
		{
			foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(Control))
			{
				if (property.Name == name)
				{
					return property;
				}
			}
			return null;
		}

		protected override void TearDown()
		{
			if (control != null)
			{
				control.Dispose();
			}
			base.TearDown();
		}

		#endregion
	}
}
