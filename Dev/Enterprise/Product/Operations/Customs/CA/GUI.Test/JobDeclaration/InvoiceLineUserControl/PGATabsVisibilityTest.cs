using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class PGATabsVisibilityTest : TestCaseWithFactory
	{
		public void TestVisibilityWhenEnablingAndDisablingProgramsViaUI()
		{
			var invoiceLine = CreateInvoiceLine();
			using (var form = new FormWithPGATabsForTest(invoiceLine))
			{
				form.Show();

				foreach (var pgaRequirement in invoiceLine.PGARequirements.Cast<PGARequirement>())
				{
					foreach (var programRequirement in pgaRequirement.ProgramCodeRequirements.Cast<PGAProgramRequirement>())
					{
						programRequirement.DeclareYes = true;
						CheckTabVisibilities(invoiceLine, form);
					}
				}

				foreach (var pgaRequirement in invoiceLine.PGARequirements.Cast<PGARequirement>())
				{
					foreach (var programRequirement in pgaRequirement.ProgramCodeRequirements.Cast<PGAProgramRequirement>())
					{
						programRequirement.DeclareYes = false;
						CheckTabVisibilities(invoiceLine, form);
					}
				}
			}
		}

		public void TestVisibilityWhenPGADefinedByTariff()
		{
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, PGACodes.Codes.CFIA, "1100001234");
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, PGACodes.Codes.HC, "2100001234", HCPGADepartmentCodes.Codes.BBC);
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, PGACodes.Codes.HC, "2200001234", HCPGADepartmentCodes.Codes.VET);
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, PGACodes.Codes.TC, "3100001234", TCPGADepartmentCodes.Codes.TPR);
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, PGACodes.Codes.TC, "3200001234", TCPGADepartmentCodes.Codes.VPR);
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, PGACodes.Codes.GAC, "4100001234");

			Factory.Save();

			var invoiceLine = CreateInvoiceLine();

			using (var form = new FormWithPGATabsForTest(invoiceLine))
			{
				form.Show();

				invoiceLine.JI_Tariff = "1100001234";
				AssertEquals("Y", invoiceLine.CA_CFIAInd);
				AssertEquals("Y", invoiceLine.CFIAPGAHeader.CA_AllProgramInd);
				CheckTabVisibilities(invoiceLine, form);

				invoiceLine.JI_Tariff = "2100001234";
				AssertEquals("Y", invoiceLine.CA_HCInd);
				AssertEquals("Y", invoiceLine.HCPGAHeader.CA_BBCProgramInd);
				CheckTabVisibilities(invoiceLine, form);

				invoiceLine.JI_Tariff = "2200001234";
				AssertEquals("Y", invoiceLine.CA_HCInd);
				AssertEquals("Y", invoiceLine.HCPGAHeader.CA_VETProgramInd);
				CheckTabVisibilities(invoiceLine, form);

				invoiceLine.JI_Tariff = "3100001234";
				AssertEquals("Y", invoiceLine.CA_TCInd);
				AssertEquals("Y", invoiceLine.TCPGAHeader.CA_TPRProgramInd);
				CheckTabVisibilities(invoiceLine, form);

				invoiceLine.JI_Tariff = "3200001234";
				AssertEquals("Y", invoiceLine.CA_TCInd);
				AssertEquals("Y", invoiceLine.TCPGAHeader.CA_VPRProgramInd);
				CheckTabVisibilities(invoiceLine, form);

				invoiceLine.JI_Tariff = "4100001234";
				AssertEquals("Y", invoiceLine.CA_GACInd);
				AssertEquals("Y", invoiceLine.GACPGAHeader.CA_AllProgramInd);
				CheckTabVisibilities(invoiceLine, form);
			}
		}

		public void TestVisibilityWhenRequirementsAreCopiedFromProduct()
		{
			// no PGA example
			CreateImportPartPivot("NONE", "1234567890");

			// all PGAs and programms example
			var allPivot = CreateImportPartPivot("ALL", "1234567891");
			foreach (var requirement in allPivot.PGARequirements.OfType<PGARequirement>().SelectMany(r => r.ProgramCodeRequirements).OfType<PGAProgramRequirement>())
			{
				requirement.Indicator = "Y";
			}

			// single program example
			var gacPivot = CreateImportPartPivot("GAC", "1234567892");
			gacPivot.CCA_GACIndicator = "Y";
			gacPivot.GACPGAHeader.CA_AllProgramInd = "Y";

			// multiple programs example
			CusClassPartPivot hcPivot = CreateImportPartPivot("HC:Blood & Med. Devices", "1234567893");
			hcPivot.CCA_HCIndicator = "Y";
			hcPivot.HCPGAHeader.CA_BBCProgramInd = "Y";
			hcPivot.HCPGAHeader.CA_MDEProgramInd = "Y";

			// mixed example
			var mixedPivot = CreateImportPartPivot("CFIA, HC:Pesticides & Vet. Drugs", "1234567894");
			mixedPivot.CCA_CFIAIndicator = "Y";
			mixedPivot.CFIAPGAHeader.CA_AllProgramInd = "Y";
			mixedPivot.CCA_HCIndicator = "Y";
			mixedPivot.HCPGAHeader.CA_PESProgramInd = "Y";
			mixedPivot.HCPGAHeader.CA_VETProgramInd = "Y";

			partsFactory.Save();

			var invoiceLine = CreateInvoiceLine();

			using (var form = new FormWithPGATabsForTest(invoiceLine))
			{
				form.Show();

				invoiceLine.JI_PartNo = "ALL";
				// sanity check, some indicators must be set
				AssertEquals("Y", invoiceLine.CA_CFIAInd);
				AssertEquals("Y", invoiceLine.CA_GACInd);
				AssertEquals("Y", invoiceLine.CA_HCInd);
				AssertEquals("Y", invoiceLine.CA_DFOInd);
				AssertEquals("Y", invoiceLine.CFIAPGAHeader.CA_AllProgramInd);
				AssertEquals("Y", invoiceLine.GACPGAHeader.CA_AllProgramInd);
				AssertEquals("Y", invoiceLine.HCPGAHeader.CA_BBCProgramInd);
				AssertEquals("Y", invoiceLine.DFOPGAHeader.CA_ABIProgramInd);

				CheckTabVisibilities(invoiceLine, form);
				invoiceLine.JI_PartNo = "NONE";
				// sanity check, some indicators must be unset
				AssertEquals("", invoiceLine.CA_CFIAInd);
				AssertEquals("", invoiceLine.CA_GACInd);
				AssertEquals("", invoiceLine.CA_HCInd);
				AssertEquals("", invoiceLine.CA_DFOInd);
				AssertEquals(true, invoiceLine.CFIAPGAHeader.IsNull);
				AssertEquals(true, invoiceLine.GACPGAHeader.IsNull);
				AssertEquals(true, invoiceLine.HCPGAHeader.IsNull);
				AssertEquals(true, invoiceLine.DFOPGAHeader.IsNull);

				CheckTabVisibilities(invoiceLine, form);

				invoiceLine.JI_PartNo = "GAC";

				AssertEquals("Y", invoiceLine.CA_GACInd);
				AssertEquals("Y", invoiceLine.GACPGAHeader.CA_AllProgramInd);

				CheckTabVisibilities(invoiceLine, form);

				invoiceLine.JI_PartNo = "HC:Blood & Med. Devices";

				AssertEquals("", invoiceLine.CA_GACInd);
				AssertEquals(true, invoiceLine.GACPGAHeader.IsNull);
				AssertEquals("Y", invoiceLine.CA_HCInd);
				AssertEquals("Y", invoiceLine.HCPGAHeader.CA_BBCProgramInd);
				AssertEquals("Y", invoiceLine.HCPGAHeader.CA_MDEProgramInd);

				CheckTabVisibilities(invoiceLine, form);

				invoiceLine.JI_PartNo = "CFIA, HC:Pesticides & Vet. Drugs";

				AssertEquals("Y", invoiceLine.CA_CFIAInd);
				AssertEquals("Y", invoiceLine.CFIAPGAHeader.CA_AllProgramInd);
				AssertEquals("Y", invoiceLine.CA_HCInd);
				AssertEquals("", invoiceLine.HCPGAHeader.CA_BBCProgramInd);
				AssertEquals("", invoiceLine.HCPGAHeader.CA_MDEProgramInd);
				AssertEquals("Y", invoiceLine.HCPGAHeader.CA_PESProgramInd);
				AssertEquals("Y", invoiceLine.HCPGAHeader.CA_VETProgramInd);

				CheckTabVisibilities(invoiceLine, form);

				// sumulate editing pivot in another window
				mixedPivot.CFIAPGAHeader.CA_AllProgramInd = "";
				mixedPivot.CCA_CFIAIndicator = "";
				mixedPivot.CCA_DFOIndicator = "Y";
				mixedPivot.DFOPGAHeader.CA_ABIProgramInd = "Y";
				partsFactory.Save();

				AssertEquals("Should NOT replace existing PGA data", "Y", invoiceLine.CA_CFIAInd);
				AssertEquals("Should NOT replace existing PGA data", "Y", invoiceLine.CFIAPGAHeader.CA_AllProgramInd);
				AssertEquals("Should NOT replace existing PGA data", "Y", invoiceLine.CA_HCInd);
				AssertEquals("Should NOT replace existing PGA data", "", invoiceLine.HCPGAHeader.CA_BBCProgramInd);
				AssertEquals("Should NOT replace existing PGA data", "", invoiceLine.HCPGAHeader.CA_MDEProgramInd);
				AssertEquals("Should NOT replace existing PGA data", "Y", invoiceLine.HCPGAHeader.CA_PESProgramInd);
				AssertEquals("Should NOT replace existing PGA data", "Y", invoiceLine.HCPGAHeader.CA_VETProgramInd);

				CheckTabVisibilities(invoiceLine, form);
			}
		}

		BusinessObjectFactory partsFactory;
		MasterFiles.Business.OrgHeader importer;
		MasterFiles.Business.OrgHeader supplier;
		CusClassification classification;

		protected override void SetUp()
		{
			base.SetUp();

			partsFactory = NewFactory();

			importer = Factory.New<MasterFiles.Business.OrgHeader>();
			importer.FillWithValidTestData();
			supplier = Factory.New<MasterFiles.Business.OrgHeader>();
			supplier.FillWithValidTestData();
			classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "TestLookup";
			classification.CC_ClassificationType = Enterprise.Customs.Business.BaseCusClassification.ClassificationType.IMP;
			Factory.Save();
		}

		JobComInvoiceLine CreateInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			return invoiceLine;
		}

		CusClassPartPivot CreateImportPartPivot(ZString partNum, ZString tariffNumber)
		{
			var part = partsFactory.New<OrgSupplierPart>();
			part.OP_PartNum = partNum;
			part.OP_Desc = "Some part";
			part.OP_StockKeepingUnit = "KG";
			part.RelatedOrganisations.AddOwner(partsFactory.Load<MasterFiles.Business.OrgHeader>(importer.PK));
			part.RelatedOrganisations.AddSupplier(partsFactory.Load<MasterFiles.Business.OrgHeader>(supplier.PK));

			var importPivot = part.PivotsForBinding.AddNew();
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			importPivot.CCA_RN_NKOrigin = Enterprise.Core.Constants.CountryCodes.Canada;
			importPivot.CCA_ProvinceOfOrigin = CanadianProvinceList.Codes.Alberta;
			importPivot.CI_TariffNum = tariffNumber;

			return importPivot;
		}

		static void CheckTabVisibilities(JobComInvoiceLine invoiceLine, Control owner)
		{
			// Practice shown that tests that compare itermediate values from PGARequirementCollection and PGAProgramRequirementCollection with UI or invoice line often miss scenarios or are not fully set up.
			// This method performs a comparison of invoice line properties with tab visibilities to make sure that the whole chain of wrappers works correctly.

			// ---- Uncomment the code below to see the form and interact with it during the test. ----
			//System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
			//while (sw.ElapsedMilliseconds < 3000)
			//{
			//	System.Windows.Forms.Application.DoEvents();
			//}

			CombineAssertions(() =>
			{
				AssertEquals("CFIA", invoiceLine.CFIAPGAHeader.CA_AllProgramInd == "Y", HasTabs(owner, "CFIA"));

				AssertEquals("HC/API", invoiceLine.HCPGAHeader.CA_APIProgramInd == "Y", HasTabs(owner, "HC", "Active Pharmaceutical Ingredients"));
				AssertEquals("HC/BBC", invoiceLine.HCPGAHeader.CA_BBCProgramInd == "Y", HasTabs(owner, "HC", "Blood and Blood Components"));
				AssertEquals("HC/CTO", invoiceLine.HCPGAHeader.CA_CTOProgramInd == "Y", HasTabs(owner, "HC", "Cells, Tissues and Organs"));
				AssertEquals("HC/CPR", invoiceLine.HCPGAHeader.CA_CPRProgramInd == "Y", HasTabs(owner, "HC", "Consumer Products"));
				AssertEquals("HC/DSE", invoiceLine.HCPGAHeader.CA_DSEProgramInd == "Y", HasTabs(owner, "HC", "Donor Semen or Ova"));
				AssertEquals("HC/HDR", invoiceLine.HCPGAHeader.CA_HDRProgramInd == "Y", HasTabs(owner, "HC", "Human Drugs (Including Radiopharmaceuticals)"));
				AssertEquals("HC/OCS", invoiceLine.HCPGAHeader.CA_OCSProgramInd == "Y", HasTabs(owner, "HC", "Office of Controlled Substances"));
				AssertEquals("HC/MDE", invoiceLine.HCPGAHeader.CA_MDEProgramInd == "Y", HasTabs(owner, "HC", "Medical Devices"));
				AssertEquals("HC/NHP", invoiceLine.HCPGAHeader.CA_NHPProgramInd == "Y", HasTabs(owner, "HC", "Natural Health Products"));
				AssertEquals("HC/PES", invoiceLine.HCPGAHeader.CA_PESProgramInd == "Y", HasTabs(owner, "HC", "Pesticides (Pest Management Regulatory Agency)"));
				AssertEquals("HC/RED", invoiceLine.HCPGAHeader.CA_REDProgramInd == "Y", HasTabs(owner, "HC", "Radiation Emitting Devices"));
				AssertEquals("HC/VET", invoiceLine.HCPGAHeader.CA_VETProgramInd == "Y", HasTabs(owner, "HC", "Veterinary Drugs"));

				AssertEquals("PHAC", invoiceLine.PHACPGAHeader.CA_HAPProgramInd == "Y", HasTabs(owner, "PHAC"));

				AssertEquals("TC/TPR", invoiceLine.TCPGAHeader.CA_TPRProgramInd == "Y", HasTabs(owner, "TC", "Tires Program"));
				AssertEquals("TC/VPR", invoiceLine.TCPGAHeader.CA_VPRProgramInd == "Y", HasTabs(owner, "TC", "Vehicle Program"));

				AssertEquals("ECCC/WRM", invoiceLine.ECCCPGAHeader.CA_WRMProgramInd == "Y", HasTabs(owner, "ECCC", "Waste Reduction And Management Division"));
				AssertEquals("ECCC/ODS", invoiceLine.ECCCPGAHeader.CA_ODSProgramInd == "Y", HasTabs(owner, "ECCC", "Ozone-Depleting Substances"));
				AssertEquals("ECCC/WEN", invoiceLine.ECCCPGAHeader.CA_WENProgramInd == "Y", HasTabs(owner, "ECCC", "Wildlife Enforcement"));
				AssertEquals("ECCC/VEE", invoiceLine.ECCCPGAHeader.CA_VEEProgramInd == "Y", HasTabs(owner, "ECCC", "Vehicle And Engine Emissions"));

				AssertEquals("NRCAN/EEF", invoiceLine.NRCanPGAHeader.CA_EEFProgramInd == "Y", HasTabs(owner, "NRCAN", "Office of Energy Efficiency"));
				AssertEquals("NRCAN/EXP", invoiceLine.NRCanPGAHeader.CA_EXPProgramInd == "Y", HasTabs(owner, "NRCAN", "Explosives"));
				AssertEquals("NRCAN/RDA", invoiceLine.NRCanPGAHeader.CA_RDAProgramInd == "Y", HasTabs(owner, "NRCAN", "Rough Diamonds"));

				AssertEquals("DFO/ABI", invoiceLine.DFOPGAHeader.CA_ABIProgramInd == "Y", HasTabs(owner, "DFO", "Aquatic Biotechnology Program"));
				AssertEquals("DFO/AIS", invoiceLine.DFOPGAHeader.CA_AISProgramInd == "Y", HasTabs(owner, "DFO", "Aquatic Invasive Species Program"));
				AssertEquals("DFO/TTP", invoiceLine.DFOPGAHeader.CA_TTPProgramInd == "Y", HasTabs(owner, "DFO", "Trade Tracking Program (Fisheries Resource Management)"));

				AssertEquals("CNSC", invoiceLine.CNSCPGAHeader.CA_AllProgramInd == "Y", HasTabs(owner, "CNSC"));

				AssertEquals("GAC", invoiceLine.GACPGAHeader.CA_AllProgramInd == "Y", HasTabs(owner, "GAC"));
			});
		}

		static bool HasTabs(Control control, params string[] selector)
		{
			foreach (var title in selector)
			{
				control = control.FindSingleOrDefault<ZTabPage>(p => p.Text == title && p.TabVisible);
				if (control == null)
				{
					return false;
				}
			}
			return true;
		}

		sealed class FormWithPGATabsForTest : ZForm
		{
			public FormWithPGATabsForTest(JobComInvoiceLine invoiceLine)
			{
				SetupForm(invoiceLine);
			}

			void SetupForm(JobComInvoiceLine invoiceLine)
			{
				SuspendLayout();

				MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 600, true);

				var tabControl = new ZTabControl();
				tabControl.Dock = DockStyle.Fill;
				Controls.Add(tabControl);

				PGATabCollection tabCollection = new PGATabCollection(tabControl, null, true);
				tabCollection.Update(invoiceLine.PGARequirements);

				ResumeLayout();
			}
		}
	}
}
