using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public class AlternateGLAccountsValidation : ZValidation
	{
		public AlternateGLAccountsValidation(AlternateGLAccounts parent, BusinessObjectFactory factory) : base(parent)
		{
			this.Parent = parent;
			Factory = factory;
		}

		readonly AlternateGLAccounts Parent;
		readonly BusinessObjectFactory Factory;

		public override Type AutoValidationType => GetType();

		public void ValidateChartPK()
		{
			ValidateCalculatedProperty(Parent.ChartPKInfo);
		}

		protected virtual void CheckChartPK()
		{
			if (!Parent.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.ChartPKInfo);

				if (!Parent.ChartPKInfo.HasErrors())
				{
					ListValidation.ErrorIfInvalidPK(Parent.ChartPKInfo);
				}
			}
		}

		public void ValidateAccountType()
		{
			ValidateCalculatedProperty(Parent.AccountTypeInfo);
		}

		protected virtual void CheckAccountType()
		{
			if (!Parent.ReadOnly)
			{
				var alternateGLAccount = Parent.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount;
				alternateGLAccount.Validation.ValidateAGA_AccountType();
				if (alternateGLAccount.AGA_AccountTypeInfo.HasErrors())
				{
					var errorMessageBuilder = new ZStringBuilder(alternateGLAccount.AGA_AccountTypeInfo.GetErrors().Select(n => n.Message));
					Parent.AccountTypeInfo.AddError(errorMessageBuilder.ToStringWithNewLineBetweenAppends());
				}
			}
		}

		public void ValidateParentGLAccountPK()
		{
			ValidateCalculatedProperty(Parent.ParentGLAccountPKInfo);
		}

		protected virtual void CheckParentGLAccountPK()
		{
			if (!Parent.ReadOnly && !Parent.ParentGLAccountPKInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.ParentGLAccountPKInfo);

				if (!Parent.ParentGLAccountPKInfo.HasErrors())
				{
					ListValidation.ErrorIfInvalidPK(Parent.ParentGLAccountPKInfo);
				}

				if (!Parent.ParentGLAccountPKInfo.HasErrors() && !string.IsNullOrEmpty(Parent.AccountType))
				{
					var glHeader = Factory.Load<AccGLHeader>(Parent.ParentGLAccountPK);
					if (glHeader.AG_AccountType != Parent.AccountType)
					{
						Parent.ParentGLAccountPKInfo.AddError(Res.GetString("349F7A9F-691F-43DC-BBCE-114762F6B940", "Parent GL Accounts should match up with the Account Type."));
					}

					if (!Parent.ParentGLAccountPKInfo.HasErrors() && !glHeader.AlternateGLAccountDissections.Any())
					{
						var query = new ZQuery();
						query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AG_GLHeader, Parent.ParentGLAccountPK);
						query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AAC_AlternateChart, Parent.ChartPK);
						query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AGA_AlternateGLAccount, SQLComparisonOperator.NotEqual, Parent.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.PK);
						var attribute = Factory.LoadTop1<AccAlternateGLAccountAttribute>(query);
						if (attribute != null)
						{
							Parent.ParentGLAccountPKInfo.AddError(Res.GetString("F935B16A-AC7B-45D7-9765-F1BC0CD66675", "The selected Parent Account has been referenced to Alternate Account {0} of Chart {1}, please enter another one.", attribute.AlternateGLAccount.AGA_AccountNum, attribute.AlternateChart.AAC_Code));
						}
					}
				}

				if (!Parent.ParentGLAccountPKInfo.HasErrors()
					&& Parent.IsInDatabase
					&& Parent.OriginalParentGLAccount != null
					&& Parent.ParentGLAccount != null
					&& !Parent.OriginalParentGLAccount.AlternateGLAccountDissections.Cast<AccAlternateGLAccountDissection>().Any(x =>
						x.ADC_AAC_AlternateChart == Parent.ChartPK
						&& x.ADC_SeparateNumbering)
					&& Parent.ParentGLAccount.AlternateGLAccountDissections.Cast<AccAlternateGLAccountDissection>().Any(x =>
						x.ADC_AAC_AlternateChart == Parent.ChartPK
						&& x.ADC_SeparateNumbering))
				{
					Parent.ParentGLAccountPKInfo.AddError(Res.GetString("2F02C663-D0E7-4D2C-9E03-98CEB2054F38", "You cannot change the Parent Account to '{0}' as it has dissection configuration with separate numbering. Please change the Parent Account back to '{1}'.", Parent.ParentGLAccount.AG_AccountNum, Parent.OriginalParentGLAccount.AG_AccountNum));
				}
			}
		}

		public override void ValidateAll()
		{
			ValidateChartPK();
			ValidateAccountType();
			ValidateParentGLAccountPK();
		}
	}
}
