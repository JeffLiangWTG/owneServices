using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	public sealed class Phase5DepartureDetailsLayout : EU.NCTS.GUI.Phase5DepartureDetailsLayout
	{
		public override EU.NCTS.GUI.DepartureDetailsLayoutBuilder<NctsHeader> GetBuilder() => new DepartureDetailsLayoutBuilder<NctsHeader>();
	}
}
