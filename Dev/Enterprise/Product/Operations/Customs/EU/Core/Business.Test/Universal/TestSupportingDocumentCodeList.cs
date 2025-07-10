using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Testing
{
	public class TestSupportingDocumentCodeList
	{
		public string Code { get; set; }
		public string Country { get; set; }
		public bool IsImport { get; set; }
		public bool HasPermitAttribute { get; set; }

		public TestSupportingDocumentCodeList(string code, string country = "", bool isImport = true, bool hasPermitAttribute = false)
		{
			country = string.IsNullOrEmpty(country) ? (string)GlbCompany.CurrentCompany.GC_RN_NKCountryCode : country;
			Code = code;
			IsImport = isImport;
			Country = country;
			HasPermitAttribute = hasPermitAttribute;
		}
	}
}
