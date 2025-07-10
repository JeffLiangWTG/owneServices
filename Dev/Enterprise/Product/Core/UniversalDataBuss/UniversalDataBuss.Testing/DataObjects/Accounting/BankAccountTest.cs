using System.Collections.Generic;
using System.Linq;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(BankAccount))]
	class BankAccountTest : DataObjectTestCase<BankAccount>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			int bankBranchNameMaxLength = new[] { AccAPAccountDetailsSchema.A1_BankBranchName.MaxLength, GetDefaultFieldLength() }.Max();

			return new Dictionary<string, int>()
			{
				{ nameof(BankAccount.Method), AccAPAccountDetailsSchema.A1_PaymentMethod.MaxLength },
				{ nameof(BankAccount.Currency), AccAPAccountDetailsSchema.A1_RX_NKAccountCurrency.MaxLength },
				{ nameof(BankAccount.AccountName), AccAPAccountDetailsSchema.A1_AccountName.MaxLength },
				{ nameof(BankAccount.BankName), AccAPAccountDetailsSchema.A1_BankName.MaxLength },
				{ nameof(BankAccount.BankBranchName), bankBranchNameMaxLength },
				{ nameof(BankAccount.BankSwift), AccAPAccountDetailsSchema.A1_BankSwift.MaxLength },
				{ nameof(BankAccount.BankBranch), AccAPAccountDetailsSchema.A1_BankBsb.MaxLength },
				{ nameof(BankAccount.AccountNumber), AccAPAccountDetailsSchema.A1_BankAccount.MaxLength },
				{ nameof(BankAccount.IBANNumber), AccAPAccountDetailsSchema.A1_IBANNumber.MaxLength },
				{ nameof(BankAccount.EFTUserId), AccBankAccountSchema.AB_AccountEFTUserID.MaxLength },
				{ nameof(BankAccount.AutoDDRFormat), AccBankAccountSchema.AB_AutoDDRFormat.MaxLength },
				{ nameof(BankAccount.BankAccountCode), AccBankAccountSchema.AB_Code.MaxLength },
				{ nameof(BankAccount.BankAccountDescription), AccBankAccountSchema.AB_Desc.MaxLength },
				{ nameof(BankAccount.BankUniqueAccNo), AccBankAccountSchema.AB_FullAccountNumber.MaxLength },
				{ nameof(BankAccount.BankAddress), AccBankAccountSchema.AB_BankAddress.MaxLength }
			};
		}
	}
}

