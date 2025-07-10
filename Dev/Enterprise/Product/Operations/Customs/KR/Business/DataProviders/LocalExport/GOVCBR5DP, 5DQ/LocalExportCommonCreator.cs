using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class LocalExportCommonCreator
	{
		public LocalExportEntryHeader Create(CusEntryHeader entry)
		{
			var localExportData = new LocalExportEntryHeader();
			PopulateHeaderCore(entry, localExportData);
			PopulateEntryLines(entry, localExportData);
			PopulateOrganisations(entry, localExportData);
			return localExportData;
		}

		void PopulateHeaderCore(CusEntryHeader entry, LocalExportEntryHeader localExportData)
		{
			var declaration = entry.Declaration;
			localExportData.DeclarationType = declaration.JE_MessageSubType;
			localExportData.DeclarationCustomsOffice = declaration.JE_CustomsOffice;
			localExportData.DeclarationCustomsDivision = declaration.JE_CustomsDivision;
			localExportData.GoodsType = declaration.JE_ExportGoodsType;

			localExportData.DeclarationNumber = entry.EntryNumber;
			localExportData.DrawbackApplicantType = entry.RandomHeader.JZ_DRWApplicantType;

			foreach (var invoice in entry.InvoiceHeaders())
			{
				localExportData.TotalGrossWeight += Core.Constants.Weight.Convert(invoice.JZ_Weight, invoice.JZ_WeightUQ, Core.Constants.Weight.Kilograms);
				localExportData.TotalPackages += (int)invoice.JZ_NoOfPacks;
			}

			PopulateHeader(entry, localExportData);
		}
		protected virtual void PopulateHeader(CusEntryHeader entry, LocalExportEntryHeader localExportData)
		{
		}

		void PopulateEntryLines(CusEntryHeader entry, LocalExportEntryHeader localExportData)
		{
			var entryLines = entry.MergedLines.Cast<CusEntryLine>().OrderBy(x => x.CL_LineNumber);
			var entryLineDataList = new List<LocalExportEntryLine>();
			foreach (CusEntryLine entryLine in entryLines)
			{
				var invoiceLine = entryLine.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault();

				if (invoiceLine != null)
				{
					var entryLineData = new LocalExportEntryLine();
					PopulateEntryLineCore(entryLine, invoiceLine, entryLineData);
					localExportData.TotalDeclarationAmount += entryLineData.FOBAmount;
					entryLineDataList.Add(entryLineData);
				}
			}
			localExportData.EntryLines = entryLineDataList.ToArray();
		}

		void PopulateOrganisations(CusEntryHeader entry, LocalExportEntryHeader localExportData)
		{
			if (entry.Declaration.Supplier != null)
			{
				localExportData.Supplier = new Organisation(Messaging.RoleType.Supplier)
				{
					CompanyName = entry.Declaration.Supplier.OH_FullName,
					RepresentativeName = entry.Declaration.Supplier.GetRepresentativeName(),
					AddressLine1 = entry.Declaration.Supplier.GetCustomsAddressThenMainAddress().Address1,
					AddressLine2 = entry.Declaration.Supplier.GetCustomsAddressThenMainAddress().Address2
				};
				var idNumbers = entry.Declaration.Supplier.GetRegistrationIDNumbers(new string[] { IdentificationType.BusinessRegNo, IdentificationType.UnipassIDForOrganization });
				localExportData.Supplier.SetRegistrationIDNumbers(idNumbers);
			}

			if (entry.RandomHeader.Manufacturer != null)
			{
				localExportData.Manufacturer = new Organisation(Messaging.RoleType.Manufacturer)
				{
					CompanyName = entry.RandomHeader.Manufacturer.OH_FullName,
					RepresentativeName = entry.RandomHeader.Manufacturer.GetRepresentativeName(),
					AddressLine1 = entry.RandomHeader.Manufacturer.MainAddress.Address1,
					AddressLine2 = entry.RandomHeader.Manufacturer.MainAddress.Address2
				};
				var idNumbers = entry.RandomHeader.Manufacturer.GetRegistrationIDNumbers(new string[] { IdentificationType.BusinessRegNo, IdentificationType.UnipassIDForOrganization });
				localExportData.Manufacturer.SetRegistrationIDNumbers(idNumbers);
			}

			if (entry.RandomHeader.Buyer != null)
			{
				localExportData.Importer = new Organisation(RoleType.Importer)
				{
					CompanyName = entry.RandomHeader.Buyer.OH_FullName,
					RepresentativeName = entry.RandomHeader.Buyer.GetRepresentativeName(),
					AddressLine1 = entry.RandomHeader.Buyer.MainAddress.Address1,
					AddressLine2 = entry.RandomHeader.Buyer.MainAddress.Address2,
					Postcode = entry.RandomHeader.Buyer.MainAddress.Postcode,
					RoadNameCode = entry.RandomHeader.Buyer.MainAddress.GetRoadNameCode(),
					BuildingNumber = entry.RandomHeader.Buyer.MainAddress.GetBuildingNumber(),
				};
				var idNumbers = entry.RandomHeader.Buyer.GetRegistrationIDNumbers(new string[] { IdentificationType.BusinessRegNo });
				localExportData.Importer.SetRegistrationIDNumbers(idNumbers);
			}

			PopulateMoreOrganisations(entry, localExportData);
		}

		void PopulateEntryLineCore(CusEntryLine entryLine, JobComInvoiceLine invoiceLine, LocalExportEntryLine entryLineData)
		{
			entryLineData.EntryLineNo = entryLine.CL_LineNumber;
			entryLineData.HSCode = entryLine.CL_AdValoremTariff;
			entryLineData.InvoiceDescription = entryLine.CL_Description;
			entryLineData.GoodsNo = invoiceLine.JI_SerialNumber;
			entryLineData.QuantityUnit = invoiceLine.HasHSRequiringInvQuantityInCustomsUQ ? invoiceLine.JI_CustomsUnitQty : invoiceLine.JI_InvoiceUQ;
			entryLineData.DocumentNo = invoiceLine.SupportingDocumentReferenceNumber;
			entryLineData.DocumentType = invoiceLine.SupportingDocumentCode;
			entryLineData.PackagesType = invoiceLine.JI_PackType;
			entryLineData.PreviousTransactionReferenceNo = invoiceLine.JI_PreviousEntryNumber;
			entryLineData.PreviousTransactionReferenceNoType = invoiceLine.JI_OriginalStateDocType;
			entryLineData.FOBAmount = entryLine.CL_CustomsValue;

			SumUpInvoiceLineQuantities(entryLineData, entryLine);

			if (invoiceLine.JI_InboundDate.IsValid)
			{
				entryLineData.InboundDate = invoiceLine.JI_InboundDate.ToDateTime();
			}

			PopulateEntryLine(entryLine, invoiceLine, entryLineData);
		}
		protected virtual void PopulateEntryLine(CusEntryLine entryLine, JobComInvoiceLine invoiceLine, LocalExportEntryLine entryLineData)
		{
		}

		void SumUpInvoiceLineQuantities(LocalExportEntryLine entryLineData, CusEntryLine entryLine)
		{
			var invoiceLines = entryLine.InvoiceLines.Cast<JobComInvoiceLine>();
			foreach (var invoiceLine in invoiceLines)
			{
				entryLineData.Quantity += invoiceLine.HasHSRequiringInvQuantityInCustomsUQ ? invoiceLine.JI_CustomsQuantity : invoiceLine.JI_InvoiceQuantity;
				entryLineData.Packages += invoiceLine.JI_NoOfPacks;
				entryLineData.NetWeight += Core.Constants.Weight.Convert(invoiceLine.JI_NetWeight, invoiceLine.JI_NetWeightUQ, Core.Constants.Weight.Kilograms);
			}
		}

		protected virtual void PopulateMoreOrganisations(CusEntryHeader entry, LocalExportEntryHeader localExportData)
		{
		}
	}
}
