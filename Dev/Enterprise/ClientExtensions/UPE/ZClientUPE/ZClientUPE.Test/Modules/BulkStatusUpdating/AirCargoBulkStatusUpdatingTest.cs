using System;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	[TestedType(typeof(AirCargoBulkStatusUpdating))]
	sealed class AirCargoBulkStatusUpdatingTest : BulkStatusUpdatingTestCase
	{
		protected override Type ExpectedNonPersistentProcessQueueType
		{
			get
			{
				return typeof(AirCargoBulkStatusUpdatingQueue);
			}
		}

		protected override ProcessQueueType.Enum ExpectedQueueTypeToBulkUpdate
		{
			get
			{
				return ProcessQueueType.Enum.Customs;
			}
		}

		protected override IQueueFilterBusinessObject GetNewQueueFilterBusinessObject()
		{
			return new UPEAirCargoFilterBusinessObject();
		}

		protected override IProcessQueueParent GetNewItemToBulkUpdate()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			return mAWB.ChildBills.AddNew();
		}

		protected override ZString ValidQueueNameToBulkUpdateTo
		{
			get
			{
				return CargoReportQueueCodeDescriptionPairList.Codes.EIR;
			}
		}

		protected override ZString ValidStatusToBulkUpdateTo
		{
			get
			{
				return ReasonCodeDescriptionPairList.Codes.BP_PhoneNumberMissing;
			}
		}

		protected override ZString ValidSubStatusToBulkUpdateTo
		{
			get
			{
				return StatusCodeDescriptionPairList.Codes.DT_FaxEmailSent;
			}
		}

		protected override bool ExpectedAssignedToEnabled
		{
			get
			{
				return false;
			}
		}
	}
}
