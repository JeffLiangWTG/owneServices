using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(BackDateInvoicesConfiguration))]
	public class BackDateInvoicesConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateOverridePostDate()
		{
			GlbCompany anotherCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(anotherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			BackDateInvoicesConfiguration backDateConfig = new BackDateInvoicesConfiguration(new FallbackLevel(anotherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)) { OverridePostDate = true };
			AssertNoErrors(backDateConfig);

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.Inner.DeleteValue(anotherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			backDateConfig.RunPreSaveValidation();
			AssertHasError(backDateConfig.OverridePostDateInfo, "You cannot configure back posting options here because 'Back Posting' has not been enabled for this company under the 'Allow Back Posting Sub Ledger Transaction' registry.");

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			backDateConfig.RunPreSaveValidation();
			AssertNoErrors(backDateConfig);
		}

		public void TestValidateDefaultPostDateFromInvoiceDate()
		{
			GlbCompany anotherCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(anotherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			BackDateInvoicesConfiguration backDateConfig = new BackDateInvoicesConfiguration(new FallbackLevel(anotherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)) { DefaultPostDateFromInvoiceDate = true };
			AssertNoErrors(backDateConfig);

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.Inner.DeleteValue(anotherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			backDateConfig.RunPreSaveValidation();
			AssertHasError(backDateConfig.DefaultPostDateFromInvoiceDateInfo, "You cannot configure back posting options here because 'Back Posting' has not been enabled for this company under the 'Allow Back Posting Sub Ledger Transaction' registry.");

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			backDateConfig.RunPreSaveValidation();
			AssertNoErrors(backDateConfig);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			BackDateInvoicesConfiguration result = new BackDateInvoicesConfiguration();

			result.OverridePostDate = true;
			result.DefaultPostDateFromInvoiceDate = false;

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected new BackDateInvoicesConfiguration BizObj
		{
			get { return (BackDateInvoicesConfiguration)base.BizObj; }
		}

		#endregion

	}
}
