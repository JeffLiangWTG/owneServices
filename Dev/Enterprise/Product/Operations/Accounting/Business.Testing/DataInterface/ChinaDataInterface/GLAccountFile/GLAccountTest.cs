using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	public class GLAccountTest : FileOutputTest
	{
		protected override void SetValuesBeforeAssert()
		{
		}

		protected override IFileOutput GetOutputToTest()
		{
			AccGLAccountDescriptor account = Factory.New(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			account.AJ_AccountDescription = "其他应收款-存出保证金";
			account.AJ_LocalAccountNumber = "1133.01";
			account.AJ_DebitCredit = "DR";
			account.AJ_Language = Constants.Languages.ChineseSimplified;
			GLAccount testAccount = new GLAccount(account);
			return testAccount;
		}

		protected override string SetExpectedResult()
		{
			return DataInterfaceConstant.Quote + "1133.01" + DataInterfaceConstant.Quote + DataInterfaceConstant.Tab + DataInterfaceConstant.Quote + "其他应收款-存出保证金" + DataInterfaceConstant.Quote + DataInterfaceConstant.Tab + "1" + System.Environment.NewLine;
		}
	}
}