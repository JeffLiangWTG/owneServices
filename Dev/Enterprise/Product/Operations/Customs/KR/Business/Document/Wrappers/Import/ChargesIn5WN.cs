using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("Soon to be used")]
	public class ChargesIn5WN : NonPersistentBusinessObject, IChargesIn5WN
	{
		public ChargesIn5WN(ChargesIn5WNSerializable chargesIn5WNSerializable, BusinessObjectFactory factory) : base(factory)
		{
			this.chargesIn5WNSerializable = chargesIn5WNSerializable;
		}

		readonly IChargesIn5WN chargesIn5WNSerializable;

		public ZShort VersionNumber => chargesIn5WNSerializable.VersionNumber;
		public ZString VersionDescription => chargesIn5WNSerializable.VersionDescription;

		public ICharges RefundAmounts => chargesIn5WNSerializable.RefundAmounts;
	}
}
