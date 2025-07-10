using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class BranchChooserTest : TestCaseWithFactory
	{
		public void TestGetBranchForDefaultToBlankWithSingleRegistryOptionSelected()
		{
			SetUpRegistry(1, 0, 0, 0);
			AssertEquals("Branch should be empty", ZGuid.Empty, BranchChooser.GetBranch(null, null, null));
		}

		public void TestGetBranchForDefaultToBlankWithAllRegistryOptionsSelected()
		{
			SetUpRegistry(1, 2, 3, 4);
			AssertEquals("Branch should be empty", ZGuid.Empty, BranchChooser.GetBranch(null, null, null));
		}

		public void TestGetBranchForDefaultToBlankWithMultipleRegistryOptionsSelected()
		{
			SetUpRegistry(3, 1, 2, 0);
			AssertEquals("Branch should be empty", ZGuid.Empty, BranchChooser.GetBranch(null, null, null));
		}

		public void TestGetBranchOfOrganisation()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			GlbBranch branch = GlbCompany.CurrentCompany.Branches[0];
			org.CompanyData.OB_GB_ControllingBranch = branch.PK;
			Factory.Save();

			SetUpRegistry(0, 0, 1, 0);

			GlbBranch result = Factory.Load<GlbBranch>(BranchChooser.GetBranch(org, null, null, null));
			AssertEquals("Branch is active", true, result.GB_IsActive);
			AssertNotNull("Branch should not be null", result);

			org.Branch.GB_IsActive = false;
			Factory.Save();

			result = Factory.Load<GlbBranch>(BranchChooser.GetBranch(org, null, null, null));
			AssertNull("Branch should be null", result);

			org.Branch.GB_IsActive = true;
			Factory.Save();

			result = Factory.Load<GlbBranch>(BranchChooser.GetBranch(null, org, null, null));
			AssertEquals("Branch is active", true, result.GB_IsActive);
			AssertNotNull("Branch should not be null", result);

			org.Branch.GB_IsActive = false;
			Factory.Save();

			result = Factory.Load<GlbBranch>(BranchChooser.GetBranch(null, org, null, null));
			AssertNull("Branch should not be null", result);
		}

		public void TestGetBranchForBranchRelatedToPortOrOperationsBranchWithSingleRegistryOptionSelected()
		{
			GlbCompany newCompany = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch operationsBranch = CreateBranchAndAssociateWithPort(newCompany, "OPABC");
			GlbBranch branchAUFRE = CreateBranchAndAssociateWithPort(newCompany, "AUFRE");
			GlbBranch branchAUSYD = CreateBranchAndAssociateWithPort(newCompany, "AUSYD");
			GlbBranchExtraPorts additionalRelatedPort = branchAUSYD.ExtraPorts.AddNew();
			additionalRelatedPort.GY_RL_NKAdditionalBranchRelatedPort = "KRDDO";
			Factory.Save();

			RefUNLOCO aUFRE = Factory.LoadFromUniqueKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, new ZString("AUFRE"));
			RefUNLOCO aUSYD = Factory.LoadFromUniqueKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, new ZString("AUSYD"));
			RefUNLOCO nZAKL = Factory.LoadFromUniqueKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, new ZString("NZAKL"));
			RefUNLOCO kRDDO = Factory.LoadFromUniqueKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, new ZString("KRDDO"));

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchAUSYD.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				SetUpRegistry(0, 1, 0, 0);

				GlbBranch result = Factory.Load<GlbBranch>(BranchChooser.GetBranch(null, null, aUFRE, nZAKL));
				AssertEquals("Branch is active", true, result.GB_IsActive);
				AssertNotNull("Branch should not be null", result);
				AssertEquals("Branch Home Port", "AUFRE", result.GB_RL_NKHomePort);

				result = Factory.Load<GlbBranch>(BranchChooser.GetBranch(null, null, aUFRE, aUSYD));
				AssertEquals("Branch is active", true, result.GB_IsActive);
				AssertNotNull("Branch should not be null", result);
				AssertEquals("Branch Home Port (Discharge Port takes precedence over Origin Port)", "AUSYD", result.GB_RL_NKHomePort);

				result = Factory.Load<GlbBranch>(BranchChooser.GetBranch(null, null, nZAKL, kRDDO));
				AssertEquals("Branch is active", true, result.GB_IsActive);
				AssertNotNull("Branch should not be null", result);
				AssertEquals("Branch Home Port (Additional Related Ports)", "KRDDO", result.ExtraPorts[0].GY_RL_NKAdditionalBranchRelatedPort);

				branchAUFRE.GB_IsActive = false;
				branchAUSYD.GB_IsActive = false;
				Factory.Save();

				result = Factory.Load<GlbBranch>(BranchChooser.GetBranch(null, null, aUFRE, nZAKL));
				AssertNull("Branch should be null", result);

				result = Factory.Load<GlbBranch>(BranchChooser.GetBranch(null, null, aUFRE, aUSYD));
				AssertNull("Branch should be null", result);

				result = Factory.Load<GlbBranch>(BranchChooser.GetBranch(null, null, nZAKL, kRDDO));
				AssertNull("Branch should be null", result);

				operationsBranch.GB_IsActive = true;
				Factory.Save();

				result = Factory.Load<GlbBranch>(BranchChooser.GetBranch(JobInvoicingConsumerTypes.WarehouseInwards, null, null, aUFRE, nZAKL, operationsBranch));
				AssertNotNull("Branch should not be null", result);

				operationsBranch.GB_IsActive = false;
				Factory.Save();

				result = Factory.Load<GlbBranch>(BranchChooser.GetBranch(JobInvoicingConsumerTypes.WarehouseInwards, null, null, aUFRE, nZAKL, operationsBranch));
				AssertNull("Branch should be null", result);
			}
		}

		public void TestGetBranchForBranchRelatedToPortOrWarehouseBranchWithMultipleRegistryOptionsSelected()
		{
			GlbCompany newCompany = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch branchAUFRE = CreateBranchAndAssociateWithPort(newCompany, "AUFRE");
			GlbBranch branchAUSYD = CreateBranchAndAssociateWithPort(newCompany, "AUSYD");
			Factory.Save();

			RefUNLOCO aUFRE = Factory.LoadFromUniqueKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, new ZString("AUFRE"));
			RefUNLOCO aUSYD = Factory.LoadFromUniqueKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, new ZString("AUSYD"));
			RefUNLOCO nZAKL = Factory.LoadFromUniqueKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, new ZString("NZAKL"));

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchAUSYD.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				SetUpRegistry(4, 2, 1, 3);

				GlbBranch result = Factory.Load<GlbBranch>(BranchChooser.GetBranch(null, null, aUFRE, nZAKL));
				AssertNotNull("Branch should not be null", result);
				AssertEquals("Branch Home Port", "AUFRE", result.GB_RL_NKHomePort);

				result = Factory.Load<GlbBranch>(BranchChooser.GetBranch(null, null, aUFRE, aUSYD));
				AssertNotNull("Branch should not be null", result);
				AssertEquals("Branch Home Port (Discharge Port takes precedence over Origin Port)", "AUSYD", result.GB_RL_NKHomePort);
			}
		}

		public void TestGetBranchForBranchRelatedToPortOrOperationBranchWhereOnlyBranchIsInOtherCompany()
		{
			GlbCompany newCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch branchAUMEL = CreateBranchAndAssociateWithPort(newCompany1, "AUMEL");
			GlbBranch branchAUPER = CreateBranchAndAssociateWithPort(newCompany1, "AUPER");

			GlbCompany newCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch branchInOtherCompany = CreateBranchAndAssociateWithPort(newCompany2, "AUSYD");

			Factory.Save();

			RefUNLOCO aUFRE = Factory.LoadFromUniqueKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, new ZString("AUFRE"));
			RefUNLOCO aUSYD = Factory.LoadFromUniqueKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, new ZString("AUSYD"));
			RefUNLOCO nZAKL = Factory.LoadFromUniqueKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, new ZString("NZAKL"));

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchAUMEL.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				SetUpRegistry(0, 1, 0, 0);

				GlbBranch result = Factory.Load<GlbBranch>(BranchChooser.GetBranch(null, null, aUSYD, nZAKL));
				AssertNull("Branch should be null", result);
			}
		}

		public void TestGetBranchForBranchRelatedToPortOrOperationBranchForWarehouseBranch()
		{
			GlbCompany newCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch branchAUFRE = CreateBranchAndAssociateWithPort(newCompany1, "AUFRE");
			GlbBranch branchAUSYD = CreateBranchAndAssociateWithPort(newCompany1, "AUSYD");

			GlbCompany newCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch branchInOtherCompany = CreateBranchAndAssociateWithPort(newCompany2, "AUBNE");

			Factory.Save();

			RefUNLOCO aUFRE = Factory.LoadFromUniqueKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, new ZString("AUFRE"));
			RefUNLOCO aUSYD = Factory.LoadFromUniqueKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, new ZString("AUSYD"));
			RefUNLOCO nZAKL = Factory.LoadFromUniqueKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, new ZString("NZAKL"));

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchAUSYD.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				SetUpRegistry(0, 1, 0, 0);

				ZGuid pK = BranchChooser.GetBranch(JobInvoicingConsumerTypes.WarehouseInwards, null, null, null, null, branchAUSYD);
				GlbBranch result = Factory.Load<GlbBranch>(pK);
				AssertNotNull("Branch should not be null", result);
				AssertEquals("Branch Home Port", "AUSYD", result.GB_RL_NKHomePort);

				pK = BranchChooser.GetBranch(JobInvoicingConsumerTypes.Shipment, null, null, null, null, branchInOtherCompany);
				result = Factory.Load<GlbBranch>(pK);
				AssertNull("Branch should be null (not in our company", result);

				pK = BranchChooser.GetBranch(JobInvoicingConsumerTypes.LocalCartage, null, null, null, null, branchAUSYD);
				result = Factory.Load<GlbBranch>(pK);
				AssertNotNull("Branch should not be null", result);
				AssertEquals("Branch Home Port", "AUSYD", result.GB_RL_NKHomePort);

				pK = BranchChooser.GetBranch(JobInvoicingConsumerTypes.WarehouseAdHocServiceJob, null, null, null, null, branchAUFRE);
				result = Factory.Load<GlbBranch>(pK);
				AssertNotNull("Branch should not be null", result);
				AssertEquals("Branch Home Port", "AUFRE", result.GB_RL_NKHomePort);
			}
		}

		#region TestGetBranchForBranchRelatedPickupDepotOrOperationsBranch

		public void TestGetBranchForBranchRelatedPickupDepotOrOperationsBranch_TransportConsignment()
		{
			AssertRelatedPickupDepotOrOperationsBranch(JobInvoicingConsumerTypes.TransportBookingConsignment);
		}

		public void TestGetBranchForBranchRelatedPickupDepotOrOperationsBranch_TransportBooking()
		{
			AssertRelatedPickupDepotOrOperationsBranch(JobInvoicingConsumerTypes.TransportBooking);
		}

		void AssertRelatedPickupDepotOrOperationsBranch(JobInvoicingConsumerType consumerType)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branchAUSYD = CreateBranchAndAssociateWithPort(company, "AUSYD");

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchAUSYD.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				SetUpRegistry(0, 1, 0, 0);

				ZGuid pK = BranchChooser.GetBranch(consumerType, null, null, null, null, branchAUSYD);
				var result = Factory.Load<GlbBranch>(pK);
				AssertNotNull("Branch should not be null", result);
				AssertEquals("Branch Home Port", "AUSYD", result.GB_RL_NKHomePort);
			}
		}

		#endregion

		public void TestGetBranchForBranchOfOrganisationWithSingleRegistryOptionSelected()
		{
			GlbCompany newCompany = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();

			GlbBranch branch1 = CreateBranchAndAssociateWithPort(newCompany, "AUSYD");
			branch1.GB_Code = "BR1";
			GlbBranch branch2 = CreateBranchAndAssociateWithPort(newCompany, "AUSYD");
			branch1.GB_Code = "BR2";
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				ObjectCreator.AALSHI.CompanyData.OB_GB_ControllingBranch = branch1.PK;
			}
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch2.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				ObjectCreator.ABIGAS.CompanyData.OB_GB_ControllingBranch = branch2.PK;
			}
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				SetUpRegistry(0, 0, 1, 0);

				AssertEquals("Branch should be taken from the Organisation", branch1.PK, BranchChooser.GetBranch(ObjectCreator.AALSHI, null, null));
				AssertEquals("Branch should be taken from the Organisation", branch2.PK, BranchChooser.GetBranch(ObjectCreator.ABIGAS, null, null));
			}
		}

		public void TestGetBranchForBranchOfOrganisationWithMultipleRegistryOptionsSelected()
		{
			GlbCompany newCompany = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();

			GlbBranch branch1 = CreateBranchAndAssociateWithPort(newCompany, "AUSYD");
			branch1.GB_Code = "BR1";
			GlbBranch branch2 = CreateBranchAndAssociateWithPort(newCompany, "AUSYD");
			branch1.GB_Code = "BR2";
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				ObjectCreator.AALSHI.CompanyData.OB_GB_ControllingBranch = branch1.PK;
			}
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch2.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				ObjectCreator.ABIGAS.CompanyData.OB_GB_ControllingBranch = branch2.PK;
			}
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				SetUpRegistry(4, 1, 2, 3);
				AssertEquals("Branch should be taken from the Organisation", branch1.PK, BranchChooser.GetBranch(ObjectCreator.AALSHI, null, null));
				AssertEquals("Branch should be taken from the Organisation", branch2.PK, BranchChooser.GetBranch(ObjectCreator.ABIGAS, null, null));
			}
		}

		public void TestGetBranchForBranchOfOrganisationWhereOnlyBranchIsInOtherCompany()
		{
			GlbCompany thisCompany = Factory.NewWithValidTestData<GlbCompany>();
			GlbCompany otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();

			GlbBranch branchInThisCompany = CreateBranchAndAssociateWithPort(thisCompany, "AUMEL");
			branchInThisCompany.GB_Code = "ABC";
			GlbBranch branchInOtherCompany = CreateBranchAndAssociateWithPort(otherCompany, "AUMEL");
			branchInThisCompany.GB_Code = "XYZ";
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchInThisCompany.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				ObjectCreator.AALSHI.CompanyData.OB_GB_ControllingBranch = branchInThisCompany.PK;
			}
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchInOtherCompany.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				ObjectCreator.ABIGAS.CompanyData.OB_GB_ControllingBranch = branchInOtherCompany.PK;
			}
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchInThisCompany.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				SetUpRegistry(0, 0, 1, 0);

				GlbBranch result = Factory.Load<GlbBranch>(BranchChooser.GetBranch(ObjectCreator.AALSHI, null, null));
				AssertNotNull("Branch should not be null", result);
				AssertEquals("Branch should be taken from the Organisation", branchInThisCompany.GB_Code, result.GB_Code);

				AssertEquals("Branch should be empty: The Branch associated with this Organisation is not in this company", ZGuid.Empty, BranchChooser.GetBranch(ObjectCreator.ABIGAS, null, null));
			}
		}

		public void TestGetBranchForCurrentUserWithSingleRegistryOptionSelected()
		{
			SetUpRegistry(0, 0, 0, 1);
			ZGuid defaultedBranch = BranchChooser.GetBranch(null, null, null);
			AssertEquals("Should be GlbBranch.CurrentBranch.PK", GlbBranch.CurrentBranch.PK, defaultedBranch);
		}

		public void TestGetBranchForCurrentUserWithMultipleRegistryOptionsSelected()
		{
			SetUpRegistry(2, 0, 0, 1);
			ZGuid defaultedBranch = BranchChooser.GetBranch(null, null, null);
			AssertEquals("Should be GlbBranch.CurrentBranch.PK", GlbBranch.CurrentBranch.PK, defaultedBranch);
		}

		public void TestGetBranchForCurrentUserWithAllRegistryOptionsSelected()
		{
			SetUpRegistry(4, 3, 2, 1);
			ZGuid defaultedBranch = BranchChooser.GetBranch(null, null, null);
			AssertEquals("Should be GlbBranch.CurrentBranch.PK", GlbBranch.CurrentBranch.PK, defaultedBranch);
		}

		public void TestValidActiveBranchCodeFromCustomBranchChooser()
		{
			SetUpNewBranch("AAA");
			SetCustomDefaultBranchConfigInRegistry("return 'AAA'");
			var branch = BranchChooser.GetBranch(null, null, Shipment);
			AssertEquals("Default branch should be based on Custom Branch Chooser result", TestBranch.PK, branch);
		}

		public void TestEmptyBranchCodeFromCustomBranchChooser()
		{
			var rule = new JobBranchDefaultOrderRule();
			rule.DefaultToBlank = 0;
			rule.DefaultToBranchRelatedToPortOrWarehouseBranch = 0;
			rule.DefaultToBranchOfOrganisation = 0;
			rule.DefaultToLoginUserDefault = 1;

			SetCustomDefaultBranchConfigInRegistry("return obj.SomeNonExistingPropertName");
			AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);
			var branch = BranchChooser.GetBranch(null, null, Shipment);
			AssertEquals("Default branch should be based on fallback", GlbBranch.CurrentBranch.PK, branch);
		}

		public void TestGetBranchBizObjFromBranchCode()
		{
			SetUpNewBranch("BBB");
			AssertEquals("Branches should match", TestBranch, BranchChooser.GetBranchBizObjFromBranchCode(Factory, "BBB"));
			AssertEquals("Branches should match", null, BranchChooser.GetBranchBizObjFromBranchCode(Factory, "***"));
			AssertEquals("Branches should match", null, BranchChooser.GetBranchBizObjFromBranchCode(Factory, ZString.Empty));
			AssertEquals("Branches should match", null, BranchChooser.GetBranchBizObjFromBranchCode(null, "BBB"));
		}

		public void TestInvalidOrInactiveBranchFromCustomBranchChooser()
		{
			SetUpNewBranch("CCC");
			SetCustomDefaultBranchConfigInRegistry("return 'CCC'");
			TestBranch.GB_IsActive = false;
			var branch = BranchChooser.GetBranch(null, null, Shipment);
			AssertEquals("Default branch should be empty", ZGuid.Empty, branch);
		}

		public void TestCustomDefaultingRuleEngineIsInvokedWhenRegistryIsTurnedOn()
		{
			var jobPlugin = Shipment;
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = jobPlugin.PK;

			SetUpNewBranch("AAA");
			SetCustomDefaultBranchConfigInRegistry("return 'AAA'");

			var defaultBranch = ZGuid.NewZGuid();

			var companyMock = new Mock<ICompany>();
			var branchMock = new Mock<IBranch>();
			var departmentMock = new Mock<IDepartment>();
			var branchDefaultingManagerMock = new Mock<IJobBillingBranchDefaultingManager>();
			branchDefaultingManagerMock.Setup(x => x.SetDefaultValue(jobPlugin, Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment));
			branchDefaultingManagerMock.Setup(x => x.DefaultValue).Returns(defaultBranch);

			ObjectFactory.Substitute(branchDefaultingManagerMock.Object);

			AccountingConfigurationRegistry.Instance.CustomBranchDefaultingRulesEngineConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var result = BranchChooser.GetBranch(null, null, jobPlugin);
			AssertEquals("Falls back to Python version", TestBranch.PK, result);

			branchDefaultingManagerMock.Verify(x => x.SetDefaultValue(jobPlugin, Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment), Times.Never);
			branchDefaultingManagerMock.Verify(x => x.DefaultValue, Times.Never);

			AccountingConfigurationRegistry.Instance.CustomBranchDefaultingRulesEngineConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			result = BranchChooser.GetBranch(null, null, jobPlugin);
			AssertEquals("Picks up the value from rules engine", defaultBranch, result);

			branchDefaultingManagerMock.Verify(x => x.SetDefaultValue(jobPlugin, Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment), Times.Once);
			branchDefaultingManagerMock.Verify(x => x.DefaultValue, Times.AtLeast(1));
		}

		public void TestCustomDefaultingRuleEngineReturnsNullValue()
		{
			var jobPlugin = Shipment;
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = jobPlugin.PK;

			SetUpNewBranch("AAA");
			SetCustomDefaultBranchConfigInRegistry("return 'AAA'");

			var result = BranchChooser.GetBranch(null, null, jobPlugin);
			AssertEquals("Uses the Python version", TestBranch.PK, result);

			var companyMock = new Mock<ICompany>();
			var branchMock = new Mock<IBranch>();
			var departmentMock = new Mock<IDepartment>();
			var branchDefaultingManagerMock = new Mock<IJobBillingBranchDefaultingManager>();
			branchDefaultingManagerMock.Setup(x => x.SetDefaultValue(jobPlugin, Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment));
			branchDefaultingManagerMock.Setup(x => x.DefaultValue).Returns((IZType)null);

			ObjectFactory.Substitute(branchDefaultingManagerMock.Object);

			AccountingConfigurationRegistry.Instance.CustomBranchDefaultingRulesEngineConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			result = BranchChooser.GetBranch(null, null, jobPlugin);
			AssertEquals("Falls back to Python version", TestBranch.PK, result);

			branchDefaultingManagerMock.Verify(x => x.SetDefaultValue(jobPlugin, Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment), Times.Once);
			branchDefaultingManagerMock.Verify(x => x.DefaultValue, Times.AtLeast(1));
		}

		public void TestCustomeDefaultingRuleEngine_FallsBackToLoginBranchIfNoRulesFound()
		{
			AccountingConfigurationRegistry.Instance.CustomBranchDefaultingRulesEngineConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var branch1 = ObjectCreator.CreateBranch("AU1", GlbCompany.CurrentCompany);

			var localClient = ObjectCreator.ABIGAS;
			var context = "JBR";
			var subContext = RulesContextSubType.ShipmentJob.GetCode();

			var ruleSet1 = ObjectCreator.CreateProductionRuleSet(context, subContext, 1);
			ObjectCreator.CreateProductionRules(ruleSet1, context, 1, "LocalClient", localClient.PK, branch1.PK);

			Factory.Save();

			var shipment = ObjectCreator.CreateShipment("S001001", false);
			var job = ObjectCreator.CreateJob(shipment, false, localClientOrg: localClient);

			var result = BranchChooser.GetBranch(null, null, shipment);

			AssertEquals("branch1 should be returned as branch as rulset1 was satisfied", branch1.PK, result);

			job.JH_OA_LocalChargesAddr = ObjectCreator.AALSHI.MainAddress.PK;

			result = BranchChooser.GetBranch(null, null, shipment);

			AssertEquals("Login branch should be returned as branch as no rule is satisfied", GlbBranch.CurrentBranch.PK, result);
		}

		public void TestGetBranchByDynamicBranchCode_PickupAgentBranch()
		{
			var plugIn = ObjectCreator.CreateShipment("S00001000");
			var branchPK = BranchChooser.GetBranchByDynamicBranchCode(BranchChooser.PickupAgentBranch, plugIn);
			AssertEquals(ZGuid.Empty, branchPK);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			var branch1 = ObjectCreator.CreateBranch("AU1", GlbCompany.CurrentCompany);
			var branch2 = ObjectCreator.CreateBranch("AU2", GlbCompany.CurrentCompany);
			branch2.GB_OH_OrgProxy = org.PK;

			plugIn.PickupAgentPK = org.PK;

			branchPK = BranchChooser.GetBranchByDynamicBranchCode(BranchChooser.PickupAgentBranch, plugIn);
			AssertEquals(branch2.PK, branchPK);
		}

		public void TestGetBranchByDynamicBranchCode_DeliveryAgentBranch()
		{
			var plugIn = ObjectCreator.CreateShipment("S00001000");
			var branchPK = BranchChooser.GetBranchByDynamicBranchCode(BranchChooser.DeliveryAgentBranch, plugIn);
			AssertEquals(ZGuid.Empty, branchPK);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			var branch1 = ObjectCreator.CreateBranch("AU1", GlbCompany.CurrentCompany);
			var branch2 = ObjectCreator.CreateBranch("AU2", GlbCompany.CurrentCompany);
			branch2.GB_OH_OrgProxy = org.PK;

			plugIn.JS_OH_DeliveryAgent = org.PK;

			branchPK = BranchChooser.GetBranchByDynamicBranchCode(BranchChooser.DeliveryAgentBranch, plugIn);
			AssertEquals(branch2.PK, branchPK);
		}

		public void TestGetBranchByDynamicBranchCode_ControllingAgentBranch()
		{
			var plugIn = ObjectCreator.CreateShipment("S00001000");
			var branchPK = BranchChooser.GetBranchByDynamicBranchCode(BranchChooser.ControllingAgentBranch, plugIn);
			AssertEquals(ZGuid.Empty, branchPK);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			var branch1 = ObjectCreator.CreateBranch("AU1", GlbCompany.CurrentCompany);
			var branch2 = ObjectCreator.CreateBranch("AU2", GlbCompany.CurrentCompany);
			branch2.GB_OH_OrgProxy = org.PK;

			plugIn.ControllingAgentNameOrPK = org.PK.ToString();

			branchPK = BranchChooser.GetBranchByDynamicBranchCode(BranchChooser.ControllingAgentBranch, plugIn);
			AssertEquals(branch2.PK, branchPK);
		}

		public void TestGetBranchByDynamicBranchCode_SendingForwarderBranch()
		{
			var plugIn = ObjectCreator.CreateConsol("C0001");
			var branchPK = BranchChooser.GetBranchByDynamicBranchCode(BranchChooser.SendingForwarderBranch, plugIn);
			AssertEquals(ZGuid.Empty, branchPK);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			var branch1 = ObjectCreator.CreateBranch("AU1", GlbCompany.CurrentCompany);
			var branch2 = ObjectCreator.CreateBranch("AU2", GlbCompany.CurrentCompany);
			branch2.GB_OH_OrgProxy = org.PK;

			plugIn.JK_OA_SendingForwarderAddress = org.MainAddress.PK;

			branchPK = BranchChooser.GetBranchByDynamicBranchCode(BranchChooser.SendingForwarderBranch, plugIn);
			AssertEquals(branch2.PK, branchPK);
		}
		public void TestGetBranchByDynamicBranchCode_ReceivingForwarderBranch()
		{
			var plugIn = ObjectCreator.CreateConsol("C0001");
			
			var branchPK = BranchChooser.GetBranchByDynamicBranchCode(BranchChooser.ReceivingForwarderBranch, plugIn);
			AssertEquals(ZGuid.Empty, branchPK);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			var branch1 = ObjectCreator.CreateBranch("AU1", GlbCompany.CurrentCompany);
			var branch2 = ObjectCreator.CreateBranch("AU2", GlbCompany.CurrentCompany);
			branch2.GB_OH_OrgProxy = org.PK;

			plugIn.JK_OA_ReceivingForwarderAddress = org.MainAddress.PK;

			branchPK = BranchChooser.GetBranchByDynamicBranchCode(BranchChooser.ReceivingForwarderBranch, plugIn);
			AssertEquals(branch2.PK, branchPK);
		}

		void SetCustomDefaultBranchConfigInRegistry(ZString config)
		{
			var customDefaultBranchConfiguration = new CustomDefaultBranchConfiguration();
			customDefaultBranchConfiguration.ConfigAsString = config;
			AccountingConfigurationRegistry.Instance.CustomDefaultBranchConfiguration.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, customDefaultBranchConfiguration);
			Factory.Save();
		}

		void SetUpNewBranch(ZString branchCode)
		{
			TestBranch = ObjectCreator.CreateBranch(branchCode, GlbCompany.CurrentCompany);
			Factory.Save();
		}

		IJobInvoicingPlugIn Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = ObjectCreator.CreateShipment("S00001000");
				}
				return shipment;
			}
		}
		IJobInvoicingPlugIn shipment;

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			ObjectCreator = new TestObjectCreator(Factory);
			BranchChooser = new BranchChooser(Factory);
		}

		GlbBranch CreateBranchAndAssociateWithPort(GlbCompany company, ZString portCode)
		{
			GlbBranch newBranch = company.Branches.AddNew();
			newBranch.GB_Code = portCode.Left(3);
			newBranch.GB_RL_NKHomePort = portCode;
			return newBranch;
		}

		void SetUpRegistry(ZShort defaultToBlank, ZShort defaultToBranchRelatedToPortOrWarehouseBranch,
			ZShort defaultToBranchOfOrganisation, ZShort defaultToLoginUserDefault)
		{
			JobBranchDefaultOrderRule rule = new JobBranchDefaultOrderRule();

			rule.DefaultToBlank = defaultToBlank;
			rule.DefaultToBranchRelatedToPortOrWarehouseBranch = defaultToBranchRelatedToPortOrWarehouseBranch;
			rule.DefaultToBranchOfOrganisation = defaultToBranchOfOrganisation;
			rule.DefaultToLoginUserDefault = defaultToLoginUserDefault;

			AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);
		}

		GlbBranch TestBranch;
		BranchChooser BranchChooser;
		TestObjectCreator ObjectCreator;

		#endregion
	}
}
