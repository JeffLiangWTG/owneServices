using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Business.Wizards.CFSP
{
	public interface ISfdWizardManager
	{
		void CreateSfd();
		SfdWizard SfdWizard { get; }
		JobDeclaration Declaration { get; }
		bool ExecutedSuccessfully { get; }
	}

	public class SfdWizardManager : ISfdWizardManager
	{
		public SfdWizardManager(JobDeclaration declaration)
		{
			this.Declaration = declaration;
			this.SfdWizard = new SfdWizard(declaration.Factory);
		}

		public void CreateSfd()
		{
			Declaration.Invoices.DeleteAll();
			Declaration.InvoiceLines.RemoveAndDeleteAll();
			Declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var cei = Declaration.CustomsEntryInstructions.Count == 0 ? Declaration.CustomsEntryInstructions.AddNew() : Declaration.CustomsEntryInstructions[0];
			Declaration.JE_MessageType = MessageTypeList.Codes.Import;
			cei.CEI_Style = ImportSADDeclarationTypeList.Codes.ImportFullDeclaration;
			Declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;
			cei.CEI_SubStyle = GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.SdpSfdGoodsNotArrived;
			Declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			Declaration.JE_TotalNoOfPacks = SfdWizard.NumberPackages;
			Declaration.JE_OH_Importer = SfdWizard.Consignee;
			Declaration.JE_TotalNoOfPacksPackType = SfdWizard.PackageType;
			Declaration.JE_GoodsDescription = SfdWizard.GoodsDesc.Left(Declaration.JE_GoodsDescriptionInfo.MaxLength);
			Declaration.JE_LocationOfGoods = SfdWizard.LocationOfGoods.Left(3);
			Declaration.SubLocation = SfdWizard.LocationOfGoods.Right(3);
			Declaration.JE_TotalNoOfPacksPackType = "PKG";
			Declaration.JE_EidrType = EidrTypeList.Codes.CFS;

			var invoice = Declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = SfdWizard.InvoiceNumber;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Procedure = SfdWizard.CPC;
			invoiceLine.JI_Description = SfdWizard.GoodsDesc;
			invoiceLine.Taxes.RemoveAndDeleteAll();  // no taxes needed, so remove any that may be present from the taxLineCreator

			var mb = Declaration.PrimaryMasterBill;
			if (mb == null)
			{
				Declaration.JE_MasterBill = "Master";
				mb = Declaration.PrimaryMasterBill;
			}
			var cr = (BasePackingGroup)mb.PackingGroups.FirstOrDefault() ?? mb.PackingGroups.AddNew();
			var cw = cr.Packages.AddNew();
			cw.CW_MarksAndNos = SfdWizard.MarksAndNumbers;
			cw.CW_PackQty = SfdWizard.NumberPackages;
			cw.CW_PackType = SfdWizard.PackageType;
			var pivot = (InvoiceLinePackagePivot)invoiceLine.PackagesPivot.AddPivotFor(cw);
			pivot.CHC_NumberOfPacks = SfdWizard.NumberPackages;

			Declaration.ZG_VATDeferNumber = "";
			Declaration.JE_DefermentAccountNumber = "";
			Declaration.ZG_VATDeferType = "";
			Declaration.JE_PaymentMethod = "";

			IBusiness declaration = Declaration;
			declaration.RunPreSaveValidationFetch(true);
			declaration.RunPreSaveValidation();
			ExecutedSuccessfully = true;
		}

		public bool ExecutedSuccessfully { get; set; }

		public JobDeclaration Declaration { get; private set; }
		public SfdWizard SfdWizard { get; private set; }
	}
}
