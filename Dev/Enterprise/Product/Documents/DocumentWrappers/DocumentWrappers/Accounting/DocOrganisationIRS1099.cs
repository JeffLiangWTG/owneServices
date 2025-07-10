using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappersCore;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers
{
	public class DocOrganisationIRS1099 : DocOrganisation, IParametrizedDocWrapper
	{
		protected DocOrganisationIRS1099(IOrganisationDetails source, BusinessObjectFactory factoryForWrapper)
			: base(source, factoryForWrapper)
		{
		}

		public new static DocOrganisationIRS1099 New(OrgHeader orgHeader, BusinessObjectFactory factoryForWrapper)
		{
			return orgHeader == null ? null : new DocOrganisationIRS1099(OrgHeaderSource.New(orgHeader, factoryForWrapper), factoryForWrapper);
		}

		#region Properties

		ZDecimal? fPayAmount;
		public ZDecimal PayAmount
		{
			get { return fPayAmount ?? (fPayAmount = GetPayAmount()).Value; }
		}

		ZDecimal GetPayAmount()
		{
			DbCommand command = Db.Connection.Command(TotalPayAmountSQL);
			command.AddParameter("@Company", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
			command.AddParameter("@Year", SqlDbType.Int, Parameters["Calendar Year"]);
			return new ZDecimal(Utilities.ConvertToDecimal(command.ExecuteScalar()));
		}

		string TotalPayAmountSQL
		{
			get
			{
				return @"
SELECT 
	SUM(" + AccTransactionHeaderSchema.Constants.AH_InvoiceAmount + @") AS TotalPayAmount
FROM
	" + AccTransactionHeaderSchema.Constants.SqlSchemaName + "." + AccTransactionHeaderSchema.Constants.TableName + @" 
	INNER JOIN " + GlbBranchSchema.Constants.SqlSchemaName + "." + GlbBranchSchema.Constants.TableName + "  ON " + GlbBranchSchema.Constants.PK + " = " + AccTransactionHeaderSchema.Constants.AH_GB + " AND " + GlbBranchSchema.Constants.GB_GC + @" = @Company
WHERE
	" + AccTransactionHeaderSchema.Constants.AH_TransactionType + " = '" + TransactionTypes.Payment + @"'
	AND " + AccTransactionHeaderSchema.Constants.AH_Ledger + " = '" + LedgerTypes.AccountsPayable + @"'
	AND " + AccTransactionHeaderSchema.Constants.AH_OH + " = '" + OrgHeader.PK + @"'
	AND DATEPART(YEAR, " + AccTransactionHeaderSchema.Constants.AH_PostDate + @") = @Year";
			}
		}

		#endregion

		#region IParametrizedDocWrapper Members

		public Dictionary<string, object> Parameters
		{
			get { return fParameters; }
			set
			{
				fPayAmount = null;
				fParameters = value;
			}
		}

		Dictionary<string, object> fParameters;

		#endregion
	}
}
