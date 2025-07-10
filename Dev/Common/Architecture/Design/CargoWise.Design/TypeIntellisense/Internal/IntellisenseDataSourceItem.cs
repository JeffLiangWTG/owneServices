using System;
using System.Diagnostics;

namespace CargoWise.Design.TypeIntellisense
{
	/// <summary>
	/// An intellisense item.
	/// </summary>
	[DebuggerDisplay("DisplayName={DisplayName},CloseMatch={CloseMatch}")]
	internal class IntellisenseDataSourceItem
	{
		public IntellisenseDataSourceItem(string displayName, string nameToReplaceWith, IntellisenseItemType type)
		{
			if (displayName == null)
			{
				throw new ArgumentNullException(nameof(displayName));
			}

			DisplayName = displayName;
			NameToReplaceWith = nameToReplaceWith;
			Type = type;
		}

		public IntellisenseDataSourceItem(string displayName, string nameToReplaceWith, Type typeToGetTypeFrom, bool isNamespace)
		{
			if (displayName == null)
			{
				throw new ArgumentNullException(nameof(displayName));
			}

			DisplayName = displayName;
			NameToReplaceWith = nameToReplaceWith;
			Type = isNamespace ? IntellisenseItemType.Namespace : GetIntellisenseItemTypeFromType(typeToGetTypeFrom);
		}

		/// <summary>
		/// Get the name of the user-selectable item.
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// Get the name to replace the selection.
		/// </summary>
		public string NameToReplaceWith { get; private set; }

		/// <summary>
		/// Get the type of the intellisense item type.
		/// </summary>
		public IntellisenseItemType Type { get; private set; }

		public override bool Equals(object obj)
		{
			return
				obj is IntellisenseDataSourceItem rhs &&
				DisplayName == rhs.DisplayName;
		}

		public override int GetHashCode()
		{ return DisplayName.GetHashCode(); }

		public override string ToString()
		{ return GetType().Name + ": " + DisplayName; }

		#region Implementation

		static IntellisenseItemType GetIntellisenseItemTypeFromType(Type type)
		{
			IntellisenseItemType result = IntellisenseItemType.Class;
			if (type.IsInterface)
			{
				result = IntellisenseItemType.Interface;
			}
			else if (type.IsEnum)
			{
				result = IntellisenseItemType.Enum;
			}
			else if (type.IsValueType)
			{
				result = IntellisenseItemType.Struct;
			}
			else if (typeof(Delegate).IsAssignableFrom(type))
			{
				result = IntellisenseItemType.Delegate;
			}

			return result;
		}

		#endregion
	}
}
