using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GB.Business.Interfaces
{
	public interface IMessageAttachee
	{
		ZGuid PK { get; }

		GlbBranch Branch { get; }

		string JobNumber { get; }

		string DataGroupingCode { get; }

		BusinessObjectFactory Factory { get; }

		ZString MovementReferenceNumber { get; }

		ZString LocalReferenceNumber { get; }

		ZString JobReference { get; }

		EDIMessageCollection Messages { get; }

		Logs Logs { get; }

		void UpdateStatusIfNotEmpty(ZString status);

		ZString Gateway { get; }
	}
}
