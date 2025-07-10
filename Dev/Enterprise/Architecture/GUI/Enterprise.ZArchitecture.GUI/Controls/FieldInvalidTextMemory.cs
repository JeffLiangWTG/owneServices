using System;
using System.Collections.Generic;

using CargoWise.Common.Collections;

namespace Enterprise.ZArchitecture.GUI
{
	public static class FieldInvalidTextMemory
	{
		public static string GetInvalidText(object obj, string propertyName)
		{
			var dictionary = Memory[obj];
			string result = null;
			if (dictionary != null)
			{
				dictionary.TryGetValue(propertyName, out result);
			}
			return result ?? "";
		}

		public static void SetInvalidText(object obj, string propertyName, string invalidText)
		{
			var dictionary = Memory[obj];
			if (dictionary == null && !string.IsNullOrEmpty(invalidText))
			{
				dictionary = new Dictionary<string, string>();
				Memory[obj] = dictionary;
			}
			if (dictionary != null)
			{
				if (string.IsNullOrEmpty(invalidText))
				{
					dictionary.Remove(propertyName);
				}
				else
				{
					dictionary[propertyName] = invalidText;
				}
				if (dictionary.Count == 0)
				{
					Memory.Remove(obj);
				}
			}
		}

		static WeakReferencedKeyDictionary<object, Dictionary<string, string>> Memory
		{
			get { return memory ?? (memory = new WeakReferencedKeyDictionary<object, Dictionary<string, string>>()); }
		}
		[ThreadStatic]
		static WeakReferencedKeyDictionary<object, Dictionary<string, string>> memory;
	}
}
