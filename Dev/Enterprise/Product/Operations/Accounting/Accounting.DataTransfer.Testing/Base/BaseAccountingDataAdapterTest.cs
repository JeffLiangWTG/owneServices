using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	abstract class BaseAccountingDataAdapterTest<TBusinessObject, TValueObject> : ValueObjectDataAdapterTest<TBusinessObject, TValueObject>
			where TBusinessObject : BusinessObject
			where TValueObject : IValueObject
	{
		protected override void SetUp()
		{
			base.SetUp();

			new AccountingPeriodTestHelper().PostPeriodsForEntireYear(PostDate.Year, GlbCompany.CurrentCompany.PK);
			var newStaffUser = Factory.NewWithValidTestData<GlbStaff>();
			newStaffUser.GS_LoginName = "NewStaffUser";
			newStaffUser.GS_FullName = "New Staff User";
			newStaffUser.GS_Code = "DP";

			Factory.Save();
			userContextChange = Env.SetTemporaryUserContext("NewStaffUser", GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
			Env.Security.OrgDetailsModifyOrgTypeFlagAP.IsAllowed = true;
			Env.Security.OrgDetailsModifyOrgTypeFlagAR.IsAllowed = true;
			Env.Security.OrgDetailsNewOrgTypeFlagAP.IsAllowed = true;
			Env.Security.OrgDetailsNewOrgTypeFlagAR.IsAllowed = true;
			Env.Security.BranchModify.IsAllowed = true;
		}

		protected override void TearDown()
		{
			userContextChange.Dispose();
			base.TearDown();
		}

		protected ZDateTime PostDate
		{
			get { return new ZDateTime(2005, 01, 01, 10, 30, 0); }
		}

		protected AccChargeCode ChargeCodeCC1
		{
			get
			{
				if (fChargeCodeCC1 == null)
				{
					fChargeCodeCC1 = ObjectCreator.CC1;

					fChargeCodeCC1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

					fChargeCodeCC1.AC_AG_RevenueAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.10.10")).PK;
					fChargeCodeCC1.AC_AG_WIPAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.10.20")).PK;
					fChargeCodeCC1.AC_AG_AccrualAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.20.10")).PK;
					fChargeCodeCC1.AC_AG_CostAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.20.20")).PK;

					AccGroups salesGroup1 = new BusinessObjectFactory().New<AccGroups>();
					salesGroup1.AR_Code = "SAL";
					salesGroup1.AR_Desc = "Sales Group 1";
					salesGroup1.Factory.Save();
					fChargeCodeCC1.AC_AR_SalesGroup = salesGroup1.PK;

					AccGroups expenseGroup1 = new BusinessObjectFactory().New<AccGroups>();
					expenseGroup1.AR_Code = "EXP";
					expenseGroup1.AR_Desc = "Expense Group 1";
					expenseGroup1.Factory.Save();
					fChargeCodeCC1.AC_AR_ExpenseGroup = expenseGroup1.PK;
				}

				return fChargeCodeCC1;
			}
		}

		AccChargeCode fChargeCodeCC1;

		protected AccChargeCode ChargeCodeCC3
		{
			get
			{
				if (fChargeCodeCC3 == null)
				{
					fChargeCodeCC3 = ObjectCreator.CC3;

					fChargeCodeCC3.AC_AG_RevenueAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.10.10")).PK;
					fChargeCodeCC3.AC_AG_WIPAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.10.20")).PK;
					fChargeCodeCC3.AC_AG_AccrualAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.20.10")).PK;
					fChargeCodeCC3.AC_AG_CostAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.20.20")).PK;

					Factory.Save();
				}

				return fChargeCodeCC3;
			}
		}

		AccChargeCode fChargeCodeCC3;

		protected AccChargeCode ChargeCodeCC4
		{
			get
			{
				if (fChargeCodeCC4 == null)
				{
					fChargeCodeCC4 = ObjectCreator.RevenueNoTaxChargeCode;

					fChargeCodeCC4.AC_AG_RevenueAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.10.10")).PK;
					fChargeCodeCC4.AC_AG_WIPAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.10.20")).PK;
					fChargeCodeCC4.AC_AG_AccrualAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.20.10")).PK;
					fChargeCodeCC4.AC_AG_CostAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.20.20")).PK;

					Factory.Save();
				}

				return fChargeCodeCC4;
			}
		}

		AccChargeCode fChargeCodeCC4;

		protected AccGLHeader ClearingAccount
		{
			get
			{
				return Factory.LoadFromUniqueKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, new ZString("6820.00.00"));
			}
		}

		protected OrgHeader Header
		{
			get
			{
				if (fHeader == null)
				{
					fHeader = new BusinessObjectFactory().NewWithValidTestData<OrgHeader>();
					fHeader.OH_FullName = @"The Fullname of the Organisation";
					fHeader.OH_IsCreditor = true;
					fHeader.OH_IsDebtor = true;
					fHeader.Factory.Save();
					fHeader = Factory.Load<OrgHeader>(fHeader.PK);
				}

				return fHeader;
			}
		}

		OrgHeader fHeader;

		protected TestObjectCreator ObjectCreator
		{
			get { return fObjectCreator ?? (fObjectCreator = new TestObjectCreator(Factory)); }
		}

		TestObjectCreator fObjectCreator;

		IDisposable userContextChange;

		protected override bool IsCreateOrUpdateFromValueObjectSupported
		{
			get
			{
				return false;
			}
		}

		public new void TestOnlySaveDataWhenNoRecordsHaveErrorsCheckBoxVisible()
		{
			IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
			AssertEquals(true, adapter.OnlySaveDataWhenNoRecordsHaveErrorsCheckBoxVisible);
		}

		public new void TestOnlySaveDataWhenNoRecordsHaveErrorsCheckBoxChecked()
		{
			IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
			AssertEquals(false, adapter.OnlySaveDataWhenNoRecordsHaveErrorsCheckBoxChecked);

			AccountingConfigurationRegistry.Instance.DataImportShouldSaveOnlyWhenThereAreNoErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, adapter.OnlySaveDataWhenNoRecordsHaveErrorsCheckBoxChecked);
		}
	}
}
