using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;
using CargoWise.Common;

namespace CargoWise.ComponentModel.Design
{
	/// <summary>
	/// Apply this to the same property you apply the DTypeValueIntellisenseEditor to restrict the
	/// possible types that can be selected by the user. You can apply this attribute multiple times
	/// to provide or'ing of types. For example:<br/>
	/// <br/>
	/// [DTypeValueIntellisenseEditorSubtypeFilter(typeof(TestInterface1))]<br/>
	/// [DTypeValueIntellisenseEditorSubtypeFilter(typeof(TestInterface2), typeof(TestInterface3))]<br/>
	///	<br/>
	///	says to list all types that are subtypes of<br/>
	///	(TestInterface1 or (TestInterface2 and TestInterface3)).
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	[Serializable]
	public sealed class TypeValueIntellisenseEditorSubtypeFilterAttribute : Attribute
	{
		public TypeValueIntellisenseEditorSubtypeFilterAttribute(Type baseType)
			: this(new Type[] { baseType })
		{
		}

		public TypeValueIntellisenseEditorSubtypeFilterAttribute(Type baseType1, Type baseType2)
			: this(new Type[] { baseType1, baseType2 })
		{
		}

		public TypeValueIntellisenseEditorSubtypeFilterAttribute(Type baseType1, Type baseType2, Type baseType3)
			: this(new Type[] { baseType1, baseType2, baseType3 })
		{
		}

		[CLSCompliant(false)]
		public TypeValueIntellisenseEditorSubtypeFilterAttribute(params Type[] baseTypes)
			: this(null, baseTypes)
		{
			if (!(baseTypes == null || (baseTypes != null && baseTypes.Length > 0)))
			{
				throw new ArgumentException("If you use this overload you must provide the base types", nameof(baseTypes));
			}
		}

		public TypeValueIntellisenseEditorSubtypeFilterAttribute(string typeFilterMember, Type baseType)
			: this(typeFilterMember, new Type[] { baseType })
		{
		}

		public TypeValueIntellisenseEditorSubtypeFilterAttribute(string typeFilterMember, Type baseType1, Type baseType2)
			: this(typeFilterMember, new Type[] { baseType1, baseType2 })
		{
		}

		public TypeValueIntellisenseEditorSubtypeFilterAttribute(string typeFilterMember, Type baseType1, Type baseType2, Type baseType3)
			: this(typeFilterMember, new Type[] { baseType1, baseType2, baseType3 })
		{
		}

		[CLSCompliant(false)]
		public TypeValueIntellisenseEditorSubtypeFilterAttribute(string typeFilterMember, params Type[] baseTypes)
		{
			if (!(baseTypes == null || (baseTypes != null && baseTypes.Length > 0)))
			{
				throw new ArgumentException("If you use this overload you must provide the base types", nameof(baseTypes));
			}

			this.typeFilterMember = typeFilterMember;
			this.baseTypes = baseTypes;
		}

		public Type[] BaseTypes
		{
			get { return baseTypes; }
		}
		readonly Type[] baseTypes;

		public string TypeFilterMember
		{
			get { return typeFilterMember; }
		}
		readonly string typeFilterMember;

		public string ImportNamespacesMember { get; set; }

		public Type[] GetBaseTypes(ITypeResolutionService typeResolutionService)
		{
			Argument.NotNull(typeResolutionService, nameof(typeResolutionService)); // Suggested By ReviewBot 
			Type[] result = null;
			if (BaseTypes != null)
			{
				var list = new List<Type>();
				foreach (var baseType in BaseTypes)
				{
					if (baseType != null)
					{
						var loadedType = typeResolutionService.GetType(baseType.FullName);
						if (loadedType != null)
						{
							list.Add(loadedType);
						}
					}
				}
				result = list.ToArray();
			}
			return result;
		}

		public static void CheckAppliedCorrectly(PropertyDescriptor property)
		{
			Argument.NotNull(property, nameof(property));
			var attributes = property.GetAttributesAllowMultiple(typeof(TypeValueIntellisenseEditorSubtypeFilterAttribute));
			foreach (TypeValueIntellisenseEditorSubtypeFilterAttribute attribute in attributes)
			{
				if (attribute != null && property.ComponentType != null)
				{
					if (!string.IsNullOrEmpty(attribute.TypeFilterMember))
					{
						var method = property.ComponentType.GetMethod(attribute.TypeFilterMember, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ITypeResolutionService), typeof(IEnumerable<Type>) }, null);
						if (method == null)
						{
							throw new ArgumentException("TypeFilterMember method 'IEnumerable<Type> " + attribute.TypeFilterMember + "(ITypeResolutionService, IEnumerable<Type>) not found on " + property.ComponentType.FullName);
						}
						else if (!typeof(IEnumerable<Type>).IsAssignableFrom(method.ReturnType))
						{
							throw new ArgumentException("TypeFilterMember method '" + attribute.TypeFilterMember + "(ITypeResolutionService, IEnumerable) must have return type IEnumerable<Type>");
						}
					}

					if (!string.IsNullOrEmpty(attribute.ImportNamespacesMember))
					{
						var method = property.ComponentType.GetMethod(attribute.ImportNamespacesMember, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Array.Empty<Type>(), null);
						if (method == null)
						{
							throw new ArgumentException("ImportNamespacesMember method 'IEnumerable<string> " + attribute.TypeFilterMember + "() not found on " + property.ComponentType.FullName);
						}
						else if (!typeof(IEnumerable<string>).IsAssignableFrom(method.ReturnType))
						{
							throw new ArgumentException("ImportNamespacesMember method '" + attribute.ImportNamespacesMember + "() must have return type IEnumerable<string>");
						}
					}
				}
			}
		}
	}
}
