
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Business.Wizards.EIDR
{
	public interface IEidrWizardManager
	{
		void CreateEidr();
		EidrWizard EidrWizard { get; }
		JobDeclaration Declaration { get; }
		bool ExecutedSuccessfully { get; }
	}

	public class EidrWizardManager : IEidrWizardManager
	{
		public EidrWizardManager(JobDeclaration declaration)
		{
			Argument.NotNull(declaration, "declaration");

			this.Declaration = declaration;
			this.EidrWizard = new EidrWizard(declaration);
		}

		public void CreateEidr()
		{
			const string PreviousDocumentOtherCode = "ZZZ";

			Declaration.Invoices.DeleteAll();
			Declaration.InvoiceLines.RemoveAndDeleteAll();
			Declaration.CustomsEntryInstructions.RemoveAndDeleteAll();

			Declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
			Declaration.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportSupplementaryDeclaration;
			Declaration.JE_EntrySubStyle = GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.LcpEidrSupplementaryDeclarationImportOrWarehouseRemovalSdiSdw;
			Declaration.JE_TransportMode = EidrWizard.TransportMode;
			Declaration.JE_GoodsDescription = EidrWizard.DescriptionOfGoods;
			Declaration.JE_EntryAuthorisationDate = EidrWizard.DateOfImport;
			Declaration.JE_DateOfArrival = EidrWizard.DateOfImport;
			Declaration.JE_TotalWeight = EidrWizard.NetMassInKG;
			Declaration.JE_GoodsOrigin = EidrWizard.CountryOfOrigin;
			Declaration.JE_TotalNoOfPacks = EidrWizard.NumberOfPackages;
			Declaration.JE_TotalNoOfPacksPackType = EidrWizard.TypeOfPackages;
			Declaration.JE_EidrType = EidrWizard.EIDRType;
			Declaration.JE_OA_DeclarantAddress = EidrWizard.DeclarantOrgAddressPk;
			Declaration.JE_SuppDecDueDate = EidrWizard.SupplementaryDeclarationDueDate;
			Declaration.WarehouseDocAddress.E2_OA_Address = EidrWizard.WarehouseOrgAddressPk;

			var cusEntryInstruction = Declaration.CusEntryInstruction;
			cusEntryInstruction.CEI_OA_Warehouse2 = EidrWizard.WarehouseOrgAddressPk;

			var invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = EidrWizard.InvoiceNumber;
			invoiceHeader.JZ_InvoiceAmount = EidrWizard.CustomsValueInGBP;
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedKingdom;

			var invoiceLine = Declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = EidrWizard.CPC;
			invoiceLine.JI_Description = EidrWizard.DescriptionOfGoods;
			invoiceLine.JI_CountryOfOrigin = EidrWizard.CountryOfOrigin;
			invoiceLine.JI_LinePrice = EidrWizard.CustomsValueInGBP;
			invoiceLine.JI_NetWeight = EidrWizard.NetMassInKG;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;

			foreach (var reference in EidrWizard.AdditionalReferenceNumbers.Split(','))
			{
				if (reference.Trim() != string.Empty)
				{
					var pd = invoiceLine.PreviousDocuments.AddNew();
					pd.CSI_Code = PreviousDocumentOtherCode;
					pd.CSI_ReferenceNumber = reference.TrimEndSpaceTab().TrimStart();
					pd.CSI_SubType = PreviousDocumentClassList.Codes.PreviousDocument;
				}
			}

			IBusiness declaration = Declaration;
			declaration.RunPreSaveValidationFetch(true);
			declaration.RunPreSaveValidation();

			ExecutedSuccessfully = true;
		}

		public EidrWizard EidrWizard { get; private set; }
		public JobDeclaration Declaration { get; private set; }
		public bool ExecutedSuccessfully { get; set; }
	}
}
