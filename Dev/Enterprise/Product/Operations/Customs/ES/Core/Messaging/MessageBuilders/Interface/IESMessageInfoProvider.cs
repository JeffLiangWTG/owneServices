using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IESMessageInfoProvider : IESMessageBusinessObject
	{
		GlbStaff Broker { get; }
		ZString MRN { get; }
		ZString DocumentJobReference { get; }
	}
}
