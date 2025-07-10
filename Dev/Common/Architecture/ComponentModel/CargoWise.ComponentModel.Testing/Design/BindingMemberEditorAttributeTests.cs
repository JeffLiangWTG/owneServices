#if DEBUG
using System;
using System.ComponentModel;
using System.Reflection;
using CargoWise.Common;
using CargoWise.ComponentModel.Design;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class BindingMemberEditorAttributeTests : TestCase
	{
		public void TestGetBindingMemberTypeFilter_WithTypeSpecified()
		{ AssertEquals(typeof(int), AttributeWithTypeSpecifiedFilter.GetBindingMemberTypeFilter(Component, BindingMember_WithTypeSpecifiedFilter_Property)); }

		public void TestGetBindingMemberTypeFilter_WithTypeMemberSpecified()
		{ AssertEquals(typeof(double), AttributeWithTypeMemberSpecifiedFilter.GetBindingMemberTypeFilter(Component, BindingMember_WithMemberSpecifiedTypeFilter_Property)); }

		#region CheckAppliedCorrectly

		public static void CheckAppliedCorrectly(Type type)
		{
			Argument.NotNull(type, nameof(type));
			CheckAppliedCorrectlyForProperties(type);
			CheckAppliedCorrectlyForExtenderMethods(type);
		}

		static void CheckAppliedCorrectlyForProperties(Type type)
		{
			try
			{
				Assembly.Load("CargoWise.Design");
				var properties = TypeDescriptor.GetProperties(type);

				foreach (PropertyDescriptor property in properties)
				{
					if (property != null)
					{
						var attribute = (BindingMemberEditorAttribute)property.Attributes[typeof(BindingMemberEditorAttribute)];
						if (attribute != null)
						{
							var editor = property.GetEditor(Type.GetType(DesignerTypes.UITypeEditor));
							var bindingMemberEditorType = Type.GetType(DesignerTypes.BindingMemberEditor);
							if (editor == null || (bindingMemberEditorType != null && editor.GetType().FullName != bindingMemberEditorType.FullName))
							{
								throw new InvalidOperationException("[Editor(typeof(BindingMemberEditor), typeof(UITypeEditor))] must be applied to property '" + property.Name + "'");
							}

							if (properties[attribute.DataSourceTypeMember] == null)
							{
								throw new InvalidOperationException("Could not find DataSourceTypeMember property '" + attribute.DataSourceTypeMember + "'");
							}
							if (!string.IsNullOrEmpty(attribute.BindingMemberTypeFilterMember) &&
								properties[attribute.BindingMemberTypeFilterMember] == null)
							{
								throw new InvalidOperationException("Could not find BindingMemberTypeFilterMember property '" + attribute.BindingMemberTypeFilterMember + "'");
							}
						}
					}
				}
			}
			catch (System.IO.FileNotFoundException)
			{
				Assert(true);
			}
		}

		static void CheckAppliedCorrectlyForExtenderMethods(Type type)
		{
			Argument.NotNull(type, nameof(type)); // Suggested By ReviewBot 
			var properties = PropertyDescriptorCollectionWithMetaData.FromType(type).AllPropertiesSafe();
			foreach (var method in type.GetMethods())
			{
				var attributes = (BindingMemberEditorAttribute[])method.GetCustomAttributes(typeof(BindingMemberEditorAttribute), true);
				var attribute = attributes.Length == 0 ? null : attributes[0];
				if (attribute != null)
				{
					var editorAttributes = (EditorAttribute[])method.GetCustomAttributes(typeof(EditorAttribute), true);
					var editorAttribute1Type = editorAttributes.Length > 0 && editorAttributes[0] != null ? Type.GetType(editorAttributes[0].EditorTypeName) : null;
					var bindingMemberEditorType = Type.GetType(DesignerTypes.BindingMemberEditor);
					if (editorAttributes.Length == 0 || (editorAttribute1Type != null && bindingMemberEditorType != null && editorAttribute1Type.FullName != bindingMemberEditorType.FullName))
					{
						throw new InvalidOperationException("[Editor(typeof(BindingMemberEditor), typeof(UITypeEditor))] must be applied to extender method '" + method.Name + "'");
					}

					if (attribute.DataSourceTypeMember != null && properties != null)
					{
						type.GetMethod(attribute.DataSourceTypeMember);
						if (!HasExtenderMethod(type, attribute.DataSourceTypeMember) && properties[attribute.DataSourceTypeMember] == null)
						{
							throw new InvalidOperationException("Could not find DataSourceTypeMember property '" + attribute.DataSourceTypeMember + "'");
						}
						if (!string.IsNullOrEmpty(attribute.BindingMemberTypeFilterMember) && !HasExtenderMethod(type, attribute.BindingMemberTypeFilterMember) &&
							properties[attribute.BindingMemberTypeFilterMember] == null)
						{
							throw new InvalidOperationException("Could not find BindingMemberTypeFilterMember property '" + attribute.BindingMemberTypeFilterMember + "'");
						}
					}
				}
			}
		}

		static bool HasExtenderMethod(Type type, string extenderPropertyName)
		{
			Argument.NotNull(type, nameof(type)); // Suggested By ReviewBot 
			var extenderMethod = type.GetMethod("Get" + extenderPropertyName, BindingFlags.Public | BindingFlags.Instance);
			var attributes = (ProvidePropertyAttribute[])type.GetCustomAttributes(typeof(ProvidePropertyAttribute), true);
			foreach (var attribute in attributes)
			{
				if (extenderMethod != null && attribute != null && attribute.PropertyName == extenderPropertyName)
				{
					return true;
				}
			}
			return false;
		}

		[ExpectNoExceptions]
		public void TestAppliedCorrectlyForAllTypes()
		{
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
			try
			{
				foreach (Assembly assembly in AssembliesToCheckAttributesOn.GetAssemblies())
				{
					foreach (Type type in assembly.GetExportedTypes())
					{
						if (!type.IsGenericTypeDefinition)
						{
							CheckAppliedCorrectly(type);
						}
					}
				}
			}
			finally
			{
				AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(CurrentDomain_AssemblyResolve);
			}
		}

		[ExpectNoExceptions]
		public void TestCheckAppliedCorrectly()
		{
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
			try
			{
				CheckAppliedCorrectly(typeof(CorrectlyApplied));
				CheckIncorrectlyApplied(typeof(IncorrectlyApplied_WithoutEditor));
				CheckIncorrectlyApplied(typeof(IncorrectlyApplied_WithNonExistantDataSourceTypeMember));
				CheckIncorrectlyApplied(typeof(IncorrectlyApplied_WithNonExistantDataSourceTypeMember_WithExtenderProperty));
				CheckIncorrectlyApplied(typeof(IncorrectlyApplied_WithNonExistantBindingMemberTypeFilterMember));
				CheckIncorrectlyApplied(typeof(IncorrectlyApplied_WithNonExistantBindingMemberTypeFilterMember_WithExtenderProperty));
				CheckIncorrectlyApplied(typeof(IncorrectlyApplied_WithNoExtenderProvidePropertyAttribute_ForDataSourceType));
				CheckIncorrectlyApplied(typeof(IncorrectlyApplied_WithNoExtenderProvidePropertyAttribute_ForBindingMemberTypeFilterMember));
			}
			finally
			{
				AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(CurrentDomain_AssemblyResolve);
			}
		}

		bool inCurrentDomain_AssemblyResolve;
		Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			Assembly result = null;
			if (!inCurrentDomain_AssemblyResolve)
			{
				inCurrentDomain_AssemblyResolve = true;
				try
				{
					result = Assembly.Load(args.Name);
				}
				finally
				{
					inCurrentDomain_AssemblyResolve = false;
				}
			}
			return result;
		}

		void CheckIncorrectlyApplied(Type type)
		{
			try
			{
				CheckAppliedCorrectly(type);
				Fail("Expected an exception for incorrectly applied type " + type.Name);
			}
			catch (InvalidOperationException)
			{
			}
		}

		class CorrectlyApplied : IncorrectlyApplied_WithoutEditor
		{
			[Editor(DesignerTypes.BindingMemberEditor, DesignerTypes.UITypeEditor)]
			[BindingMemberEditor("DataSourceType", "BindingMemberTypeFilter")]
			public override string BindingMember
			{ get { return ""; } }
		}

		class IncorrectlyApplied_WithoutEditor
		{
			[BindingMemberEditor("DataSourceType", "BindingMemberTypeFilter")]
			public virtual string BindingMember
			{ get { return ""; } }

			public Type DataSourceType
			{ get { return typeof(object); } }

			public Type BindingMemberTypeFilter
			{ get { return typeof(object); } }
		}

		class IncorrectlyApplied_WithNonExistantDataSourceTypeMember
		{
			[Editor(DesignerTypes.BindingMemberEditor, DesignerTypes.UITypeEditor)]
			[BindingMemberEditor("NonPublicDataSourceType", "BindingMemberTypeFilter")]
			public string BindingMember
			{ get { return ""; } }

			public Type BindingMemberTypeFilter
			{ get { return typeof(object); } }
		}

		[ProvideProperty("BindingMember", "")]
		[ProvideProperty("NonPublicDataSourceType", "")]
		[ProvideProperty("BindingMemberTypeFilter", "")]
		class IncorrectlyApplied_WithNonExistantDataSourceTypeMember_WithExtenderProperty
		{
			[Editor(DesignerTypes.BindingMemberEditor, DesignerTypes.UITypeEditor)]
			[BindingMemberEditor("NonPublicDataSourceType", "BindingMemberTypeFilter")]
			public string GetBindingMember()
			{ return ""; }

			public Type GetBindingMemberTypeFilter()
			{ return typeof(object); }
		}

		class IncorrectlyApplied_WithNonExistantBindingMemberTypeFilterMember
		{
			[Editor(DesignerTypes.BindingMemberEditor, DesignerTypes.UITypeEditor)]
			[BindingMemberEditor("DataSourceType", "NonPublicBindingMemberTypeFilter")]
			public string BindingMember
			{ get { return ""; } }

			public Type DataSourceType
			{ get { return typeof(object); } }
		}

		[ProvideProperty("BindingMember", "")]
		[ProvideProperty("DataSourceType", "")]
		[ProvideProperty("NonPublicBindingMemberTypeFilter", "")]
		class IncorrectlyApplied_WithNonExistantBindingMemberTypeFilterMember_WithExtenderProperty
		{
			[Editor(DesignerTypes.BindingMemberEditor, DesignerTypes.UITypeEditor)]
			[BindingMemberEditor("DataSourceType", "NonPublicBindingMemberTypeFilter")]
			public string GetBindingMember()
			{ return ""; }

			public Type GetDataSourceType()
			{ return typeof(object); }
		}

		[ProvideProperty("BindingMember", "")]
		[ProvideProperty("BindingMemberTypeFilterMember", "")]
		class IncorrectlyApplied_WithNoExtenderProvidePropertyAttribute_ForDataSourceType
		{
			[Editor(DesignerTypes.BindingMemberEditor, DesignerTypes.UITypeEditor)]
			[BindingMemberEditor("DataSourceType", "BindingMemberTypeFilter")]
			public string GetBindingMember()
			{ return ""; }

			public Type GetDataSourceType()
			{ return typeof(object); }

			public Type GetBindingMemberTypeFilter()
			{ return typeof(object); }
		}

		[ProvideProperty("BindingMember", "")]
		[ProvideProperty("DataSourceType", "")]
		class IncorrectlyApplied_WithNoExtenderProvidePropertyAttribute_ForBindingMemberTypeFilterMember
		{
			[Editor(DesignerTypes.BindingMemberEditor, DesignerTypes.UITypeEditor)]
			[BindingMemberEditor("DataSourceType", "BindingMemberTypeFilter")]
			public string GetBindingMember()
			{ return ""; }

			public Type GetDataSourceType()
			{ return typeof(object); }

			public Type GetBindingMemberTypeFilter()
			{ return typeof(object); }
		}

		#endregion

		#region Test Classes

		class TestComponent
		{
			[BindingMemberEditor("DataSourceType", typeof(int))]
			public string BindingMember_WithTypeSpecifiedFilter
			{ get { return ""; } }

			[BindingMemberEditor("DataSourceType", "BindingMemberTypeFilter")]
			public string BindingMember_WithMemberSpecifiedTypeFilter
			{ get { return ""; } }

			public Type BindingMemberTypeFilter
			{ get { return typeof(double); } }

			public Type DataSourceType
			{ get { return typeof(string); } }
		}

		#endregion

		#region Implementation

		TestComponent Component
		{
			get
			{
				if (component == null)
				{
					component = new TestComponent();
				}
				return component;
			}
		}
		TestComponent component;

		PropertyDescriptor BindingMember_WithTypeSpecifiedFilter_Property
		{ get { return TypeDescriptor.GetProperties(Component)["BindingMember_WithTypeSpecifiedFilter"]; } }

		BindingMemberEditorAttribute AttributeWithTypeSpecifiedFilter
		{
			get
			{
				if (attributeWithTypeSpecifiedFilter == null)
				{
					PropertyDescriptor bindingMemberProperty = TypeDescriptor.GetProperties(Component)["BindingMember_WithTypeSpecifiedFilter"];
					attributeWithTypeSpecifiedFilter = (BindingMemberEditorAttribute)BindingMember_WithTypeSpecifiedFilter_Property.Attributes[typeof(BindingMemberEditorAttribute)];
				}
				return attributeWithTypeSpecifiedFilter;
			}
		}
		BindingMemberEditorAttribute attributeWithTypeSpecifiedFilter;

		PropertyDescriptor BindingMember_WithMemberSpecifiedTypeFilter_Property
		{ get { return TypeDescriptor.GetProperties(Component)["BindingMember_WithMemberSpecifiedTypeFilter"]; } }

		BindingMemberEditorAttribute AttributeWithTypeMemberSpecifiedFilter
		{
			get
			{
				if (attributeWithTypeMemberSpecifiedFilter == null)
				{
					attributeWithTypeMemberSpecifiedFilter = (BindingMemberEditorAttribute)BindingMember_WithMemberSpecifiedTypeFilter_Property.Attributes[typeof(BindingMemberEditorAttribute)];
				}
				return attributeWithTypeMemberSpecifiedFilter;
			}
		}
		BindingMemberEditorAttribute attributeWithTypeMemberSpecifiedFilter;

		#endregion
	}
}
#endif
