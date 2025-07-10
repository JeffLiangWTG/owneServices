using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import5GWEntry : ExtendedOfficeHoursEntry, IImport5GWEntry
	{
		public string ReferenceNumberType { get; set; }
		public string HSDescription { get; set; }
		public string BondedAreaCode { get; set; }
		public string PayerCompanyName { get; set; }

		ZString IImport5GWEntry.ReferenceNumberType => ReferenceNumberType;
		ZString IImport5GWEntry.HSDescription => HSDescription;
		ZString IImport5GWEntry.BondedAreaCode => BondedAreaCode;
		ZString IImport5GWEntry.PayerCompanyName => PayerCompanyName;
	}
}
