using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(Allocation))]
	public class AllocationTest : NonPersistentBusinessObjectTestCase
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		public void TestRunPreSaveValidation()
		{
			Allocation.Queue = "TST";
			Allocation.Reason = "TST";
			Allocation.RunPreSaveValidation();
			AssertEquals(true, Allocation.QueueInfo.HasErrors());
			AssertEquals(true, Allocation.ReasonInfo.HasErrors());
		}

		public void TestQueueValidation()
		{
			Allocation.Queue = "TST";
			AssertEquals(true, Allocation.QueueInfo.HasErrors());
			Allocation.Queue = DeclarationQueueCodeDescriptionPairList.Codes.Classification;
			AssertEquals(false, Allocation.QueueInfo.HasErrors());
			Allocation.Queue = "";
			AssertEquals(true, Allocation.QueueInfo.HasErrors());
		}

		public void TestQueueList()
		{
			AssertNotNull(Allocation.QueueList);
		}

		public void TestReasonValidation()
		{
			Allocation.Reason = "TST";
			AssertEquals(true, Allocation.ReasonInfo.HasErrors());
			Allocation.Reason = ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration;
			AssertEquals(false, Allocation.ReasonInfo.HasErrors());
			Allocation.Reason = "";
			AssertEquals(false, Allocation.ReasonInfo.HasErrors());
		}

		public void TestReasonList()
		{
			AssertNotNull(Allocation.ReasonList);
		}

		public void TestClassifierAllocationList()
		{
			AssertNotNull(Allocation.ClassifierAllocationList);
		}

		public void TestReLoadClassifierAllocation_QueueSet()
		{
			SetUpQueues(false);
			Allocation.Queue = DeclarationQueueCodeDescriptionPairList.Codes.Classification;
			Allocation.ReLoadClassifierAllocation();
			AssertEquals(3, Allocation.ClassifierAllocationList.Count);
			ClassifierAllocation classifierAllocation = Allocation.ClassifierAllocationList[0];
			AssertEquals("DD", classifierAllocation.AllocatedTo);
			AssertEquals(1, classifierAllocation.NumberAllocated);
			classifierAllocation = Allocation.ClassifierAllocationList[1];
			AssertEquals("AA", classifierAllocation.AllocatedTo);
			AssertEquals(2, classifierAllocation.NumberAllocated);
			classifierAllocation = Allocation.ClassifierAllocationList[2];
			AssertEquals("BB", classifierAllocation.AllocatedTo);
			AssertEquals(3, classifierAllocation.NumberAllocated);
		}

		public void TestReLoadClassifierAllocation_QueueAndReasonSet()
		{
			SetUpQueues(true);
			Allocation.Queue = DeclarationQueueCodeDescriptionPairList.Codes.Classification;
			Allocation.Reason = ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration;
			Allocation.ReLoadClassifierAllocation();
			AssertEquals(1, Allocation.ClassifierAllocationList.Count);
			ClassifierAllocation classifierAllocation = Allocation.ClassifierAllocationList[0];
			AssertEquals("AA", classifierAllocation.AllocatedTo);
			AssertEquals(2, classifierAllocation.NumberAllocated);
		}

		void SetUpQueues(bool setReason)
		{
			UPEJobDeclaration uPEJobDeclaration = Factory.New<UPEJobDeclaration>();
			uPEJobDeclaration.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo = "AA";
			uPEJobDeclaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Classification;
			if (setReason)
			{
				uPEJobDeclaration.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration;
			}
			else
			{
				uPEJobDeclaration.CurrentQueue.P4_CustomsStatus = "";
			}

			uPEJobDeclaration = Factory.New<UPEJobDeclaration>();
			uPEJobDeclaration.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo = "AA";
			uPEJobDeclaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Classification;
			if (setReason)
			{
				uPEJobDeclaration.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration;
			}
			else
			{
				uPEJobDeclaration.CurrentQueue.P4_CustomsStatus = "";
			}

			uPEJobDeclaration = Factory.New<UPEJobDeclaration>();
			uPEJobDeclaration.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo = "AA";
			uPEJobDeclaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding;
			if (setReason)
			{
				uPEJobDeclaration.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration;
			}

			uPEJobDeclaration = Factory.New<UPEJobDeclaration>();
			uPEJobDeclaration.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo = "BB";
			uPEJobDeclaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Classification;
			uPEJobDeclaration.CurrentQueue.P4_CustomsStatus = "";
			uPEJobDeclaration = Factory.New<UPEJobDeclaration>();
			uPEJobDeclaration.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo = "BB";
			uPEJobDeclaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Classification;
			uPEJobDeclaration.CurrentQueue.P4_CustomsStatus = "";
			uPEJobDeclaration = Factory.New<UPEJobDeclaration>();
			uPEJobDeclaration.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo = "BB";
			uPEJobDeclaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Classification;
			uPEJobDeclaration.CurrentQueue.P4_CustomsStatus = "";
			uPEJobDeclaration = Factory.New<UPEJobDeclaration>();
			uPEJobDeclaration.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo = "CC";
			uPEJobDeclaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Completed;
			uPEJobDeclaration = Factory.New<UPEJobDeclaration>();
			uPEJobDeclaration.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo = "CC";
			uPEJobDeclaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Completed;
			uPEJobDeclaration = Factory.New<UPEJobDeclaration>();
			uPEJobDeclaration.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo = "DD";
			uPEJobDeclaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Completed;
			uPEJobDeclaration = Factory.New<UPEJobDeclaration>();
			uPEJobDeclaration.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo = "DD";
			uPEJobDeclaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Classification;
			uPEJobDeclaration.CurrentQueue.P4_CustomsStatus = "";
			Factory.Save();
		}

		[TestDate(2006, 3, 31)]
		public void TestReAllocate_OneStaffIsNotAClassifierAndIsIncluded()
		{
			CreateAndSaveJobs();
			Allocation.Queue = DeclarationQueueCodeDescriptionPairList.Codes.Classification;
			Allocation.ReLoadClassifierAllocation();
			AssertEquals(5, Allocation.ClassifierAllocationList.Count);
			Allocation.ClassifierAllocationList.Sort("AllocatedTo", ListSortDirection.Ascending);
			Allocation.ClassifierAllocationList[0].IncludeForAllocation = false;
			Allocation.ClassifierAllocationList[4].IncludeForAllocation = false;
			Allocation.ReAllocate();
			Allocation.ReLoadClassifierAllocation();
			AssertEquals(5, Allocation.ClassifierAllocationList.Count);
			Allocation.ClassifierAllocationList.Sort("AllocatedTo", ListSortDirection.Ascending);
			AssertEquals(0, Allocation.ClassifierAllocationList[0].NumberAllocated);
			AssertEquals(5, Allocation.ClassifierAllocationList[1].NumberAllocated);
			AssertEquals(5, Allocation.ClassifierAllocationList[2].NumberAllocated);
			AssertEquals(5, Allocation.ClassifierAllocationList[3].NumberAllocated);
			AssertEquals(0, Allocation.ClassifierAllocationList[4].NumberAllocated);
		}

		[TestDate(2006, 3, 31)]
		public void TestReAllocate_OneStaffIsNotAClassifierAndIsExcluded()
		{
			CreateAndSaveJobs();
			Allocation.Queue = DeclarationQueueCodeDescriptionPairList.Codes.Classification;
			Allocation.ReLoadClassifierAllocation();
			AssertEquals(5, Allocation.ClassifierAllocationList.Count);
			Allocation.ClassifierAllocationList.Sort("AllocatedTo", ListSortDirection.Ascending);
			Allocation.ClassifierAllocationList[1].IncludeForAllocation = false;
			Allocation.ClassifierAllocationList[3].IncludeForAllocation = false;
			Allocation.ReAllocate();
			Allocation.ReLoadClassifierAllocation();
			AssertEquals(4, Allocation.ClassifierAllocationList.Count);
			Allocation.ClassifierAllocationList.Sort("AllocatedTo", ListSortDirection.Ascending);
			AssertEquals(5, Allocation.ClassifierAllocationList[0].NumberAllocated);
			AssertEquals(0, Allocation.ClassifierAllocationList[1].NumberAllocated);
			AssertEquals(5, Allocation.ClassifierAllocationList[2].NumberAllocated);
			AssertEquals(5, Allocation.ClassifierAllocationList[3].NumberAllocated);
		}

		public void TestReAllocate_AllFalse()
		{
			CreateAndSaveJobs();
			Allocation.Queue = DeclarationQueueCodeDescriptionPairList.Codes.Classification;
			Allocation.ReLoadClassifierAllocation();
			AssertEquals(5, Allocation.ClassifierAllocationList.Count);
			Allocation.ClassifierAllocationList.Sort("AllocatedTo", ListSortDirection.Ascending);
			Allocation.ClassifierAllocationList[0].IncludeForAllocation = false;
			Allocation.ClassifierAllocationList[1].IncludeForAllocation = false;
			Allocation.ClassifierAllocationList[2].IncludeForAllocation = false;
			Allocation.ClassifierAllocationList[3].IncludeForAllocation = false;
			Allocation.ClassifierAllocationList[4].IncludeForAllocation = false;
			Allocation.ReAllocate();
			Allocation.ReLoadClassifierAllocation();
			AssertEquals(5, Allocation.ClassifierAllocationList.Count);
			Allocation.ClassifierAllocationList.Sort("AllocatedTo", ListSortDirection.Ascending);
			AssertEquals(6, Allocation.ClassifierAllocationList[0].NumberAllocated);
			AssertEquals(2, Allocation.ClassifierAllocationList[1].NumberAllocated);
			AssertEquals(1, Allocation.ClassifierAllocationList[2].NumberAllocated);
			AssertEquals(1, Allocation.ClassifierAllocationList[3].NumberAllocated);
			AssertEquals(5, Allocation.ClassifierAllocationList[4].NumberAllocated);
		}

		public void TestReAllocate_NoRecords()
		{
			Allocation.Queue = DeclarationQueueCodeDescriptionPairList.Codes.Classification;
			Allocation.ReLoadClassifierAllocation();
			AssertEquals(0, Allocation.ClassifierAllocationList.Count);
			Allocation.ReAllocate();
			Allocation.ReLoadClassifierAllocation();
			AssertEquals(0, Allocation.ClassifierAllocationList.Count);
		}

		[TestDate(2006, 3, 31)]
		public void TestReAllocate_IncludeReasonFilter()
		{
			CreateAndSaveJobs();
			Allocation.Queue = DeclarationQueueCodeDescriptionPairList.Codes.Classification;
			Allocation.Reason = ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration;
			Allocation.ReLoadClassifierAllocation();
			AssertEquals(4, Allocation.ClassifierAllocationList.Count);
			Allocation.ClassifierAllocationList.Sort("AllocatedTo", ListSortDirection.Ascending);
			Allocation.ClassifierAllocationList[1].IncludeForAllocation = false;
			Allocation.ClassifierAllocationList[2].IncludeForAllocation = false;
			Allocation.ClassifierAllocationList[3].IncludeForAllocation = false;
			Allocation.ReAllocate();
			Allocation.ReLoadClassifierAllocation();
			AssertEquals(4, Allocation.ClassifierAllocationList.Count);
			Allocation.ClassifierAllocationList.Sort("AllocatedTo", ListSortDirection.Ascending);
			AssertEquals(3, Allocation.ClassifierAllocationList[0].NumberAllocated);
			AssertEquals(0, Allocation.ClassifierAllocationList[1].NumberAllocated);
			AssertEquals(0, Allocation.ClassifierAllocationList[2].NumberAllocated);
			AssertEquals(0, Allocation.ClassifierAllocationList[3].NumberAllocated);
		}

		void CreateAndSaveJobs()
		{
			CreateNewStaff("AA", true);
			AddJobAndAllocate(DeclarationQueueCodeDescriptionPairList.Codes.Classification, ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration, "AA");
			AddJobAndAllocate(DeclarationQueueCodeDescriptionPairList.Codes.Classification, "", "AA");
			AddJobAndAllocate(DeclarationQueueCodeDescriptionPairList.Codes.Classification, ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration, "AA");
			AddJobAndAllocate(DeclarationQueueCodeDescriptionPairList.Codes.Classification, "", "AA");
			AddJobAndAllocate(DeclarationQueueCodeDescriptionPairList.Codes.Classification, "", "AA");
			AddJobAndAllocate(DeclarationQueueCodeDescriptionPairList.Codes.Classification, "", "AA");
			CreateNewStaff("BB", true);
			AddJobAndAllocate(DeclarationQueueCodeDescriptionPairList.Codes.Classification, "", "BB");
			AddJobAndAllocate(DeclarationQueueCodeDescriptionPairList.Codes.Classification, "", "BB");
			CreateNewStaff("CC", true);
			AddJobAndAllocate(DeclarationQueueCodeDescriptionPairList.Codes.Classification, "", "CC");
			CreateNewStaff("DD", false);
			AddJobAndAllocate(DeclarationQueueCodeDescriptionPairList.Codes.Classification, "", "DD");
			CreateNewStaff("EE", true);
			AddJobAndAllocate(DeclarationQueueCodeDescriptionPairList.Codes.Classification, "", "EE");
			AddJobAndAllocate(DeclarationQueueCodeDescriptionPairList.Codes.Classification, ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration, "EE");
			AddJobAndAllocate(DeclarationQueueCodeDescriptionPairList.Codes.Classification, "", "EE");
			AddJobAndAllocate(DeclarationQueueCodeDescriptionPairList.Codes.Classification, "", "EE");
			AddJobAndAllocate(DeclarationQueueCodeDescriptionPairList.Codes.Classification, "", "EE");
			Factory.Save();
		}

		GlbStaff CreateNewStaff(string code, bool belongsToClassifierGroup)
		{
			GlbStaff result = Factory.NewWithValidTestData<GlbStaff>();
			result.GS_Code = code;
			if (belongsToClassifierGroup)
			{
				result.Groups.Add(ClassifierGroup);
			}

			return result;
		}

		GlbGroup ClassifierGroup
		{
			get
			{
				if (fClassifierGroup == null)
				{
					fClassifierGroup = Factory.NewWithValidTestData<GlbGroup>();
					fClassifierGroup.GG_Code = UPEDataRegistry.Instance.ClassifierStaffGroupCode.Value;
				}

				return fClassifierGroup;
			}
		}

		GlbGroup fClassifierGroup;
		void AddJobAndAllocate(string queue, string reason, string allocatedTo)
		{
			UPEJobDeclaration uPEJobDeclaration = Factory.New<UPEJobDeclaration>();
			uPEJobDeclaration.CurrentQueue.P4_CustomsQueue = queue;
			uPEJobDeclaration.CurrentQueue.P4_CustomsStatus = reason;
			uPEJobDeclaration.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo = allocatedTo;
		}

		#region Allocation
		Allocation Allocation
		{
			get
			{
				if (fAllocation == null)
				{
					fAllocation = new Allocation(Factory);
				}

				return fAllocation;
			}
		}

		Allocation fAllocation;
		#endregion
		#region Base Test Overrides
		protected override BusinessObject GetNewBusinessObject()
		{
			return new Allocation(Factory);
		}
		#endregion
	}
}
