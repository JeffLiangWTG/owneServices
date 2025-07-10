using System;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using AlternateGLAccounts = Enterprise.Accounting.Business.AlternateGLAccounts;

namespace Enterprise.Accounting.GUI
{
	public class AlternateGLAccountZGuidFindBox : ZGuidFindBox, IShowEditOrViewForm
	{
		protected override IModuleDecisionProvider GetModuleDecisionProvider(ZFilterModule module)
		{
			return new AlternateGLAccountModuleDecisionProvider(this);
		}

		void IShowEditOrViewForm.SetCodePropertyFromText(IZForm form)
		{
			var zForm = form as ZForm;
			if (zForm != null)
			{
				var bizObj = zForm.BusinessEntity as AlternateGLAccounts;
				if (bizObj != null)
				{
					var firstAlternateGLAccountWithAttributeSet = bizObj.FirstAlternateGLAccountWithAttributeSet;
					var accountNum = IFindBox.Code;
					var account = firstAlternateGLAccountWithAttributeSet.AlternateGLAccount;
					var maxLength = account.AGA_AccountNumInfo?.MaxLength ?? int.MaxValue;
					var trimmedValue = (maxLength > -1) ? accountNum.Substring(0, Math.Min(accountNum.Length, maxLength)) : accountNum;
					account[AccAlternateGLAccountSchema.Constants.AGA_AccountNum] = trimmedValue;
					firstAlternateGLAccountWithAttributeSet.AlternateGLAccountNum = trimmedValue;
					bizObj.RefreshBinding();
					zForm.DisplayMode = ODisplayMode.New;
				}
			}
		}
	}
}
