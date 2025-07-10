using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Data.Mutex;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SeaCargoOutturnHeaderProcessorJob : IHouseBillsCargoMessageProcessorJobWithMutex
	{
		public SeaCargoOutturnHeaderProcessorJob(CusOutturnHeader masterBill)
		{
			this.masterBill = Argument.NotNull(masterBill, "masterBill");
		}
		readonly CusOutturnHeader masterBill;

		public bool SendChildren => false;

		public ZGlobalMutex Mutex => masterBill.SendSEAOUTMutex;

		public ZString JobNumber => masterBill.C6_SendersMessageReference;

		public IGlbBranch Branch => null;

		public CusOutturnHeader MasterBill => masterBill;

		public IEnumerable<BusinessObject> Children => masterBill.Outturns;

		public string ReasonWhyNotAcceptable
		{
			get
			{
				var result = string.Empty;

				if (MasterBill.HasErrors)
				{
					var errors = new ZStringBuilder();
					MasterBill.GetErrors().ForEach(action => errors.Append(action.Message));
					result = errors.ToString();
				}
				else if (MasterBill.HasOutturnHeaderMessageErrors)
				{
					result = MasterBill.MessageErrorsString;
				}

				return result;
			}
		}

		public bool IsAcceptable => !MasterBill.HasErrors && !MasterBill.HasOutturnHeaderMessageErrors;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		public CMRMessageManager GetMessageManager(BusinessObject houseBill)
		{
			return new CusUnderbondSEAOUTManager((CusOutturnHeader)houseBill);
		}

		protected string GetReferenceNumber(CusOutturnHeader outturnHeader)
		{
			return outturnHeader.C6_SendersMessageReference;
		}

		public void SetSACIfRequired(BusinessObject houseBill)
		{
		}

		BusinessObject IHouseBillsCargoMessageProcessorJob.MasterBill
		{
			get { return MasterBill; }
		}

		string IHouseBillsCargoMessageProcessorJob.GetReferenceNumber(BusinessObject outturnHeader)
		{
			return GetReferenceNumber((CusOutturnHeader)outturnHeader);
		}
	}
}
