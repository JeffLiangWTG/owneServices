using CargoWise.Types;
using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations.Testing
{
	internal class JASInvoicingValidationTest : JXCValidationTestCase
	{
		public void TestDomainValidationShouldSubclassFromAutoValidationType()
		{
			AssertEquals(typeof(AutoAccTransactionHeaderValidation), typeof(JXCInvoicingValidation).BaseType);
		}

		public void TestValidateAH_OH()
		{
			JASOrgHeader debtor = Factory.New<JASOrgHeader>();
			debtor.OH_Code = "DEBTORORG";
			AdjustmentNote.AH_OH = debtor.PK;
			AssertHasJXCWarning(AdjustmentNote.AH_OHInfo, JXCConstants.JXCWarningPrefix + "Debtor does not have JAS Netting Code", JXCConstants.JXCWarningPrefix + "Debtor does not have JAS Office Code");
			debtor.NettingCode = "AUSYD";
			AdjustmentNote.Validation.ValidateAH_OH();
			AssertHasJXCWarning(AdjustmentNote.AH_OHInfo, JXCConstants.JXCWarningPrefix + "Debtor does not have JAS Office Code");
			debtor.OfficeCode = "AUSYD";
			AdjustmentNote.Validation.ValidateAH_OH();
			AssertHasNoJXCWarnings(AdjustmentNote.AH_OHInfo);
		}

		public void TestValidateAH_GB()
		{
			AdjustmentNote.AH_GB = ZGuid.Empty;
			AssertHasJXCWarning(AdjustmentNote.AH_GBInfo, JXCConstants.JXCWarningPrefix + "Cannot export JXC Financial Message for this branch as there is no Proxy Organisation setup");
			AdjustmentNote.AH_GB = Factory.New<GlbBranch>().PK;
			AssertHasJXCWarning(AdjustmentNote.AH_GBInfo, JXCConstants.JXCWarningPrefix + "Cannot export JXC Financial Message for this branch as there is no Proxy Organisation setup");
			JASOrgHeader orgProxy = Factory.New<JASOrgHeader>();
			AdjustmentNote.Branch.GB_OH_OrgProxy = orgProxy.PK;
			AdjustmentNote.Validation.ValidateAH_GB();
			AssertHasJXCWarning(AdjustmentNote.AH_GBInfo, JXCConstants.JXCWarningPrefix + "Proxy Organisation for the current Branch does not have JAS Netting Code", JXCConstants.JXCWarningPrefix + "Proxy Organisation for the current Branch does not have JAS Office Code");
			orgProxy.OfficeCode = "AUMEL";
			orgProxy.NettingCode = "AUCOR";
			AdjustmentNote.Validation.ValidateAH_GB();
			AssertHasNoJXCWarnings(AdjustmentNote.AH_GBInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			new JXCDomainValidationManager(Factory).ManageJXCValidations(JXCExportValidationType.Invoicing);
		}

		JASARAdjustmentNote AdjustmentNote
		{
			get
			{
				if (fAdjustmentNote == null)
				{
					fAdjustmentNote = Factory.New<JASARAdjustmentNote>();
				}

				return fAdjustmentNote;
			}
		}

		JASARAdjustmentNote fAdjustmentNote;
	}
}
