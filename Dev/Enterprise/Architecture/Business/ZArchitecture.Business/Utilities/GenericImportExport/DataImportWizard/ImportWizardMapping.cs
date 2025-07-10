using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.ZArchitecture.DataMapping.ImportExportWizard;
using Res = Enterprise.ZArchitecture.Business.Res;
using ResString = Enterprise.ZArchitecture.Business.ResString;

namespace Enterprise.ZArchitecture.DataMapping
{
	public class ImportWizardMapping : ImportExportMapping<ImportWizardMapping, ImportWizard>
	{
		public ImportWizardMapping(IImportPropertyInfo property, ImportWizardMappingCollection parentCollection)
			: base(property, parentCollection)
		{
		}

		#region Properties

		#region FileColumnIndexOrder

		public List<int> FileColumnIndexOrder { get; set; } = new List<int>();

		int lastAdded = -1;
		public void AddFileColumnIndex(int index)
		{
			if (lastAdded != index)
			{
				lastAdded = index;
				if (FileColumnIndexOrder.Contains(index))
				{
					FileColumnIndexOrder.Remove(index);
				}
				FileColumnIndexOrder.Add(index);
				if (!IsValidationSuspended)
				{
					ValidateMapping();
				}
				if (!Delimiter_ReadOnly && Delimiter.IsEmpty)
				{
					Delimiter = SupportedDelimiter.Space;
				}
			}
		}

		public void ClearAllFileColumnIndex()
		{
			FileColumnIndexOrder.Clear();
			delimiter = ZString.Empty;
			lastAdded = -1;
		}

		#endregion

		#region MapAs

		protected override void OnMapAsChanged()
		{
			ValidateMapAs();
		}

		bool HasParentOfType(Type curr, Type parent)
		{
			if (curr.Name == parent.Name)
			{
				return true;
			}

			if (curr.BaseType == null)
			{
				return false;
			}

			return HasParentOfType(curr.BaseType, parent);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Suffix string")]
		SchemaColumn TryGetScehmaColumn(IList list)
		{
			var bizObjType = BusinessObjectCollection.GetElementTypeFromCollectionType(list.GetType());
			var columnName = MapAsLookup.GetDescriptionFromCode(MapAs);
			var tableName = HasParentOfType(bizObjType, typeof(NonPersistentBusinessObject)) ? string.Empty : BusinessObjectFactory.GetTableNameFromType(bizObjType);

			var output = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(columnName, tableName);

			const string MultilingualSuffix = "Multilingual";
			if (TryFromGuidDropEditList(list) && output == null && columnName.EndsWith(MultilingualSuffix))
			{
				var modifiedColumnName = columnName.Substring(0, columnName.Length - MultilingualSuffix.Length);
				return ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(modifiedColumnName, tableName);
			}

			return output;
		}

		bool TryFromGuidDropEditList(IList list)
		{
			var result = FindGuidFromList(MappedFrom, list, MapAs);
			return result.IsValid && !result.IsEmpty;
		}

		bool MapAsMappingIsValid()
		{
			var list = GetListForGuidDropEdit();
			if (list != null)
			{
				return TryFromGuidDropEditList(list) || TryGetScehmaColumn(list) != null;
			}

			return true;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "MapAs string")]
		void ValidateMapAs()
		{
			MapAsInfo.ClearAllNotifications();

			if (Property != null && IsMapped() && MapAs == "Description")
			{
				if (!MapAsMappingIsValid())
				{
					MapAsInfo.AddError(Res.GetString("08735AA9-1E5C-44B4-86DB-D66F820A36DD", "{0} cannot be used for mapping to {1}", MapAs, Text));
				}
			}
		}

		#endregion

		#region MappedField

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		public ZString MappedField => ZString.Join(DelimiterCharacter, FileColumnIndexOrder.Select(index => ZString.Format("Field[{0}]", index)).ToArray());

		public ZPropertyInfo MappedFieldInfo
		{
			get { return GetZPropertyInfo(nameof(MappedField)); }
		}

		#endregion

		#region Delimiter

		public ZString Delimiter
		{
			get
			{
				return delimiter;
			}
			set
			{
				if (delimiter != value)
				{
					SetNonPersistentPropertyValue(DelimiterInfo, ref delimiter, value);
					if (!IsValidationSuspended)
					{
						ValidateDelimiter();
					}
				}
			}
		}

		ZString delimiter;

		public ZPropertyInfo DelimiterInfo
		{
			get { return GetZPropertyInfo(nameof(Delimiter)); }
		}

		void ValidateDelimiter()
		{
			DelimiterInfo.ClearAllNotifications();
			if (!Delimiter.IsEmpty)
			{
				if (!Delimiters.ContainsCode(Delimiter))
				{
					CustomMapListInfo.AddError(Res.GetString("C07EEBE0-3451-4B34-B1E3-603B43A0A10F", "Enter a valid Delimiter."));
				}
			}
		}

		protected bool Delimiter_ReadOnly => FileColumnIndexOrder.Count < 2;

		ZString DelimiterCharacter
		{
			get
			{
				ZString delimiter;
				switch (Delimiter)
				{
					case SupportedDelimiter.Space:
						delimiter = " ";
						break;
					case SupportedDelimiter.Tab:
						delimiter = "\t";
						break;
					case SupportedDelimiter.Newline:
						delimiter = "\r\n";
						break;
					default:
						delimiter = Delimiter;
						break;
				}
				return delimiter;
			}
		}

		public string GetMappedFieldValue(string[] values) => string.Join(DelimiterCharacter, FileColumnIndexOrder.Where(index => index >= 0 && index < values.Length).Select(index => values[index]).Where(val => !string.IsNullOrEmpty(val)));

		#endregion

		#region Delimiters

		public CodeDescriptionPairList Delimiters
		{
			get
			{
				if (delimiters == null)
				{
					delimiters = new CodeDescriptionPairList(ParentCollection.Parent.Delimiters);
					delimiters.AddPair(SupportedDelimiter.Newline, ResString.GetMultilingualString("84AB9A68-D638-440C-B6EE-E3B56C4E8EAE", "Newline"));
				}

				return delimiters;
			}
		}
		CodeDescriptionPairList delimiters;

		#endregion

		#region MappedFrom

		public ZString MappedFrom => ZString.Join(DelimiterCharacter, FileColumnIndexOrder.Select(index => new ZString(ParentCollection.Parent.GetFileContentValue(index))).Where(val => !val.IsEmpty).ToArray());

		public ZPropertyInfo MappedFromInfo
		{
			get { return GetZPropertyInfo(nameof(MappedFrom)); }
		}

		#endregion

		#region DefaultValue

		[CargoWise.ComponentModel.MaxLength(50)]
		public ZString DefaultValue
		{
			get { return defaultValue; }
			set
			{
				if (defaultValue != value)
				{
					SetNonPersistentPropertyValue(DefaultValueInfo, ref defaultValue, value);
				}
			}
		}

		ZString defaultValue;

		public ZPropertyInfo DefaultValueInfo
		{
			get { return GetZPropertyInfo(nameof(DefaultValue)); }
		}

		protected bool DefaultValue_ReadOnly
		{
			get { return PropertyIsReadOnly; }
		}

		#endregion

		#region Expression

		protected override bool Expression_ReadOnly => FileColumnIndexOrder.Count > 0;

		protected override void OnExpressionChanged()
		{
			exprDelegate = null;
			if (!IsValidationSuspended)
			{
				ValidateMapping();
			}
		}

		[BusinessObjectTestExclude]

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Name of a constant")]
		public Func<string[], string> ExprDelegate
		{
			get
			{
				if (exprDelegate == null && Expression.Length > 0)
				{
					exprDelegate = ParentCollection.Parent.DlrProxy.CreateLambda<Func<string[], string>>(Expression, "Field");
				}

				return exprDelegate;
			}
		}
		Func<string[], string> exprDelegate;

		#endregion

		#region ProperCase

		ZBool properCase;
		public ZBool ProperCase
		{
			get { return properCase; }
			set
			{
				if (properCase != value)
				{
					SetNonPersistentPropertyValue(ProperCaseInfo, ref properCase, value);
					if (!IsValidationSuspended)
					{
						ValidateProperCase();
					}
				}
			}
		}

		public ZPropertyInfo ProperCaseInfo
		{
			get { return GetZPropertyInfo(nameof(ProperCase)); }
		}

		protected bool ProperCase_ReadOnly
		{
			get { return !typeof(ZString).IsAssignableFrom(Property.PropertyType) && !typeof(ZMultilingual).IsAssignableFrom(Property.PropertyType); }
		}

		void ValidateProperCase()
		{
			ProperCaseInfo.ClearAllNotifications();
			if (ProperCase)
			{
				if (Property.CharacterCasing == ZCharacterCasing.Upper)
				{
					ProperCaseInfo.AddError(Res.GetString("0e30c661-ce8f-43e0-b0d0-3a3fba164aca", "Will not have an effect as the field is upper case only."));
				}
				else if (Property.CharacterCasing == ZCharacterCasing.Lower)
				{
					ProperCaseInfo.AddError(Res.GetString("6a9241be-3dad-4d20-bf28-b30f49d34c5d", "Will not have an effect as the field is lower case only."));
				}
			}
		}

		#endregion

		#region PropertyIsReadOnly

		public bool PropertyIsReadOnly { get; set; }

		#endregion

		#region UpdateExisting

		ZBool updateExisting;
		public ZBool UpdateExisting
		{
			get { return updateExisting; }
			set
			{
				if (updateExisting != value)
				{
					SetNonPersistentPropertyValue(UpdateExistingInfo, ref updateExisting, value);
				}
			}
		}

		public ZPropertyInfo UpdateExistingInfo
		{
			get { return GetZPropertyInfo(nameof(UpdateExisting)); }
		}

		#endregion

		#region Western characters only

		ZBool westernCharactersOnly;
		public ZBool WesternCharactersOnly
		{
			get { return westernCharactersOnly; }
			set
			{
				if (westernCharactersOnly != value)
				{
					SetNonPersistentPropertyValue(WesternCharactersOnlyInfo, ref westernCharactersOnly, value);
				}
			}
		}

		public ZPropertyInfo WesternCharactersOnlyInfo
		{
			get { return GetZPropertyInfo(nameof(WesternCharactersOnly)); }
		}

		#endregion

		#endregion

		public bool IsMapped()
		{
			return HasMappedFrom() || HasMappedExpression() || HasMappedDefault();
		}

		public bool HasMappedFrom() => FileColumnIndexOrder.Count > 0;

		public bool HasMappedExpression() => !Expression.IsEmpty;

		public bool HasMappedDefault() => !DefaultValue.IsEmpty;

		#region TryParse

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		[SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
		public bool TryParse(BusinessObject bizObj, string[] values, out IZType result, out Exception exception, string fallbackValue = null)
		{
			exception = null;
			if (HasMappedFrom())
			{
				var value = GetMappedFieldValue(values);
				if (!string.IsNullOrEmpty(value))
				{
					return TryParse(bizObj, value, out result, fallbackValue);
				}
			}

			if (HasMappedExpression())
			{
				try
				{
					if (TryParse(bizObj, ExprDelegate(values), out result, fallbackValue))
					{
						return true;
					}
				}
				catch (Exception ex) when (ParentCollection.Parent.DlrProxy.IsDlrException(ex))
				{
					throw new DlrException(ex.Message);
				}
			}

			if (HasMappedDefault())
			{
				if (TryParse(bizObj, DefaultValue, out result, fallbackValue))
				{
					return true;
				}
			}

			result = null;
			return false;
		}

		BusinessObjectFactory factory;

		[SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Same as ZDateTime version")]
		bool TryParse(BusinessObject bizObj, string value, out IZType result, string fallbackValue = null)
		{
			if (CustomMapList.Length > 0)
			{
				TryFromCustomMapList(ref value);
			}

			object resultIntermediate = null;
			if (IsType(bizObj, typeof(ZString)))
			{
				ZString str;
				if (TryFromFindBoxListProvider(bizObj, value, out str))
				{
					resultIntermediate = str;
				}
				else if (value.Length > 0)
				{
					resultIntermediate = value;
				}
			}
			else if (IsType(bizObj, typeof(ZGuid)))
			{
				ZGuid guid = Guid.Empty;
				if (Property.GetType().FullName == "Enterprise.ZArchitecture.GUI.DataMapping.ZGridImportPropertyInfo")
				{
					Type type = Property.GetType();
					var columnStyle = type.GetProperty("ColumnStyle").GetValue(Property, null);
					if (columnStyle.GetType().FullName == "Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyle")
					// if the columnstyle is ZGuidDropEditColumnStyle, the data source should come from the list retrieved using the BindToList(or List attribute) setting in the meta data with the associated business object.
					{
						if (TryFromGuidDropEditList(bizObj, value, out guid))
						{
							resultIntermediate = guid;
						}
						//special case for ZAddress fields/backed by ZAddressList, such as DepotAddressPK backed by DepotAddressPK_ZAddress.OrgAddress_List
						if (!(new ZGuid(guid).IsValid) && (GetListForGuidDropEdit()?.GetType()) == typeof(ZAddressList))
						{
							if (factory == null)
							{
								factory = new BusinessObjectFactory();
							}
							var address = factory.LoadTop1<IOrgAddress>(new ZQuery(OrgAddressSchema.OA_Code, value));
							if (address != null)
							{
								resultIntermediate = guid = address.PK;
							}
						}
					}
				}
				if (guid == Guid.Empty)
				{
					if (TryFromFindBoxListProvider(bizObj, value, out guid))
					{
						resultIntermediate = guid;
					}
				}
				if (!guid.IsValid && !string.IsNullOrEmpty(fallbackValue))
				{
					IZType result2;
					TryParse(bizObj, fallbackValue, out result2);
					resultIntermediate = result2;
				}
			}
			else if (IsType(bizObj, typeof(ZDateTime)))
			{
				DateTime dt;
				if (TryGetTimeSpanFromText(value, out ZDateTime dateTime))
				{
					resultIntermediate = dateTime;
				}
				else if (DateTime.TryParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
					|| DateTime.TryParse(value, out dt))
				{
					var zDateTime = new ZDateTime(dt);
					if (zDateTime.IsValid && zDateTime.IsValidSmallDateTime)
					{
						resultIntermediate = zDateTime;
					}
				}
			}
			else if (IsType(bizObj, typeof(ZDate)))
			{
				DateTime dt;
				if (DateTime.TryParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
					|| DateTime.TryParse(value, out dt))
				{
					var zDateTime = new ZDateTime(dt);
					if (zDateTime.IsValid && zDateTime.IsValidSmallDateTime)
					{
						resultIntermediate = zDateTime.Date;
					}
				}
			}
			else if (IsType(bizObj, typeof(ZDateTimeOffset)))
			{
				if (DateTime.TryParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
				{
					resultIntermediate = new ZDateTimeOffset(dt); //using current branch UNLOCO for offset
				}
				else if (ZDateTimeOffset.TryParse(value, out var dto) && dto.IsValid && dto.IsValidSmallDateTime)
				{
					resultIntermediate = dto;
				}
			}
			else if (IsType(bizObj, typeof(ZGeography)))
			{
				ZGeography geo;
				if (ZGeography.TryParse(value, out geo))
				{
					resultIntermediate = geo;
				}
			}
			else if (IsType(bizObj, typeof(ZDecimal)))
			{
				NumberFormatInfo numberFormat;

				if (ParentCollection.Parent.UseCurrentCountryNumberFormatting)
				{
					numberFormat = Culture.CurrentCompanyCountryCulture.NumberFormat;
				}
				else
				{
					numberFormat = CultureInfo.CurrentCulture.NumberFormat;
				}

				for (int i = 0; i < value.Length; i++)
				{
					if (char.IsDigit(value[i]) ||
						value[i] == numberFormat.CurrencyDecimalSeparator[0] ||
						value[i] == numberFormat.NegativeSign[0] ||
						value[i] == numberFormat.PositiveSign[0])
					{
						value = value.Substring(i);
						break;
					}
				}

				for (int i = value.Length - 1; i >= 0; i--)
				{
					if (char.IsDigit(value[i]) ||
						value[i] == numberFormat.CurrencyDecimalSeparator[0])
					{
						value = value.Substring(0, i + 1);
						break;
					}
				}

				try
				{
					resultIntermediate = ZDecimal.Parse(value, numberFormat);
				}
				catch (ArgumentException)
				{
				}
				catch (FormatException)
				{
				}
				catch (OverflowException)
				{
				}
			}
			else if (IsType(bizObj, typeof(ZInt)))
			{
				ZInt v;
				if (ZInt.TryParse(value, out v))
				{
					resultIntermediate = v;
				}
			}
			else if (IsType(bizObj, typeof(ZShort)))
			{
				ZShort v;
				if (ZShort.TryParse(value, out v))
				{
					resultIntermediate = v;
				}
			}
			else if (IsType(bizObj, typeof(ZByte)))
			{
				ZByte v;
				if (ZByte.TryParse(value, out v))
				{
					resultIntermediate = v;
				}
			}
			else if (IsType(bizObj, typeof(ZBool)))
			{
				switch (value.ToUpperInvariant())
				{
					case "Y":
					case "1":
					case "T":
					case "YES":
					case "TRUE":
						resultIntermediate = ZBool.True;
						break;

					case "N":
					case "0":
					case "F":
					case "NO":
					case "FALSE":
						resultIntermediate = ZBool.False;
						break;
				}
			}
			else
			{
				resultIntermediate = value;
			}

			if (resultIntermediate != null)
			{
				if (Property.IsMultiControl && !(resultIntermediate is ZString))
				{
					resultIntermediate = new ZString(resultIntermediate.ToString());
				}

				if (resultIntermediate is string || resultIntermediate is ZString)
				{
					resultIntermediate = ApplyCasing(resultIntermediate.ToString());
				}
			}

			result = (IZType)resultIntermediate;

			return result != null;
		}

		ZString ApplyCasing(string text)
		{
			if (Property.CharacterCasing == ZCharacterCasing.Lower)
			{
				return text.ToLower();
			}
			else if (Property.CharacterCasing == ZCharacterCasing.Upper)
			{
				return text.ToUpper();
			}
			else if (ProperCase)
			{
				return ParentCollection.Parent.ToProperCase(text);
			}

			return text;
		}

		bool TryGetTimeSpanFromText(string value, out ZDateTime result)
		{
			var timeSpanParts = value.Split(':');

			if (timeSpanParts.Length == 2 && timeSpanParts[1].Length <= 2)
			{
				string hourString = timeSpanParts[0].Length == 0 ? "0" : timeSpanParts[0];
				string minuteString = timeSpanParts[1].Length == 0 ? "0" : timeSpanParts[1];

				if (int.TryParse(hourString, out int hours) && int.TryParse(minuteString, out int minutes) && hours > -1000 && hours < 1000 && minutes >= 0 && minutes < 60)
				{
					int addOrSubtract = hourString.StartsWith("-", StringComparison.OrdinalIgnoreCase) ? -1 : 1;
					result = new TimeSpan(hours, minutes * addOrSubtract, 0);
					return true;
				}
			}

			result = ZDateTime.Empty;
			return false;
		}

		bool TryFromFindBoxListProvider(BusinessObject bizObj, string value, out ZGuid result)
		{
			result = ZGuid.Invalid;
			IFindBoxListProvider findBoxListProvider = Property.GetFindBoxListProvider(bizObj);
			if (findBoxListProvider != null)
			{
				string alternateKey = MapAsLookup.GetDescriptionFromCode(MapAs);
				if (string.IsNullOrEmpty(alternateKey))
				{
					result = findBoxListProvider.PrimaryKeyFromCode(value);
				}
				else
				{
					IFindBoxListProviderEx findBoxListProviderEx = (IFindBoxListProviderEx)findBoxListProvider;
					result = findBoxListProviderEx.PrimaryKeyFromAlternateKey(alternateKey, (ZString)value);
				}
			}

			return result.IsValid && !result.IsEmpty;
		}

		bool TryFromFindBoxListProvider(BusinessObject bizObj, string value, out ZString result)
		{
			result = ZString.Empty;
			IFindBoxListProvider findBoxListProvider = Property.GetFindBoxListProvider(bizObj);
			if (findBoxListProvider != null)
			{
				string alternateKey = MapAsLookup.GetDescriptionFromCode(MapAs);
				if (string.IsNullOrEmpty(alternateKey))
				{
					result = value;
				}
				else
				{
					IFindBoxListProviderDescriptionEx findBoxListProviderDescriptionEx = (IFindBoxListProviderDescriptionEx)findBoxListProvider;
					result = findBoxListProviderDescriptionEx.CodeFromDescription(value);
				}
			}

			return result.IsValid && !result.IsEmpty;
		}

		bool TryFromGuidDropEditList(BusinessObject bizObj, string value, out ZGuid result)
		{
			result = ZGuid.Invalid;
			IList list = GetListForGuidDropEdit();
			if (list != null)
			{
				result = FindGuidFromList(value, list, MapAs);
			}
			return result.IsValid && !result.IsEmpty;
		}

		protected override IFindBoxListProvider GetFindBoxListProvider()
		{
			IList list;
			if (ParentCollection.Parent.BindToLists.TryGetValue(MappingName, out list))
			{
				IFindBoxListProvider findBoxListProvider = list as IFindBoxListProvider;
				return findBoxListProvider;
			}

			return null;
		}

		IList GetListForGuidDropEdit()
		{
			IList list = null;
			ParentCollection.Parent.BindToLists.TryGetValue(MappingName, out list);
			return list;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "string constant")]
		internal ZGuid FindGuidFromList(string value, IList list, string mapAs)
		{
			var codeDescription = mapAs == "Code" ? list.Cast<ICodeDescription>().FirstOrDefault(item => item.Code.Equals(value)) : list.Cast<ICodeDescription>().FirstOrDefault(item => item.Description.Equals(value));
			return (codeDescription as BusinessObject)?.PK ?? ZGuid.Missing;
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateMapping();
		}

		void ValidateMapping()
		{
			MappedFieldInfo.ClearAllNotifications();
			ExpressionInfo.ClearAllNotifications();

			if (Property != null && Property.IsMandatory && !IsMapped())
			{
				MappedFieldInfo.AddError(Res.GetString("a2e95505-7780-41fa-95c1-f7ef8d109019", "Please map {0} before importing", MappingName));
			}

			if (MapAsSet)
			{
				ValidateMapAs();
			}
		}

		#endregion
	}
}
