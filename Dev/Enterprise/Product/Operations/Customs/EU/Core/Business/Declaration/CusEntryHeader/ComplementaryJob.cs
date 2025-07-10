using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class ComplementaryJob
	{
		public ComplementaryJob()
		{
		}

		public ZString Reference { get; set; }

		public ZString ParentTableCode { get; set; }

		public IGuaranteeJobParent Parent { get; set; }
	}
}
