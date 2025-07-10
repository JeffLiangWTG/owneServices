using System;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Network.NetworkEntity
{
	[Serializable]
	public class LinkShapeState
	{
		public string From { get; set; }
		public string To { get; set; }
		public string FPProcessHeaderLink { get; set; }
		public string BNAType { get; set; }
	}
}
