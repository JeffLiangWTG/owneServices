#if DEBUG
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	internal static class TypeDescriptionProviderAttributeChecker
	{
		static readonly Dictionary<Type, bool> checkedTypes = new Dictionary<Type, bool>();

		/// <summary>
		/// Checks if <see cref="TypeDescriptionProviderAttribute"/> is appled to a type.
		/// </summary>
		/// <param name="type">Type to check</param>
		/// <returns>
		///  <c>true</c> if attribute is applied, <c>false</c> otherwise.
		/// </returns>
		public static bool IsAppliedTo(Type type)
		{
			bool result;
			if (checkedTypes.TryGetValue(type, out result))
			{
				return result;
			}

			result = false;

			var attributes = type.GetCustomAttributes(typeof(TypeDescriptionProviderAttribute), true);
			if (attributes.Length != 0)
			{
				var attr =
							(TypeDescriptionProviderAttribute)attributes[0];

				if (Type.GetType(attr.TypeName) == typeof(Internal.ZControlTypeDescriptionProvider))
				{
					result = true;
				}
			}

			checkedTypes[type] = result;

			return result;
		}
	}
}
#endif
