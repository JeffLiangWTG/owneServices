using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Data.Mutex;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IHouseBillsCargoMessageProcessorJob : ICargoMessageProcessorJob
	{
		BusinessObject MasterBill { get; }
		IEnumerable<BusinessObject> Children { get; }
		CMRMessageManager GetMessageManager(BusinessObject houseBill);
		void SetSACIfRequired(BusinessObject houseBill);
		string ReasonWhyNotAcceptable { get; }
		string GetReferenceNumber(BusinessObject houseBill);
		bool SendChildren { get; }
	}

	public interface IHouseBillsCargoMessageProcessorJobWithMutex : IHouseBillsCargoMessageProcessorJob
	{
		ZGlobalMutex Mutex { get; }
		ZString JobNumber { get; }
		IGlbBranch Branch { get; }
	}
}
