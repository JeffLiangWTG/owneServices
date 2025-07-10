using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	public class FindBoxListProvider : IFindBoxListProvider, IFindBoxListProviderEx, IFindBoxListProviderDescriptionEx
	{
		public FindBoxListProvider(IBusinessObjectCollection collection)
		{
			fList = collection;
		}

		#region Should Auto Complete

		public virtual bool AutoCompleteOnCommit
		{
			get { return false; }
		}

		#endregion

		#region Nearest Match

		//returns new code, and whether autocomplete succeeded (true) or failed (false)
		public (string, bool) NearestMatch(string code, bool explicitAutoComplete, int cursor = -1)
		{
			//handle case where = might be literal - if checking for (current code with a literal = inserted) finds an autocompleted match, then signal that we want to return JUST the literal = by returning exactly that to our caller
			if (cursor >= 0)
			{
				var codeWithEquals = code.Insert(cursor, "=");
				var firstResult = NearestMatchCore(codeWithEquals, explicitAutoComplete);
				if (firstResult.Item2 && firstResult.Item1.StartsWith(codeWithEquals, StringComparison.OrdinalIgnoreCase))
				{
					return (codeWithEquals, true);
				}
			}
			return NearestMatchCore(code, explicitAutoComplete);
		}

		public virtual (string, bool) NearestMatchCore(string code, bool explicitAutoComplete)
		{
			ZQuery query = new ZQuery();
			AddCodeStartsWithFilter(query, code);
			query.AddToFilter(List.CompleteFilter);
			AddIsActiveFilter(query, code);
			BusinessObject bizObj = List.Factory.LoadTop1(GetTypeOfElements(code), query);

			return (bizObj != null) ? (bizObj[GetCodePropertyName(code)].ToString(), true) : (code, false);
		}

		public virtual string NearestDescriptionMatch(string description, bool explicitAutoComplete)
		{
			ZQuery filter = new ZQuery();
			AddDescriptionStartsWithFilter(filter, description);
			filter.AddToFilter(List.CompleteFilter);
			AddIsActiveFilter(filter, description);
			BusinessObject bizo = List.Factory.LoadTop1(List.TypeOfElements, filter);

			return (bizo != null) ? bizo[GetDescriptionPropertyName(List.TypeOfElements)].ToString() : description;
		}

		#endregion

		#region Code and Description Filters

		protected virtual void AddCodeStartsWithFilter(ZQuery query, string code)
		{
			SchemaColumn codeColumn = GetCodeSchemaColumnFromCode(code);

			if (codeColumn != null && code.Length <= codeColumn.MaxLength)
			{
				query.AddToFilter(JoinCondition.And, codeColumn, SQLComparisonOperator.StartsWith, code);
				query.OrderBy = GetCodePropertyName(code);
			}
			else
			{
				query.IsNoResultQuery = true;
			}
		}

		protected virtual void AddDescriptionStartsWithFilter(ZQuery query, string description)
		{
			SchemaColumn descriptionColumn = GetDescriptionSchemaColumnFrom();
			if (descriptionColumn != null)
			{
				query.AddToFilter(JoinCondition.And, descriptionColumn, SQLComparisonOperator.StartsWith, description);
				query.OrderBy = GetDescriptionPropertyName(List.TypeOfElements);
			}
			else
			{
				query.IsNoResultQuery = true;
			}
		}

		protected virtual void AddCodeEqualsFilter(ZQuery query, string code)
		{
			SchemaColumn codeColumn = GetCodeSchemaColumnFromCode(code);

			if (codeColumn != null)
			{
				query.AddToFilter(JoinCondition.And, codeColumn, SQLComparisonOperator.Equal, code);
			}
			else
			{
				query.IsNoResultQuery = true;
			}
		}

		protected virtual void AddDescriptionEqualsFilter(ZQuery query, string description)
		{
			SchemaColumn descriptionColumn = GetDescriptionSchemaColumnFrom();
			if (descriptionColumn != null)
			{
				query.AddToFilter(JoinCondition.And, descriptionColumn, SQLComparisonOperator.Equal, description);
			}
			else
			{
				query.IsNoResultQuery = true;
			}
		}

		SchemaColumn GetCodeSchemaColumnFromCode(string code)
		{
			return ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(GetCodePropertyName(code), GetTableNameFromCode(code));
		}

		SchemaColumn GetDescriptionSchemaColumnFrom()
		{
			return ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(GetDescriptionPropertyName(List.TypeOfElements), GetTableNameFromCode(List.TypeOfElements));
		}

		string GetTableNameFromCode(string code)
		{
			return GetTableNameFromCode(GetTypeOfElements(code));
		}

		string GetTableNameFromCode(Type elementType)
		{
			return BusinessObjectFactory.GetTableNameFromType(elementType);
		}

		#endregion

		#region Add IsActive Filters

		protected virtual void AddIsActiveFilter(ZQuery query, string code)
		{
			string expectedValue = "Y";
			SchemaColumn column = GetIsActiveSchemaColumnFromCode(code);
			if (column == null)
			{
				column = GetIsCancelledSchemaColumnFromCode(code);
				expectedValue = "N";
			}
			if (column != null)
			{
				query.AddToFilter(JoinCondition.And, column, SQLComparisonOperator.Equal, expectedValue);
			}
		}

		SchemaColumn GetIsActiveSchemaColumnFromCode(string code)
		{
			if (GetTypeOfElements(code).GetCustomAttributes(typeof(IsActivePropertyAttribute), true).Length == 0)
			{
				return null;
			}

			return ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(GetIsActivePropertyName(code), GetTableNameFromCode(code));
		}

		SchemaColumn GetIsCancelledSchemaColumnFromCode(string code)
		{
			if (GetTypeOfElements(code).GetCustomAttributes(typeof(IsCancelledPropertyAttribute), true).Length == 0)
			{
				return null;
			}

			return ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(GetIsCancelledPropertyName(code), GetTableNameFromCode(code));
		}

		#endregion

		#region Description From Code

		public virtual string DescriptionFromCode(string code)
		{
			BusinessObject bizObj = BizObjFromCodeWithCompleteFilter(code) ?? BizObjFromCodeWithRelationshipFilter(code);			
			return (bizObj != null) ? bizObj[GetDescriptionPropertyName(code)].ToString() : null;
		}

		#endregion

		#region Description From Primary Key

		public virtual string DescriptionFromPrimaryKey(ZGuid pK)
		{
			string result = null;
			if (pK.IsValid)
			{
				Type type = GetTypeOfElements(pK);
				BusinessObject bizObj = List.Factory.Load(type, pK);
				if (bizObj != null)
				{
					result = bizObj[GetDescriptionPropertyName(type)].ToString();
				}
			}

			return result;
		}

		#endregion

		#region PK From Code

		public virtual ZGuid PrimaryKeyFromCode(string code)
		{
			ZGuid result = ZGuid.Empty;

			if (!string.IsNullOrEmpty(code))
			{
				BusinessObject bizObj = BizObjFromCodeWithCompleteFilter(code);
				if (bizObj == null)
				{
					bizObj = BizObjFromCodeWithRelationshipFilter(code);
					if (bizObj != null)
					{
						result = bizObj.PK;
						var bizO = bizObj as ICancellable;
						if (bizO == null || !bizO.IsCancelled)
						{
							result = ZGuid.Missing;
						}
					}
					else
					{
						result = ZGuid.Invalid;
					}
				}
				else
				{
					result = bizObj.PK;
				}
			}

			return result;
		}

		#endregion

		#region Code from PK

		public virtual string CodeFromPrimaryKey(ZGuid pK)
		{
			string result = "";

			if (pK.IsValid)
			{
				BusinessObject bizObj = List.Factory.Load(GetTypeOfElements(pK), pK);
				if (bizObj != null)
				{
					var codePropertyName = GetCodePropertyName(pK);
					if (codePropertyName != null)
					{
						result = bizObj[codePropertyName].ToString();
					}
				}
			}

			return result;
		}

		#endregion

		#region Business Object from Code

		public IEnumerable<BusinessObject> GetBusinessObjectsFromCode(string code)
		{
			return GetBusinessObjectsFromCodeCore(code);
		}

		protected virtual IEnumerable<BusinessObject> GetBusinessObjectsFromCodeCore(string code)
		{
			IEnumerable<BusinessObject> result = Enumerable.Empty<BusinessObject>();

			if (!string.IsNullOrEmpty(code))
			{
				result = BizObjsFromCodeWithCompleteFilter(code);
				if (!(result.Any() && result.First() != null))
				{
					result = BizObjsFromCodeWithRelationshipFilter(code);
				}
			}

			return result;
		}

		public BusinessObject GetBusinessObjectFromCode(string code)
		{
			var bizObjs = GetBusinessObjectsFromCode(code);
			return bizObjs.FirstOrDefault();
		}

		public IEnumerable<BusinessObject> GetBusinessObjectsFromCodeWithoutFilter(string code)
		{
			return BizObjsFromCodeWithoutFilter(code);
		}

		public BusinessObject GetBusinessObjectFromCodeWithoutFilter(string code)
		{
			var bizObjs = BizObjsFromCodeWithoutFilter(code);
			return bizObjs.FirstOrDefault();
		}

		#endregion

		#region Code From Description

		public virtual string CodeFromDescription(string description)
		{
			BusinessObject bizo = BizObjFromDescriptionWithFilter(description) ?? BizObjFromDescriptionWithoutFilter(description);
			return bizo?[GetCodePropertyName(List.TypeOfElements)].ToString();
		}

		#endregion

		#region List

		public IBusinessObjectCollection List
		{
			get { return fList; }
		}

		readonly IBusinessObjectCollection fList;

		#endregion

		#region Implementation

		protected ZQuery CompleteFilter
		{
			get { return List.CompleteFilter; }
		}

		#region Code Property

		protected virtual string GetCodePropertyName(string code)
		{
			if (string.IsNullOrEmpty(fCodePropertyName))
			{
				fCodePropertyName = CodePropertyAttribute.CodePropertyNameFromType(GetTypeOfElements(code));
			}

			return fCodePropertyName;
		}

		protected virtual string GetCodePropertyName(ZGuid pK)
		{
			if (string.IsNullOrEmpty(fCodePropertyName))
			{
				fCodePropertyName = CodePropertyAttribute.CodePropertyNameFromType(GetTypeOfElements(pK));
				if (fCodePropertyName == null)
				{
					var type = GetTypeOfElements(pK);
					throw new Exception("Could not retrieve PropertyName from CodePropertyAttribute located on " + type.FullName);
				}
			}

			return fCodePropertyName;
		}

		protected virtual string GetCodePropertyName(Type elementType)
		{
			if (string.IsNullOrEmpty(fCodePropertyName))
			{
				fCodePropertyName = CodePropertyAttribute.CodePropertyNameFromType(elementType);
			}

			return fCodePropertyName;
		}

		string fCodePropertyName;

		#endregion

		#region Description Property

		string GetDescriptionPropertyName(string code)
		{
			if (string.IsNullOrEmpty(fDescriptionPropertyName))
			{
				return GetDescriptionPropertyName(GetTypeOfElements(code));
			}

			return fDescriptionPropertyName;
		}

		protected virtual string GetDescriptionPropertyName(Type typeOfElements)
		{
			if (string.IsNullOrEmpty(fDescriptionPropertyName))
			{
				fDescriptionPropertyName = DescriptionPropertyAttribute.DescriptionPropertyNameFromType(typeOfElements);
			}

			return fDescriptionPropertyName;
		}

		string fDescriptionPropertyName;

		#endregion

		#region IsActive Property

		string isActivePropertyName;
		protected virtual string GetIsActivePropertyName(string code)
		{
			if (string.IsNullOrEmpty(isActivePropertyName))
			{
				isActivePropertyName = IsActivePropertyAttribute.IsActivePropertyName(GetTypeOfElements(code));
			}

			return isActivePropertyName;
		}

		#endregion

		#region IsActive Property

		string isCancelledPropertyName;
		protected virtual string GetIsCancelledPropertyName(string code)
		{
			if (string.IsNullOrEmpty(isCancelledPropertyName))
			{
				isCancelledPropertyName = IsCancelledPropertyAttribute.IsCancelledPropertyName(GetTypeOfElements(code));
			}

			return isCancelledPropertyName;
		}

		#endregion

		protected virtual IEnumerable<BusinessObject> BizObjsFromCodeWithCompleteFilter(string code)
		{
			var bizObjs = Enumerable.Empty<BusinessObject>();

			if (!string.IsNullOrEmpty(code))
			{
				ZQuery query = new ZQuery();
				AddCodeEqualsFilter(query, code);
				query.AddToFilter(List.CompleteFilter);

				bizObjs = List.Factory.Load(GetTypeOfElements(code), query);
			}

			return bizObjs;
		}

		protected BusinessObject BizObjFromCodeWithCompleteFilter(string code)
		{
			var bizObjs = BizObjsFromCodeWithCompleteFilter(code);
			return bizObjs.FirstOrDefault();
		}

		protected virtual IEnumerable<BusinessObject> BizObjsFromCodeWithRelationshipFilter(string code)
		{
			var bizObjs = Enumerable.Empty<BusinessObject>();

			if (!string.IsNullOrEmpty(code))
			{
				ZQuery query = new ZQuery();
				AddCodeEqualsFilter(query, code);
				query.AddToFilter(List.RelationshipFilter);

				bizObjs = List.Factory.Load(GetTypeOfElements(code), query);
			}

			return bizObjs;
		}

		protected BusinessObject BizObjFromCodeWithRelationshipFilter(string code)
		{
			var bizObjs = BizObjsFromCodeWithRelationshipFilter(code);
			return bizObjs.FirstOrDefault();
		}

		protected virtual IEnumerable<BusinessObject> BizObjsFromCodeWithoutFilter(string code)
		{
			var bizObjs = Enumerable.Empty<BusinessObject>();

			if (!string.IsNullOrEmpty(code))
			{
				ZQuery query = new ZQuery();
				AddCodeEqualsFilter(query, code);

				bizObjs = List.Factory.Load(GetTypeOfElements(code), query);
			}

			return bizObjs;
		}

		protected BusinessObject BizObjFromCodeWithoutFilter(string code)
		{
			var bizObjs = BizObjsFromCodeWithoutFilter(code);
			return bizObjs.FirstOrDefault();
		}

		protected virtual BusinessObject BizObjFromDescriptionWithFilter(string description)
		{
			BusinessObject bizObj = null;

			if (!string.IsNullOrEmpty(description))
			{
				ZQuery filter = new ZQuery();
				AddDescriptionEqualsFilter(filter, description);
				filter.AddToFilter(List.CompleteFilter);
				bizObj = List.Factory.LoadTop1(List.TypeOfElements, filter);
			}
			return bizObj;
		}

		protected virtual BusinessObject BizObjFromDescriptionWithoutFilter(string description)
		{
			BusinessObject bizObj = null;

			if (!string.IsNullOrEmpty(description))
			{
				ZQuery filter = new ZQuery();
				AddDescriptionEqualsFilter(filter, description);
				filter.AddToFilter(List.RelationshipFilter);
				bizObj = List.Factory.LoadTop1(List.TypeOfElements, filter);
			}
			return bizObj;
		}

		protected Type GetTypeOfElements(string code)
		{
			ICompositeCollection compositeCollection = List as ICompositeCollection;
			return (compositeCollection != null) ? compositeCollection.TypeOfElementFromCode(code) : List.TypeOfElements;
		}

		protected Type GetTypeOfElements(ZGuid pk)
		{
			ICompositeCollection compositeCollection = List as ICompositeCollection;
			return (compositeCollection != null) ? compositeCollection.TypeOfElementFromPK(pk) : List.TypeOfElements;
		}

		#endregion

		#region IFindBoxListProviderEx Members

		public virtual IList<AlternateKey> AlternateKeys
		{
			get
			{
				string descriptionProperty = DescriptionPropertyAttribute.DescriptionPropertyNameFromType(List.TypeOfElements);
				return new AlternateKey[] { new AlternateKey(descriptionProperty, SchemaColumnType.String, (NoResString)"Description") };
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Multilingual suffix")]
		const string MultilingualSuffix = "Multilingual";

		public virtual ZGuid PrimaryKeyFromAlternateKey(string columnName, IZType value)
		{
			ZGuid result = ZGuid.Empty;
			if (value.IsValid && !value.IsEmpty)
			{
				var column = GetSchemaColumn(columnName, out var tableName);

				if (column == null && columnName.EndsWith(MultilingualSuffix))
				{
					var modifiedColumnName = columnName.Substring(0, columnName.Length - MultilingualSuffix.Length);
					column = GetSchemaColumn(modifiedColumnName, out tableName);
				}

				if (column == null)
				{
					ErrorReporter.ReportOnce("AlternateKeyColumn" + columnName, $"IFindBoxListProviderEx.PrimaryKeyFromAlternateKey: columnName='{columnName}' from tableName='{tableName}' was not found.");
				}
				else
				{
					BusinessObject bizObj = BizObjFromAlternateKeyWithFilter(column, value);
					if (bizObj == null)
					{
						bizObj = BizObjFromAlternateKeyWithoutFilter(column, value);
						if (bizObj != null)
						{
							result = ZGuid.Missing;
						}
						else
						{
							result = ZGuid.Invalid;
						}
					}
					else
					{
						result = bizObj.PK;
					}
				}
			}

			return result;
		}

		public virtual IZType AlternateKeyFromPrimaryKey(string columnName, ZGuid pk)
		{
			IZType result = null;
			if (pk.IsValid)
			{
				BusinessObject bizObj = List.Factory.Load(GetTypeOfElements(pk), pk);
				if (bizObj != null)
				{
					try
					{
						result = bizObj[columnName] as IZType;
					}
					catch (ArgumentException)
					{
						ErrorReporter.ReportOnce("AlternateKeyColumn" + columnName, "IFindBoxListProviderEx.PrimaryKeyFromAlternateKey: " + columnName + " was not found.");
					}
				}
			}

			return result;
		}

		BusinessObject BizObjFromAlternateKeyWithFilter(SchemaColumn column, IZType value)
		{
			ZQuery query = new ZQuery();
			AddAlternateKeyEqualsFilter(query, column, value);
			query.AddToFilter(List.CompleteFilter);

			return GetBizObj(List.TypeOfElements, query, value);
		}

		BusinessObject BizObjFromAlternateKeyWithoutFilter(SchemaColumn column, IZType value)
		{
			ZQuery query = new ZQuery();
			AddAlternateKeyEqualsFilter(query, column, value);

			return GetBizObj(List.TypeOfElements, query, value);
		}

		protected BusinessObject GetBizObj(Type bizOType, ZQuery query, IZType value)
		{
			BusinessObject bizObj = null;
			BusinessObject[] candidates = List.Factory.Load(bizOType, query);
			if (candidates.Length == 1)
			{
				bizObj = candidates[0];
			}

			return bizObj;
		}

		void AddAlternateKeyEqualsFilter(ZQuery query, SchemaColumn column, object value)
		{
			query.AddToFilter(JoinCondition.And, column, SQLComparisonOperator.Equal, value);
		}

		SchemaColumn GetSchemaColumn(string columnName, out string tableName)
		{
			tableName = BusinessObjectFactory.GetTableNameFromType(List.TypeOfElements);
			return ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(columnName, tableName);
		}

		public ICodeDescription GetCustomCodeDescription(BusinessObject bizo)
		{
			var type = bizo.GetType();
			var codeProperty = GetCodePropertyName(type);
			var descriptionProperty = GetDescriptionPropertyName(type);

			return new CodeDescriptionFromCustomProperties(bizo, codeProperty, descriptionProperty);
		}

		class CodeDescriptionFromCustomProperties : ICodeDescription
		{
			readonly BusinessObject bizo;
			readonly string codeProp;
			readonly string descriptionProp;

			public CodeDescriptionFromCustomProperties(BusinessObject bizo, string codeProp, string descriptionProp)
			{
				this.bizo = bizo;
				this.codeProp = codeProp;
				this.descriptionProp = descriptionProp;
			}

			public object PK => bizo.PK;

			public string Code => codeProp != null ? bizo[codeProp].ToString() : ((ICodeDescription)bizo).Code;

			public string Description => descriptionProp != null ? bizo[descriptionProp].ToString() : ((ICodeDescription)bizo).Description;
		}

		#endregion
	}
}
