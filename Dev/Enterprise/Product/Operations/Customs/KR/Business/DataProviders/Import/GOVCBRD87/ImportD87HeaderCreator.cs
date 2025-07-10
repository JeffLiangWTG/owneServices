using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.KR.Business
{
	public class ImportD87HeaderCreator
	{
		public ImportD87Header Create(CusEntryHeader entry)
		{
			var importD87HeaderData = new ImportD87Header();
			importD87HeaderData.CarnetCertificateNumber = entry.Declaration.JE_AgentsReference;
			importD87HeaderData.RepresentativeProductName = entry.Declaration.Notes.GetAllNotes().Cast<StmNote>().FirstOrDefault(x => x.ST_Description == PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description)?.ST_NoteText ?? ZString.Empty;
			importD87HeaderData.BondedAreaCode = entry.Declaration.JE_LocationOtherInformation;
			importD87HeaderData.CarnetUseCode = entry.Declaration.JE_ExportGoodsType;
			importD87HeaderData.DeclarationCustomsOffice = entry.Declaration.JE_CustomsOffice;
			importD87HeaderData.DeclarationCustomsDivision = entry.Declaration.JE_CustomsDivision;
			if (entry.Declaration.JE_EntryDate.IsValid)
			{
				importD87HeaderData.EffectiveToDate = entry.Declaration.JE_EntryDate.ToDateTime();
			}
			var invHeader = entry.Declaration.Invoices.Cast<JobComInvoiceHeader>().FirstOrDefault();
			importD87HeaderData.TotalInvoiceAmount = invHeader.JZ_InvoiceAmount;
			importD87HeaderData.InvoiceCurrency = invHeader.JZ_RX_NKInvoice_Currency;

			importD87HeaderData.TotalQty = entry.Declaration.JE_TotalNoOfPieces;
			importD87HeaderData.TotalGrossWeight = entry.Declaration.JE_TotalWeight;
			importD87HeaderData.TotalGrossWeighUnit = entry.Declaration.JE_TotalWeightUnit;
			importD87HeaderData.TotalPackQty = entry.Declaration.JE_TotalNoOfPacks;
			importD87HeaderData.PackType = entry.Declaration.JE_TotalNoOfPacksPackType;

			var relatedBill = entry.RelatedBill;
			if (relatedBill != null && relatedBill.IsHouseBill)
			{
				importD87HeaderData.HouseBillSplitDeclarationIndicator = relatedBill.CU_HBSplitDecInd.ToUpper().Equals(HouseBillSplitDeclarationIndicatorCodeList.Codes.Y);
			}
			importD87HeaderData.CargoManagementNo = entry.RandomHeader.JZ_ImportCargoManagementNumber;

			PopulateimportD87OrgHeader(entry, importD87HeaderData);

			return importD87HeaderData;
		}

		void PopulateimportD87OrgHeader(CusEntryHeader entry, ImportD87Header importD87HeaderData)
		{
			if (entry.Declaration.SupplierAddress != null)
			{
				importD87HeaderData.Supplier = new Organisation(RoleType.Supplier)
				{
					CompanyName = entry.Declaration.SupplierAddress.CompanyName
				};
			}

			if (entry.Declaration.ImporterAddress != null)
			{
				importD87HeaderData.Importer = new Organisation(RoleType.Importer)
				{
					CompanyName = entry.Declaration.ImporterAddress.CompanyName,
					AddressLine1 = entry.Declaration.ImporterAddress.Address1,
					AddressLine2 = entry.Declaration.ImporterAddress.Address2,
					PhoneNumber = entry.Declaration.ImporterAddress.OA_Phone
				};
			}

			importD87HeaderData.UnipassDeclarantID = entry.Declaration.UNIPASSDeclarantID;
		}
	}
}
