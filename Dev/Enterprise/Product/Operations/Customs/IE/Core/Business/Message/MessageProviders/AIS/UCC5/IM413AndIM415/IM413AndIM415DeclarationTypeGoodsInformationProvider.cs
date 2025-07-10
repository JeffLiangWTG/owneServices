using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM413AndIM415DeclarationTypeGoodsInformationProvider : IIM413AndIM415DeclarationTypeGoodsInformation
	{
		public IM413AndIM415DeclarationTypeGoodsInformationProvider(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}
		readonly JobDeclaration declaration;

		public decimal GrossMass => 0m;

		public string TotalPackageNumber => declaration.JE_TotalNoOfPacks.ToString();
	}
}
