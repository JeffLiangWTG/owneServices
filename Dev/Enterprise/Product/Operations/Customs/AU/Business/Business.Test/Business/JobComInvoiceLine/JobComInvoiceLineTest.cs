using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class JobComInvoiceLineTest : TestCaseWithFactory
	{
		#region Implementation

		protected string partNum = "XXX111";
		protected string partNum2 = "YYY111";
		protected string exportTariffNum2 = "1111.11.11";
		protected string lookup = "XXX";
		protected ZString exportCustomsUQ = "BO";

		JobComInvoiceHeader fHeader;
		protected JobComInvoiceHeader Header
		{
			get
			{
				if (fHeader == null)
				{
					fHeader = JobDec.Invoices.AddNew();
					fHeader.JZ_JZ_GroupInvoiceFK = JobDec.JobComInvoiceGroupHeaders[0].PK;
					fHeader.JZ_OH_Supplier = Consignor.PK;
					fHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
				}
				return fHeader;
			}
		}

		JobComInvoiceLine fLine;
		protected JobComInvoiceLine Line
		{
			get
			{
				if (fLine == null)
				{
					fLine = Header.JobComInvoiceLines.AddNew();
				}
				return fLine;
			}
		}

		JobComInvoiceLine fDetatchedLine;
		protected JobComInvoiceLine DetatchedLine
		{
			get
			{
				if (fDetatchedLine == null)
				{
					fDetatchedLine = Factory.New<JobComInvoiceLine>();
				}
				return fDetatchedLine;
			}
		}

		#region Lazy Created objects for tests

		protected const string ImportTariffNum = "1234.56.78";
		protected const string ExportTariffNum = "9999.99.99";

		AUCAHECC fAHECC;
		protected AUCAHECC AHECC
		{
			get
			{
				if (fAHECC == null)
				{
					fAHECC = Factory.New<AUCAHECC>();
					fAHECC.UA_AHECC = ExportTariffNum;
					fAHECC.UA_UQ = exportCustomsUQ;
					fAHECC.UA_Index = 1;
				}
				return fAHECC;
			}
		}

		AUCAHECC fImportAHECC;
		protected AUCAHECC ImportAHECC
		{
			get
			{
				if (fImportAHECC == null)
				{
					fImportAHECC = Factory.New<AUCAHECC>();
					fImportAHECC.UA_AHECC = ImportTariffNum;
					fImportAHECC.UA_UQ = "NR";
					fImportAHECC.UA_Index = 1;
				}
				return fImportAHECC;
			}
		}

		RefCountry fNZCountry;
		protected RefCountry NZCountry
		{
			get
			{
				if (fNZCountry == null)
				{
					fNZCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "NZ");
				}
				return fNZCountry;
			}
		}

		RefCountry fAUCountry;
		protected RefCountry AUCountry
		{
			get
			{
				if (fAUCountry == null)
				{
					fAUCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
				}
				return fAUCountry;
			}
		}

		RefCurrency fAUDCurrency;
		protected RefCurrency AUDCurrency
		{
			get
			{
				if (fAUDCurrency == null)
				{
					fAUDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
				}
				return fAUDCurrency;
			}
		}

		RefCurrency fUSDCurrency;
		protected RefCurrency USDCurrency
		{
			get
			{
				if (fUSDCurrency == null)
				{
					fUSDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
				}
				return fUSDCurrency;
			}
		}

		AUCClass fClass_KG;
		protected AUCClass Class_KG
		{
			get
			{
				if (fClass_KG == null)
				{
					fClass_KG = Factory.LoadFromNaturalKey<AUCClass>(AUCClassSchema.UJ_Code, "2934.30.00 30");
				}
				return fClass_KG;
			}
		}

		protected Classification fImportClass;
		protected Classification ImportClass
		{
			get
			{
				if (fImportClass == null)
				{
					fImportClass = Factory.New<Classification>();
					fImportClass.CC_ClassificationType = JobDeclaration.ClassificationType.IMP;
					fImportClass.CC_LookupCode = lookup;
					fImportClass.CC_TariffNum = ImportTariffNum;
				}
				return fImportClass;
			}
		}

		protected void MakePartUnit()
		{
			OrgPartUnit tempUnit = PartUnit;
		}

		OrgPartUnit fPartUnit;
		protected OrgPartUnit PartUnit
		{
			get
			{
				if (fPartUnit == null)
				{
					ZGuid partPK = Part.PK; // To flush Part to database
					fPartUnit = Factory.New<OrgPartUnit>();
					fPartUnit.OF_OP = partPK;
					fPartUnit.OF_PackType = exportCustomsUQ; //AHECC.UQ "BO"
					fPartUnit.OF_ParentPackType = "KG"; //Part.OrderUQ
					fPartUnit.OF_QuantityInParent = 1000m;
				}
				return fPartUnit;
			}
		}

		OrgHeader fConsignee;
		protected OrgHeader Consignee
		{
			get
			{
				if (fConsignee == null)
				{
					fConsignee = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true));
				}
				return fConsignee;
			}
		}

		OrgHeader fConsignor;
		protected OrgHeader Consignor
		{
			get
			{
				if (fConsignor == null)
				{
					fConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true));
				}
				return fConsignor;
			}
		}

		JobDeclaration fJobDec;
		protected virtual JobDeclaration JobDec
		{
			get
			{
				if (fJobDec == null)
				{
					fJobDec = Factory.New<JobDeclaration>();
					fJobDec.JE_OH_Importer = Consignor.PK; // Changed from 'Part.RelatedOrganisations[0].OU_OH' to stop Factory.Save() firing
					fJobDec.JE_ExportDate = ZDateTime.Now.Date;
				}
				return fJobDec;
			}
		}

		Classification fExportClass;
		protected Classification ExportClass
		{
			get
			{
				if (fExportClass == null)
				{
					fExportClass = Factory.New<Classification>();
					fExportClass.CC_LookupCode = "EXPLookup";
					fExportClass.CC_ClassificationType = JobDeclaration.ClassificationType.EXP;
					fExportClass.CC_LookupCode = lookup;
					fExportClass.CC_TariffNum = ExportTariffNum;
				}
				return fExportClass;
			}
		}

		AUOrgSupplierPart fPart;
		protected AUOrgSupplierPart Part
		{
			get
			{
				if (fPart == null)
				{
					fPart = Factory.New<AUOrgSupplierPart>();
					fPart.OP_PartNum = partNum;
					OrgPartRelation relation = fPart.RelatedOrganisations.AddNew();
					relation.OU_OH = Consignor.PK;
					relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
					var pivot = fPart.PivotsForBinding.AddNew();
					pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
					pivot.CI_CC = ExportClass.PK;
					fPart.OP_StockKeepingUnit = "KG";
					MakePartUnit();
					Factory.Save();
				}
				return fPart;
			}
		}

		#endregion

		#endregion
	}
}
