using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public class AlternateGLAccountFilterHelper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public AlternateGLAccountFilterHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Filter Properties

		#region MaximumRows

		[BusinessObjectTestExclude]
		public int? MaximumRows
		{
			get { return fMaximumRows; }
			set { fMaximumRows = value; }
		}

		int? fMaximumRows;

		#endregion

		#region Filter String

		public ZString PlainFilterWithoutParamValues
		{
			get
			{
				var filterString = "SELECT " +
					AccAlternateGLAccount.Schema.PK + ", " +
					AccAlternateChart.Schema.AAC_GC_Company + ", " +
					AccAlternateGLAccount.Schema.AGA_AAC_AlternateChart + ", " +
					AccAlternateGLAccount.Schema.AGA_AccountNum + ", " +
					AccAlternateGLAccount.Schema.AGA_Description + ", " +
					AccAlternateGLAccount.Schema.AGA_AccountType + ", " +
					AccGLHeader.Schema.AG_CashFlowType + ", " +
					AccAlternateGLAccount.Schema.AGA_DebitCredit + ", " +
					AccAlternateGLAccount.Schema.AGA_AGA_AlternateNum + ", " +
					AccAlternateGLAccount.Schema.AGA_AGA_PercentNum + ", " +
					AccAlternateGLAccount.Schema.AGA_AGA_ConsolidationNum + ", " +
					AccAlternateGLAccount.Schema.AGA_AGA_HeaderDependsOnTotal + ", " +
					AccAlternateGLAccount.Schema.AGA_TotalLevel + ", " +
					AccAlternateGLAccount.Schema.AGA_ReportSection + ", " +
					AccAlternateGLAccount.Schema.AGA_PrintSequence + ", " +
					AccAlternateGLAccount.Schema.AGA_SystemCreateTimeUtc + ", " +
					AccAlternateGLAccount.Schema.AGA_SystemCreateUser + ", " +
					AccAlternateGLAccount.Schema.AGA_SystemLastEditTimeUtc + ", " +
					AccAlternateGLAccount.Schema.AGA_SystemLastEditUser + ", " +
					AccAlternateGLAccountAttribute.Schema.AAA_AG_GLHeader + ", " +
					AlternateGLAccountCombineParentAccount.Schema.Attribute_OCG + ", " +
					AlternateGLAccountCombineParentAccount.Schema.Attribute_SPR + ", " +
					AlternateGLAccountCombineParentAccount.Schema.Attribute_TIC + ", " +
					AlternateGLAccountCombineParentAccount.Schema.Attribute_LFO + ", " +
					AlternateGLAccountCombineParentAccount.Schema.Attribute_LFE + ", " +
					AlternateGLAccountCombineParentAccount.Schema.Attribute_ORG + " " +
					"FROM " + AccAlternateGLAccountSchema.Constants.SqlSchemaName + "." + AccAlternateGLAccountSchema.Constants.TableName + " Account " +
					"INNER JOIN " + AccAlternateChartSchema.Constants.SqlSchemaName + "." + AccAlternateChartSchema.Constants.TableName +
					" ON " + AccAlternateChart.Schema.PK + " = Account." + AccAlternateGLAccount.Schema.AGA_AAC_AlternateChart　+
					" LEFT JOIN ( " +
					"SELECT " + AccAlternateGLAccountAttribute.Schema.AAA_AGA_AlternateGLAccount + ", " +
					AccAlternateGLAccountAttribute.Schema.AAA_AG_GLHeader +
					", LFO AS Attribute_LFO, TIC AS Attribute_TIC, ORG AS Attribute_ORG, OCG AS Attribute_OCG, LFE AS Attribute_LFE, SPR AS Attribute_SPR" +
					" FROM ( SELECT " + AccAlternateGLAccountAttribute.Schema.AAA_AGA_AlternateGLAccount + ", " +
					AccAlternateGLAccountAttribute.Schema.AAA_AG_GLHeader + ", " +
					AccAlternateGLAccountAttribute.Schema.AAA_Attribute + ", " +
					"ISNULL(NULLIF(CONVERT(VARCHAR(36), " + AccAlternateGLAccountAttribute.Schema.AAA_Value + "), ''), CONVERT(VARCHAR(36), " + AccAlternateGLAccountAttribute.Schema.AAA_AttributeValueID + ")) AS AttributeValue" +
					" FROM " + AccAlternateGLAccountAttributeSchema.Constants.SqlSchemaName + "." + AccAlternateGLAccountAttributeSchema.Constants.TableName +
					" ) AS SourceAttribute PIVOT (MAX(AttributeValue) for " + AccAlternateGLAccountAttribute.Schema.AAA_Attribute + " IN (ORG, OCG, TIC, LFE, LFO, SPR)) AS PivotAttribute" + " ) Attribute" +
					" ON " + "Attribute." + AccAlternateGLAccountAttribute.Schema.AAA_AGA_AlternateGLAccount + " = " + "Account." + AccAlternateGLAccount.Schema.PK +
					" LEFT JOIN AccGLHeader ON AAA_AG_GLHeader = AG_PK" +
					" WHERE ( " + AccAlternateChart.Schema.AAC_GC_Company + " = @CompanyPK" +
					" OR " + AccAlternateChart.Schema.AAC_GC_Company + " IS NULL )";
				if (OuterQuery != null)
				{
					filterString += OuterQueryParametrisedString;
				}

				if (MaximumRows != null)
				{
					filterString = "SELECT TOP " + MaximumRows.Value.ToString() + " * FROM (" + filterString + ") AS result";
				}

				return filterString;
			}
		}

		#endregion

		#endregion

		#region OuterQuery

		ZQuery OuterQuery;

		public void SetOuterQuery(ZQuery query)
		{
			OuterQuery = query;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Partial SQL")]
		ZString OuterQueryParametrisedString
		{
			get
			{
				var filterString = ZString.Empty;
				if (OuterQuery != null)
				{
					if (OuterQuery.ParameterisedText.ParameterisedQueryText.Trim().Length > 0)
					{
						filterString = " AND ( " + OuterQuery.ParameterisedText.ParameterisedQueryText + " ) ";
					}
				}
				return filterString;
			}
		}

		#endregion

		#region Paramaters

		public ZSqlParameterCollection FilterParameters
		{
			get
			{
				var filterParams = new ZSqlParameterCollection();

				filterParams.Add("@CompanyPK", GlbCompany.CurrentCompany.PK, AccAlternateChartSchema.AAC_GC_Company);

				if (OuterQuery != null)
				{
					filterParams.AddRange(OuterQuery.ParameterisedText.Parameters);
				}

				return filterParams;
			}
		}

		#endregion
	}
}
