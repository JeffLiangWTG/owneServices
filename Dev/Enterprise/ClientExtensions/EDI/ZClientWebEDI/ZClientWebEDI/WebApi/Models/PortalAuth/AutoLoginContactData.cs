using System.Diagnostics.CodeAnalysis;

namespace Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.PortalAuth
{
	[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Not (yet) needed.")]
	public struct AutoLoginContactData
	{
		public string licence { get; set; }
		public string dbnumber { get; set; }
		public string contact { get; set; }
		public string baseurl { get; set; }
		public string password { get; set; }
	}
}