
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISEntityIdValidation : AQISSingleValueValidation
	{
		public AQISEntityIdValidation(AQISEntityId parent)
			: base(parent)
		{
			this.aQISEntityId = parent;
		}

		public override ZInt MaximumNumberOfRecordsAllowed
		{
			get { return 10; }
		}

		public override void ValidateCodeAgainstLookupList()
		{
			ListValidation.MessageErrorIfInvalidCode(aQISEntityId.CodeInfo, aQISEntityId.Lookups.AQISEntityIdList);
		}

		readonly AQISEntityId aQISEntityId;
	}
}
