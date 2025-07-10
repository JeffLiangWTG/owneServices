using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class ExportCancellationHeaderCreator
	{
		public ExportCancellationHeader Create(CusEntryHeader entry)
		{
			var result = new ExportCancellationHeader();
			var declaration = entry.Declaration;

			result.ExportDeclarationNumber = entry.EntryNumber;
			result.DeclarationCustomsOffice = declaration.JE_CustomsOffice;
			result.DeclarationCustomsDivision = declaration.JE_CustomsDivision;

			var supplier = declaration.SupplierAddress;
			if (supplier != null)
			{
				result.Exporter = new Organisation(RoleType.Exporter)
				{
					CompanyName = supplier.CompanyName
				};

				var idNumberAndTypes = supplier.GetRegistrationIDNumbers(new string[] { IdentificationType.UnipassIDForOrganization });
				result.Exporter.SetRegistrationIDNumbers(idNumberAndTypes);
			}

			result.UnipassDeclarantID = declaration.UNIPASSDeclarantID;

			return result;
		}
	}
}
