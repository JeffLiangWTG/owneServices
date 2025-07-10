using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public sealed class OptionGroup : FilterFieldValueSerialisable, IDescriptionPairListSupportField, IJsonSerializable
	{
		public OptionGroup(BusinessObjectFactory factory)
			: base(factory)
		{
			Initialise();
		}

		void Initialise()
		{
			descriptionCodePairList = new ZBoolDescriptionPairList();
			descriptionToCodeMapper = new Dictionary<string, string>();
			sqlParams = new Dictionary<string, SqlParameter>();
			descriptionCodePairList.OnPairChanged += new ZBoolDescriptionPairChangedEventHandler(DescriptionCodePairList_OnChanged);
		}

		#region Constructor For IJsonSerializable

		internal OptionGroup(OptionGroupJsonData data)
			: base(data)
		{
			Initialise();

			for (var i = 0; i < data.Options.Count; i++)
			{
				AddOption(data.Options[i].Description, data.Options[i].Code, data.Options[i].Value);
			}
		}

		#endregion

		protected override void SetNecessaryPropertiesForDeserializingCore(FilterField origin, CollectionOfIFilter filterCollection)
		{
			base.SetNecessaryPropertiesForDeserializingCore(origin, filterCollection);

			if (origin is OptionGroup optionGroupField)
			{
				UpdateOptions(optionGroupField);
			}
		}

		internal void UpdateOptions(OptionGroup optionGroupField)
		{
			var codeMapForDeserialisedField = new Dictionary<string, Item>();
			var newOptions = new List<Item>();
			var hasDuplicatedCode = false;
			foreach (var pair in DescriptionCodePairList)
			{
				var code = GetCodeFromDescription(pair.Description);
				if (code == null)
				{
					continue;
				}

				if (codeMapForDeserialisedField.ContainsKey(code))
				{
					/*
					 *	In a previous version, the options with duplicated code could be saved in the database. This is unexpected.
					 *	Therefore, the below logic will correct the options that with duplicated code.
					 *  WI00719648, WI00699755 will supply more information.
					 */

					hasDuplicatedCode = true;
					if (codeMapForDeserialisedField[code].Desc != pair.Description)
					{
						var newCode = optionGroupField.GetCodeFromDescription(pair.Description);
						if (newCode != null && newCode != code)
						{
							codeMapForDeserialisedField.Add(newCode, new Item() { Code = newCode, Desc = pair.Description, Value = pair.Value });
						}
					}
					else if (pair.Value)
					{
						codeMapForDeserialisedField[code].Value = pair.Value;
					}
				}
				else
				{
					codeMapForDeserialisedField.Add(code, new Item() { Code = code, Desc = pair.Description, Value = pair.Value });
				}
			}

			foreach (var pair in optionGroupField.DescriptionCodePairList)
			{
				var code = optionGroupField.GetCodeFromDescription(pair.Description);
				if (!codeMapForDeserialisedField.ContainsKey(code))
				{
					newOptions.Add(new Item() { Code = code, Desc = pair.Description, Value = pair.Value });
				}
			}

			if (newOptions.Count > 0 || hasDuplicatedCode)
			{
				newOptions.AddRange(codeMapForDeserialisedField.Select(e => e.Value).ToList());
				UpdateBindableBooleanItems(newOptions);
			}
		}

		public override bool IsEmpty
		{
			get
			{
				return string.IsNullOrEmpty(NonEmptyWhereClause());
			}
		}

		#region List<IBindableBooleanItem>

		public List<IBindableBooleanItem> BindableBooleanItems
		{
			get
			{
				if (optionGroupItemList == null)
				{
					optionGroupItemList = new List<IBindableBooleanItem>();

					foreach (ZBoolDescriptionPair pair in DescriptionCodePairList)
					{
						optionGroupItemList.Add(new OptionGroupBindableBooleanItem(pair));
					}
				}
				return optionGroupItemList;
			}
		}
		List<IBindableBooleanItem> optionGroupItemList;

		#endregion

		#region DescriptionCodePairList

		public ZBoolDescriptionPairList DescriptionCodePairList
		{
			get { return descriptionCodePairList; }
		}

		ZBoolDescriptionPairList descriptionCodePairList;
		Dictionary<string, string> descriptionToCodeMapper;
		Dictionary<string, SqlParameter> sqlParams;

		#endregion

		protected override string NonEmptyWhereClause()
		{
			ArrayList childWhereClauses = new ArrayList();

			foreach (ZBoolDescriptionPair pair in DescriptionCodePairList)
			{
				var whereClause = pair.Value ? FieldName + " = " + sqlParams[pair.Description].ParameterName : "";
				if (!string.IsNullOrEmpty(whereClause))
				{
					childWhereClauses.Add("(" + whereClause + ")");
				}
			}

			return string.Join(" OR ", (string[])childWhereClauses.ToArray(typeof(string)));
		}

		protected override SqlParameterList GetSqlParametersCore()
		{
			SqlParameterList parameters = new SqlParameterList();
			foreach (string key in sqlParams.Keys)
			{
				if (DescriptionCodePairList[key].Value)
				{
					parameters.Add(sqlParams[key]);
				}
			}
			return parameters;
		}

		internal void AddAllOptions(List<Item> list)
		{
			if (DescriptionCodePairList.Count > 0)
			{
				throw new InvalidOperationException("Can only call AddAllOptions once");
			}

			// Dictionary of descriptions in use.
			// Duplicate descriptions will be modified by appending the code to make them unique.
			// The value will be the Item with that original, unmodified description if only one Item has been found.
			// The value will become null once other Items with the same description are found.
			Dictionary<string, Item> descItem = new Dictionary<string, Item>(list.Count);
			Dictionary<string, Item> codeItem = new Dictionary<string, Item>(list.Count);

			for (int i = 0; i < list.Count; ++i)
			{
				var item = list[i];
				if (codeItem.ContainsKey(item.Code))
				{
					// duplicate code - discard
					list[i] = null;
				}
				else
				{
					codeItem.Add(item.Code, item);

					Item duplicate;
					if (descItem.TryGetValue(item.Desc, out duplicate))
					{
						if (duplicate != null)
						{
							descItem[item.Desc] = null;
							AppendCode(descItem, duplicate);
						}
						AppendCode(descItem, item);
					}
					else
					{
						descItem.Add(item.Desc, item);
					}
				}
			}

			foreach (var item in list)
			{
				if (item != null)
				{
					AddOption(item.Desc, item.Code, item.Value);
				}
			}
		}

		static void AppendCode(Dictionary<string, Item> descItem, Item item)
		{
			do
			{
				item.Desc = item.Desc + " (" + item.Code + ')';
				Item duplicate;
				if (!descItem.TryGetValue(item.Desc, out duplicate))
				{
					descItem.Add(item.Desc, null);
				}

				item = duplicate;
			} while (item != null);
		}

		internal void AddAllOptions(ICodeDescriptionPairList list)
		{
			var options = new List<Item>(list.Count);
			foreach (ICodeDescription pair in list)
			{
				options.Add(new Item() { Code = pair.Code, Desc = pair.Description, Value = false });
			}
			AddAllOptions(options);
		}

		internal string GetCodeFromDescription(string description)
		{
			return descriptionToCodeMapper.ContainsKey(description) ? descriptionToCodeMapper[description] : null;
		}

		internal void UpdateBindableBooleanItems(List<Item> list)
		{
			Initialise();
			AddAllOptions(list);
		}

		internal class Item
		{
			public string Code;
			public string Desc;
			public bool Value;
		}

#if DEBUG
		public
#endif
		void AddOption(string displayName, string code, bool value = false)
		{
			ZBoolDescriptionPair item = new ZBoolDescriptionPair(displayName, new ZBool(value));
			item.PK = ZGuid.NewZGuid();

			DescriptionCodePairList.Add(item);

			if (!descriptionToCodeMapper.ContainsKey(displayName))
			{
				descriptionToCodeMapper.Add(displayName, code);
			}

			var hasAddedParam = false;
			try
			{
				var tablePrefix = CargoWise.Schema.Schema.GetPrefixFromColumnName(FieldName);
				var tableSchema = EnterpriseSchema.GetTableSchemaFromColumnNamePrefix(tablePrefix);
				if (tableSchema != null)
				{
					var column = tableSchema.GetSchemaColumn(FieldName);
					if (column != null)
					{
						var sqlParam = new SqlParameter(SqlParameterNameGenerator.Next(), column.SqlDbType);
						if (column.SqlDbType == System.Data.SqlDbType.Bit)
						{
							sqlParam.Value = code == "Y" || code == "1";
						}
						else
						{
							sqlParam.Value = code;
						}
						sqlParams.Add(displayName, sqlParam);
						hasAddedParam = true;
					}
				}
			}
			catch (Exception)
			{
				// keeps old behaviour safely
			}
			if (!hasAddedParam)
			{
				sqlParams.Add(displayName, new SqlParameter(SqlParameterNameGenerator.Next(), code));
			}
		}

		public override FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get
			{
				return FilterFieldSuggestedUserControlType.OptionGroupUserControl;
			}
		}

		protected override bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField)
		{
			if (otherFilterField is OptionGroup)
			{
				string values = otherFilterField.ValueAsObject.ToString();
				if (values.Length > 0)
				{
					string[] selectedItems = values.Split(new char[] { ',' });
					foreach (string item in selectedItems)
					{
						if (!this.descriptionToCodeMapper.ContainsValue(item.Trim()))
						{
							return false;
						}
					}
				}
				return true;
			}
			return false;
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			foreach (ZBoolDescriptionPair pair in DescriptionCodePairList)
			{
				pair.Value = true;
			}
		}

		public override void ClearValueForUnitTest()
		{
			foreach (ZBoolDescriptionPair pair in DescriptionCodePairList)
			{
				pair.Value = false;
			}
		}
#endif

		public override object ValueAsObject
		{
			get
			{
				return ValueAsStringForSerialisation;
			}
		}

		void DescriptionCodePairList_OnChanged(ZBoolDescriptionPairChangedEventArgs e)
		{
			SetHasChangesBecauseTheValueHasBeenSetByTheUser();
		}

		#region ValueProviders
		protected override void AddSpecialisedValueProviders()
		{
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".CommaSeperatedValues", new ValueReplacers.ReplacementProviderMethod(GetCommaSeperatedValues)));
		}

		protected override void AddSpecialisedValueProviderDocumenters()
		{
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.CommaSeperatedValues>", ResString.GetMultilingualString("9fd758b4-03d7-4abf-bdb7-c7572e443597", "Returns the selected Descriptions as comma separated list.")));
		}

		object GetCommaSeperatedValues(string macro, Report report)
		{
			ArrayList selectedOptions = new ArrayList();

			foreach (ZBoolDescriptionPair pair in DescriptionCodePairList)
			{
				if (pair.Value)
				{
					selectedOptions.Add("\"" + descriptionToCodeMapper[pair.Description] + "\"");
				}
			}

			return String.Join(" , ", (string[])selectedOptions.ToArray(typeof(string)));
		}

		#endregion

		public bool IsRadioButton;

		#region IFilter Members

		public override void SafeCopyValuesFrom(IFilter source)
		{
			if (source is OptionGroup)
			{
				foreach (ZBoolDescriptionPair pair in ((OptionGroup)source).DescriptionCodePairList)
				{
					ZBoolDescriptionPair destinationPair = this.DescriptionCodePairList[pair.Description];
					if (destinationPair != null)
					{
						destinationPair.Value = pair.Value;
					}
				}
			}
		}

		public override void ClearValues()
		{
			foreach (ZBoolDescriptionPair pair in this.DescriptionCodePairList)
			{
				pair.Value = false;
			}
		}

		#endregion

		#region IDescriptionPairListSupportField implement

		List<string> IDescriptionPairListSupportField.DescriptionList
		{
			get
			{
				var result = new List<string>();
				if (DescriptionCodePairList != null)
				{
					foreach (var item in DescriptionCodePairList)
					{
						result.Add(item.Description);
					}
				}
				return result;
			}
		}

		#endregion

		protected override string ValueAsStringForSerialisationInternal
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				foreach (ZBoolDescriptionPair decriptionCodePair in DescriptionCodePairList)
				{
					if (decriptionCodePair.Value)
					{
						result.Append(descriptionToCodeMapper[decriptionCodePair.Description.ToString()]);
					}
				}
				return result.ToStringWithDelimiterBetweenAppends(", ");
			}
			set
			{
				if (value.Length > 0)
				{
					string[] selectedItems = value.Split(new char[] { ',' });
					foreach (string item in selectedItems)
					{
						foreach (ZBoolDescriptionPair pair in DescriptionCodePairList)
						{
							if (item.Trim() == descriptionToCodeMapper[pair.Description.ToString()])
							{
								pair.Value = true;
							}
						}
					}
				}
			}
		}

		protected override void LoadValueIfNotOverriddenByUser(Report reportContainingDataSource)
		{
			// Never Load a default value. BG: Don't know why. Investigate later.
		}

		public override void FillFilterData(ReportFilterData reportFilterData)
		{
			var filterData = new OptionGroupFilter();
			SetBaseFilterData(filterData);

			DescriptionCodePairList.OfType<ZBoolDescriptionPair>().Select(z => new BoolDescription { Description = z.Description, Value = z.Value }).ForEach(filterData.Options.Add);

			reportFilterData.OptionGroupFilterCollection.Add(filterData);
		}

		public override void SetFilterValue(ReportFilterData reportFilterData)
		{
			var selectedValue = reportFilterData.OptionGroupFilterCollection.FirstOrDefault(a => a.DisplayName == DisplayName);
			if (selectedValue != null)
			{
				selectedValue.Options.ForEach(o =>
				{
					DescriptionCodePairList.FirstOrDefault(d => d.Description == o.Description).Value = o.Value;
				});
			}
		}

		#region IJsonSerializable Members

		public object GetJsonData()
		{
			var filterData = new OptionGroupJsonData();
			SetJsonData(filterData);
			DescriptionCodePairList.OfType<ZBoolDescriptionPair>().Select(z => new BoolCodeDescription { Code = descriptionToCodeMapper[z.Description], Description = z.Description, Value = z.Value }).ForEach(filterData.Options.Add);
			return filterData;
		}

		#endregion
	}
}
