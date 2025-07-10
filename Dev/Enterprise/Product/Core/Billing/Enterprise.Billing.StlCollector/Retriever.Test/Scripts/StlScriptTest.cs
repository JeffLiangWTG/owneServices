using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using CargoWise.Data;
using Enterprise.Billing.Integration;
using Enterprise.Billing.StlCollector.Retriever.Testing.Scripts;
using Enterprise.Integration.Billing;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json.Linq;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts
{
	abstract class StlScriptTest : BaseStlScriptTest
	{
		public override void TestFieldMaxSizes()
		{
			AssertEquals("FeatureCode.Length <= 3", true, ScriptToTest.Code.Length <= 3);
			AssertEquals("RoleName.Length <= 50", true, ScriptToTest.Role.Length <= 50);
			AssertEquals("ModuleName.Length <= 50", true, ScriptToTest.Module.Length <= 50);
			AssertEquals("FunctionName.Length <= 50", true, ScriptToTest.Function.Length <= 50);
			AssertEquals("FeatureName.Length <= 75", true, ScriptToTest.Feature.Length <= 75);
		}

		public void TestTransactionReference01Field()
		{
			var ref01PtyInfo = typeof(BaseStlScript).GetProperty("TransactionReference01", BindingFlags.Instance | BindingFlags.NonPublic);
			var ref01ExpressionObj = ref01PtyInfo.GetValue(ScriptToTest);
			var ref01Expression = ref01ExpressionObj?.ToString();
			if (FeatureCodesThatAreNotSubmittedToTheBillingDatabase.Contains(ScriptToTest.Code))
			{
				Assert("Reference1 should be null or empty for features that are not submitted to the billing database", string.IsNullOrEmpty(ref01Expression));
				return;
			}

			if (CompositeExpressionRegex.IsMatch(ref01Expression))
			{
				Assert("Expression contain affixed characters to ensure it will not be blank", true);
				return;
			}

			var afterLastDotMatch = AfterLastDotRegex.Match(ref01Expression);
			var ref01Field = afterLastDotMatch.Groups["FIELD"].Value;
			if (Ref01FieldMissingCheckConstraintList.Contains(ref01Field))
			{
				Assert("Excluded for now. Check constraint pending.", true);
				return;
			}

			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				SELECT chk.name
				FROM
					sys.columns c
					INNER JOIN sys.tables t ON t.object_id = c.object_id
					INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
					LEFT JOIN sys.check_constraints chk
						ON chk.parent_object_id = t.object_id
						AND chk.parent_column_id = c.column_id
						AND (	chk.definition = '(datalength([{1}])>(0))'
								OR chk.definition = '([{1}]<>'''')'
								OR (chk.definition like '(![{1}!]=%' ESCAPE '!' AND chk.definition not like '(%![{1}!]=''''%' ESCAPE '!'))
				WHERE
					s.name not in ('{0}')
					AND t.is_ms_shipped = 0
					AND c.name = '{1}'",
				string.Join("','", Db.SqlReservedSchemas),
				ref01Field
			);

			var objDbResult = TestConnection.ExecuteScalar(sqlText);
			if (objDbResult == null)
			{
				Assert("Calculated column to ensure it will not be blank", true);
			}
			else
			{
				AssertEquals("Does TransactionReference01 field [" + ref01Field + "] have a check constraint ensuring a non-empty value?", true, (objDbResult != DBNull.Value));
			}
		}

		/// <summary>
		/// DO NOT add more entries here.
		/// Remove fields as check constraints are added.
		/// </summary>
		readonly string[] Ref01FieldMissingCheckConstraintList = new string[]
		{
			"AH_TransactionNum",
			"BH_JobReference",
			"BT_SendersMessageReference",
			"C4_SendersMessageReference",
			"C6_SendersMessageReference",
			"CE_EntryNum",
			"EI_SessionGuid",
			"JD_OrderNumber",
			"JE_DeclarationReference",
			"JH_JobNum",
			"JJ_ConsignmentID",
			"JK_UniqueConsignRef",
			"JS_UniqueConsignRef",
			"KJ_JobID",
			"KM_JobID",
			"KP_PackageID",
			"LT_LandedCostType",
			"NC_JobNumber",
			"WRC_ConsignmentID",
			"WW_WarehouseName",
			"EM_ReceiveTransmit",
			"R6_ContainerNum"
		};

		readonly string[] FeatureCodesThatAreNotSubmittedToTheBillingDatabase = new string[] { "USG" };

		public void TestTransactionGuidReferenceField()
		{
			var guidRefPtyInfo = typeof(BaseStlScript).GetProperty("TransactionGuidReference", BindingFlags.Instance | BindingFlags.NonPublic);
			string guidRefExpression = guidRefPtyInfo.GetValue(ScriptToTest).ToString();

			if (PkFieldRegex.IsMatch(guidRefExpression))
			{
				var pkMatches = PkFieldRegex.Matches(guidRefExpression);

				AssertEquals("TransactionGuidReference property has at least one PK field", true, pkMatches.Count > 0);

				foreach (Match pkMatch in pkMatches)
				{
					string sqlText = string.Format(CultureInfo.InvariantCulture, @"
						SELECT TOP (1) tp.name, c.is_nullable
						FROM
							sys.columns c
							INNER JOIN sys.tables t ON t.object_id = c.object_id
							INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
							INNER JOIN sys.types tp ON tp.system_type_id = c.system_type_id
						WHERE
							s.name not in ('{0}')
							AND c.name = '{1}'",
						string.Join("','", Db.SqlReservedSchemas),
						pkMatch.Value
					);

					string fieldType = null;
					bool fieldIsNullable = true;

					using (var reader = TestConnection.Command(sqlText).ExecuteReader())
					{
						if (reader.Read())
						{
							fieldType = reader[0].ToString().ToLowerInvariant();
							fieldIsNullable = reader.GetBoolean(1);
						}
					}

					AssertEquals("TransactionGuidReference field [" + pkMatch.Value + "] type", "uniqueidentifier", fieldType);
					AssertEquals("Is TransactionGuidReference field [" + pkMatch.Value + "] nullable?", false, fieldIsNullable);
				}
			}
			else if (ScriptToTest.Code == "GSH")
			{
				AssertEquals(AccBillingHeaderSchema.Constants.ABH_ParentId, guidRefExpression);
				AssertEquals("TransactionGuidReference field type", "UniqueIdentifier", AccBillingHeaderSchema.ABH_ParentId.TypeInfo);
				AssertEquals("Is TransactionGuidReference field nullable?", false, AccBillingHeaderSchema.ABH_ParentId.IsNullable);
			}
			else if (IsGuidAutogeneratedBasedOffTheRecordTime())
			{
				Assert(true); //The guid is autogenerated based off the record time for these collectors, which is sufficient in these cases.
			}
		}

		static readonly string[] CollectorsGeneratingGuidBasedOffTheRecordTime = new string[]
		{
			"EAS",
			"TSK",
			"MLS",
			"EXR",
			"TRT",
			"RHC",
			"OOB",
			"MLA",
			"EXA",
			"WFT",
			"WFW",
			"WFC",
			"WJC",
			"WTC",
			"TLC",
			"VBC",
			"CST",
			"WKP",
			"WKI",
			"USS",
			"TBS",
			"VUC",
			"OUC",
			"SAC",
			"TE1",
			"ROC",
			"ECS",
			"EDC",
			"CCD",
			"VCA",
			"ROV",
			"COR"
		};

		bool IsGuidAutogeneratedBasedOffTheRecordTime()
		{
			return CollectorsGeneratingGuidBasedOffTheRecordTime.Contains(ScriptToTest.Code);
		}

		static readonly Regex CompositeExpressionRegex = new Regex(@"(\+|')", RegexOptions.Compiled | RegexOptions.Singleline);
		static readonly Regex AfterLastDotRegex = new Regex(@"^.*?(?<FIELD>[^.]+)\W*$", RegexOptions.Compiled | RegexOptions.Singleline);
		static readonly Regex PkFieldRegex = new Regex(@"\b\w{2,3}_PK\b", RegexOptions.Compiled);

		#region Implementation

		protected abstract void AssertResultSet(IEnumerable<IStlTransaction> transactions);

		protected string GetSqlText()
		{
			var sqlText = ScriptToTest.ScriptText;
			const string orderClause = @"ORDER BY CompanyCode, TransactionDateUtc, UserCode, TransactionReference01, TransactionReference02, TransactionGuidReference ";
			var indexOfRecompileOption = sqlText.IndexOf("OPTION (RECOMPILE)", StringComparison.CurrentCulture);
			return indexOfRecompileOption >= 0 ? sqlText.Insert(indexOfRecompileOption, orderClause) : sqlText + orderClause;
		}

		protected override void AssertTransactions(IEnumerable<IStlTransaction> transactions)
		{
			AssertTransactionGuidReference(transactions);
			AssertAdditionalRefs(transactions);
			if (CollectedAsUsageTransaction)
			{
				transactions = AssertAndRemoveNonSerializableProperties(transactions.ToList());
			}

			AssertResultSet(transactions);
		}

		void AssertTransactionGuidReference(IEnumerable<IStlTransaction> transactions)
		{
			foreach (var transaction in transactions)
			{
				if (CollectedAsUsageTransaction)
				{
					Assert("AdditionalRefs does not contain Reference5", JObject.Parse(transaction.AdditionalRefs).ContainsKey("Reference5"));
				}
				var transactionGuidReference = transaction.Reference5;

				AssertEquals(
					"TransactionGuidReference is a valid GUID [" + transactionGuidReference + "]",
					true, Guid.TryParse(transactionGuidReference, out _));
			}
		}

		void AssertAdditionalRefs(IEnumerable<IStlTransaction> transactions)
		{
			var transactionProperties = (!CollectedAsUsageTransaction ? typeof(BillingTransaction) : typeof(UsageTransaction)).GetProperties();
			var transactionTypeName = !CollectedAsUsageTransaction ? nameof(BillingTransaction) : nameof(UsageTransaction);

			foreach (var transaction in transactions)
			{
				if (!string.IsNullOrEmpty(transaction.AdditionalRefs))
				{
					var jsonObj = JObject.Parse(transaction.AdditionalRefs);

					foreach (var jsonProperty in jsonObj.Properties())
					{
						var jsonPropertyValue = jsonProperty.Value;
						foreach (var transactionProperty in transactionProperties.Where(x => x.GetCustomAttribute<XmlIgnoreAttribute>() == null))
						{
							if (transactionProperty.Name.Equals(jsonProperty.Name, StringComparison.InvariantCultureIgnoreCase))
							{
								var transactionPropertyValue = transaction.GetPropertyValue(transactionProperty.Name);
								Assert($"JSON Property={transactionProperty.Name} Clashes with {transactionTypeName} property", transactionPropertyValue == null || jsonPropertyValue.ToString().Equals(transactionPropertyValue.ToString()));
							}
						}
					}
				}
			}
		}

		IEnumerable<IStlTransaction> AssertAndRemoveNonSerializableProperties(IEnumerable<IStlTransaction> transactions)
		{
			foreach (var transaction in transactions)
			{
				var additionalRefsObj = JObject.Parse(transaction.AdditionalRefs);
				foreach (var propertyInfo in transaction.GetType().GetProperties().Where(x => x.GetCustomAttribute<XmlIgnoreAttribute>() != null && x.PropertyType == typeof(string)))
				{
					var value = propertyInfo.GetValue(transaction, null);
					if (string.IsNullOrEmpty((string)value) || value.ToString() == "[BLANK]")
					{
						Assert($"AdditionalRefs contains null or empty '{propertyInfo.Name}'", !additionalRefsObj.ContainsKey(propertyInfo.Name));
					}
					else
					{
						if (additionalRefsObj.ContainsKey(propertyInfo.Name))
						{
							AssertEquals($"Nonserializable '{propertyInfo.Name}' property's value does not equal JProperty's value", value.ToString(), additionalRefsObj[propertyInfo.Name].Value<string>());
							transaction.AdditionalRefs = Regex.Replace(transaction.AdditionalRefs, $",\"{propertyInfo.Name}\":\".*\"", string.Empty);
						}
					}
				}
			}

			return transactions;
		}

		protected void AssertRow(IStlTransaction transaction, string assertMsgId, string companyCode, string branchCode, DateTime transactionDateUtc, string userCode, int itemCount, string transactionReference01, string transactionReference02 = null, string transactionReference03 = null, string transactionReference04 = null)
		{
			AssertEquals($"Transaction[{assertMsgId}] CompanyCode", companyCode, transaction.GetCompanyCode());
			AssertEquals($"Transaction[{assertMsgId}] BranchCode", branchCode, transaction.GetBranchCode());
			AssertEquals($"Transaction[{assertMsgId}] TransactionDateUtc", transactionDateUtc, transaction.ServiceOccuredUTC);
			AssertEquals($"Transaction[{assertMsgId}] UserCode", userCode, transaction.ClientStaffCode);
			AssertEquals($"Transaction[{assertMsgId}] ItemCount", itemCount, transaction.BillableCount);
			AssertEquals($"Transaction[{assertMsgId}] TransactionReference01", transactionReference01, transaction.Reference1);
			AssertEquals($"Transaction[{assertMsgId}] TransactionReference02", transactionReference02, transaction.Reference2);
			AssertEquals($"Transaction[{assertMsgId}] TransactionReference03", transactionReference03, transaction.Reference3);
			AssertEquals($"Transaction[{assertMsgId}] TransactionReference04", transactionReference04, transaction.Reference4);
		}

		protected void AssertRowMatchingRef1(IEnumerable<IStlTransaction> transactions, string assertMsgId, string companyCode, string branchCode, DateTime transactionDateUtc, string userCode, int itemCount, string transactionReference01, string transactionReference02 = null, string transactionReference03 = null, string transactionReference04 = null)
		{
			var transaction = FindRowByRef1(transactions, transactionReference01);
			AssertRow(transaction, assertMsgId, companyCode, branchCode, transactionDateUtc, userCode, itemCount, transactionReference01, transactionReference02, transactionReference03, transactionReference04);
		}

		protected void AssertRowMatchingRef1AndRef2(IEnumerable<IStlTransaction> transactions, string assertMsgId, string companyCode, string branchCode, DateTime transactionDateUtc, string userCode, int itemCount, string transactionReference01, string transactionReference02 = null, string transactionReference03 = null, string transactionReference04 = null)
		{
			var transaction = FindRowByRef1AndRef2(transactions, transactionReference01, transactionReference02);
			AssertRow(transaction, assertMsgId, companyCode, branchCode, transactionDateUtc, userCode, itemCount, transactionReference01, transactionReference02, transactionReference03, transactionReference04);
		}

		protected void AssertRowMatchingOccured(IEnumerable<IStlTransaction> transactions, string assertMsgId, string companyCode, string branchCode, DateTime transactionDateUtc, string userCode, int itemCount, string transactionReference01, string transactionReference02 = null, string transactionReference03 = null, string transactionReference04 = null)
		{
			var transaction = FindRowByOccured(transactions, transactionDateUtc);
			AssertRow(transaction, assertMsgId, companyCode, branchCode, transactionDateUtc, userCode, itemCount, transactionReference01, transactionReference02, transactionReference03, transactionReference04);
		}

		protected IEnumerable<IStlTransaction> FindRowsByRef1(IEnumerable<IStlTransaction> transactions, string reference1) => transactions.Where(t => t.Reference1 == reference1);

		protected IStlTransaction FindRowByRef1(IEnumerable<IStlTransaction> transactions, string reference1) => transactions.Single(t => t.Reference1 == reference1);
		protected IStlTransaction FindRowByRef2(IEnumerable<IStlTransaction> transactions, string reference2) => transactions.Single(t => t.Reference2 == reference2);
		protected IStlTransaction FindRowByRef1AndRef2(IEnumerable<IStlTransaction> transactions, string reference1, string reference2) => transactions.Single(t => t.Reference1 == reference1 && t.Reference2 == reference2);
		protected IStlTransaction FindRowByOccured(IEnumerable<IStlTransaction> transactions, DateTime serviceOccuredUTC) => transactions.Single(t => t.ServiceOccuredUTC == serviceOccuredUTC);
		#endregion
	}
}
