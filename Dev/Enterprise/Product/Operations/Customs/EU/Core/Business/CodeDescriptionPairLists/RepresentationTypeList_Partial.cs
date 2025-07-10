using CargoWise.Types;

namespace Enterprise.Customs.EU.Business
{
	public partial class RepresentationTypeList : Integration.Customs.EU.IRepresentationTypeList
	{
		public ZString _2DirectCode => Codes._2Direct;

		public ZString _2DirectDescription => Descriptions._2Direct;
	}
}
