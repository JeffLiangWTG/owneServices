
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using AccConstants = Enterprise.Accounting.Business.AccountingUtils;

namespace Enterprise.Accounting.Business.Base.Unmatching
{
	public abstract partial class MatchGroupFilterHelper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public MatchGroupFilterHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Filter Properties

		#region Organisation

		public ZGuid Organisation
		{
			get { return fOrganisation; }
			set { fOrganisation = value; }
		}

		ZGuid fOrganisation;

		#endregion

		#region NumberType

		public ZString NumberType
		{
			get { return fNumberType; }
			set { fNumberType = value; }
		}

		ZString fNumberType;

		protected SchemaColumn NumberTypeSchema
		{
			get
			{
				if (NumberType == AccConstants.NumberFilterTypes.TransactionNumber)
				{
					return ViewMatchGroupSchema.MG_TransactionNum;
				}

				if (NumberType == AccConstants.NumberFilterTypes.ChequeReferenceNumber)
				{
					return ViewMatchGroupSchema.MG_ChequeOrReference;
				}

				if (NumberType == AccConstants.NumberFilterTypes.ConsolidationNumber)
				{
					return ViewMatchGroupSchema.MG_ConsolidatedInvoiceRef;
				}

				if (NumberType == AccConstants.NumberFilterTypes.MatchGroupNumber)
				{
					return ViewMatchGroupSchema.MG_MatchGroupNum;
				}

				return null;
			}
		}

		#endregion

		#region NumberFilter

		public ZString NumberFilter
		{
			get { return fNumberFilter; }
			set { fNumberFilter = value; }
		}

		ZString fNumberFilter;

		#endregion

		#region DateType

		public ZString DateType
		{
			get { return fDateType; }
			set { fDateType = value; }
		}

		ZString fDateType;

		protected SchemaColumn DateTypeSchema
		{
			get
			{
				if (DateType == AccConstants.DateFilterTypes.DueDate)
				{
					return ViewMatchGroupSchema.MG_DueDate;
				}

				if (DateType == AccConstants.DateFilterTypes.PostDate)
				{
					return ViewMatchGroupSchema.MG_PostDate;
				}

				if (DateType == AccConstants.DateFilterTypes.TransactionDate)
				{
					return ViewMatchGroupSchema.MG_InvoiceDate;
				}

				if (DateType == AccConstants.DateFilterTypes.MatchDate)
				{
					return ViewMatchGroupSchema.MG_MatchDate;
				}

				return null;
			}
		}

		#endregion

		#region FromDateFilter

		public ZDateTime FromDateFilter
		{
			get { return fFromDateFilter; }
			set { fFromDateFilter = value; }
		}

		ZDateTime fFromDateFilter;

		#endregion

		#region ToDateFilter

		public ZDateTime ToDateFilter
		{
			get { return fToDateFilter; }
			set
			{
				fToDateFilter = value;
				if (fToDateFilter.IsValid)
				{
					fToDateFilter = new ZDateTime(value.Year, value.Month, value.Day, 23, 59, 59);
				}
			}
		}

		ZDateTime fToDateFilter;

		#endregion

		#region Ledger

		public abstract ZString Ledger { get; }

		#endregion

		#region MaximumRows

		[BusinessObjectTestExclude]
		public int? MaximumRows
		{
			get { return fMaximumRows; }
			set { fMaximumRows = value; }
		}

		int? fMaximumRows;

		#endregion

		#endregion

		#region Filter String

		public ZString PlainFilterWithoutParamValues
		{
			get
			{
				ZString filterString = ZString.Empty;

				if (OuterMatchGroupQuery != null)
				{
					filterString = "SELECT " +
						ViewMatchGroup.Schema.MG_MatchGroupNum
						+ " AS " + UnmatchingRow.Schema.MatchGroupNum + ", " +
						ViewMatchGroup.Schema.MG_MatchDate + " AS " +
						UnmatchingRow.Schema.MatchDate +
						" FROM " + ViewMatchGroupSchema.Constants.SqlSchemaName + "." + ViewMatchGroupSchema.Constants.TableName +
						" WHERE " + ViewMatchGroup.Schema.MG_GC + " = @CompanyPK" +
						" AND " + ViewMatchGroup.Schema.MG_Ledger + " = @Ledger" +
						OuterMatchGroupQueryParametrisedString +
						" GROUP BY " + ViewMatchGroup.Schema.MG_MatchGroupNum + ", " +
						ViewMatchGroup.Schema.MG_MatchDate;
				}
				else
				{
					filterString = "SELECT " +
						ViewMatchGroup.Schema.MG_MatchGroupNum
						+ " AS " + UnmatchingRow.Schema.MatchGroupNum + ", " +
						ViewMatchGroup.Schema.MG_MatchDate + " AS " +
						UnmatchingRow.Schema.MatchDate +
						" FROM " + ViewMatchGroupSchema.Constants.SqlSchemaName + "." + ViewMatchGroupSchema.Constants.TableName +
						" WHERE " + ViewMatchGroup.Schema.MG_GC + " = @CompanyPK" +
						" AND " + ViewMatchGroup.Schema.MG_Ledger + " = @Ledger" +
						OrganisationFilterString +
						NumberFilterString +
						DateFilterString +
						" GROUP BY " + ViewMatchGroup.Schema.MG_MatchGroupNum + ", " +
						ViewMatchGroup.Schema.MG_MatchDate;
				}

				if (MaximumRows != null)
				{
					filterString = "SELECT TOP " + MaximumRows.Value.ToString() + " * FROM(" + filterString + ") AS T1";
				}

				return filterString;
			}
		}

		#endregion

		#region Implementation

		#region OuterMatchGroupQuery

		ZQuery OuterMatchGroupQuery;

		public void SetOuterMatchGroupQuery(ZQuery outerMatchGroupQuery)
		{
			OuterMatchGroupQuery = outerMatchGroupQuery;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Query string template")]
		ZString OuterMatchGroupQueryParametrisedString
		{
			get
			{
				ZString filterString = ZString.Empty;
				if (OuterMatchGroupQuery != null)
				{
					if (OuterMatchGroupQuery.ParameterisedText.ParameterisedQueryText.Trim().Length > 0)
					{
						filterString = " AND ( " + OuterMatchGroupQuery.ParameterisedText.ParameterisedQueryText + " ) ";
					}
				}
				return filterString;
			}
		}

		#endregion

		#region OrganisationFilterString

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Query string template")]
		ZString OrganisationFilterString
		{
			get
			{
				ZString orgFilterString = ZString.Empty;
				if (!Organisation.IsEmpty)
				{
					orgFilterString = " AND " + ViewMatchGroup.Schema.MG_OH + " = @Organisation";
				}
				return orgFilterString;
			}
		}

		#endregion

		#region NumberFilterString

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Query string template")]
		ZString NumberFilterString
		{
			get
			{
				ZString numberFilterString = ZString.Empty;
				if (!NumberType.IsEmpty && !NumberFilter.IsEmpty && NumberType.IsValid && NumberType != AccConstants.NumberFilterTypes.None)
				{
					numberFilterString = " AND " + NumberTypeSchema.Name + " = @Number";
				}
				return numberFilterString;
			}
		}

		#endregion

		#region DateFilterString

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Query string template")]
		ZString DateFilterString
		{
			get
			{
				ZString dateFilterString = ZString.Empty;
				if (!DateType.IsEmpty && DateType != AccConstants.DateFilterTypes.None)
				{
					if (FromDateFilter.IsValid)
					{
						dateFilterString = " AND " + DateTypeSchema.Name + " >= @FromDate";
					}
					if (ToDateFilter.IsValid)
					{
						dateFilterString += " AND " + DateTypeSchema.Name + " <= @ToDate";
					}
				}
				return dateFilterString;
			}
		}

		#endregion

		#endregion

		#region Paramaters

		public ZSqlParameterCollection FilterParameters
		{
			get
			{
				ZSqlParameterCollection filterParams = new ZSqlParameterCollection();

				filterParams.Add("@CompanyPK", GlbCompany.CurrentCompany.PK, ViewMatchGroupSchema.MG_GC);
				filterParams.Add("@Ledger", Ledger, ViewMatchGroupSchema.MG_Ledger);

				if (OuterMatchGroupQuery != null)
				{
					filterParams.AddRange(OuterMatchGroupQuery.ParameterisedText.Parameters);
				}
				else
				{
					if (!Organisation.IsEmpty)
					{
						filterParams.Add("@Organisation", Organisation, ViewMatchGroupSchema.MG_OH);
						filterParams.Add("@PaymentType", ZArchitecture.Core.TransactionTypes.Payment, ViewMatchGroupSchema.MG_TransactionType);
					}

					if (!NumberType.IsEmpty && !NumberFilter.IsEmpty)
					{
						filterParams.Add("@Number", NumberFilter, NumberTypeSchema);
					}

					if (!DateType.IsEmpty && FromDateFilter.IsValid)
					{
						filterParams.Add("@FromDate", FromDateFilter, DateTypeSchema);
					}

					if (!DateType.IsEmpty && ToDateFilter.IsValid)
					{
						filterParams.Add("@ToDate", ToDateFilter, DateTypeSchema);
					}
				}

				return filterParams;
			}
		}

		#endregion
	}
}
