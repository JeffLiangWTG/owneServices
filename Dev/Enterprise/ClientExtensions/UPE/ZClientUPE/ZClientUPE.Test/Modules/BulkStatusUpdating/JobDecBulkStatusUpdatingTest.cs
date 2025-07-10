using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	[TestedType(typeof(JobDecBulkStatusUpdating))]
	sealed class JobDecBulkStatusUpdatingTest : BulkStatusUpdatingTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new JobDecBulkStatusUpdating(Factory, Array.Empty<IProcessQueueParent>());
		}

		protected override Type ExpectedNonPersistentProcessQueueType
		{
			get
			{
				return typeof(JobDecBulkStatusUpdatingQueue);
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
			return new UPEJobDeclarationFilterBusinessObject();
		}

		protected override IProcessQueueParent GetNewItemToBulkUpdate()
		{
			return Factory.New<UPEJobDeclaration>();
		}

		protected override ZString ValidQueueNameToBulkUpdateTo
		{
			get
			{
				return DeclarationQueueCodeDescriptionPairList.Codes.BCA;
			}
		}

		protected override ZString ValidStatusToBulkUpdateTo
		{
			get
			{
				return ReasonCodeDescriptionPairList.Codes.B5_ClientRegistration;
			}
		}

		protected override ZString ValidSubStatusToBulkUpdateTo
		{
			get
			{
				return StatusCodeDescriptionPairList.Codes.BQ_NoAnswer;
			}
		}

		protected override bool ExpectedAssignedToEnabled
		{
			get
			{
				return true;
			}
		}
	}
}
