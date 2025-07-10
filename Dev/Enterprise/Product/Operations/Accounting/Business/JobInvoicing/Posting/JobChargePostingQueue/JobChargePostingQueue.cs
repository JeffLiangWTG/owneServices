using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;

using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobChargePostingQueue : AutoJobChargePostingQueue, IJobChargePostingQueue
	{
		public JobChargePostingQueue(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[List("Lookups.PostingInstructions")]
		public override ZString JPQ_PostingInstruction
		{
			get => base.JPQ_PostingInstruction;
			set => base.JPQ_PostingInstruction = value;
		}

		#region IJobChargePostingQueue

		ZInt IJobChargePostingQueue.GroupID => JPQ_GroupID;
		ZGuid IJobChargePostingQueue.ParentID => JPQ_ParentID;
		ZString IJobChargePostingQueue.ParentTableCode => JPQ_ParentTableCode;
		ZString IJobChargePostingQueue.PostingInstruction => JPQ_PostingInstruction;
		ZGuid IJobChargePostingQueue.ChargePK => JPQ_JR;
		ZByte IJobChargePostingQueue.HashVersion => JPQ_HashVersion;
		ZBlob IJobChargePostingQueue.ChargeValuesHash => JPQ_ChargeValuesHash;

		#endregion

		#region Overrides

		public override void OnSaving()
		{
			base.OnSaving();

			if (!IsInDatabase)
			{
				var nextNumberService = Factory.ServiceContainer.GetService<NextGroupIdService>();
				if (nextNumberService == null)
				{
					nextNumberService = Factory.ServiceContainer.AddService(new NextGroupIdService(Factory));
					Factory.Saved += Factory_Saved;
				}
				JPQ_GroupID = nextNumberService.GroupId;
			}
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			Factory.Saved -= Factory_Saved;
			Factory.ServiceContainer.RemoveService<NextGroupIdService>();
		}

		#endregion

		public static JobChargePostingQueue CreateNew(BusinessObjectFactory factory, string postingInstruction, Charge charge, ZGuid jobParentPK, ZString jobParentTablePrefix)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(charge, nameof(charge));

			var result = factory.New<JobChargePostingQueue>();
			result.JPQ_JR = charge.PK;
			result.JPQ_PostingInstruction = postingInstruction;

			switch (postingInstruction)
			{
				case JobChargePostingQueueLookups.PostCost:
					result.JPQ_HashVersion = AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion;
					result.JPQ_ChargeValuesHash = charge.CalculateCostPartHash(result.JPQ_HashVersion);
					break;
				case JobChargePostingQueueLookups.PostRevenue:
					result.JPQ_HashVersion = AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion;
					result.JPQ_ChargeValuesHash = charge.CalculateSellPartHash(result.JPQ_HashVersion);
					break;
				default:
					throw new ArgumentException(FormattableString.Invariant($"Incorrenct '{postingInstruction}' charge posting instruction type."));
			}

			result.JPQ_ParentID = jobParentPK;
			result.JPQ_ParentTableCode = jobParentTablePrefix;

			return result;
		}

		class NextGroupIdService : IService
		{
			internal NextGroupIdService(BusinessObjectFactory factory)
			{
				GroupId = Int32.Parse(AccountingNumberFountainWrapperFactory.Instance.JobChargePostingQueueGroupId.GetNext(factory), CultureInfo.InvariantCulture);
			}

			internal ZInt GroupId { get; }
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			var charge = Factory.NewWithValidTestData<Charge>();
			JPQ_JR = Charge.PK;
			JPQ_ParentID = charge.Job.JH_ParentID;
			JPQ_ParentTableCode = charge.Job.JH_ParentTableCode;
			JPQ_PostingInstruction = JobChargePostingQueueLookups.PostCost;
		}
#endif
	}

	public interface IJobChargePostingQueue
	{
		ZInt GroupID { get; }
		ZGuid ParentID { get; }
		ZString ParentTableCode { get; }
		ZString PostingInstruction { get; }
		ZGuid ChargePK { get; }
		ZByte HashVersion { get; }
		ZBlob ChargeValuesHash { get; }
	}

	public struct JobChargePostingQueueData : IJobChargePostingQueue
	{
		public JobChargePostingQueueData(JobChargePostingQueue queue) : this(queue.JPQ_GroupID, queue.JPQ_ParentID, queue.JPQ_ParentTableCode, queue.JPQ_PostingInstruction, queue.JPQ_JR, queue.JPQ_HashVersion, queue.JPQ_ChargeValuesHash)
		{ }

		public JobChargePostingQueueData(ZInt groupID, ZGuid parentID, ZString parentTableCode, ZString postingInstruction, ZGuid chargePK, ZByte hashVersion, ZBlob chargeValuesHash)
		{
			GroupID = groupID;
			ParentID = parentID;
			ParentTableCode = parentTableCode;
			PostingInstruction = postingInstruction;
			ChargePK = chargePK;
			HashVersion = hashVersion;
			ChargeValuesHash = chargeValuesHash;
		}

		public ZInt GroupID { get; }
		public ZGuid ParentID { get; }
		public ZString ParentTableCode { get; }
		public ZString PostingInstruction { get; }
		public ZGuid ChargePK { get; }
		public ZByte HashVersion { get; }
		public ZBlob ChargeValuesHash { get; }

		public override bool Equals(object obj)
		{
			if (obj is JobChargePostingQueueData)
			{
				var compareTo = (JobChargePostingQueueData)obj;
				return this.GroupID.Equals(compareTo.GroupID)
					&& this.ParentID.Equals(compareTo.ParentID)
					&& this.ParentTableCode.Equals(compareTo.ParentTableCode)
					&& this.PostingInstruction.Equals(compareTo.PostingInstruction)
					&& this.ChargePK.Equals(compareTo.ChargePK)
					&& this.HashVersion.Equals(compareTo.HashVersion)
					&& this.ChargeValuesHash.Equals(compareTo.ChargeValuesHash);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return GroupID.GetHashCode()
				^ ParentID.GetHashCode()
				^ ParentTableCode.GetHashCode()
				^ PostingInstruction.GetHashCode()
				^ ChargePK.GetHashCode()
				^ HashVersion.GetHashCode()
				^ ChargeValuesHash.GetHashCode();
		}

		public static bool operator ==(JobChargePostingQueueData data1, JobChargePostingQueueData data2)
		{
			return data1.Equals(data2);
		}

		public static bool operator !=(JobChargePostingQueueData data1, JobChargePostingQueueData data2)
		{
			return !data1.Equals(data2);
		}
	}
}