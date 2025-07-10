using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitHeader))]
	sealed class CusExitHeaderTest : EU.ExitControl.Business.Testing.CusExitHeaderAbstractTest<CusExitHeader>
	{
		public void TestValidation()
		{
			AssertType<CusExitHeaderValidation>(exitHeader.Validation);
		}

		public void TestCusExitConsignments()
		{
			AssertType<ExitControlBase.Business.CusExitConsignmentCollection<CusExitConsignment>>(exitHeader.CusExitConsignments);
		}

		public void TestCusExitReports()
		{
			AssertType<ExitControlBase.Business.CusExitReportCollection<CusExitReport>>(exitHeader.CusExitReports);
		}

		public void TestDefaultDataFromParent()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
			var entryHeader4 = declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			var mrn1 = CusEntryNumber.New(entryHeader1, CusEntryNumberTypes.Standard.MovementReferenceNumber, declaration.CountryCode);
			var mrn2 = CusEntryNumber.New(entryHeader2, CusEntryNumberTypes.Standard.MovementReferenceNumber, declaration.CountryCode);
			var lrn1 = CusEntryNumber.New(entryHeader3, CusEntryNumberTypes.Standard.LocalReferenceNumber, declaration.CountryCode);
			var lrn2 = CusEntryNumber.New(entryHeader4, CusEntryNumberTypes.Standard.LocalReferenceNumber, declaration.CountryCode);
			mrn1.CE_EntryNum = "MRN1";
			mrn2.CE_EntryNum = "MRN2";
			lrn1.CE_EntryNum = "LRN1";
			lrn2.CE_EntryNum = "LRN2";

			CombineAssertions(() =>
			{
				var consignments = exitHeader.CusExitConsignments;
				var consignment1 = consignments.AddNew();
				consignment1.CXC_MovementReference = "MRN1";
				var consignment2 = consignments.AddNew();
				consignment2.CXC_MovementReference = "MRN3";
				var consignment3 = consignments.AddNew();
				consignment3.CXC_LocalReference = "LRN1";
				var consignment4 = consignments.AddNew();
				consignment4.CXC_LocalReference = "LRN3";

				exitHeader.Parent = declaration;
				exitHeader.DefaultDataFromParent(false);
				AssertContainsExactElementsInAnyOrder(new[] { "MRN1-", "MRN2-", "MRN3-", "-LRN1", "-LRN2", "-LRN3" }, exitHeader.CusExitConsignments.Select(x => $"{x.CXC_MovementReference}-{x.CXC_LocalReference}"));
			});
		}

		public void TestDefaultDataFromParent_ShouldNotCreateLRNConsignment_WhenMRNConsignmentExists()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var mrn = CusEntryNumber.New(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, declaration.CountryCode);
			mrn.CE_EntryNum = "MRN1";
			var lrn = CusEntryNumber.New(entryHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, declaration.CountryCode);
			lrn.CE_EntryNum = "LRN1";

			var consignment = exitHeader.CusExitConsignments.AddNew();
			consignment.CXC_MovementReference = "MRN1";

			CombineAssertions(() =>
			{
				AssertEquals("Precondition", 1, exitHeader.CusExitConsignments.Count);

				exitHeader.Parent = declaration;
				exitHeader.DefaultDataFromParent(false);
				AssertEquals("Count", 1, exitHeader.CusExitConsignments.Count);
				AssertEquals("CXC_MovementReference", "MRN1", exitHeader.CusExitConsignments[0].CXC_MovementReference);
				AssertEquals("CXC_LocalReference", ZString.Empty, exitHeader.CusExitConsignments[0].CXC_LocalReference);
			});
		}

		public void TestDefaultDataFromParent_IncludeHeaderData()
		{
			var company = Factory.New<GlbCompany>();
			var orgProxy = Factory.New<OrgHeader>();
			var branch = company.Branches.AddNew();
			branch.GB_OH_OrgProxy = orgProxy.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch.PK;
			var supplier = Factory.New<OrgHeader>();
			declaration.JE_OH_Supplier = supplier.PK;

			var exitHeader = Factory.New<CusExitHeader>();
			exitHeader.Parent = declaration;
			exitHeader.DefaultDataFromParent(true);
			CombineAssertions(() =>
			{
				AssertEquals("CXH_GB_Branch", branch.PK, exitHeader.CXH_GB_Branch);
				AssertEquals("CXH_OH_Exporter", supplier.PK, exitHeader.CXH_OH_Exporter);
				AssertEquals("CXH_OA_Carrier", branch.OrgProxy.MainAddress.PK, exitHeader.CXH_OA_Carrier);
			});
		}

		public void TestDefaultDataFromParent_Shipment_NoDeclaration()
		{
			CreateUNLOCOsForTest();

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "Consignor";

			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "OrgProxy";
			Factory.Save();

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = orgProxy.PK;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "USCLT";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			shipment.CustomsEntryNumber = "MRN001";

			var consol = Factory.New<ForwardingConsol>();

			consol.JK_MasterBillNum = "02032646283";
			consol.Shipments.Add(shipment);
			consol.Transports.RemoveAndDeleteAll();
			consol.Transports.AddNew("DE123", "US123");

			var exitHeader = Factory.New<CusExitHeader>();
			exitHeader.Parent = shipment;
			exitHeader.DefaultDataFromParent(true);
			CombineAssertions(() =>
			{
				AssertEquals("CXH_OH_Exporter", consignor.PK, exitHeader.CXH_OH_Exporter);
				AssertEquals("CXH_OA_Carrier", GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, exitHeader.CXH_OA_Carrier);

				var exitConsignment = exitHeader.CusExitConsignments.Single();
				AssertEquals("CXC_MovementReference", "MRN001", exitConsignment.CXC_MovementReference);
				AssertEquals("CXC_ReferenceNumber", "02032646283", exitConsignment.CXC_ReferenceNumber);

				exitHeader.DefaultDataFromParent(true);
				AssertEquals("Should not create consignment when MRNConsignment exists", 1, exitHeader.CusExitConsignments.Count);

				exitConsignment.CXC_MovementReference = "MRN002";
				exitHeader.DefaultDataFromParent(true);
				AssertEquals("Should create consignment when MRN changes", 2, exitHeader.CusExitConsignments.Count);
			});
		}

		public void TestDefaultDataFromParent_Shipment_DeclarationExists()
		{
			CreateUNLOCOsForTest();

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "Consignor";

			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "OrgProxy";
			Factory.Save();

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = orgProxy.PK;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "DE123";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			shipment.CustomsEntryNumber = "MRN001";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "US123";
			consol.JK_MasterBillNum = "02032646283";
			consol.Shipments.Add(shipment);
			consol.Transports.RemoveAndDeleteAll();
			consol.Transports.AddNew("DE123", "US123");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var mrn = CusEntryNumber.New(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, declaration.CountryCode);
			mrn.CE_EntryNum = "MRN001";

			var exitHeader = Factory.New<CusExitHeader>();
			exitHeader.Parent = shipment;

			exitHeader.DefaultDataFromParent(true);

			var exitConsignment = exitHeader.CusExitConsignments.Single();

			CombineAssertions(() =>
			{
				AssertEquals("CXH_OA_Carrier", declaration.Branch.OrgProxy.MainAddress.PK, exitHeader.CXH_OA_Carrier);

				AssertEquals("CXC_MovementReference", "MRN001", exitConsignment.CXC_MovementReference);
				AssertEquals("CXC_ReferenceNumber", "02032646283", exitConsignment.CXC_ReferenceNumber);

				exitHeader.DefaultDataFromParent(true);
				AssertEquals("Should not create consignment when MRNConsignment exists", 1, exitHeader.CusExitConsignments.Count);

				exitConsignment.CXC_MovementReference = "MRN002";
				exitHeader.DefaultDataFromParent(true);
				AssertEquals("Should create consignment when MRN changes", 2, exitHeader.CusExitConsignments.Count);
			});
		}

		public void TestDefaultDataFromParent_Shipment_WhenNoMatchedConsol()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "USCLT";
			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			shipment.CustomsEntryNumber = "MRN001";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_MasterBillNum = "02032646283";
			consol.Shipments.Add(shipment);

			var exitHeader = Factory.New<CusExitHeader>();
			exitHeader.Parent = shipment;
			exitHeader.DefaultDataFromParent(false);

			var exitConsignment = exitHeader.CusExitConsignments.Single();
			AssertEquals("CXC_ReferenceNumber", ZString.Empty, exitConsignment.CXC_ReferenceNumber);
		}

		public void TestIsAutomatedValidationEnabledTRA()
		{
			var header = GetNewBusinessObject(Factory);

			CombineAssertions(() =>
			{
				SetEnableAutomatedValidationTRA(header, enableAutomatedValidationTRA: false);
				AssertEquals("TRA Automated Validation false", expected: false, header.IsAutomatedValidationEnabledTRA);

				SetEnableAutomatedValidationTRA(header, enableAutomatedValidationTRA: true);
				AssertEquals("TRA Automated Validation true", expected: true, header.IsAutomatedValidationEnabledTRA);
			});
		}

		public void TestIsAutomatedValidationEnabledTRA_Cached()
		{
			var header = GetNewBusinessObject(Factory);
			var mock = Factory.NewMoq<ProcessTask>();
			CombineAssertions(() =>
			{
				SetEnableAutomatedValidationTRA(header, enableAutomatedValidationTRA: false);
				header.WorkflowItems.Add(mock.Object);
				mock.Invocations.Clear();
				AssertEquals("TRA Automated Validation true", expected: false, header.IsAutomatedValidationEnabledTRA);

				AssertEquals("TRA Automated Validation cached", expected: false, header.IsAutomatedValidationEnabledTRA);
				mock.VerifyGet(m => m.P9_Type, Times.Exactly(1));
			});
		}

		public void TestDefaultDataFromShipment_MultipleMRNs()
		{
			CreateUNLOCOsForTest();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "USCLT";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "US123";
			consol.JK_MasterBillNum = "02032646283";
			consol.Shipments.Add(shipment);
			consol.Transports.RemoveAndDeleteAll();
			consol.Transports.AddNew("DE123", "US123");

			var num1 = shipment.CusEntryNumbers.AddNew();
			num1.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			num1.CE_EntryNum = "MRN001345435345";

			var num2 = shipment.CusEntryNumbers.AddNew();
			num2.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			num2.CE_EntryNum = "MRN002345345435";

			var exitHeader = Factory.New<CusExitHeader>();
			exitHeader.Parent = shipment;
			exitHeader.DefaultDataFromParent(true);

			var movementReferences = exitHeader.CusExitConsignments.Select(x => x.CXC_MovementReference.ToString()).ToArray();
			var referenceNumbers = exitHeader.CusExitConsignments.Select(x => x.CXC_ReferenceNumber.ToString()).ToArray();

			CombineAssertions(() =>
			{
				AssertEquals("Should create multiple consignment when MRN changes", 2, exitHeader.CusExitConsignments.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "MRN001345435345", "MRN002345345435" }, movementReferences);
				AssertContainsExactElementsInAnyOrder(new[] { "02032646283", "02032646283" }, referenceNumbers);
			});
		}

		internal static void SetEnableAutomatedValidationTRA(CusExitHeader header, bool enableAutomatedValidationTRA)
		{
			if (enableAutomatedValidationTRA)
			{
				var processTask = header.WorkflowItems.AddNew();
				processTask.P9_Type = "TRG";
				processTask.TriggerConditions.TriggerEventCode = "CES";
				processTask.TriggerConditions.TriggerCondition = "REF";
				processTask.TriggerConditions.TriggerConditionValue = "310";
				processTask.P9_LineTriggerType = "CXR";
				processTask.ProcessTaskNotifications.AddNew().PQ_TriggerType = "TRA";
			}
			else
			{
				header.WorkflowItems.RemoveAndDeleteAll();
			}

			header.Factory.InvalidateCachedProperties();
		}

		void CreateUNLOCOsForTest()
		{
			var helper = new MasterFilesTestHelper(Factory);
			var germany = RefCountry.LoadFromCountryCode(Factory, "DE");
			var unitedStates = RefCountry.LoadFromCountryCode(Factory, "US");
			helper.CreateUnlocoIfNotExists("DE123", germany);
			helper.CreateUnlocoIfNotExists("US123", unitedStates);
			Factory.Save();
		}
	}
}
