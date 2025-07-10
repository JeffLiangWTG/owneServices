using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public class DeclarationExportPermitCollection : Customs.Business.CusCodeDataCollection<DeclarationExportPermit>
	{
		public DeclarationExportPermitCollection(JobDeclaration declaration)
			: base(declaration, CusCodeDataTypeList.Codes.Permit)
		{
		}

		public new DeclarationExportPermit AddNew(ZString permitNumber)
		{
			return AddNew(CusCodeDataTypeList.Codes.Permit, permitNumber);
		}
	}
}
