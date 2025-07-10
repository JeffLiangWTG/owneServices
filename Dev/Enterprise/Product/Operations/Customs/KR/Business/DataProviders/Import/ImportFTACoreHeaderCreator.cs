using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public abstract class ImportFTACoreHeaderCreator<THeader, TLine>
		where THeader : ImportFTAHeaderCore, new()
		where TLine : ImportFTALine, new()
	{
		public THeader Create(CusEntryHeader entry)
		{
			var headerData = new THeader();
			var declaration = entry.Declaration;

			headerData.StatementNumber5WN = entry.EntryInstruction?.CEI_StatementNumber5WN ?? ZString.Empty;
			headerData.ImportDeclarationNumber = entry.EntryNumber;
			headerData.DeparturePort = entry.Declaration.PortOfLoading?.Description ?? ZString.Empty;
			headerData.LawCode = entry.EntryInstruction?.CEI_FTARelationArticleCode ?? ZString.Empty;
			if (declaration.JE_ExportDate.IsValid)
			{
				headerData.DepartureDate = declaration.JE_ExportDate.ToDateTime();
			}
			headerData.DepartureCountryCode = declaration.JE_RL_NKPortOfLoading.SubstringSafe(0, 2);
			headerData.TransshipmentYN = declaration.TransshipmentYN;
			if (declaration.JE_TransshipmentDate.IsValid)
			{
				headerData.TransshipmentDate = declaration.JE_TransshipmentDate.ToDateTime();
			}
			headerData.TransshipmentCountryCode = declaration.TransshipmentCountryCode;

			var unloco = entry.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, declaration.JE_TransshipmentPort);
			headerData.TransshipmentPort = unloco?.RL_PortName ?? ZString.Empty;
			headerData.Supplier = PopulateSupplier(entry.RandomHeader.Supplier, RoleType.Supplier);
			headerData.Manufacturer = PopulateManufacturer(entry.RandomHeader.ManufacturerAddress, RoleType.Manufacturer);
			headerData.Importer = PopulateImporter(declaration.ImporterAddress);

			headerData.EntryLines = PopulateEntryLines(entry.MergedLines.OrderBy(x => x.CL_LineNumber));

			PopulateMoreEntryFields(headerData, entry);
			return headerData;
		}

		TLine[] PopulateEntryLines(IEnumerable<CusEntryLine> mergedLines)
		{
			var entryLineDataList = new List<TLine>();
			foreach (CusEntryLine line in mergedLines)
			{
				var ftaSeqNo = line.CL_FTASequenceNumber;
				if (!ftaSeqNo.IsEmpty)
				{
					entryLineDataList.Add(PopulateEntryLine(line));
				}
			}
			return entryLineDataList.ToArray();
		}

		TLine PopulateEntryLine(CusEntryLine entryLine)
		{
			var entryLineData = new TLine();
			var invoiceLine = entryLine.RandomLine;
			entryLineData.EntryLineNo = entryLine.CL_LineNumber;
			entryLineData.SequenceNo = entryLine.CL_FTASequenceNumber;
			entryLineData.HSCode = invoiceLine.JI_Tariff.SubstringSafe(0, 6);
			entryLineData.AdditionalInvoiceIssuedInThirdCountryYN = !invoiceLine.JI_RN_NKSecondCommercialInvoiceCountry.IsEmpty ? Constants.YesNo.Yes : Constants.YesNo.No;
			entryLineData.AdditionalInvoiceIssuingThirdCountryCode = invoiceLine.JI_RN_NKSecondCommercialInvoiceCountry;
			if (invoiceLine.JI_CoveredByCOOExporter)
			{
				entryLineData.CertificateOfOriginExporterNumber = invoiceLine.CountryOfOriginExporterNumber;
			}
			entryLineData.CountryOfOrigin = invoiceLine.JI_CountryOfOrigin;
			entryLineData.CertificateOfOriginSplitOrder = invoiceLine.COOSplitOrder;
			entryLineData.CountryOfOriginSupportingDocType = invoiceLine.JI_COOSupportingDocType;
			entryLineData.DutyRateCode = invoiceLine.JI_PrimaryPreference;
			entryLineData.TariffRate = invoiceLine.UniversalDutyRate.GetRate();
			if (invoiceLine.JI_PrimaryPreference.SubstringSafe(0, 3) == Constants.ZZ.Preferences.IsraelFTA)
			{
				entryLineData.Manufacturer = PopulateLineManufacturer(invoiceLine.InvoiceHeader.ManufacturerAddress, RoleType.Manufacturer);
			}

			var certificateOfOrigin = invoiceLine.CertificateOfOriginData;
			if (certificateOfOrigin != null)
			{
				entryLineData.CertificateOfOriginProductType = certificateOfOrigin.CSI_Code;
				entryLineData.AssociatedCOOIssuingCountryCode = certificateOfOrigin.CSI_RN_NKCountryCode;
				if (certificateOfOrigin.CSI_DateOfIssue.IsValid)
				{
					entryLineData.CertificateOfOriginIssueDate = certificateOfOrigin.CSI_DateOfIssue.ToDateTime();
				}
				entryLineData.CertificateOfOriginNo = certificateOfOrigin.CSI_ReferenceNumber;
				entryLineData.CertifiticateOfOriginIssuingAgencyType = CertifiticateOfOriginIssuedTypeList.GetCertifiticateOfOriginIssuingAgencyType(certificateOfOrigin.CSI_IssuerType);
				entryLineData.CertificateOfOriginAgencyName = certificateOfOrigin.CSI_Description;
				entryLineData.CertifiticateOfOriginIssuerType = CertifiticateOfOriginIssuedTypeList.GetCertifiticateOfOriginIssuerType(certificateOfOrigin.CSI_IssuerType);
			}

			entryLineData.CertificateOfOriginTotalNetWeight = entryLine.CertificateOfOriginTotalNetWeightInKG;
			entryLineData.NetWeight = entryLine.NetWeightInKG; 
			PopulateMoreEntryLineFields(entryLineData, entryLine);
			return entryLineData;
		}
		Organisation PopulateSupplier(OrgHeader header, RoleType type)
		{
			Organisation result = null;
			if (header != null)
			{
				var address = header.GetCustomsAddressThenMainAddress();
				result = new Organisation(type)
				{
					CompanyName = address.CompanyName,
					CountryCode = address.OA_RN_NKCountryCode,
					RepresentativeName = header.GetRepresentativeName(),
					AddressLine1 = address.Address1,
					PhoneNumber = address.OA_Phone,
					FaxNumber = address.OA_Fax
				};
			}
			return result;
		}
		Organisation PopulateManufacturer(OrgAddress address, RoleType type)
		{
			Organisation result = null;
			if (address != null)
			{
				result = new Organisation(type)
				{
					CompanyName = address.CompanyName,
					RepresentativeName = address.Header.GetRepresentativeName(),
					AddressLine1 = address.Address1,
					PhoneNumber = address.OA_Phone,
					FaxNumber = address.OA_Fax
				};
			}

			return result;
		}
		Organisation PopulateImporter(OrgAddress importerAddress)
		{
			Organisation result = null;
			if (importerAddress != null)
			{
				result = new Organisation(RoleType.Importer)
				{
					CompanyName = importerAddress.CompanyName,
					RepresentativeName = importerAddress.Header.GetRepresentativeName(),
					AddressLine1 = importerAddress.Address1,
					AddressLine2 = importerAddress.Address2,
					Postcode = importerAddress.Postcode,
					BuildingNumber = importerAddress.GetBuildingNumber(),
					RoadNameCode = importerAddress.GetRoadNameCode(),
					PhoneNumber = importerAddress.OA_Phone,
					FaxNumber = importerAddress.OA_Fax,
					Email = importerAddress.OA_Email,
					IsIndividual = importerAddress.Header.GetIsIndividual()
				};
				var firstID = importerAddress.GetRegistrationFirstMatchedBusinessOrIndividualIDConverted();
				IDNumberAndType[] idNumbers = null;
				if (result.IsIndividual)
				{
					idNumbers = importerAddress.GetRegistrationIDNumbers(firstID != null ? new string[] { firstID.Type, IdentificationType.UnipassIDForIndividual } : new string[] { IdentificationType.UnipassIDForIndividual });
				}
				else
				{
					idNumbers = importerAddress.GetRegistrationIDNumbers(firstID != null ? new string[] { firstID.Type, IdentificationType.UnipassIDForOrganization } : new string[] { IdentificationType.UnipassIDForOrganization });
				}
				result.SetRegistrationIDNumbers(idNumbers);
			}
			return result;
		}
		Organisation PopulateLineManufacturer(OrgAddress address, RoleType type)
		{
			Organisation result = null;
			if (address != null)
			{
				result = new Organisation(type)
				{
					AddressLine1 = address.Address1,
					AddressLine2 = address.Address2,
					Postcode = address.Postcode,
				};
			}

			return result;
		}
		protected virtual void PopulateMoreEntryFields(THeader headerData, CusEntryHeader header)
		{ }
		protected virtual void PopulateMoreEntryLineFields(TLine lineData, CusEntryLine line)
		{ }
	}
}
