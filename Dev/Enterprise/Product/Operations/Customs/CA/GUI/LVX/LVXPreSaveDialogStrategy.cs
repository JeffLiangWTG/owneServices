using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	class LVXPreSaveDialogStrategy : PreSaveDialogStrategy
	{
		public LVXPreSaveDialogStrategy(JobDeclaration source)
		{
			this.source = source;
		}
		readonly JobDeclaration source;

		ZBool ShouldRunPreSaveActionForLVX => source != null && source.IsLVX && (source.LVXInvoiceHeader?.IsAttachedToPersistentLVXDeclaration ?? ZBool.False);

		#region Override

		protected override bool ShouldRunPreSaveAction()
		{
			if (ShouldRunPreSaveActionForLVX)
			{
				var lvxInvoiceHeader = source.LVXInvoiceHeader;
				if (lvxInvoiceHeader.IsRemissionAll())
				{
					return lvxInvoiceHeader.CheckCurrentRemissionConditions(LVXJobComInvoiceHeaderExtension.RemissionAll) ? true : GetUserContinueResult(RemissionAllMessage);
				}
				else if (lvxInvoiceHeader.IsRemissionMexicoAndUSDutyAndTax())
				{
					return lvxInvoiceHeader.CheckCurrentRemissionConditions(LVXJobComInvoiceHeaderExtension.RemissionMexicoAndUSDutyAndTax) ? true : GetUserContinueResult(RemissionMexicoAndUSDutyAndTaxMessage);
				}
				else if (lvxInvoiceHeader.IsRemissionMexicoAndUSDutyOnly())
				{
					return lvxInvoiceHeader.CheckCurrentRemissionConditions(LVXJobComInvoiceHeaderExtension.RemissionMexicoAndUSDutyOnly) ? true : GetUserContinueResult(RemissionMexicoAndUSDutyOnlyMessage);
				}
			}
			return false;
		}

		internal string RemissionAllMessage => string.Format(@"Goods with a value for duty of CAD ${0:0.00} or less will have any applicable customs duties and taxes waived when imported from any country/region.

Click Apply to override duties and taxes to set them to CAD $0.00 and use Special Authority code {1} or click Cancel to not apply the Special Authority code {1}.", CLVSThresholdRT1, DutyAndTaxManager.TaxRemittedOICNumber1);

		internal string RemissionMexicoAndUSDutyAndTaxMessage => string.Format(@"Goods with a value for duty of CAD ${0:0.00} or less will have any applicable customs duties and taxes waived when imported from United States or Mexico only.

Click Apply to override duties and taxes to set them to CAD $0.00 and use Special Authority code {1} or click Cancel to not apply the Special Authority code {1}.", CLVSThresholdRT2, DutyAndTaxManager.TaxRemittedOICNumber2);

		internal string RemissionMexicoAndUSDutyOnlyMessage => string.Format(@"Goods with a value for duty of CAD ${0:0.00} or greater, up to and including CAD ${1:0.00}, will be free of customs duties (however, taxes will remain applicable) when imported from the United States or Mexico only.

Click Apply to override duties to set them to CAD $0.00 and use Special Authority code {2} or click Cancel to not apply the Special Authority code {2}.", CLVSThresholdRT2 + 0.01m, CLVSThresholdRT3, DutyAndTaxManager.TaxRemittedOICNumber3);

		#region Thresholds

		ZDecimal CLVSThresholdRT1
		{
			get
			{
				if (!thresholdRT1.HasValue)
				{
					var lvxInvoiceHeader = source?.LVXInvoiceHeader;
					if (lvxInvoiceHeader != null)
					{
						thresholdRT1 = UniversalReferenceConstants.GetCLVSThreshold(source.Factory, lvxInvoiceHeader.JZ_ValuationDateOverride.IsValid ? lvxInvoiceHeader.JZ_ValuationDateOverride : ZDateTime.Today, Universal.Constants.RefCusTaxOrFeeTypes.CACLVSRemissionThresholdAll);
					}
				}
				return thresholdRT1.Value;
			}
		}
		ZDecimal? thresholdRT1;

		ZDecimal CLVSThresholdRT2
		{
			get
			{
				if (!thresholdRT2.HasValue)
				{
					var lvxInvoiceHeader = source?.LVXInvoiceHeader;
					if (lvxInvoiceHeader != null)
					{
						thresholdRT2 = UniversalReferenceConstants.GetCLVSThreshold(source.Factory, lvxInvoiceHeader.JZ_ValuationDateOverride.IsValid ? lvxInvoiceHeader.JZ_ValuationDateOverride : ZDateTime.Today, Universal.Constants.RefCusTaxOrFeeTypes.CACLVSRemissionThresholdDutyAndTax);
					}
				}
				return thresholdRT2.Value;
			}
		}
		ZDecimal? thresholdRT2;

		ZDecimal CLVSThresholdRT3
		{
			get
			{
				if (!thresholdRT3.HasValue)
				{
					var lvxInvoiceHeader = source?.LVXInvoiceHeader;
					if (lvxInvoiceHeader != null)
					{
						thresholdRT3 = UniversalReferenceConstants.GetCLVSThreshold(source.Factory, lvxInvoiceHeader.JZ_ValuationDateOverride.IsValid ? lvxInvoiceHeader.JZ_ValuationDateOverride : ZDateTime.Today, Universal.Constants.RefCusTaxOrFeeTypes.CACLVSRemissionThresholdDutyOnly);
					}
				}
				return thresholdRT3.Value;
			}
		}
		ZDecimal? thresholdRT3;

		#endregion

		ZBool GetUserContinueResult(string message)
		{
			using (var messageBox = new ZMessageBox(message, Res.GetString("E1991D61-3594-451D-9E83-F694A9AB56BB", "Override Duties and Taxes?"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question, Res.GetString("8109A4CB-B6B8-4CE6-BC65-AD9A6D50292F", "Apply")))
			{
				return ZFormModaliser.ShowMessageBoxWithoutDispose(messageBox) == DialogResult.OK;
			}
		}

		protected override ContinueWithSave RunPreSaveAction()
		{
			if (ShouldRunPreSaveActionForLVX)
			{
				using (source.SuspendMarkApportionmentDirty())
				{
					var lvxInvoiceHeader = source.LVXInvoiceHeader;
					if (lvxInvoiceHeader.IsRemissionAll())
					{
						lvxInvoiceHeader.ApplyCLVSRemissionThresholdAll();
					}
					else if (lvxInvoiceHeader.IsRemissionMexicoAndUSDutyAndTax())
					{
						lvxInvoiceHeader.ApplyCLVSRemissionMexicoAndUSDutyAndTax();
					}
					else if (lvxInvoiceHeader.IsRemissionMexicoAndUSDutyOnly())
					{
						lvxInvoiceHeader.ApplyCLVSRemissionMexicoAndUSDutyOnly();
					}
				}
			}
			return ContinueWithSave.Yes;
		}

		#endregion
	}
}
