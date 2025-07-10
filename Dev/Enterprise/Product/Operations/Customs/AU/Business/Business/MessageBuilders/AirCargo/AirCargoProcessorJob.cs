using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Data.Mutex;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirCargoProcessorJob : AirCargoMessageProcessorJobBase, IHouseBillsCargoMessageProcessorJobWithMutex
	{
		public AirCargoProcessorJob(CusMAWB masterBill, bool shouldDelaySending = false)
			: base(shouldDelaySending)
		{
			this.masterBill = Argument.NotNull(masterBill, "masterBill");
		}

		#region Implementation

		readonly CusMAWB masterBill;

		protected override bool IsAir => true;

		protected override bool IsDischargedAtAustralianPort => masterBill.CM_RL_NKDischargePort.StartsWith(Core.Constants.CountryCodes.Australia);

		protected override CusMAWB MasterBillCore => masterBill;

		protected override IEnumerable<BusinessObject> ChildrenCore => MasterBill.ChildBills;

		protected override string GetReferenceNumberCore(BusinessObject child) => string.Format("Mawb {0}, Hawb {1}", masterBill.CM_MAWB, ((CusHAWB)child).CS_HAWB);

		protected override CMRMessageManager GetMessageManagerCore(BusinessObject child) => new CusHAWBAIRCRMessageManager((CusHAWB)child, shouldDelaySending);

		protected override void SetSACIfRequiredCore(BusinessObject child)
		{
			// Not required
		}

		#endregion // Implementation

		#region ICargoMessageBatchProcessorJob

		ZGlobalMutex IHouseBillsCargoMessageProcessorJobWithMutex.Mutex => masterBill.SendAIRCRMutex;

		ZString IHouseBillsCargoMessageProcessorJobWithMutex.JobNumber => masterBill.CM_MAWB + (masterBill.CM_MasterHouseBill.IsEmpty ? "" : "/" + masterBill.CM_MasterHouseBill);

		MasterFiles.Integration.IGlbBranch IHouseBillsCargoMessageProcessorJobWithMutex.Branch => masterBill.Branch;

		#endregion
	}
}
