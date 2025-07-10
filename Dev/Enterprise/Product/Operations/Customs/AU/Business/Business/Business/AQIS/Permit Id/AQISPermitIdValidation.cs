
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISPermitIdValidation : AQISSingleValueValidation
	{
		public AQISPermitIdValidation(AQISPermitId parent)
			: base(parent)
		{
		}

		public override ZInt MaximumNumberOfRecordsAllowed
		{
			get { return 10; }
		}

		public override void ValidateCodeAgainstLookupList()
		{
		}
	}
}
