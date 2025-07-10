using System.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	public class GLAccount : IFileOutput
	{
		public GLAccount(AccGLAccountDescriptor account)
		{
			fAccount = account;
		}

		readonly AccGLAccountDescriptor fAccount;

		#region IFileElement Members

		public void Write(StreamWriter writer)
		{
			string output = DataInterfaceConstant.Quote + DataInterfaceUtils.RemoveTrailingZero(AccountNum) + DataInterfaceConstant.Quote + DataInterfaceConstant.Tab +
				DataInterfaceConstant.Quote + AccountDescription + DataInterfaceConstant.Quote + DataInterfaceConstant.Tab +
				DebitCredit;
			writer.WriteLine(output);
		}

		#endregion

		#region Properties

		string AccountNum
		{
			get
			{
				if (fAccountNum.IsEmpty && fAccount != null)
				{
					fAccountNum = fAccount.AJ_LocalAccountNumber;
				}
				return fAccountNum;
			}
		}

		ZString fAccountNum;

		string AccountDescription
		{
			get
			{
				if (fAccountDescription.IsEmpty)
				{
					fAccountDescription = fAccount.AJ_AccountDescription;
				}
				return fAccountDescription;
			}
		}

		ZString fAccountDescription;

		string DebitCredit
		{
			get
			{
				if (fDebitCredit.IsEmpty && fAccount != null)
				{
					fDebitCredit = fAccount.AJ_DebitCredit == "DR" ? "1" : "-1";
				}
				return fDebitCredit;
			}
		}

		ZString fDebitCredit;

		#endregion

	}
}
