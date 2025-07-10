using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace CargoWise.Common
{
	/// <summary>
	/// Various reflection related methods that are missing from the standard library.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Util")]
	public static class ReflectionUtil
	{
		/// <summary>
		/// Convenience <see cref="BindingFlags"/> value that will
		/// match all private and public, static and instance members on a class
		/// in a case inSenSItivE fashion.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags")]
		public const BindingFlags AllMembersCaseInsensitiveFlags =
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
			BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase;

		/// <summary>
		/// Convenience <see cref="BindingFlags"/> value that will
		/// match all private and public, static and instance members on a class
		/// throught inheritance hierarchy in a case sensitive fashion.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags")]
		public const BindingFlags DefaultBindingFlags =
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
			BindingFlags.Static | BindingFlags.FlattenHierarchy;

		/// <summary>
		/// Gets the field value.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <param name="fieldName">Name of the field.</param>
		/// <returns>Value stored in field</returns>
		/// <exception cref="MissingFieldException">If field with specified <paramref name="fieldName"/> can not be found in type declaration</exception>
		/// <exception cref="ArgumentNullException"><paramref name="obj"/> is null</exception>
		/// <exception cref="ArgumentNullException"><paramref name="fieldName"/> is null or empty string</exception>
		[SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj")]
		public static object GetFieldValue(object obj, string fieldName)
		{
			Argument.NotNull(obj, nameof(obj));
			Argument.NotNullOrEmpty(fieldName, nameof(fieldName));

			FieldInfo field = GetField(obj.GetType(), fieldName)
				?? throw new MissingFieldException(obj.GetType().FullName, fieldName);

			return field.GetValue(obj);
		}

		/// <summary>
		/// Sets the field value.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <param name="fieldName">Name of the field.</param>
		/// <param name="value">The value to put in the field.</param>
		/// <exception cref="MissingFieldException">If field with specified <paramref name="fieldName"/> can not be found in type declaration</exception>
		/// <exception cref="ArgumentNullException"><paramref name="obj"/> is null</exception>
		/// <exception cref="ArgumentNullException"><paramref name="fieldName"/> is null or empty string</exception>
		[SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj")]
		public static void SetFieldValue(object obj, string fieldName, object value)
		{
			Argument.NotNull(obj, nameof(obj));
			Argument.NotNullOrEmpty(fieldName, nameof(fieldName));

			FieldInfo field = GetField(obj.GetType(), fieldName)
				?? throw new MissingFieldException(obj.GetType().FullName, fieldName);

			field.SetValue(obj, value);
		}

		/// <summary>
		/// Gets the property value.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns>Value returned from property call</returns>
		/// <exception cref="MissingMemberException">If property with specified <paramref name="propertyName"/> can not be found in type declaration</exception>
		/// <exception cref="ArgumentNullException"><paramref name="obj"/> is null</exception>
		/// <exception cref="ArgumentNullException"><paramref name="propertyName"/> is null or empty string</exception>
		[SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj")]
		public static object GetPropertyValue(object obj, string propertyName)
		{
			Argument.NotNull(obj, nameof(obj));
			Argument.NotNullOrEmpty(propertyName, nameof(propertyName));

			PropertyInfo property = GetProperty(obj.GetType(), propertyName)
				?? throw new MissingMemberException(obj.GetType().FullName, propertyName);

			return property.GetValue(obj, null);
		}

		/// <summary>
		/// Sets the property value.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="value">The value to put in the property.</param>
		/// <exception cref="MissingMemberException">If property with specified <paramref name="propertyName"/> can not be found in type declaration</exception>
		/// <exception cref="ArgumentNullException"><paramref name="obj"/> is null</exception>
		/// <exception cref="ArgumentNullException"><paramref name="propertyName"/> is null or empty string</exception>
		[SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj")]
		public static void SetPropertyValue(object obj, string propertyName, object value)
		{
			Argument.NotNull(obj, nameof(obj));
			Argument.NotNullOrEmpty(propertyName, nameof(propertyName));

			PropertyInfo property = GetProperty(obj.GetType(), propertyName)
				?? throw new MissingMemberException(obj.GetType().FullName, propertyName);

			property.SetValue(obj, value, null);
		}

		/// <summary>
		/// Finds the field by it's name.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="fieldName">Name of the field.</param>
		/// <returns>
		/// An instance of <see cref="FieldInfo"/> class
		/// or <c>null</c> if field wasn't found
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is null</exception>
		/// <exception cref="ArgumentException"><paramref name="fieldName"/> is null or empty string</exception>
		public static FieldInfo GetField(Type type, string fieldName)
		{
			Argument.NotNull(type, nameof(type));
			Argument.NotNullOrEmpty(fieldName, nameof(fieldName));

			foreach (FieldInfo field in GetFields(type))
			{
				if (field != null && field.Name == fieldName)
				{
					return field;
				}
			}

			return null;
		}

		/// <summary>
		/// Finds the property by it's name.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns>
		/// An instance of <see cref="PropertyInfo"/> class
		/// or <c>null</c> if property wasn't found
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is null</exception>
		/// <exception cref="ArgumentNullException"><paramref name="propertyName"/> is null or empty string</exception>
		public static PropertyInfo GetProperty(Type type, string propertyName)
		{
			Argument.NotNull(type, nameof(type));
			Argument.NotNullOrEmpty(propertyName, nameof(propertyName));

			foreach (PropertyInfo property in type.GetProperties(DefaultBindingFlags))
			{
				if (property != null && property.Name == propertyName)
				{
					return property;
				}
			}

			return null;
		}

		/// <summary>
		/// Returns all fields declared in a type throught inheritance hierarchy.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>The list of <see cref="FieldInfo"/> class instances.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is null</exception>
		public static FieldInfo[] GetFields(Type type)
		{
			Argument.NotNull(type, nameof(type));

			List<FieldInfo> result = new List<FieldInfo>();
			GetFieldsUpToBase(result, type);

			return result.ToArray();
		}

		/// <summary>
		/// Finds the specified attribute on type declaration.
		/// </summary>
		/// <typeparam name="T">Type of attribute</typeparam>
		/// <param name="obj">The object to use.</param>
		/// <returns>An instance of attribute or <c>null</c> if attribute is not specified</returns>
		/// <exception cref="ArgumentNullException"><paramref name="obj"/> is null</exception>
		[SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj")]
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public static T GetAttribute<T>(object obj) where T : Attribute
		{
			Argument.NotNull(obj, nameof(obj));
			return GetAttribute<T>(obj.GetType());
		}

		/// <summary>
		/// Finds the specified attribute on type declaration.
		/// </summary>
		/// <typeparam name="T">Type of attribute</typeparam>
		/// <param name="type">The type to use.</param>
		/// <returns>An instance of attribute or <c>null</c> if attribute is not specified</returns>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is null</exception>
		[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public static T GetAttribute<T>(Type type) where T : Attribute
		{
			Argument.NotNull(type, nameof(type));
			var customAttributes = Attribute.GetCustomAttributes(type);
			if (customAttributes != null)
			{
				foreach (Attribute attribute in customAttributes)
				{
					if (attribute != null && attribute.GetType() == typeof(T))
					{
						return (T)attribute;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Finds the specified attribute on member declaration.
		/// </summary>
		/// <typeparam name="T">Type of attribute</typeparam>
		/// <param name="info">The member to use.</param>
		/// <returns>An instance of attribute or <c>null</c> if attribute is not specified</returns>
		/// <exception cref="ArgumentNullException"><paramref name="info"/> is null</exception>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public static T GetAttribute<T>(MemberInfo info) where T : Attribute
		{
			Argument.NotNull(info, nameof(info));
			var customAttributes = Attribute.GetCustomAttributes(info);
			if (customAttributes != null)
			{
				foreach (Attribute attr in customAttributes)
				{
					if (attr != null && attr.GetType() == typeof(T))
					{
						return (T)attr;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Invokes the method with given name on the given object.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <param name="methodName">Name of the method.</param>
		/// <exception cref="AmbiguousMatchException">More than one method is found with the specified name and matching the specified binding constraints</exception>
		/// <exception cref="MissingMethodException">If method with specified <paramref name="methodName"/> can not be found in type declaration</exception>
		/// <exception cref="ArgumentNullException"><paramref name="obj"/> is null</exception>
		/// <exception cref="ArgumentNullException"><paramref name="methodName"/> is null or empty string</exception>
		[SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj")]
		public static void InvokeMethod(object obj, string methodName)
		{
			Argument.NotNull(obj, nameof(obj));
			Argument.NotNull(methodName, nameof(methodName));
			InvokeMethod(obj, methodName, Array.Empty<object>());
		}

		/// <summary>
		/// Invokes the method with given name on the given object using spefied argument as parameter.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <param name="methodName">Name of the method.</param>
		/// <param name="arg">The parameter value to pass.</param>
		/// <exception cref="AmbiguousMatchException">More than one method is found with the specified name and matching the specified binding constraints</exception>
		/// <exception cref="MissingMethodException">If method with specified <paramref name="methodName"/> can not be found in type declaration</exception>
		/// <exception cref="ArgumentNullException"><paramref name="obj"/> is null</exception>
		/// <exception cref="ArgumentNullException"><paramref name="methodName"/> is null or empty string</exception>
		[SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj")]
		public static void InvokeMethod(object obj, string methodName, object arg)
		{
			Argument.NotNull(obj, nameof(obj));
			Argument.NotNull(methodName, nameof(methodName));
			InvokeMethod(obj, methodName, new object[] { arg });
		}

		/// <summary>
		/// Invokes the method with given name on the given object using spefied arguments as parameters.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <param name="methodName">Name of the method.</param>
		/// <param name="args">The parameters values to pass.</param>
		/// <exception cref="AmbiguousMatchException">More than one method is found with the specified name and matching the specified binding constraints</exception>
		/// <exception cref="MissingMethodException">If method with specified <paramref name="methodName"/> can not be found in type declaration</exception>
		/// <exception cref="ArgumentNullException"><paramref name="obj"/> is null</exception>
		/// <exception cref="ArgumentNullException"><paramref name="methodName"/> is null or empty string</exception>
		[SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj")]
		public static void InvokeMethod(object obj, string methodName, object[] args)
		{
			Argument.NotNull(obj, nameof(obj));
			Argument.NotNull(methodName, nameof(methodName));

			MethodInfo method = obj.GetType().GetMethod(methodName, DefaultBindingFlags)
				?? throw new MissingMethodException(obj.GetType().FullName, methodName);

			method.Invoke(obj, args);
		}

		/// <summary>
		/// Determines whether the specified object implements given interface.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <param name="interfaceType">Type of the interface.</param>
		/// <returns>
		/// 	<c>true</c> if the specified object implements interface; otherwise, <c>false</c>.
		/// </returns>
		[SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj")]
		public static bool HasInterface(object obj, Type interfaceType)
		{
			Argument.NotNull(obj, nameof(obj));
			Argument.NotNull(interfaceType, nameof(interfaceType));
			return HasInterface(obj.GetType(), interfaceType);
		}

		/// <summary>
		/// Determines whether the specified type implements given interface type.
		/// </summary>
		/// <param name="objectType">The object type.</param>
		/// <param name="interfaceType">Type of the interface.</param>
		/// <returns>
		/// 	<c>true</c> if the specified type implements interface type; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="objectType"/> is null</exception>
		/// <exception cref="ArgumentNullException"><paramref name="interfaceType"/> is null</exception>
		public static bool HasInterface(Type objectType, Type interfaceType)
		{
			Argument.NotNull(objectType, nameof(objectType));
			Argument.NotNull(interfaceType, nameof(interfaceType));

			Type[] interfaces = objectType.GetInterfaces();

			Type found = Array.Find(interfaces, delegate(Type type)
			{
				return type == interfaceType;
			});

			return found != null;
		}

		/// <summary>
		/// Get a type given the type name. If the type is nested, '.' may be substituted for '+'.
		/// </summary>
		public static Type GetTypeFromAssemblyFixupDotsBetweenNestedType(Assembly assembly, string typeName)
		{
			Argument.NotNull(assembly, nameof(assembly)); // Suggested By ReviewBot
			Argument.NotNull(typeName, nameof(typeName));
			Type result;
			do
			{
				result = assembly.GetType(typeName);
				int lastDotIndex = typeName.LastIndexOf(".", StringComparison.Ordinal);
				if (lastDotIndex == -1)
				{
					break;
				}
				typeName = typeName.Substring(0, lastDotIndex) + "+" + typeName.Substring(lastDotIndex + 1);
			}
			while (result == null);
			return result;
		}

		/// <summary>
		/// Get whether the given type name is most likely not in the System assembly.
		/// </summary>
		public static bool IsMaybeNonSystemAssemblyName(string fullName)
		{
			Argument.NotNull(fullName, nameof(fullName)); // Suggested By ReviewBot
			return !fullName.StartsWith("mscorlib", StringComparison.Ordinal) && // standard assembly name
					 !fullName.StartsWith("System.", StringComparison.Ordinal) && // standard assembly name
					 !fullName.StartsWith("System,", StringComparison.Ordinal) && // standard assembly name
					 !(fullName == "System"); // standard assembly name
		}

		#region Support

		static void GetFieldsUpToBase(List<FieldInfo> container, Type type)
		{
			Argument.NotNull(type, nameof(type)); // Suggested By ReviewBot
			Argument.NotNull(container, nameof(container));
			FieldInfo[] fields = type.GetFields(DefaultBindingFlags);

			foreach (FieldInfo field in fields)
			{
				// FIX: ignore event fields
				if (field != null && field.FieldType.BaseType != typeof(MulticastDelegate))
				{
					container.Add(field);
				}
			}

			if (type.BaseType != typeof(object) && type.BaseType != null)
			{
				GetFieldsUpToBase(container, type.BaseType);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		internal class InternalClass
		{
			[EditorBrowsable(EditorBrowsableState.Never)]
			public class NestedPublic { }
		}

		#endregion
	}
}
