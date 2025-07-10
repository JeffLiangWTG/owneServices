#if DEBUG

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DefaultDataRig
	{
		public DefaultDataRig(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		public readonly BusinessObjectFactory Factory;

		JobDeclaration fDeclaration;
		public JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = (JobDeclaration)Factory.New(typeof(JobDeclaration));
					fDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				}
				return fDeclaration;
			}
		}

		JobComInvoiceGroupHeader fGroupHeader;
		public JobComInvoiceGroupHeader GroupHeader
		{
			get
			{
				if (fGroupHeader == null)
				{
					fGroupHeader = Declaration.JobComInvoiceGroupHeaders[0];
				}
				return fGroupHeader;
			}
		}

		JobComInvoiceHeader fInvoiceHeader;
		public JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				if (fInvoiceHeader == null)
				{
					fInvoiceHeader = GroupHeader.JobComInvoiceHeaders.AddNew();
					fInvoiceHeader.JZ_OH_Supplier = Consignor.PK;
				}
				return fInvoiceHeader;
			}
		}

		JobComInvoiceLine fInvoiceLine;
		public JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (fInvoiceLine == null)
				{
					fInvoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew();
				}
				return fInvoiceLine;
			}
		}

		Classification fBeerClassification;
		public Classification BeerClassification
		{
			get
			{
				if (fBeerClassification == null)
				{
					fBeerClassification = (Classification)Factory.New(typeof(Classification));
					fBeerClassification.CC_ClassificationType = Classification.ClassificationType.IMP;
					fBeerClassification.CC_TariffNum = "2203.00.69 20";
					fBeerClassification.CC_LookupCode = "BEER";
				}
				return fBeerClassification;
			}
		}

		AUOrgSupplierPart fBeerPart;
		public AUOrgSupplierPart BeerPart
		{
			get
			{
				if (fBeerPart == null)
				{
					fBeerPart = (AUOrgSupplierPart)Factory.New(typeof(AUOrgSupplierPart));
					fBeerPart.OP_PartNum = "BEER";
					fBeerPart.OP_Desc = "FIZZY BEER";
					fBeerPart.OP_StockKeepingUnit = "UNT";

					var pivot1 = fBeerPart.PivotsForBinding.AddNew();
					pivot1.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTI;
					pivot1.CI_CC = BeerClassification.PK;

					OrgPartRelation consignorRelation = fBeerPart.RelatedOrganisations.AddNew();
					consignorRelation.OU_OH = Consignor.PK;
					consignorRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

					OrgPartRelation consigneeRelation = fBeerPart.RelatedOrganisations.AddNew();
					consigneeRelation.OU_OH = Consignee.PK;
					consigneeRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

					OrgPartUnit uNTtoCTN = fBeerPart.PartUnits.AddNew();
					uNTtoCTN.OF_QuantityInParent = 1;
					uNTtoCTN.OF_PackType = "UNT";
					uNTtoCTN.OF_ParentPackType = "CTN";

					OrgPartUnit bottlesInCarton = fBeerPart.PartUnits.AddNew();
					bottlesInCarton.OF_QuantityInParent = 24;
					bottlesInCarton.OF_PackType = "BOT";
					bottlesInCarton.OF_ParentPackType = "CTN";

					OrgPartUnit litresInBottle = fBeerPart.PartUnits.AddNew();
					litresInBottle.OF_QuantityInParent = .330m;
					litresInBottle.OF_PackType = "L";
					litresInBottle.OF_ParentPackType = "BOT";

					OrgPartUnit litresOfAlcoholPerLitre = fBeerPart.PartUnits.AddNew();
					litresOfAlcoholPerLitre.OF_QuantityInParent = 0.048m;
					litresOfAlcoholPerLitre.OF_PackType = "LA";
					litresOfAlcoholPerLitre.OF_ParentPackType = "L";

					Factory.Save();
				}
				return fBeerPart;
			}
		}

		OrgHeader fConsignee;
		public OrgHeader Consignee
		{
			get
			{
				if (fConsignee == null)
				{
					fConsignee = (OrgHeader)Factory.New(typeof(OrgHeader));
					fConsignee.OH_Code = "CONSIGNEE";
					fConsignee.MainAddress.OA_Address1 = "Address 1";
					fConsignee.OH_IsConsignee = true;
				}
				return fConsignee;
			}
		}

		OrgHeader fConsignor;
		public OrgHeader Consignor
		{
			get
			{
				if (fConsignor == null)
				{
					fConsignor = (OrgHeader)Factory.New(typeof(OrgHeader));
					fConsignor.OH_Code = "CONSIGNOR";
					fConsignor.MainAddress.OA_Address1 = "Address 1";
					fConsignor.OH_IsConsignor = true;
				}
				return fConsignor;
			}
		}
	}
}
#endif
