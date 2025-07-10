using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.PBN.Business
{
	public sealed class PBNMessageSendingObjectLookups : ZLookups
	{
		public PBNMessageSendingObjectLookups(PBNMessageSendingObject parent) : base(parent)
		{
		}

		public CodeDescriptionPairList MessageTypes => Factory.GetCachedValue<PBNMessageTypes>();
	}
}
