using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Data.Mutex;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SeaCargoProcessorJob : SeaCargoMessageProcessorJobBase, IHouseBillsCargoMessageProcessorJobWithMutex
	{
		public SeaCargoProcessorJob(CusSCAOceanBill oceanBill, bool shouldDelaySending = false)
			: base(shouldDelaySending)
		{
			this.oceanBill = Argument.NotNull(oceanBill, "oceanBill");
		}

		#region Implementation

		readonly CusSCAOceanBill oceanBill;

		protected override bool IsDischargedAtAustralianPort
		{
			get
			{
				return this.oceanBill.CB_RL_NKPortOfDischarge.StartsWith(Core.Constants.CountryCodes.Australia);
			}
		}

		protected override bool IsSea => true;

		protected override CusSCAOceanBill OceanBillCore => oceanBill;

		protected override bool HasOceanBill => true;

		protected override ZGlobalMutex MutexCore => oceanBill.Mutex;

		protected override ZString JobNumberCore => oceanBill.CB_OceanBill + (oceanBill.CB_MasterHouseBill.IsEmpty ? "" : "/" + oceanBill.CB_MasterHouseBill);

		protected override void SetSACIfRequiredCore()
		{
		}

		#endregion // Implementation

		#region ICargoMessageBatchProcessorJob

		ZGlobalMutex IHouseBillsCargoMessageProcessorJobWithMutex.Mutex
		{
			get { return Mutex; }
		}

		ZString IHouseBillsCargoMessageProcessorJobWithMutex.JobNumber
		{
			get { return JobNumber; }
		}

		MasterFiles.Integration.IGlbBranch IHouseBillsCargoMessageProcessorJobWithMutex.Branch
		{
			get { return oceanBill.Branch; }
		}

		#endregion

		#region IHouseBillsCargoMessageProcessorJob
		public bool SendChildren => true;

		public string ReasonWhyNotAcceptable
		{
			get
			{
				var result = string.Empty;
				if (!IsDischargedAtAustralianPort)
				{
					result = "The final discharge port is not an Australian port.";
				}
				else if (OceanBill.HasErrors)
				{
					var errors = new ZStringBuilder();
					OceanBill.GetErrors().ForEach(action => errors.Append(action.Message));
					result = errors.ToString();
				}
				return result;
			}
		}

		BusinessObject IHouseBillsCargoMessageProcessorJob.MasterBill
		{
			get { return OceanBill; }
		}

		IEnumerable<BusinessObject> IHouseBillsCargoMessageProcessorJob.Children
		{
			get { return OceanBill.HouseBills; }
		}

		CMRMessageManager IHouseBillsCargoMessageProcessorJob.GetMessageManager(BusinessObject houseBill)
		{
			return new CusSCAHouseSEACRManager((CusSCAHouse)houseBill, shouldDelaySending);
		}

		void IHouseBillsCargoMessageProcessorJob.SetSACIfRequired(BusinessObject houseBill)
		{
		}

		string IHouseBillsCargoMessageProcessorJob.GetReferenceNumber(BusinessObject houseBill)
		{
			return string.Format("{0}/{1}", ((CusSCAHouse)houseBill).CA_HouseBill, OceanBill.CB_OceanBill);
		}

		#endregion
	}
}
