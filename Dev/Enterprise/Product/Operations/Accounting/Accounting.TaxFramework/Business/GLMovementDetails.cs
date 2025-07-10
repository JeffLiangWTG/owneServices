using CargoWise.Types;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public interface IGLMovementDetails
	{
		ZString TaxConfiguration { get; set; }
		ZString GLAccount { get; set; }
		ZString GLAccountDesc { get; set; }
		ZString BranchCode { get; set; }
		ZString DepartmentCode { get; set; }
		ZDecimal Amount { get; set; }
		ZDate PostDate { get; set; }
		ZString PostPeriod { get; set; }
		ZString Basis { get; set; }
		ZDecimal OSAmount { get; set; }
		ZString CurrencyCode { get; set; }
	}

	public class GLMovementDetails : IGLMovementDetails
	{
		ZString IGLMovementDetails.TaxConfiguration { get; set; }
		ZString IGLMovementDetails.GLAccount { get; set; }
		ZString IGLMovementDetails.GLAccountDesc { get; set; }
		ZString IGLMovementDetails.BranchCode { get; set; }
		ZString IGLMovementDetails.DepartmentCode { get; set; }
		ZDecimal IGLMovementDetails.Amount { get; set; }
		ZDate IGLMovementDetails.PostDate { get; set; }
		ZString IGLMovementDetails.PostPeriod { get; set; }
		ZString IGLMovementDetails.Basis { get; set; }
		ZDecimal IGLMovementDetails.OSAmount { get; set; }
		ZString IGLMovementDetails.CurrencyCode { get; set; }

		public abstract class Schema
		{
			public const string TaxConfiguration = "ETC_Code";
			public const string GLAccount = "Account";
			public const string GLAccountDesc = "Desc";
			public const string BranchCode = "GB_Code";
			public const string DepartmentCode = "GE_Code";
			public const string Amount = "ATM_Amount";
			public const string PostDate = "ATM_Date";
			public const string PostPeriod = "ATM_Period";
			public const string Basis = "ATT_Basis";
			public const string OSAmount = "OSAmount";
			public const string CurrencyCode = "Currency";
		}
	}
}
