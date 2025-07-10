using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class KafkaSecurityLookups : ZLookups
	{
		public KafkaSecurityLookups(KafkaSecurity parent)
			: base(parent) { }

		public CodeDescriptionPairList SecurityProtocols
		{
			get { return new KafkaSecurityProtocolOptions(); }
		}
	}
}
