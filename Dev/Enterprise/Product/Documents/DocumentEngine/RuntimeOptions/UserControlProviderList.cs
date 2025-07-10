using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class UserControlProviderList : NonPersistentBusinessObjectCollection<FilterField>
	{
		public UserControlProviderList()
		{
		}

		public UserControlProviderList(UserControlProviderList systemDefinedFields, UserControlProviderList userDefinedFields)
			: base()
		{
			if (systemDefinedFields != null)
			{
				AddRange(systemDefinedFields);
			}

			if (userDefinedFields != null)
			{
				foreach (FilterField userDefinedField in userDefinedFields)
				{
					if (!ContainsName(userDefinedField.DisplayName))
					{
						Add(userDefinedField);
					}
				}
			}
		}

		public UserControlProviderList(UserControlProviderList value)
		{
			AddRange(value);
		}

		public void AddRange(FilterField[] value)
		{
			for (int i = 0; (i < value.Length); i = (i + 1))
			{
				Add(value[i]);
			}
		}

		public void AddRange(UserControlProviderList value)
		{
			for (int i = 0; (i < value.Count); i = (i + 1))
			{
				Add(value[i]);
			}
		}

		public UserControlProviderList(params FilterField[] value)
		{
			foreach (FilterField field in value)
			{
				Add(field);
			}
		}

		public bool ContainsName(string fieldName)
		{
			return FieldLookupTable.ContainsKey(fieldName);
		}

		public void Add(FilterField value)
		{
			base.Add(value);
			FieldLookupTable.Add(value.DisplayName, value);
		}

		public FilterField this[string displayName]
		{
			get
			{
				if (FieldLookupTable.ContainsKey(displayName))
				{
					return FieldLookupTable[displayName];
				}
				else
				{
					return null;
				}
			}
		}

		internal void ClearFieldValues()
		{
			foreach (FilterField field in this)
			{
				field.ClearValues();
			}
		}

		#region Implementation

		readonly Dictionary<string, FilterField> FieldLookupTable = new Dictionary<string, FilterField>();

		#endregion

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TextField(Factory);
		}
	}
}
