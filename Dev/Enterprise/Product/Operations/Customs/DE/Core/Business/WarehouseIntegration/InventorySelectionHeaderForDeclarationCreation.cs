using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using static Enterprise.Customs.Business.BondedWarehousingHelper.Constants;
using static Enterprise.Customs.DE.Business.BondedWarehousingHelper.Constants;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.Business
{
	public class InventorySelectionHeaderForDeclarationCreation : Customs.Business.InventorySelectionHeader
	{
		public InventorySelectionHeaderForDeclarationCreation(CreateDeclarationBizObj createDeclarationBizObj, bool updateWarehouse = true) : base(null)
		{
			this.createDeclarationBizObj = createDeclarationBizObj;
			this.updateWarehouse = updateWarehouse;
		}

		protected override void UpdateParentData()
		{
			// No Parent
		}

		protected override IWarehouseProductLine CreateProductLineFromWarehouseDataCore(WhsInventoryWrapper inventoryWrapper,
			IWhsDocketLine whsReceiveLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, ZDecimal invoiceQuantity,
			ZDecimal ratio)
		{
			return null;
		}

		protected override FilterBusinessObjectDefaults GetFilterDefaultsCore()
		{
			var filters = new FilterBusinessObjectDefaults();

			var orgProxy = GlbBranch.CurrentBranch.OrgProxy;
			if (orgProxy != null)
			{
				filters.Add(new FilterBusinessObjectDefault("Client", "Property", orgProxy.PK, isRemovable: true));

				var warehouse = orgProxy.MainAddress.GetWhsWarehouse();
				if (warehouse != null)
				{
					filters.Add(new FilterBusinessObjectDefault("Warehouse", "Property", warehouse.PK, isRemovable: true));
				}
			}

			if (!createDeclarationBizObj.CreateFromWarehouseOrder)
			{
				filters.Add(new FilterBusinessObjectDefault("Pick Area Type", "Property", (ZString)AreaTypes.Bonded, isRemovable: false));
			}

			return filters;
		}

		protected override void ImportInventoriesCore()
		{
			var createdDeclarations = new List<JobDeclaration>();
			var declarationGroupingDefinitions = SelectedLines.Cast<WhsInventoryWrapper>().Select(x => new DeclarationGroupingDefinitionProvider(x));
			foreach (var declarationGroup in declarationGroupingDefinitions.GroupBy(x => new { x.SupplierAddressPK, x.ImporterAddressPK, x.PortOfLoading, x.FirstEUArrival, x.Transport }))
			{
				var filteredSelectedLines = declarationGroup.Select(x => x.InventoryWrapper);
				var randomSelectedLine = filteredSelectedLines.First();
				var declaration = CreateAndPopulateDeclaration(randomSelectedLine);
				var header = new ImportInventorySelectionHeader(declaration);
				header.SelectedLines.AddRange(filteredSelectedLines);
				header.ImportInventories();
				CreateAndPopulateEntryInstructions(declaration, randomSelectedLine.WarehouseAddress.PK);
				CreateAndPopulateBillsAndPackages(declaration);
				PopulateInvoiceNumbersAndDates(declaration);
				Factory.Save();
				if (updateWarehouse)
				{
					var publishResult = BondedWarehousingHelper.PublishShipmentForWHSOutward(declaration);
					if (publishResult.IsEmpty)
					{
						createdDeclarations.Add(declaration);
					}
					else
					{
						declaration.Delete();
						ImportInventoriesResult += publishResult;
					}
				}
				else
				{
					var order = createDeclarationBizObj.SelectedWhsOrder;
					if (order != null)
					{
						declaration.IsOutwardOrderImported = true;

						var docketJobPivot = Factory.New<IWhsDocketJobPivot>();
						docketJobPivot.WV_WD_Docket = order.PK;
						docketJobPivot.WV_DocketType = order.WD_DocketType;
						docketJobPivot.WV_ParentId = declaration.PK;
						docketJobPivot.WV_ParentTableCode = declaration.TablePrefix;
					}
					createdDeclarations.Add(declaration);
				}
			}
			createdDeclarations.ForEach(x => x.RelatedDeclarations.AddRange(createdDeclarations.Except(new[] { x })));
			Factory.Save();
		}

		[ChildEditable]
		public new InventorySelectionLineCollection SelectionLines => (InventorySelectionLineCollection)base.SelectionLines;

		protected override IInventorySelectionLineCollection<Customs.Business.InventorySelectionLine> GetNewInventorySelectionLineCollection() => new InventorySelectionLineCollection(this);

		protected JobDeclaration CreateAndPopulateDeclaration(WhsInventoryWrapper randomSelectedLine)
		{
			var addInfosDictionary = PopulateAddInfoData(BondedWarehousingHelper.GetBondedWarehouseAttributeFromWhsInventoryWrapper(randomSelectedLine));

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_TransportMode = addInfosDictionary.GetValueSafe(WarehouseCustomsLineDetailsAddInfoKeys.Transport);
			declaration.JE_RL_NKPortOfLoading = addInfosDictionary.GetValueSafe(WarehouseCustomsLineDetailsAddInfoKeys.PortOfLoading);
			declaration.JE_RL_NKPortOfFirstArrival = addInfosDictionary.GetValueSafe(WarehouseCustomsLineDetailsAddInfoKeys.FirstEUArrival);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.JE_CustomsOffice = createDeclarationBizObj.CustomsOffice;
			declaration.JE_RN_NKTransportNationality = Core.Constants.CountryCodes.Germany;
			declaration.ZG_Box18TransportID = UniversalReferenceConstants.TransportIds.LKW;

			using (declaration.SuspendJE_MessageTypeSetting())
			{
				var supplierAddInfoString = addInfosDictionary.GetValueSafe(WarehouseCustomsLineDetailsAddInfoKeys.Supplier);
				var supplierAddress = BondedWarehousingHelper.GetOrgAddressFromBondedWarehouseAttributeAddInfo(Factory, supplierAddInfoString);
				if (supplierAddress != null)
				{
					declaration.SupplierDocumentaryAddress.E2_OA_Address = supplierAddress.PK;
				}

				var importerAddInfoString = addInfosDictionary.GetValueSafe(WarehouseCustomsLineDetailsAddInfoKeys.Importer);
				var importerAddress = BondedWarehousingHelper.GetOrgAddressFromBondedWarehouseAttributeAddInfo(Factory, importerAddInfoString);
				if (importerAddress != null)
				{
					declaration.ImporterDocumentaryAddress.E2_OA_Address = importerAddress.PK;
				}
			}

			if (createDeclarationBizObj.DeclarationType == ImportDeclarationTypeList.Codes.EZA)
			{
				declaration.ZG_MethodOfPayment = MethodOfPaymentTypes.E;
			}

			if (!createDeclarationBizObj.DeclarantsReference.IsEmpty)
			{
				declaration.JE_OwnerRef = declaration.JE_HouseBill = $"{createDeclarationBizObj.DeclarantsReference}_{++houseBillCounter:D2}";
			}
			declaration.WarehouseDocAddress.E2_OA_Address = randomSelectedLine.WarehouseAddress.PK;

			return declaration;
		}

		void CreateAndPopulateEntryInstructions(JobDeclaration declaration, ZGuid fromWareHousePK)
		{
			foreach (var invoiceLinesGroup in declaration.InvoiceLines.Where(x => x.EntryInstruction == null).GroupBy(x => x.InvoiceHeader).ToArray())
			{
				CreateAndPopulateEntryInstruction(declaration, fromWareHousePK, invoiceLinesGroup);
			}
		}

		protected void CreateAndPopulateEntryInstruction(JobDeclaration declaration, ZGuid fromWareHousePK, IEnumerable<BaseJobComInvoiceLine> invoiceLinesGroup)
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = createDeclarationBizObj.DeclarationType;
			entryInstruction.CEI_Procedure = createDeclarationBizObj.CPC;

			if (createDeclarationBizObj.DeclarationType == ImportDeclarationTypeList.Codes.EZA)
			{
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
			}
			else if (createDeclarationBizObj.DeclarationType == ImportDeclarationTypeList.Codes.VZA ||
						createDeclarationBizObj.DeclarationType == ImportDeclarationTypeList.Codes.AZ)
			{
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SimplifiedDeclaration;

				if (createDeclarationBizObj.DeclarationType == ImportDeclarationTypeList.Codes.AZ)
				{
					entryInstruction.CEI_LocalClearanceDate = ZDateTime.Today;
				}
			}

			if (declaration.ZG_IsHighValueOvrd)
			{
				var dv1DetailPivot = entryInstruction.DV1DetailsPivots[0];
				dv1DetailPivot.IsForEntryInstruction = true;
			}

			foreach (var invoiceLine in invoiceLinesGroup)
			{
				invoiceLine.JI_CEI = entryInstruction.PK;
				invoiceLine.JI_Procedure = entryInstruction.CEI_Procedure + invoiceLine.JI_Procedure.SubstringSafe(2, 2);
			}

			entryInstruction.CEI_OA_Warehouse = fromWareHousePK;
		}

		protected void CreateAndPopulateBillsAndPackages(JobDeclaration declaration)
		{
			var bill = declaration.Bills.Cast<Bill>().FirstOrDefault(b => b.CU_BillType == BillTypeList.Codes.HouseBill && b.CU_BillNum == declaration.JE_HouseBill);
			if (bill is null)
			{
				bill = declaration.Bills.AddNew();
				bill.CU_BillType = BillTypeList.Codes.HouseBill;
				bill.CU_BillNum = declaration.JE_HouseBill;
			}

			BasePackage package = declaration.Packages.Cast<Package>().FirstOrDefault(p => p.Bill == bill);
			if (package is null)
			{
				package = declaration.Packages.AddNew();
				package.CW_HouseBill = declaration.Bills[0].CU_BillUniqueCode;
			}

			package.CW_PackType = EU.Business.UniversalReferenceConstants.RefCusCodeUnPackedPackageUnitType.Unpacked;

			var invoiceLinesToLink =
				declaration.InvoiceLines.Where(il => il.JI_LineNo == 1 && il.PackagesPivot.Cast<InvoiceLinePackagePivot>().All(pp => pp.Package != package));
			foreach (var invoiceLine in invoiceLinesToLink)
			{
				invoiceLine.PackagesPivot.AddPivotFor(package);
			}
		}

		protected void PopulateInvoiceNumbersAndDates(JobDeclaration declaration)
		{
			int invoiceCounter = 0;
			foreach (var invoiceHeader in declaration.Invoices)
			{
				invoiceHeader.JZ_InvoiceNumber = $"INV{++invoiceCounter:D2}";
				invoiceHeader.JZ_InvoiceDate = ZDateTime.Today;
			}
		}

		protected override bool IsInventoryValid(IWhsInventoryView inventory)
		{
			string docketSubType = Factory.Load<IWhsDocket>(inventory.WI_WD)?.WD_DocketSubType;
			return docketSubType == "CUS";
		}

		protected readonly CreateDeclarationBizObj createDeclarationBizObj;
		int houseBillCounter;
		readonly bool updateWarehouse;
	}
}
