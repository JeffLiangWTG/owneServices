using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IAmendStatusCodeProvider
	{
		public bool IsSupportAmendStatusCode(BusinessObject sourceBusinessEntity);

		public bool ShouldShowAmendStatusCode();

		public ZString AmendStatusCodeReferenceType { get; }

		public ReadOnlyCodeDescriptionPairList AmendStatusCodeList { get; }
	}
}
