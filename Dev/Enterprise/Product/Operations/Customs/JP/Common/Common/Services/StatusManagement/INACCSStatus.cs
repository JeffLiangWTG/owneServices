using CargoWise.Types;

namespace Enterprise.Customs.JP.Common
{
	public interface INACCSStatus
	{
		public ZString MessageStatus { get; set; }

		public ZString CustomsStatus { get; set; }

		public ZString PhaseStatus { get; set; }

		public ZBool IsExport { get; }
	}
}
