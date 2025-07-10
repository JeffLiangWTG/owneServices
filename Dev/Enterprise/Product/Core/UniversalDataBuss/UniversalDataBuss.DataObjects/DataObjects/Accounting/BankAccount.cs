using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class BankAccount : IDataObject
	{
		public ZBool? IsDefaultAccount { get; set; } // A1_IsDefaultAccount

		[MaxLength(3)]
		public ZString? Method { get; set; } // A1_PaymentMethod
		[MaxLength(3)]
		public ZString? Currency { get; set; } // A1_RX_NKAccountCurrency
		[MaxLength(35)]
		public ZString? AccountName { get; set; } // A1_AccountName
		[MaxLength(35)]
		public ZString? BankName { get; set; } // A1_BankName
		[MaxLength(35)]
		public ZString? BankBranchName { get; set; } // A1_BankBranchName
		[MaxLength(35)]
		public ZString? BankSwift { get; set; } // A1_BankSwift
		[MaxLength(15)]
		public ZString? BankBranch { get; set; } // A1_BankBsb
		[MaxLength(35)]
		public ZString? AccountNumber { get; set; } //A1_BankAccount
		public Country Country { get; set; } //A1_RN_NKCountryCode
		[MaxLength(35)]
		public ZString? IBANNumber { get; set; } //A1_IBANNumber
		[MaxLength(20)]
		public ZString? EFTUserId { get; set; }
		public BankAccountType? AccountType { get; set; }
		[MaxLength(3)]
		public CodeDescriptionPair AutoDDRFormat { get; set; }
		[MaxLength(10)]
		public ZString? BankAccountCode { get; set; }
		[MaxLength(50)]
		public ZString? BankAccountDescription { get; set; }
		[MaxLength(100)]
		public ZString? BankUniqueAccNo { get; set; }
		[MaxLength(192)]
		public ZString? BankAddress { get; set; }
	}
}
