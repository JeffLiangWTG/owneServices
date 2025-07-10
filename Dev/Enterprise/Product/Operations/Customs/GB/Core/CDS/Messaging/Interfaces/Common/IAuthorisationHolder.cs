using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface IAuthorisationHolder
	{
		ZString ID { get; }
		ZString CategoryCode { get; }
	}

	public class AuthorisationHolderWrapper : IAuthorisationHolder
	{
		public AuthorisationHolderWrapper(ZString id, ZString categoryCode)
		{
			this.id = id;
			this.categoryCode = categoryCode;
		}

		public static AuthorisationHolderWrapper New(ZString id, ZString categoryCode)
		{
			return new AuthorisationHolderWrapper(id, categoryCode);
		}

		ZString IAuthorisationHolder.ID => id.SubstringSafe(0, 17);

		ZString IAuthorisationHolder.CategoryCode => categoryCode;

		readonly ZString id;
		readonly ZString categoryCode;
	}
}
