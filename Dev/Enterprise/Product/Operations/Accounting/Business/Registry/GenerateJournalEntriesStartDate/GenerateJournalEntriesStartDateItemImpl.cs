using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.Abstractions;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	public class GenerateJournalEntriesStartDateItemImpl : DateTimeRegistryItem
	{
		public GenerateJournalEntriesStartDateItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options) : base(name, category, caption, hint, storage, options)
		{
			this.DataType = new GenerateJournalEntriesStartDateType();
			OnAllValuesSavedAction = AllValuesSavedAction;
		}

		void AllValuesSavedAction()
		{
			ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask("GLQ");
		}

		protected override void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			base.SetValueCore(companyOrOwnerPK, branchPK, departmentPK, newValue);

			if ((DateTime)newValue != DateTime.MinValue)
			{
				var cdcRegistryItem = AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesCDCStartDate;
				if (cdcRegistryItem.GetFallBackValueAtAllLevels(companyOrOwnerPK, branchPK, departmentPK) == DateTime.MinValue)
				{
					cdcRegistryItem.SetValue(companyOrOwnerPK, branchPK, departmentPK, ZDateTime.UtcNow.ToSmallDateTime().ToDateTime());
					AddCdcRegistryItemStmALog(cdcRegistryItem);
				}
			}

			void AddCdcRegistryItemStmALog(DateTimeRegistryItem cdcRegistryItem)
			{
				var factory = new BusinessObjectFactory();
				var stmData = new StmData.Loader(factory).LoadTop1(cdcRegistryItem.Name, companyOrOwnerPK, Guid.Empty);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				stmData.Logs.AddNew(Events.EditedARecord, cdcRegistryItem.OnBuildLogReference(new BuildLogReferenceArgs(cdcRegistryItem, DateTime.MinValue, ZDateTime.UtcNow.ToSmallDateTime().ToDateTime())));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				factory.Save();
			}
		}
	}

	class GenerateJournalEntriesStartDateType : DateTimeRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, DateTime proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (proposedValue > ZDateTime.Today.ToDateTime())
			{
				throw new RegistryValidationException(Res.GetString("C110E56D-A75D-48B9-B294-A168F2261DD9", "The date specified must not be later than today's date."));
			}

			var oldValue = (DateTime)registryItem.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK);
			if (proposedValue > oldValue && !ValuesAreEqual(oldValue, DateTime.MinValue))
			{
				throw new RegistryValidationException(Res.GetString("EFDB582A-4839-4108-9570-E0937407060E", "The date specified must be the same or earlier than previous saved date."));
			}

			if (!ValuesAreEqual(oldValue, DateTime.MinValue) && !ValuesAreEqual(oldValue, AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK)))
			{
				throw new RegistryValidationException(Res.GetString("B9076638-4BE0-431E-9C5B-1714DC446D7D", "There are unprocessed backlog data from previous start date: {0}. You can only modify the start date after these backlog data have been processed.", ((ZDateTime)oldValue).ToShortDateString()));
			}

			var periodManagement = Calculator.GetFirstPeriodManagementFromDate(proposedValue, companyPK);
			
			if (!IsFirstStartDateOfFinanceYear(periodManagement, proposedValue))
			{
				throw new RegistryValidationException(Res.GetString("CFF91F06-E9AD-49EF-9813-5BF152D797D2", "The date specified must be the start date of the first period of an accounting year."));
			}

			ValidateControlAccounts(companyPK);

			if (!IsLatestUnprocessedFinanceYear(periodManagement, oldValue, companyPK, out var errorMessage))
			{
				throw new RegistryValidationException(errorMessage);
			}
		}

		bool IsFirstStartDateOfFinanceYear(AccPeriodManagement periodManagement, DateTime proposedValue)
		{
			var startDate = periodManagement?.AM_StartDate ?? DateTime.MinValue;
			return startDate == proposedValue && startDate != DateTime.MinValue;
		}

		void ValidateControlAccounts(Guid companyPK)
		{
			var shouldNotBeEmptyControlAccounts = new List<GuidRegistryItem>();

			shouldNotBeEmptyControlAccounts.AddRange(ForcedControlAccounts.Where(registry => registry.Value == Guid.Empty));

			var company = Factory.Load<GlbCompany>(companyPK);
			if (company.GC_IsGSTRegistered)
			{
				shouldNotBeEmptyControlAccounts.AddRange(GstControlAccounts.Where(registry => registry.Value == Guid.Empty));
				if (company.GC_IsGSTCashBasis)
				{
					shouldNotBeEmptyControlAccounts.AddRange(CashBasisVatControlAccounts.Where(registry => registry.Value == Guid.Empty));
				}
			}

			if (shouldNotBeEmptyControlAccounts.Any())
			{
				var registryNames = string.Join(", ", shouldNotBeEmptyControlAccounts.Select(r => r.Caption));
				throw new RegistryValidationException(Res.GetString("DAD3D089-1CD7-4324-BADC-07DD47F10898", "Please set up the following Control Accounts in the registry (Accounting > General Ledger Defaults > Control Account): {0}", registryNames));
			}
		}

		bool IsLatestUnprocessedFinanceYear(AccPeriodManagement periodManagement, DateTime oldValue, Guid companyPK, out string errorMessage)
		{
			ZShort year;
			errorMessage = Res.GetString("A22FB952-6D29-4007-9F2B-D23A1C47A3A7", "The date specified must be the start date of the first period of previous accounting year.");
			if (ValuesAreEqual(oldValue, DateTime.MinValue))
			{
				var currentPeriodManagement = calculator.GetPeriodManagementFromDate(ZDateTime.Now, companyPK);
				if (currentPeriodManagement != null)
				{
					errorMessage = Res.GetString("D676C9BC-54E4-47B6-A22F-8AA733EBE9D6", "The date specified must be the start date of the first period of current accounting year.");
					year = currentPeriodManagement.AM_Year;
				}
				else
				{
					year = calculator.GetPreviousPeriodManagementFromDate(ZDateTime.Now, companyPK)?.AM_Year ?? ZShort.Zero;
				}
			}
			else
			{
				year = calculator.GetPreviousPeriodManagementFromDate(oldValue, companyPK)?.AM_Year ?? ZShort.Zero;
			}

			return periodManagement.AM_Year == year;
		}

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		AccountingPeriodCalculator calculator;
		AccountingPeriodCalculator Calculator
		{
			get
			{
				if (calculator == null)
				{
					calculator = new AccountingPeriodCalculator(Factory);
				}
				return calculator;
			}
		}

		#region Control Accounts

		readonly List<GuidRegistryItem> ForcedControlAccounts = new List<GuidRegistryItem>
		{
			AccountingConfigurationRegistry.Instance.ARControlAccount,
			AccountingConfigurationRegistry.Instance.APControlAccount,
			AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount,
			AccountingConfigurationRegistry.Instance.AccruedCostControlAccount,
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount,
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount
		};

		readonly List<GuidRegistryItem> GstControlAccounts = new List<GuidRegistryItem>
		{
			AccountingConfigurationRegistry.Instance.GSTInputControlAccount,
			AccountingConfigurationRegistry.Instance.GSTOutputControlAccount
		};

		readonly List<GuidRegistryItem> CashBasisVatControlAccounts = new List<GuidRegistryItem>
		{
			AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount,
			AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount
		};

		#endregion
	}
}
