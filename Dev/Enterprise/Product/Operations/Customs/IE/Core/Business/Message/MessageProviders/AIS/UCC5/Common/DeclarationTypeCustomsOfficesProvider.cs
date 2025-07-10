using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class DeclarationTypeCustomsOfficesProvider : IDeclarationTypeCustomsOffices
	{
		public DeclarationTypeCustomsOfficesProvider(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}
		readonly JobDeclaration declaration;

		public string PresentationCustomsOffice => declaration.CustomsOffices.GetPresentationOffice()?.CY_Data;

		public string SupervisingCustomsOffice => declaration.CustomsOffices.GetSupervisingOffice()?.CY_Data;

		public string CustomsOfficeLodgement => declaration.JE_CustomsOffice;
	}
}
