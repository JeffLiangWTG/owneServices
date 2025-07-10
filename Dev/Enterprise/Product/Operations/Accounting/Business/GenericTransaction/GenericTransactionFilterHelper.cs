using System.Text;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.GenericTransaction
{
	public class GenericTransactionFilterHelper
	{
		public GenericTransactionFilterHelper(ZQuery filter)
			: this(filter, true)
		{
		}

		public GenericTransactionFilterHelper(ZQuery filter, bool observeMaximumRows)
		{
			this.Filter = filter;
			this.ObserveMaximumRows = observeMaximumRows;
			MaximumRows = filter.MaximumRows;
		}

		#region ParameterisedText

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "SQL query")]
		public string ParameterisedText
		{
			get
			{
				string topClause = (ObserveMaximumRows && MaximumRows != null) ? ("TOP " + MaximumRows.Value.ToString() + " ") : "";
				string result = "SELECT " + topClause + "* FROM GetGenericTransactions('" + ParametersList + "')" + Filter.GetAsWhereClause(false);

				return result;
			}
		}

		#endregion

		#region MaximumRows

		[BusinessObjectTestExclude]
		public int? MaximumRows
		{
			get { return fMaximumRows; }
			set { fMaximumRows = value; }
		}

		int? fMaximumRows;

		readonly bool ObserveMaximumRows;

		#endregion

		#region Implementation

		public ZQuery Filter;

		#region ParametersList

		string ParametersList
		{
			get
			{
				StringBuilder result = new StringBuilder();
				result.Append(GlbCompany.CurrentCompany.PK.ToGuid().ToString());

				return result.ToString();
			}
		}

		#endregion

		#region GetParameterName

		#endregion

	}
}
#endregion
