using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business.Declaration;

public class ImportCusFiscalReferenceLookups : UCC6ImportCusFiscalReferenceLookups
{
	public ImportCusFiscalReferenceLookups(CusFiscalReference parent)
		: base(parent)
	{
	}

	public override CodeDescriptionPairList CodeList
	{
		get
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(UCC6IMPFiscalReferenceCodeList.Codes.FR5_Vendor, UCC6IMPFiscalReferenceCodeList.Descriptions.FR5_Vendor);
			result.AddPair(UCC6IMPFiscalReferenceCodeList.Codes.FR7_TaxablePerson, UCC6IMPFiscalReferenceCodeList.Descriptions.FR7_TaxablePerson);

			return result;
		}
	}
}
