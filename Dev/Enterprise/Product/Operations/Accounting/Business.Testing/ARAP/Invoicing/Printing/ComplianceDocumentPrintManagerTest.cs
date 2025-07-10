using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Printing.Testing
{
	public class ComplianceDocumentPrintManagerTest : TestCaseWithFactory
	{
		public void TestGetComplianceDocumentPrintTask()
		{
			var complianceDocumentsHeaders = new AccComplianceDocumentHeader[2];
			complianceDocumentsHeaders[0] = Factory.NewWithValidTestData(typeof(ARComplianceDocumentHeader)) as ARComplianceDocumentHeader;
			complianceDocumentsHeaders[1] = Factory.NewWithValidTestData(typeof(ARComplianceDocumentHeader)) as ARComplianceDocumentHeader;

			StmMenuItem menu = CreateMenuItem();

			var sequenceBBB = CreateComplianceSequence("BBB", "TXC", ComplianceBookAllocationLevel.Counter, true);
			sequenceBBB.XD_RollupBehaviourWhenMaxExceeded = ComplianceRollupBehaviourType.MultiPageNoLimitation;
			sequenceBBB.XD_SU_MenuItem = menu.PK;

			complianceDocumentsHeaders[0].ADH_DocumentStatus = ComplianceDocumentStatus.Added;
			complianceDocumentsHeaders[0].ADH_ComplianceSubType = "TXC";
			complianceDocumentsHeaders[0].ADH_XD_ComplianceBook = sequenceBBB.PK;
			complianceDocumentsHeaders[0].ADH_DocumentNumber = "000000001";

			complianceDocumentsHeaders[1].ADH_DocumentStatus = ComplianceDocumentStatus.Added;
			complianceDocumentsHeaders[1].ADH_ComplianceSubType = "TXC";
			complianceDocumentsHeaders[1].ADH_XD_ComplianceBook = sequenceBBB.PK;
			complianceDocumentsHeaders[1].ADH_DocumentNumber = "000000002";

			Factory.Save();

			var complianceDocumentPrintManager = new ComplianceDocumentPrintManager();
			var printTasks = complianceDocumentPrintManager.GetComplianceDocumentPrintTasks(complianceDocumentsHeaders);

			AssertNotNull(printTasks);
			AssertEquals(1, printTasks.Count());

			var pack = printTasks.First();
			var printErrorMessage = complianceDocumentPrintManager.PrintFailureInfomation.PrintTaskErrorMessage;
			var failureCount = complianceDocumentPrintManager.PrintFailureInfomation.FailureCount;

			AssertEquals(ZString.Empty, printErrorMessage);
			AssertEquals(0, failureCount);
			AssertEquals("expect 2 document packs", 2, pack.Count);
			AssertEquals("AccComplianceDocumentSupporter", pack[0].DocumentSupporter.GetType().Name);

			var printer = Factory.NewWithValidTestData<StmPrintQueue>();
			sequenceBBB.XD_SQ_DocumentPrintQueue = printer.PK;
			printTasks = complianceDocumentPrintManager.GetComplianceDocumentPrintTasks(complianceDocumentsHeaders);
			printErrorMessage = complianceDocumentPrintManager.PrintFailureInfomation.PrintTaskErrorMessage;
			failureCount = complianceDocumentPrintManager.PrintFailureInfomation.FailureCount;

			AssertNotNull(printTasks);
			AssertEquals(1, printTasks.Count());

			pack = printTasks.First();

			AssertEquals(ZString.Empty, printErrorMessage);
			AssertEquals(0, failureCount);
			AssertEquals("expect 2 document packs", 2, pack.Count);

			var sequenceAAA = CreateComplianceSequence("AAA", "TXC", ComplianceBookAllocationLevel.Counter, true);
			sequenceAAA.XD_RollupBehaviourWhenMaxExceeded = ComplianceRollupBehaviourType.MultiPageNoLimitation;
			sequenceAAA.XD_SU_MenuItem = menu.PK;
			complianceDocumentsHeaders[1].ADH_XD_ComplianceBook = sequenceAAA.PK;
			printTasks = complianceDocumentPrintManager.GetComplianceDocumentPrintTasks(complianceDocumentsHeaders);
			printErrorMessage = complianceDocumentPrintManager.PrintFailureInfomation.PrintTaskErrorMessage;
			failureCount = complianceDocumentPrintManager.PrintFailureInfomation.FailureCount;

			AssertNotNull(printTasks);
			AssertEquals(2, printTasks.Count());
			printTasks.ForEach(x => AssertEquals(1, x.Count));
			AssertEquals(ZString.Empty, printErrorMessage);
			AssertEquals(0, failureCount);

			complianceDocumentsHeaders[1].ADH_XD_ComplianceBook = sequenceBBB.PK;
			sequenceBBB.XD_SU_MenuItem = ZGuid.Empty;
			printTasks = complianceDocumentPrintManager.GetComplianceDocumentPrintTasks(complianceDocumentsHeaders);
			AssertEquals(0, printTasks.Count());
			printErrorMessage = complianceDocumentPrintManager.PrintFailureInfomation.PrintTaskErrorMessage;
			failureCount = complianceDocumentPrintManager.PrintFailureInfomation.FailureCount;

			AssertEquals(2, failureCount);
			AssertEquals(@"No Compliance Invoice Document will be Printed.
 This Compliance Book is not configured for printing.
 If you want to print a Compliance Document please amend your Compliance Book setups and nominate an appropriate document menu for printing.", printErrorMessage);

			sequenceBBB.XD_SU_MenuItem = ZGuid.NewZGuid();
			var complianceDocumentPrintManager1 = new ComplianceDocumentPrintManager();
			printTasks = complianceDocumentPrintManager1.GetComplianceDocumentPrintTasks(complianceDocumentsHeaders);
			AssertEquals(0, printTasks.Count());
			printErrorMessage = complianceDocumentPrintManager1.PrintFailureInfomation.PrintTaskErrorMessage;
			failureCount = complianceDocumentPrintManager1.PrintFailureInfomation.FailureCount;

			AssertEquals(2, failureCount);
			AssertEquals("Government Invoice Menu can not be found for compliance subtype TXC.", printErrorMessage);
		}

		public void TestUpdateComplianceDocumentPrintCountAsPrinted()
		{
			var complianceDocumentsHeaders = new AccComplianceDocumentHeader[2];
			complianceDocumentsHeaders[0] = Factory.NewWithValidTestData(typeof(ARComplianceDocumentHeader)) as ARComplianceDocumentHeader;
			complianceDocumentsHeaders[1] = Factory.NewWithValidTestData(typeof(ARComplianceDocumentHeader)) as ARComplianceDocumentHeader;

			var menu = CreateMenuItem();

			var sequenceBBB = CreateComplianceSequence("BBB", "TXC", ComplianceBookAllocationLevel.Counter, true);
			sequenceBBB.XD_RollupBehaviourWhenMaxExceeded = ComplianceRollupBehaviourType.MultiPageNoLimitation;
			sequenceBBB.XD_SU_MenuItem = menu.PK;

			var sequenceAAA = CreateComplianceSequence("AAA", "TXE", ComplianceBookAllocationLevel.Counter, true);
			sequenceAAA.XD_RollupBehaviourWhenMaxExceeded = ComplianceRollupBehaviourType.MultiPageNoLimitation;
			Factory.Save();

			complianceDocumentsHeaders[0].ADH_DocumentStatus = ComplianceDocumentStatus.Added;
			complianceDocumentsHeaders[0].ADH_ComplianceSubType = "TXC";
			complianceDocumentsHeaders[0].ADH_XD_ComplianceBook = sequenceBBB.PK;
			complianceDocumentsHeaders[0].ADH_DocumentNumber = "BBB000001";

			complianceDocumentsHeaders[1].ADH_DocumentStatus = ComplianceDocumentStatus.Added;
			complianceDocumentsHeaders[1].ADH_ComplianceSubType = "TXE";
			complianceDocumentsHeaders[1].ADH_XD_ComplianceBook = sequenceAAA.PK;
			complianceDocumentsHeaders[1].ADH_DocumentNumber = "AAA000001";
			Factory.Save();

			var printManager = new ComplianceDocumentPrintManager();
			AssertEquals(0, complianceDocumentsHeaders[0].ADH_PrintCount);
			AssertEquals(0, complianceDocumentsHeaders[1].ADH_PrintCount);

			var task = printManager.GetComplianceDocumentPrintTasks(complianceDocumentsHeaders).First();
			task.Run(new DeliveryInstructions() { Destination = DeliveryInstructionDestination.Print });
			task.invalidDocumentParentPKs = new List<ZGuid> { complianceDocumentsHeaders[1].PK };
			printManager.UpdateComplianceDocumentPrintCountAsPrinted(task, complianceDocumentsHeaders.Select(x => x.PK).ToArray());

			AssertEquals(1, complianceDocumentsHeaders[0].ADH_PrintCount);
			AssertEquals(0, complianceDocumentsHeaders[1].ADH_PrintCount);
		}

		public void TestComplianceDocumentPrintOrder()
		{
			var complianceDocumentsHeaders = new AccComplianceDocumentHeader[5];
			complianceDocumentsHeaders[0] = Factory.NewWithValidTestData(typeof(ARComplianceDocumentHeader)) as ARComplianceDocumentHeader;
			complianceDocumentsHeaders[1] = Factory.NewWithValidTestData(typeof(ARComplianceDocumentHeader)) as ARComplianceDocumentHeader;
			complianceDocumentsHeaders[2] = Factory.NewWithValidTestData(typeof(ARComplianceDocumentHeader)) as ARComplianceDocumentHeader;
			complianceDocumentsHeaders[3] = Factory.NewWithValidTestData(typeof(ARComplianceDocumentHeader)) as ARComplianceDocumentHeader;
			complianceDocumentsHeaders[4] = Factory.NewWithValidTestData(typeof(ARComplianceDocumentHeader)) as ARComplianceDocumentHeader;
			var complianceDocumentNumbers = new ZString[] { "BBB000005", "BBB000003", "BBB000001", "BBB000004", "BBB000002" };

			var menu = CreateMenuItem();

			var sequenceBBB = CreateComplianceSequence("BBB", "TXC", ComplianceBookAllocationLevel.Counter, true);
			sequenceBBB.XD_RollupBehaviourWhenMaxExceeded = ComplianceRollupBehaviourType.MultiPageNoLimitation;
			sequenceBBB.XD_SU_MenuItem = menu.PK;

			Factory.Save();

			for (int i = 0; i < complianceDocumentsHeaders.Length; i++)
			{
				complianceDocumentsHeaders[i].ADH_DocumentStatus = ComplianceDocumentStatus.Added;
				complianceDocumentsHeaders[i].ADH_ComplianceSubType = "TXC";
				complianceDocumentsHeaders[i].ADH_XD_ComplianceBook = sequenceBBB.PK;
				complianceDocumentsHeaders[i].ADH_DocumentNumber = complianceDocumentNumbers[i];
			}
			Factory.Save();

			var printManager = new ComplianceDocumentPrintManager();
			var tasks = printManager.GetComplianceDocumentPrintTasks(complianceDocumentsHeaders);
			foreach (var task in tasks)
			{
				var documentPacks = task.GetDocumentPacks();
				var fDocumentNumber = ZString.Empty;
				foreach (var documentPack in documentPacks)
				{
					var documentNumber = ((AccComplianceDocumentHeader)documentPack.BizObject).ADH_DocumentNumber;
					Assert("Printing must be in the order of document number", fDocumentNumber.CompareTo(documentNumber) < 0);
					fDocumentNumber = documentNumber;
				}
			}
		}

		StmMenuItem CreateMenuItem()
		{
			StmMenuItem menu = Factory.NewWithValidTestData<StmMenuItem>();

			menu.SU_MenuName = "test";
			menu.SU_BusinessContext = "ARComplianceDocument";
			menu.SU_MenuPath = ZString.Empty;
			menu.SU_IsSystemDefined = true;
			menu.SU_MenuType = "DOC";
			menu.SU_GS_NKStaffCode = ZString.Empty;
			menu.SU_ContactType = "NCT";
			menu.SU_IsPublished = true;
			return menu;
		}

		AccComplianceSequence CreateComplianceSequence(string code, string sequenceClass, string allocationLevel, bool lockBy = false)
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_SequenceClass = sequenceClass;
			sequence.XD_Code = code;
			sequence.XD_StartNumber = 1;
			sequence.XD_EndNumber = 100;
			sequence.XD_NextNumber = 1;
			sequence.XD_MaximumNumberDigits = 9;
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			sequence.XD_AllocationLevel = allocationLevel;
			if (sequence.AllocationStrategy.IsBranchApplicable)
			{
				sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			}
			if (sequence.AllocationStrategy.IsDepartmentApplicable)
			{
				sequence.XD_GE_Department = GlbDepartment.CurrentDepartment.PK;
			}
			if (lockBy)
			{
				sequence.XD_LockBy = GlbStaff.CurrentUser.PK;
			}
			return sequence;
		}
	}
}
