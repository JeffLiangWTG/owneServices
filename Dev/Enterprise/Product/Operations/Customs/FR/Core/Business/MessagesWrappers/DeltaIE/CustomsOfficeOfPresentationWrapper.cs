using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class CustomsOfficeOfPresentationWrapper : IPco
	{
		CustomsOfficeOfPresentationWrapper(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}
		readonly JobDeclaration declaration;

		public string ReferenceNumber => referenceNumber ?? (referenceNumber = declaration.JE_CustomsOffice);
		string referenceNumber;

		public static CustomsOfficeOfPresentationWrapper New(JobDeclaration declaration) => declaration == null ? null : new CustomsOfficeOfPresentationWrapper(declaration);
	}
}
