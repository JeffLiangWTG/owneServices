using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class SupervisingCustomsOfficeWrapper : ISco
	{
		SupervisingCustomsOfficeWrapper(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}
		readonly JobDeclaration declaration;

		public string ReferenceNumber => referenceNumber ?? (referenceNumber = declaration.CustomsOffices?.Cast<EU.Business.EuOfficeCode>().FirstOrDefault(x => x.CY_Code == EU.Business.EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep)?.CY_Data);
		string referenceNumber;

		public static SupervisingCustomsOfficeWrapper New(JobDeclaration declaration) => declaration == null ? null : new SupervisingCustomsOfficeWrapper(declaration);
	}
}
