using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.AE.Business;

public interface IMessageAttachee : IJobNumber
{
	ZString EntryStatus { get; set; }

	ZString MessageStatus { get; set; }

	ZString DocumentIdentifier { get; }

	IBusinessObjectCollection Messages { get; }
}
