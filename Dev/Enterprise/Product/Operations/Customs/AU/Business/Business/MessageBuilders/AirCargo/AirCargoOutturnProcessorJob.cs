using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Data.Mutex;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirCargoOutturnProcessorJob : AirCargoMessageProcessorJobBase, IHouseBillsCargoMessageProcessorJobWithMutex
	{
		public AirCargoOutturnProcessorJob(CusMAWB masterBill, bool shouldDelaySending = false)
			: base(shouldDelaySending)
		{
			this.masterBill = Argument.NotNull(masterBill, "masterBill");
		}
		readonly CusMAWB masterBill;

		#region Implementation

		protected override bool IsAir => true;

		protected override bool IsDischargedAtAustralianPort => masterBill.CM_RL_NKDischargePort.StartsWith(Core.Constants.CountryCodes.Australia);

		protected override CusMAWB MasterBillCore => masterBill;

		protected override IEnumerable<BusinessObject> ChildrenCore => MasterBill.AllUnderbonds.Where(x => x.CanDoOutturn);

		protected override CMRMessageManager GetMessageManagerCore(BusinessObject child) => new CusUnderbondAIROUTManager((CusUnderbond)child, shouldDelaySending);

		protected override string GetReferenceNumberCore(BusinessObject child) => $"Mawb {MasterBill.CM_MAWB}, Outturn report for {((CusUnderbond)child).C4_SendersMessageReference}";

		protected override void SetSACIfRequiredCore(BusinessObject child)
		{
			// Not required
		}

		#endregion

		#region IHouseBillsCargoMessageProcessorJobWithMutex

		ZGlobalMutex IHouseBillsCargoMessageProcessorJobWithMutex.Mutex => MasterBill.SendAIROUTMutex;

		ZString IHouseBillsCargoMessageProcessorJobWithMutex.JobNumber => $"MAWB {MasterBill.CM_MAWB}";

		IGlbBranch IHouseBillsCargoMessageProcessorJobWithMutex.Branch => MasterBill.Branch;

		#endregion
	}
}
