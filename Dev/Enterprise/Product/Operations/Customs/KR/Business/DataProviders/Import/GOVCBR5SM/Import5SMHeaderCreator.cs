using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class Import5SMHeaderCreator : CommonValuationHeaderCreator<Import5SMHeader>
	{
		protected override void PopulateAdditionalFields(Import5SMHeader entryHeaderData, CusEntryHeader entry)
		{
			base.PopulateAdditionalFields(entryHeaderData, entry);
			PopulateOrganisations(entryHeaderData, entry);
			var entryNum = entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5SM)?.CE_EntryNum ?? ZString.Empty;
			entryHeaderData.ValueDeclarationTemplateNumber = entryNum;
			var randomHeader = entry.RandomHeader;
			switch (entryHeaderData.ValuationMethod)
			{
				case Constants.ValuationMethod.A:
					entryHeaderData.FormCData = new ();
					entryHeaderData.FormCData.Questions = new ValuationQuestionsProvider().PopulateQuestionAndAnswer(randomHeader, PriceQuestionCodeList.Codes._5EA, PriceQuestionCodeList.Codes._5EB, PriceQuestionCodeList.Codes._5E);
					break;
				case Constants.ValuationMethod.B:
					entryHeaderData.FormDData = new ();
					entryHeaderData.FormDData.ValuationMethod = entryHeaderData.ValuationMethod;
					entryHeaderData.FormDData.ValuationSupportingDocument1 = randomHeader.ValuationSupportingDocument1;
					entryHeaderData.FormDData.ValuationSupportingDocument2 = randomHeader.ValuationSupportingDocument2;
					entryHeaderData.FormDData.UseCodes = randomHeader.ValuationDeclarationCodes.Where(x => x.CY_Code.StartsWith(PriceDeclarationItemCodeList.StartingDigits.UseCode)).Select(x => new ValueDeclarationCode() { Code = x.CY_Code, CodeOtherDescription = x.CY_Data }).ToArray();
					entryHeaderData.FormDData.GoodsPricingBasis = randomHeader.ValuationDeclarationCodes.Where(x => x.CY_Code.StartsWith(PriceDeclarationItemCodeList.StartingDigits.GoodsPricingBasisCode)).Select(x => new ValueDeclarationCode() { Code = x.CY_Code, CodeOtherDescription = x.CY_Data }).ToArray();
					break;
				default:
					break;
			}

			var lines = new List<Import5SMLine>();
			foreach (var entryLine in entry.MergedLines)
			{
				var invoiceLine = entryLine.RandomLine;
				lines.Add(new ()
				{
					EntryLineNo = entryLine.CL_LineNumber,
					HSCode = invoiceLine.JI_Tariff,
					HSDescription = invoiceLine.UniversalTariff?.ZZ1_Description ?? ZString.Empty,
					InvoiceDescription = invoiceLine.JI_Description,
					BrandName = invoiceLine.JI_BrandName,
					ItemDescription = invoiceLine.JI_Model,
					Ingredient = invoiceLine.JI_Ingredient,
				});
			}
			entryHeaderData.EntryLines = lines.ToArray();
		}

		void PopulateOrganisations(Import5SMHeader entryHeaderData, CusEntryHeader entry)
		{
			var declaration = entry.Declaration;

			#region Supplier
			if (declaration.Supplier != null)
			{
				entryHeaderData.Supplier = new Organisation(RoleType.Supplier)
				{
					CompanyName = declaration.Supplier.OH_FullName,
					RepresentativeName = declaration.Supplier.GetRepresentativeName(),
					AddressLine1 = declaration.Supplier.MainAddress.Address1,
					AddressLine2 = declaration.Supplier.MainAddress.Address2,
					CountryCode = declaration.Supplier.CountryCode,
				};
			}
			#endregion

			#region Importer
			if (declaration.Importer != null)
			{
				entryHeaderData.Importer = new Organisation(RoleType.Importer)
				{
					CompanyName = declaration.Importer.OH_FullName,
					RepresentativeName = declaration.Importer.GetRepresentativeName(),
					AddressLine1 = declaration.Importer.MainAddress.Address1,
					AddressLine2 = declaration.Importer.MainAddress.Address2
				};
			}
			#endregion
		}
	}
}
