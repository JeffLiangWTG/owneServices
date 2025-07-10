using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public abstract class CommonValuationHeaderCreator<THeader>
		where THeader : Import934_5SMHeader, new()
	{
		public THeader Create(CusEntryHeader entry)
		{
			var entryHeaderData = new THeader();

			PopulateCommonHeaderFields(entryHeaderData, entry);
			PopulateAdditionalFields(entryHeaderData, entry);

			return entryHeaderData;
		}

		protected void PopulateCommonHeaderFields(THeader entryHeaderData, CusEntryHeader entry)
		{
			var randomHeader = entry.RandomHeader;
			var declaration = entry.Declaration;

			entryHeaderData.ValuationMethod = ValuationCodeList.GetValuationMethod(randomHeader.JZ_ValuationCode);
			entryHeaderData.DeclarationCustomsOffice = declaration.JE_CustomsOffice;
			entryHeaderData.DeclarationCustomsDivision = declaration.JE_CustomsDivision;

			PopulateOrganisations(entryHeaderData, entry);

			entryHeaderData.PurchaseOrderNo = randomHeader.PurchaseOrderNumber;
			if (randomHeader.PurchaseOrderDate.IsValid)
			{
				entryHeaderData.PurchaseOrderDate = randomHeader.PurchaseOrderDate.ToDateTime();
			}

			entryHeaderData.Author = new ValueDeclarationPerson()
			{
				DepartmentAndPosition = declaration.JE_AuthorJobTitle,
				Name = declaration.JE_AuthorName,
				TelephoneNumber = declaration.JE_AuthorPhone
			};
			entryHeaderData.ResponsiblePerson = new ValueDeclarationPerson()
			{
				DepartmentAndPosition = declaration.JE_AuditorJobTitle,
				Name = declaration.JE_AuditorName,
				TelephoneNumber = declaration.JE_AuditorPhone
			};
		}

		protected virtual void PopulateAdditionalFields(THeader entryHeaderData, CusEntryHeader entry)
		{ }

		void PopulateOrganisations(Import934_5SMHeader entryHeaderData, CusEntryHeader entry)
		{
			var declaration = entry.Declaration;

			#region Payer
			if (declaration.PayerAddress != null)
			{
				entryHeaderData.Payer = new Organisation(RoleType.Payer)
				{
					CompanyName = declaration.PayerAddress.CompanyName,
				};

				var matchedNumber = declaration.PayerAddress.GetRegistrationFirstMatchedBusinessOrIndividualIDConverted() ?? declaration.PayerAddress.GetRegistrationIDNumber(IdentificationType.ForeignCompanyID);
				if (matchedNumber != null)
				{
					entryHeaderData.Payer.SetRegistrationIDNumbers(new IDNumberAndType[] { matchedNumber });
				}
			}
			#endregion
		}
	}
}
