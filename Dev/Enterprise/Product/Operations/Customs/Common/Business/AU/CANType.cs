using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU
{
	public class CANType : CodeDescriptionPair
	{
		[WTG.StaticAnalysis.Annotation.CodeAlive("Is used by AU.Declaration.Business.Testing")]
		public abstract class Exemptions : CMR.CMRExportExemptionCodes
		{
			public Exemptions(object code, MultilingualString description) : base(code, description) { }
		}
		CANType(object code, string description)
			: base(code, description)
		{
		}

		public static readonly CANType CustomsAuthorityNumber = new CANType("CAN", (NoResString)"Customs Authority Number");
		public static readonly CANType ContingencyCustomsAuthorityNumber = new CANType("CCN", (NoResString)"Contingency Customs Authority Number");
	}
}
