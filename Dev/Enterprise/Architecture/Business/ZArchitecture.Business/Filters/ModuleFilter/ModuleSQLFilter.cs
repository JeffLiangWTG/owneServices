using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Microsoft.SqlServer.Management.SqlParser.Parser;

namespace Enterprise.ZArchitecture.Business
{
	public class ModuleSQLFilter : ModuleFilter
	{
		#region Constructors

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ModuleSQLFilter(FilterCategory category, ModuleFilterCollection parentCollection, Type type)
				: base(category, parentCollection)
		{
			Initialize(type);
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ModuleSQLFilter(ZString description, Type type)
				: base(description)
		{
			Initialize(type);
		}

		void Initialize(Type type)
		{
			bizoType = type;
			ReadOnly = !Globals.IsUserInteractive || !EnvProxy.Instance.Security.UseSqlFilterStrip.IsAllowed;
		}

		#endregion

		#region Type

		Type bizoType;

		public Type QueryObjectType
		{
			get { return bizoType; }
			set { bizoType = value; }
		}

		#endregion

		#region Property1

		public ZString Property1
		{
			get { return property1; }
			set
			{
				if (property1 != value)
				{
					isValidSql = false;
					SetNonPersistentPropertyValue(Property1Info, ref property1, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty1();
					}
					Property1Info.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo Property1Info
		{
			get { return GetZPropertyInfo(nameof(Property1)); }
		}

		ZString property1;
		ZString originalProperty1;

		public bool HasChanged
		{
			get { return originalProperty1 != property1; }
		}

		public bool IsValidSql(bool silentMode)
		{
			if (!isValidSql)
			{
				Validation.ValidateSqlOnFind(silentMode);
				isValidSql = !HasErrors;
			}
			return isValidSql;
		}

		bool isValidSql;

		#region Property1Validation

		public Validation Property1Validation
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return property1Validation; }
			[System.Diagnostics.DebuggerStepThrough]
			set { property1Validation = value; }
		}
		Validation property1Validation;

		#endregion

		#endregion

		#region ModuleFilterOverrides

		protected override bool ShouldClear => true;

		protected override void ClearCore()
		{
			Property1 = ZString.Empty;
		}

		protected override bool IsEmptyCore => Property1.IsEmpty;

		protected internal override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new ModuleSQLFilter(category, parentCollection, bizoType);
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var filter = (ModuleSQLFilter)filterToCopyFrom;
			Property1 = filter.Property1;
		}

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.Other; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ModuleSQLFilterValidation(this);
		}

		public new ModuleSQLFilterValidation Validation
		{
			get { return (ModuleSQLFilterValidation)base.Validation; }
		}

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { Property1 }; }
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			var query = new ZDBOnlyQuery(bizoType);
			query.AddFilterAndZSQLParameterCollection(Property1 + DbCommand.ExecuteAsReaderFlagComments, null);
			query.IgnoreBlobFieldsCheck = true;
			return query;
		}

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			var sqlText = Property1.IsValid ? Property1.ToString() : string.Empty;
			writer.WriteElementString("Property1", sqlText);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			ZString sqlString = (reader.Name == "Property1") ? reader.ReadElementString("Property1") : "";
			if (!sqlString.IsEmpty)
			{
				originalProperty1 = sqlString;
				Property1 = sqlString;
			}
		}

		#endregion

		#region Validation

		public class ModuleSQLFilterValidation : ModuleFilterValidation
		{
			public ModuleSQLFilterValidation(ModuleSQLFilter parent)
					: base(parent)
			{
				Parent = parent;
			}

			#region ValidateProperty1

			public void ValidateProperty1()
			{
				ValidateCalculatedProperty(Parent.Property1Info);
			}

			protected void CheckProperty1()
			{
				DoCheckProperty1();
			}

			[SuppressMessage("CargoWiseOne", "CW1113:Do Not Show Message Box From Business Layer")]
			protected void DoCheckProperty1()
			{
				if (!Parent.Property1.IsEmpty)
				{
					var containsGetDate = Regex.IsMatch(Parent.Property1, "GETDATE()", RegexOptions.IgnoreCase);
					if (containsGetDate)
					{
						Parent.Property1Info.AddWarning(Res.GetString("5e781d42-73f5-4636-9067-0a8cd2f90850", "Your statement includes the function '{0}', which is likely to produce incorrect results as '{0}' uses the date and time on the database server, and most date columns store UTC values. Please use '{1}' instead", "GetDate()", "GetUtcDate()"));
					}

					if (actuallyRunSqlValidation)
					{
						string errorMessage = null;
						try
						{
							var query = Parent.GetQueryUsingFilterColumns();
							if (!ObjectFactory.Get<ISystemDataRegistry>().AllowScalarFunctionsInCustomSql.Value)
							{
								errorMessage = GetScalarFunctionErrorMessage(query);
							}
							if (string.IsNullOrEmpty(errorMessage))
							{
								errorMessage = QueryIsAccessingSensitiveData();
							}
							if (string.IsNullOrEmpty(errorMessage))
							{
								errorMessage = QueryHasCorrectFormatCheck(query);
							}
						}
						finally
						{
							if (!string.IsNullOrEmpty(errorMessage))
							{
								Parent.Property1Info.AddError(errorMessage);
								if (!fSilentMode)
								{
									Globals.Message.ShowError(errorMessage);
								}
							}

							actuallyRunSqlValidation = false;
						}
					}
				}
			}

			[SuppressMessage("CargoWiseOne", "CW1107:Db.Connection.Command - Use the BusinessObjectFactory rather than hitting the DB directly.")]
			string QueryIsAccessingSensitiveData()
			{
				var sensitiveColumns = CargoWise.Data.SensitiveColumnsCache.Instance.SensitiveColumns.Select(x => x.ToLowerInvariant()).ToHashSet();
				var query = (string)this.Parent.Property1;
				var dummySQL = "SELECT 1 FROM dummy where ( " + query + " ) ";

				var parseResult = Parser.Parse(dummySQL);
				if (!parseResult.Errors.Any())
				{
					var tokens = parseResult.Script.Tokens
						.Where(token => token.IsSignificant).Select(token => NormalizeToken(token.Text)).ToList();
					if (tokens.Intersect(sensitiveColumns).Any())
					{
						return Res.GetString("16a15e49-1cb9-46a4-9886-cd21141a224a", "Custom SQL Filters cannot access sensitive information.");
					}
				}
				else
				{
					//fallback if we can't parse that's strictly more aggressive in matching
					//(I'm not sure if there's a query that won't parse but will work in SQL. this might be overengineered)
					//Normalize query by removing all characters except printable ASCII, and then lower casing it.
					var asciiQuery = Regex.Replace(query, @"[^\u0020-\u007E]+", string.Empty);
					var lowerCaseQuery = asciiQuery.ToLowerInvariant();
					foreach (var sensitiveColumn in sensitiveColumns)
					{
						if (lowerCaseQuery.Contains(sensitiveColumn))
						{
							return Res.GetString("16a15e49-1cb9-46a4-9886-cd21141a224a", "Custom SQL Filters cannot access sensitive information.");
						}
					}
				}
				return null;
			}

			string NormalizeToken(string text)
			{
				//Remove []s e.g. [GS_PasswordHash] as this is all considered one token.
				//Lower case to do a case insensitive check.
				return text.Trim('[', ']').ToLowerInvariant();
			}

			[SuppressMessage("CargoWiseOne", "CW1107:Db.Connection.Command - Use the BusinessObjectFactory rather than hitting the DB directly.")]
			[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Database schema")]
			string GetScalarFunctionErrorMessage(ZQuery query)
			{
				var tableName = BusinessObjectFactory.GetTableNameFromType(Parent.QueryObjectType, false);
				var schemaName = BusinessObjectFactory.GetTableSchemaFromType(Parent.QueryObjectType, false).SqlSchemaName ?? "dbo";
				var sqlText = $"SELECT TOP 1 * FROM {schemaName}.{tableName} WHERE {query.LiteralTextSqlFormatted}";

				try
				{
					using (var command = Db.Connection.Command(sqlText))
					{
						if (new ScalarFunctionDetector().IsScalarFunction(command, Db.Connection))
						{
							return Res.GetString("456C1F48-4D37-4B87-AA53-50ED28D5CE56", "Scalar functions are not permitted to be called from filters due to their negative performance implications.");
						}
					}
				}
				catch (SqlException ex)
				{
					return Res.GetString("D0AF99BB-D9DD-4590-95E2-BBB00CB82612", $"Could not apply SQL filter due to the following errors:{System.Environment.NewLine}{ex.Message}");
				}
				return null;
			}

			bool fSilentMode;

			[SuppressMessage("CargoWiseOne", "CW1107:Db.Connection.Command - Use the BusinessObjectFactory rather than hitting the DB directly.")]
			[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Database schema")]
			string QueryHasCorrectFormatCheck(ZQuery query)
			{
				try
				{
					var tableName = BusinessObjectFactory.GetViewNameFromType(Parent.QueryObjectType);
					var schemaName = BusinessObjectFactory.GetTableSchemaFromType(Parent.QueryObjectType)?.SqlSchemaName ?? "dbo";

					ZQuery queryWithMandatoryFilters = null;
					if (Parent != null && Parent.ActiveModuleFiltersProvider is FilterStripBusinessObject fsbo)
					{
						queryWithMandatoryFilters = fsbo.Filter;
					}
					else
					{
						var mandatoryFilters = Parent?.ActiveModuleFiltersProvider?.AlwaysAppliedModuleFilters ?? Enumerable.Empty<ModuleFilter>();
						queryWithMandatoryFilters = new ModuleFilterCollection().GetFilterQuery(mandatoryFilters);
						queryWithMandatoryFilters.AddToFilter(query);
					}

					var testQuery = $"SELECT TOP 1 * FROM (SELECT TOP 1 * FROM {schemaName}.{tableName}) arbitraryView92674a06d7d34d9d905c63c2baaf8d19 WHERE {queryWithMandatoryFilters.LiteralTextSqlFormatted}";
					using (var reader = Db.Connection.Command(testQuery).ExecuteReader())
					{
						reader.Read();
					}
				}
				catch (Exception ex) when (ex is SqlException sqlEx)
				{
					if (sqlEx.Number == 4104)
					{
						return Res.GetString("c6ff4661-bd63-4ff8-92dc-1b32aa791a75", "The SQL where clause you have typed is attempting refer to the queried table by name. Please only refer to columns.");
					}
					else
					{
						var message = ex.Message;
						if (sqlEx.Number == 245 || message.Contains("conver", StringComparison.OrdinalIgnoreCase))
						{
							message = (NoResString)"Conversion failed."; //NoResString because it's just truncating an English language SQL error message
						}
						return Res.GetString("46538037-0409-4877-A0A5-6FCC20CC5B84",
@"The Custom SQL Filter should be in the format of F1 = 'Value' AND F2 = 'Value 2'.
What you have typed is: {0}
Error message: {1}
Filters: {2}", Parent.Property1, message, query.LiteralTextADO.Replace(DbCommand.ExecuteAsReaderFlagComments, "").Replace("\n", " ").TrimEnd());
					}
				}

				return string.Empty;
			}

			#endregion

			#region ValidateSqlOnFind

			public void ValidateSqlOnFind(bool silentMode = false)
			{
				fSilentMode = silentMode;
				actuallyRunSqlValidation = true;
				ValidateCalculatedProperty(Parent.Property1Info);
			}

			bool actuallyRunSqlValidation;

			#endregion

			public override void ValidateAll()
			{
				ValidateProperty1();
				ValidateSqlOnFind();
			}

			public override Type AutoValidationType
			{
				get { return GetType(); }
			}

			protected readonly ModuleSQLFilter Parent;
		}

		#endregion

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			Property1 = RandomString(MaxLength);
		}

#endif
		#endregion
	}
}
