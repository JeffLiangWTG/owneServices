using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	public class DocBillOfLadingFormedPage : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ZInt PageNo { get; set; }
		public ZString MainBodyDetailsSection { get; set; }
		public ZString MainBodyContainersSection { get; set; }
		public ZString MainBodyExtraSection { get; set; }
		public ZString BOLClauseSection { get; set; }
		public ZString ChargesSection { get; set; }
		public ZString PackRORSection { get; set; }
	}
}
