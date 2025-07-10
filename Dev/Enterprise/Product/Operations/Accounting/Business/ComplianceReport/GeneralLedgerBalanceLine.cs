using System;
using System.Data;

using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public class GeneralLedgerBalanceLine : NonPersistentBusinessObject, IObsoleteValidation
	{
		public GeneralLedgerBalanceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public GeneralLedgerBalanceLine()
			: base()
		{
		}

		public abstract class Schema
		{
			public const string PK = "PK";
			public const string AG_PK = "AG_PK";
			public const string AG_AccountNum = "AG_AccountNum";
			public const string AG_Description = "AG_Description";
			public const string AG_AccountType = "AG_AccountType";
			public const string AG_DebitCredit = "AG_DebitCredit";
			public const string AG_AG_ConsolidationNum = "AG_AG_ConsolidationNum";
			public const string AG_ConsolidationAccountNum = "AG_ConsolidationAccountNum";
			public const string AG_ConsolidationDescription = "AG_ConsolidationDescription";
			public const string AG_ConsolidationAccountType = "AG_ConsolidationAccountType";
			public const string GeneralLedgerAmountDR = "GeneralLedgerAmountDR";
			public const string GeneralLedgerAmountCR = "GeneralLedgerAmountCR";
		}

		public static ZDataTable GetDataTable(AccComplianceReport report)
		{
			var result = new ZDataTable();
			result.Columns.Add(GeneralLedgerBalanceLine.Schema.PK, typeof(Guid));
			result.Columns.Add(GeneralLedgerBalanceLine.Schema.AG_PK, typeof(Guid));
			result.Columns.Add(GeneralLedgerBalanceLine.Schema.AG_AccountNum, typeof(string));
			result.Columns.Add(GeneralLedgerBalanceLine.Schema.AG_Description, typeof(string));
			result.Columns.Add(GeneralLedgerBalanceLine.Schema.AG_AccountType, typeof(string));
			result.Columns.Add(GeneralLedgerBalanceLine.Schema.AG_DebitCredit, typeof(string));
			result.Columns.Add(GeneralLedgerBalanceLine.Schema.AG_AG_ConsolidationNum, typeof(Guid));
			result.Columns.Add(GeneralLedgerBalanceLine.Schema.AG_ConsolidationAccountNum, typeof(string));
			result.Columns.Add(GeneralLedgerBalanceLine.Schema.AG_ConsolidationDescription, typeof(string));
			result.Columns.Add(GeneralLedgerBalanceLine.Schema.AG_ConsolidationAccountType, typeof(string));
			result.Columns.Add(GeneralLedgerBalanceLine.Schema.GeneralLedgerAmountDR, typeof(decimal));
			result.Columns.Add(GeneralLedgerBalanceLine.Schema.GeneralLedgerAmountCR, typeof(decimal));
			return result;
		}

		protected DataRow Row
		{
			get { return ((IBusinessObjectInternals)this).Row; }
		}

		#region Properties

		public ZGuid AG_PK => new ZGuid(Row[Schema.AG_PK]);

		public ZString AG_AccountNum => new ZString(Row[Schema.AG_AccountNum]);

		public ZString AG_Description => new ZString(Row[Schema.AG_Description]);

		public ZString AG_AccountType => new ZString(Row[Schema.AG_AccountType]);

		public ZString AG_DebitCredit => new ZString(Row[Schema.AG_DebitCredit]);

		public ZGuid AG_AG_ConsolidationNum => new ZGuid(Row[Schema.AG_AG_ConsolidationNum]);

		[ResourceStringData("AG_ConsolidationAccountNum", Caption = "Consolidation Account", ShortCaption = "Cons. Account")]
		public ZString AG_ConsolidationAccountNum => new ZString(Row[Schema.AG_ConsolidationAccountNum]);

		[ResourceStringData("AG_ConsolidationDescription", Caption = "Consolidation Account Description", ShortCaption = "Cons. Account Desc.")]
		public ZString AG_ConsolidationDescription => new ZString(Row[Schema.AG_ConsolidationDescription]);

		[ResourceStringData("AG_ConsolidationAccountType", Caption = "Consolidation Account Type", ShortCaption = "Cons. Account Type")]
		public ZString AG_ConsolidationAccountType => new ZString(Row[Schema.AG_ConsolidationAccountType]);

		[ResourceStringData("GeneralLedgerAmountDR", Caption = "GL Amount Debit", ShortCaption = "GL DR")]
		public ZDecimal GeneralLedgerAmountDR => new ZDecimal(Row[Schema.GeneralLedgerAmountDR]);

		[ResourceStringData("GeneralLedgerAmountCR", Caption = "GL Amount Credit", ShortCaption = "GL CR")]
		public ZDecimal GeneralLedgerAmountCR => new ZDecimal(Row[Schema.GeneralLedgerAmountCR]);

		#endregion
	}
}
