using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(DirectDebitFileCreationURL))]
	public class DirectDebitFileCreationURLTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidation()
		{
			AssertNoErrors("Precondition: Note type shouldn't have error", BizObj.BankAccountPKInfo);
			BizObj.BankAccountPK = new ZGuid();

			AssertHasError(BizObj.BankAccountPKInfo, "Please enter a Bank Account.");

			BizObj.BankWebsite = ZString.Empty;
			BizObj.RunPreSaveValidation();

			AssertHasError(BizObj.BankWebsiteInfo, "Please enter a Bank URL.");

			DirectDebitFileCreationURLCollection settings = new DirectDebitFileCreationURLCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			settings.Add(Setting);

			DirectDebitFileCreationURL setting2 = settings.AddNew();
			setting2.BankWebsite = "http:/xxx";

			AssertNoErrors(setting2.BankAccountPKInfo);
			setting2.BankAccountPK = Setting.BankAccountPK;

			AssertHasNotifications(DirectDebitFileCreationURL.ErrorDuplicateBankAccount, setting2.BankAccountPKInfo);
		}

		#region Implementation

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new DirectDebitFileCreationURL(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return Setting;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DirectDebitFileCreationURL();
		}

		protected new DirectDebitFileCreationURL BizObj
		{
			get { return (DirectDebitFileCreationURL)base.BizObj; }
		}

		DirectDebitFileCreationURL setting;
		DirectDebitFileCreationURL Setting
		{
			get
			{
				if (setting == null)
				{
					setting = (DirectDebitFileCreationURL)GetBusinessObjectToClone();
					setting.BankAccountPK = BankAccount.PK;
					setting.BankWebsite = "URL";
				}

				return setting;
			}
		}

		BusinessObject bankAccount;
		BusinessObject BankAccount
		{
			get
			{
				if (bankAccount == null)
				{
					bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
					bankAccount[AccBankAccountSchema.AB_AutoDDRFormat] = "NAB";
					Factory.Save();
				}
				return bankAccount;
			}
		}
	}

	#endregion
}
