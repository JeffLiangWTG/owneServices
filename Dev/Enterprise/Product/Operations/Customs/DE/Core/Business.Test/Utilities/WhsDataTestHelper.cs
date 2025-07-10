using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MasterFiles;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.Testing
{
	public sealed class WhsDataTestHelper : Customs.Business.Testing.WhsDataTestHelper<JobDeclaration, OrgSupplierPart, BaseCusClassification, CusClassPartPivot>
	{
		public WhsDataTestHelper()
		{
		}

		public WhsDataTestHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override JobDeclaration GetNewDeclarationCore(ZString messageType, ZString declarationReference, ZString entryNumber,
			ZDecimal quantity)
		{
			var declaration = base.GetNewDeclarationCore(messageType, declarationReference, entryNumber, quantity);
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.MessageInitiator = messageInitiator;
			declaration.SetSupportsBondedWarehousingForTesting(true);
			declaration.JE_OH_Supplier = Supplier.PK;
			return declaration;
		}

		public JobDeclaration GetNewDeclarationWithInstruction(BusinessObjectFactory factory, ZString messageType, ZString declarationReference, ZString entryNumber,
			ZDecimal quantity, bool isInward, ZString previousEntryNumber = default)
		{
			var declaration = GetNewDeclaration(messageType, declarationReference, entryNumber, quantity);
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.MessageInitiator = messageInitiator;
			declaration.SetSupportsBondedWarehousingForTesting(true);
			var invoice = declaration.Invoices.Single();
			var invoiceLine = invoice.JobComInvoiceLines.Cast<JobComInvoiceLine>().Single();
			RefCusProcedure procedure;
			if (isInward)
			{
				procedure = CreateInwardCusProcedure(factory);
			}
			else
			{
				procedure = declaration.IsExport ? CreateOutwardCusProcedureExport(factory) : CreateOutwardCusProcedure(factory);
			}

			if (declaration.IsExport)
			{
				declaration.JE_OH_Supplier = declaration.JE_OH_Importer;
			}

			invoiceLine.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode;
			invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(isInward);
			var entry = declaration.CustomsEntryHeaders.Single();
			var entryLine = entry.MergedLines.Single();

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;

			invoiceLine.JI_PreviousEntryNumber = previousEntryNumber;
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			invoiceLine.JI_BondedWhsQuantity = quantity;
			invoiceLine.JI_BondedWhsUnitQty = "KG";

			if (isInward)
			{
				entryInstruction.CEI_Style = "EZL";
				entryInstruction.CEI_SubStyle = "A";
				entryInstruction.CEI_OA_Warehouse2 = declaration.WarehouseDocAddress.E2_OA_Address;
				entryInstruction.CEI_Procedure = procedure.ZZ6_ProcedureCode;
			}
			else
			{
				entryInstruction.CEI_Style = "EZA";
				entryInstruction.CEI_SubStyle = "A";
				entryInstruction.CEI_OA_Warehouse = declaration.WarehouseDocAddress.E2_OA_Address;
				entryInstruction.CEI_Procedure = procedure.ZZ6_ProcedureCode;
			}
			invoiceLine.JI_CEI = entryInstruction.PK;

			return declaration;
		}

		public OrgHeader IprWarehouse
		{
			get
			{
				if (warehouse == null)
				{
					warehouse = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "I1");
					if (warehouse == null)
					{
						warehouse = Factory.New<OrgHeader>();
						warehouse.OH_Code = "I1";
						warehouse.OH_RL_NKClosestPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).RL_Code;
						warehouse.MainAddress.OA_Address1 = "I1 ADDRESS 1";
						warehouse.MainAddress.LocalControlledPremisesID = "23423";
						warehouse.MainAddress.OA_RN_NKCountryCode = WarehouseDefaultCountry;
					}
				}
				return warehouse;
			}
		}
		OrgHeader warehouse;

		public IWhsWarehouse IprWhsWarehouse
		{
			get
			{
				if (iprWhsWarehouse == null)
				{
					iprWhsWarehouse = Factory.LoadTop1<IWhsWarehouse>(new ZQuery(WhsWarehouseSchema.WW_WarehouseCode, "IP1"));
					if (iprWhsWarehouse == null)
					{
						var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
						var warehouseMainAddress = IprWarehouse.MainAddress;
						iprWhsWarehouse = (IWhsWarehouse)helper.CreateWarehouse(warehouseMainAddress.OA_Address1, "IP1", "IPR1");
						iprWhsWarehouse.WW_OA_WarehouseAddress = warehouseMainAddress.PK;
						iprWhsWarehouse.WW_IsBondedWarehouse = true;
						iprWhsWarehouse.WW_IsVirtualWarehouse = true;
						var area = ((IWhsArea)iprWhsWarehouse.Areas[0]);
						area.WA_AreaType = "IPR";
						area.WA_IsDefaultPickArea = true;
						area.WA_IsDefaultPutawayArea = true;
						area.WA_IsPickingArea = true;
						area.WA_IsPutawayArea = true;
						iprWhsWarehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
						iprWhsWarehouse.WW_AutoPrintPackingSlip = false;
					}
				}
				return iprWhsWarehouse;
			}
		}
		IWhsWarehouse iprWhsWarehouse;

		public JobDeclaration GetNewDeclarationWithInstructionForInwardProcessing(BusinessObjectFactory factory, ZString messageType, ZString declarationReference, ZString entryNumber,
			ZDecimal quantity, bool isInto, ZString previousEntryNumber = default)
		{
			var declaration = GetNewDeclaration(messageType, declarationReference, entryNumber, quantity);
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.MessageInitiator = messageInitiator;
			declaration.SetSupportsBondedWarehousingForTesting(true);
			declaration.WarehouseDocAddress.E2_OA_Address = IprWhsWarehouse.WW_OA_WarehouseAddress;
			var invoice = declaration.Invoices.Single();
			var invoiceLine = invoice.JobComInvoiceLines.Cast<JobComInvoiceLine>().Single();
			RefCusProcedure procedure;
			if (isInto)
			{
				procedure = CreateIntoInwardCusProcedure(factory);
			}
			else
			{
				procedure = CreateOutOfInwardCusProcedure(factory);
			}

			invoiceLine.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode;
			var entry = declaration.CustomsEntryHeaders.Single();
			var entryLine = entry.MergedLines.Single();

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;

			invoiceLine.JI_PreviousEntryNumber = previousEntryNumber;
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			invoiceLine.JI_BondedWhsQuantity = quantity;
			invoiceLine.JI_BondedWhsUnitQty = "KG";

			if (isInto)
			{
				entryInstruction.CEI_Style = "EZL";
				entryInstruction.CEI_SubStyle = "A";
				entryInstruction.CEI_OA_Warehouse2 = declaration.WarehouseDocAddress.E2_OA_Address;
				entryInstruction.CEI_Procedure = procedure.ZZ6_ProcedureCode;
			}
			else
			{
				entryInstruction.CEI_Style = "EZA";
				entryInstruction.CEI_SubStyle = "A";
				entryInstruction.CEI_OA_Warehouse = declaration.WarehouseDocAddress.E2_OA_Address;
				entryInstruction.CEI_Procedure = procedure.ZZ6_ProcedureCode;
			}

			invoiceLine.JI_CEI = entryInstruction.PK;

			return declaration;
		}

		public static RefCusProcedure CreateInwardCusProcedure(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Germany, "IM", "71", "00", "   ", "7100", "IMP", "");
			procedure.ZZ6_IntoWarehouse = "Y";
			procedure.ZZ6_OutOfWarehouse = "N";
			factory.Save();
			return procedure;
		}
		public static RefCusProcedure CreateOutOfInwardCusProcedure(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Germany, "IM", "40", "51", "   ", "4051", "IMP", "");
			procedure.ZZ6_IntoInwardProcessing = "N";
			procedure.ZZ6_OutOfInwardProcessing = "Y";
			factory.Save();
			return procedure;
		}

		public static RefCusProcedure CreateIntoInwardCusProcedure(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Germany, "IM", "51", "51", "   ", "5100", "IMP", "");
			procedure.ZZ6_IntoInwardProcessing = "Y";
			procedure.ZZ6_OutOfInwardProcessing = "N";
			factory.Save();
			return procedure;
		}

		public static RefCusProcedure CreateOutwardCusProcedure(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Germany, "IM", "40", "71", "   ", "4071", "IMP", "");
			procedure.ZZ6_IntoWarehouse = "N";
			procedure.ZZ6_OutOfWarehouse = "Y";
			factory.Save();
			return procedure;
		}

		public static RefCusProcedure CreateOutwardCusProcedureExport(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Germany, "EX", "40", "71", "   ", "4071", "EXP", "");
			procedure.ZZ6_IntoWarehouse = "N";
			procedure.ZZ6_OutOfWarehouse = "Y";
			factory.Save();
			return procedure;
		}

		public static RefCusProcedure CreateWarehouseAdjustmentProcedure(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Germany, "EX", "01", string.Empty, "   ", "4071", "WAD", "");
			procedure.ZZ6_IntoWarehouse = "N";
			procedure.ZZ6_OutOfWarehouse = "Y";
			factory.Save();
			return procedure;
		}

		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.Germany; }
		}

		public static WhsDataTestHelper New(BusinessObjectFactory factory) => new WhsDataTestHelper(factory);
	}
}
