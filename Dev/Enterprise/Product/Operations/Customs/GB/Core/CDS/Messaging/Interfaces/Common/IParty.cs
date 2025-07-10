using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface IParty
	{
		ZString ID { get; }
		ZString RoleCode { get; }
	}

	[CodeAlive("Will be used shortly")]
	class PartyWrapper : IParty
	{
		PartyWrapper(ZString id, ZString roleCode)
		{
			this.id = id;
			this.roleCode = roleCode;
		}

		public static PartyWrapper New(ZString id, ZString roleCode)
		{
			return new PartyWrapper(id, roleCode);
		}

		ZString IParty.ID => id;

		ZString IParty.RoleCode => roleCode;

		readonly ZString id;
		readonly ZString roleCode;
	}
}
