using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	[Serializable]
	public class OptionalTemplateSheetCollection : NonPersistentBusinessObject, IEnumerable<IBindableBooleanItem>, IObsoleteValidation, IJsonSerializable
	{
		#region Constructor For IJsonSerializable

		internal OptionalTemplateSheetCollection(OptionalTemplateSheetCollectionJsonData data)
		{
			sheets = new Hashtable();
			if (data?.Sheets != null)
			{
				foreach (var item in data.Sheets)
				{
					sheets.Add(item.Name, new OptionalTemplateSheet(item));
				}
			}
		}

		#endregion

		internal OptionalTemplateSheetCollection(BusinessObjectFactory factory, ValidatorPack validators) : base(factory)
		{
			Validator = validators.GetAtLeastOneFilterNotEmptyValidatorForGroup(ValidatorGroupName);
		}

		public void Add(string sheetName)
		{
			if (!sheets.Contains(sheetName))
			{
				var sheet = new OptionalTemplateSheet(Factory, sheetName, Count);
				RegisterEditableChildObject(sheet);
				sheet.Validators.Add(Validator);
				Validator.AddFilterField(sheet);
				sheets.Add(sheetName, sheet);
			}
		}
		Hashtable sheets = new Hashtable();

		public string ValidatorGroupName => (NoResString)"Optional Templates";

		public new OptionalTemplateSheet this[string sheetName]
		{
			get
			{
				return (OptionalTemplateSheet)sheets[sheetName];
			}
			internal set
			{
				sheets[sheetName] = value;
				Validator.AddFilterField(value);
				value.Validators.Add(Validator);
			}
		}

		public bool Contains(string sheetName)
		{
			return sheets.Contains(sheetName);
		}

		IEnumerator<IBindableBooleanItem> IEnumerable<IBindableBooleanItem>.GetEnumerator()
		{
			foreach (OptionalTemplateSheet element in sheets.Values)
			{
				yield return element;
			}
		}

		public void Clear()
		{
			sheets = new Hashtable();
		}

		public int Count => sheets.Count;

		[NonSerialized]
		readonly AtLeastOneFilterNotEmptyValidator Validator;

		#region IJsonSerializable Members

		public object GetJsonData() =>
			new OptionalTemplateSheetCollectionJsonData
			{
				Sheets = sheets.Values.OfType<IJsonSerializable>()
					.Select(i => (OptionalTemplateSheetJsonData)i.GetJsonData()).ToList()
			};

		#endregion
	}
}
