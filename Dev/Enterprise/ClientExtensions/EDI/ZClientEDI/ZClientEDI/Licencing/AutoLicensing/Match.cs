using Enterprise.Client.EDI.MasterFiles.Business;

namespace Enterprise.Client.EDI.Licencing.AutoLicensing
{
	public class Match
	{
		public EDIOrgHeader Org { get; set; }
		public int TotalScore { get; set; }
	}
}
