using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public class AlternateGLAccountWithAttributeSetValidation : ZValidation
	{
		public AlternateGLAccountWithAttributeSetValidation(AlternateGLAccountWithAttributeSet parent, BusinessObjectFactory factory) : base(parent)
		{
			Parent = parent;
			Factory = factory;
		}

		readonly AlternateGLAccountWithAttributeSet Parent;
		readonly BusinessObjectFactory Factory;

		public void ValidateAlternateGLAccountNum()
		{
			ValidateCalculatedProperty(Parent.AlternateGLAccountNumInfo);
		}

		protected virtual void CheckAlternateGLAccountNum()
		{
			if (!Parent.ReadOnly)
			{
				if (Parent.AlternateGLAccount != null)
				{
					Parent.AlternateGLAccount.Validation.ValidateAGA_AccountNum();
					AddErrorMessage(Parent.AlternateGLAccount.AGA_AccountNumInfo, Parent.AlternateGLAccountNumInfo);
				}

				if (!Parent.AlternateGLAccountNumInfo.HasErrors()
					&& !string.IsNullOrEmpty(Parent.AccountType)
					&& Parent.AlternateGLAccount.IsInDatabase
					&& ((ZString)Parent.AlternateGLAccount.AGA_AccountTypeInfo.OriginalValue) != Parent.AccountType)
				{
					Parent.AlternateGLAccountNumInfo.AddError(Res.GetString("212B5CE9-8DD1-40C1-AF8E-9E23ABFD89C0", @"The specified Alternate Account's Number '{0}' already existed in the Alternate Chart '{1}' with a different Account Type '{2}'.
Please enter a different value.", Parent.AlternateGLAccountNum, Parent.Chart.AAC_Code, Parent.AlternateGLAccount.AGA_AccountTypeInfo.OriginalValue));
				}

				if (!Parent.AlternateGLAccountNumInfo.HasErrors()
					&& Parent.ParentGLAccountPK.IsValid
					&& Parent.Chart != null
					&& Parent.AlternateGLAccount != null)
				{
					var query = new ZQuery();
					query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AAC_AlternateChart, Parent.Chart.PK);
					query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AG_GLHeader, Parent.ParentGLAccountPK);
					query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AGA_AlternateGLAccount, Parent.AlternateGLAccount.PK);
					var attributes = Factory.Load<AccAlternateGLAccountAttribute>(query);

					if (attributes.Select(x => x.AAA_Sequence).Distinct().Count() > 1)
					{
						Parent.AlternateGLAccountNumInfo.AddError(Res.GetString("BE78882C-DA2B-424F-80CF-6DD527D51BC5", "Cannot be the same Account Number for different combinations."));
					}
				}
			}
		}

		public void ValidateDebitCredit()
		{
			ValidateCalculatedProperty(Parent.DebitCreditInfo);
		}

		protected void CheckDebitCredit()
		{
			if (!Parent.ReadOnly)
			{
				Parent.AlternateGLAccount.Validation.ValidateAGA_DebitCredit();
				AddErrorMessage(Parent.AlternateGLAccount.AGA_DebitCreditInfo, Parent.DebitCreditInfo);
			}
		}

		public void ValidateDescription()
		{
			ValidateCalculatedProperty(Parent.DescriptionInfo);
		}

		protected void CheckDescription()
		{
			if (!Parent.ReadOnly)
			{
				Parent.AlternateGLAccount.Validation.ValidateAGA_Description();
				AddErrorMessage(Parent.AlternateGLAccount.AGA_DescriptionInfo, Parent.DescriptionInfo);
			}
		}

		public void ValidateReportSection()
		{
			ValidateCalculatedProperty(Parent.ReportSectionInfo);
		}

		protected void CheckReportSection()
		{
			if (!Parent.ReadOnly)
			{
				Parent.AlternateGLAccount.Validation.ValidateAGA_ReportSection();
				AddErrorMessage(Parent.AlternateGLAccount.AGA_ReportSectionInfo, Parent.ReportSectionInfo);
			}
		}

		public void ValidateTotalLevel()
		{
			ValidateCalculatedProperty(Parent.TotalLevelInfo);
		}

		protected void CheckTotalLevel()
		{
			if (!Parent.ReadOnly)
			{
				Parent.AlternateGLAccount.Validation.ValidateAGA_TotalLevel();
				AddErrorMessage(Parent.AlternateGLAccount.AGA_TotalLevelInfo, Parent.TotalLevelInfo);
			}
		}

		public void ValidatePrintSequence()
		{
			ValidateCalculatedProperty(Parent.PrintSequenceInfo);
		}

		protected void CheckPrintSequence()
		{
			if (!Parent.ReadOnly)
			{
				Parent.AlternateGLAccount.Validation.ValidateAGA_PrintSequence();
				AddErrorMessage(Parent.AlternateGLAccount.AGA_PrintSequenceInfo, Parent.PrintSequenceInfo);
			}
		}

		public void ValidatePercentNum()
		{
			ValidateCalculatedProperty(Parent.PercentNumInfo);
		}

		protected void CheckPercentNum()
		{
			if (!Parent.ReadOnly)
			{
				Parent.AlternateGLAccount.Validation.ValidateAGA_AGA_PercentNum();
				AddErrorMessage(Parent.AlternateGLAccount.AGA_AGA_PercentNumInfo, Parent.PercentNumInfo);
			}
		}

		public void ValidateConsolidationNum()
		{
			ValidateCalculatedProperty(Parent.ConsolidationNumInfo);
		}

		protected void CheckConsolidationNum()
		{
			if (!Parent.ReadOnly)
			{
				Parent.AlternateGLAccount.Validation.ValidateAGA_AGA_ConsolidationNum();
				AddErrorMessage(Parent.AlternateGLAccount.AGA_AGA_ConsolidationNumInfo, Parent.ConsolidationNumInfo);
			}
		}

		public void ValidateAlternateNum()
		{
			ValidateCalculatedProperty(Parent.AlternateNumInfo);
		}

		protected void CheckAlternateNum()
		{
			if (!Parent.ReadOnly)
			{
				Parent.AlternateGLAccount.Validation.ValidateAGA_AGA_AlternateNum();
				AddErrorMessage(Parent.AlternateGLAccount.AGA_AGA_AlternateNumInfo, Parent.AlternateNumInfo);
			}
		}

		public void ValidateHeaderDependsOnTotal()
		{
			ValidateCalculatedProperty(Parent.HeaderDependsOnTotalInfo);
		}

		protected void CheckHeaderDependsOnTotal()
		{
			if (!Parent.ReadOnly)
			{
				Parent.AlternateGLAccount.Validation.ValidateAGA_AGA_HeaderDependsOnTotal();
				AddErrorMessage(Parent.AlternateGLAccount.AGA_AGA_HeaderDependsOnTotalInfo, Parent.HeaderDependsOnTotalInfo);
			}
		}

		void AddErrorMessage(ZPropertyInfo sourceInfo, ZPropertyInfo targetInfo)
		{
			if (sourceInfo.HasErrors())
			{
				var errorMessageBuilder = new ZStringBuilder(sourceInfo.GetErrors().Select(n => n.Message));
				targetInfo.AddError(errorMessageBuilder.ToStringWithNewLineBetweenAppends());
			}
		}

		public override Type AutoValidationType => GetType();

		public override void ValidateAll()
		{
			ValidateAlternateGLAccountNum();
			ValidateDebitCredit();
			ValidateDescription();
			ValidateReportSection();
			ValidateTotalLevel();
			ValidatePrintSequence();
			ValidatePercentNum();
			ValidateConsolidationNum();
			ValidateAlternateNum();
			ValidateHeaderDependsOnTotal();
		}
	}
}
