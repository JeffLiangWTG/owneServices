using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers
{
	public class G5RepresentativeInfoWrapper : G5PartyInfoWrapper, IG5RepresentativeInfo
	{
		public new static G5RepresentativeInfoWrapper New(OrgAddress address) => address?.Header == null ? null : new G5RepresentativeInfoWrapper(address);

		protected G5RepresentativeInfoWrapper(OrgAddress orgA) : base(orgA)
		{
		}

		readonly static ZString FixedStatusCode = "2";

		public ZString Status => FixedStatusCode;
	}
}
