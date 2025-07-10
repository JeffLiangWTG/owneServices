using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS
{
	public interface IGlbExternalPasswordAuthorisationManager
	{
		ZBool IsAuthorised { get; set; }
	}

	public class GlbExternalPasswordAuthorisationManager : IGlbExternalPasswordAuthorisationManager
	{
		public ZBool IsAuthorised { get; set; } = false;
	}
}
