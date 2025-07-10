using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.Statement.Testing
{
	[TestedType(typeof(StatementModule))]
	class StatementModuleTest : ZModuleBasherWithFetchHintsTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.EU.FR.CustomsStatement;
		}

		protected override string CountryCode => Core.Constants.CountryCodes.France;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var statement = (CusStatementHeader)factory.New(businessObjectType);
			statement.B2_BranchDesignation = StatementEntryTypeList.Codes.Import;
			statement.B2_StatementType = StatementPeriodicityList.Codes.Day;
			return statement;
		}

		protected override void SetupDataForFetchHintsTest()
		{
			Enumerable.Range(0, 20).ForEach(CreateStatementForFetchHintTest);
			Factory.Save();
		}

		protected override ZFilterModule CreateModuleForFetchHintsTest()
		{
			return new StatementModule();
		}

		void CreateStatementForFetchHintTest(int idx)
		{
			var organisations = Factory.GetCachedValue("OrganisationsForStatementModuleTest", () => Factory.Load<OrgHeader>(new ZQuery
			{
				MaximumRows = 20
			}));

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "1111" + idx.ToString("0000");
			statement.EntryNumber = "5555" + idx.ToString("0000");
			statement.CorrelationID = "6666" + idx.ToString("0000");
			statement.B2_Status = new StatementStatusList().GetAllCodes()[idx % new StatementStatusList().Count];
			statement.B2_OH_Importer = organisations[idx].PK;
			statement.B2_PaymentType = new MethodOfPaymentList().GetAllCodes()[idx % new MethodOfPaymentList().Count];
			statement.B2_ProcessDate = ZDateTime.Today.AddSeconds(idx);
			statement.B2_BranchDesignation = new StatementEntryTypeImpExpList().GetAllCodes()[idx % new StatementEntryTypeImpExpList().Count];
			statement.B2_StatementType = new StatementPeriodicityList().GetAllCodes()[idx % new StatementPeriodicityList().Count];
			statement.B2_EntryFilerCode = "2222" + idx.ToString("000000");
			statement.B2_AccountNo = "3333" + idx.ToString("00000000");
			statement.B2_ImporterCustomsID = "4444" + idx.ToString("0000000000");
			statement.B2_DueDate = ZDateTime.Today.AddHours(idx);
			statement.B2_ProcessDate = ZDateTime.Today.AddMinutes(idx);
		}

		protected override bool HasFailedFetchHint(TableHitCount tableSelect)
		{
			if ((tableSelect.TableName == OrgHeader.Schema.TableName || tableSelect.TableName == CusStatementLine.Schema.TableName) && tableSelect.Value == 20)  // CusStatementHeader.ImporterFullName needs to fetch OrgHeader
			{
				return false;
			}
			return base.HasFailedFetchHint(tableSelect);
		}

		public void TestSecurityCheckpoint()
		{
			using (var module = new StatementModule())
			{
				AssertEquals(Env.Security.FRCustomsStatement, module.SecurityCheckpoint);
			}
		}

		public void TestLicenceCheckpoint()
		{
			using (var module = new StatementModule())
			{
				AssertEquals(Env.Licence.ImportBroker, module.LicenceCheckPoint);
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = new StatementModule())
			{
				AssertType<CusStatementHeaderCollection>(module.GetNewBusinessObjectCollection());
			}
		}

		public void TestGetNewFilterControl()
		{
			using (var module = new StatementModule())
			{
				using (var filterControl = module.GetNewFilterControlForGrid())
				{
					AssertType<StatementFilterControl>(filterControl);
				}
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = new StatementModule())
			{
				AssertType<StatementFilterStripBusinessObject>(module.FilterBusinessObject);
			}
		}

		public void TestAllowNew()
		{
			using (var module = new StatementModule())
			{
				AssertEquals(true, module.AllowNew);
			}
		}

		public void TestAllowView()
		{
			using (var module = new StatementModule())
			{
				AssertEquals(true, module.AllowView);
			}
		}

		public void TestAllowEdit()
		{
			using (var module = new StatementModule())
			{
				AssertEquals(true, module.AllowEdit);
			}
		}

		public void TestAllowDelete()
		{
			using (var module = new StatementModule())
			{
				AssertEquals("CusStatementHeader is not allowed to be deleted (see trigger trgCusStatementHeader_Del).", false, module.AllowDelete);
			}
		}
	}
}
