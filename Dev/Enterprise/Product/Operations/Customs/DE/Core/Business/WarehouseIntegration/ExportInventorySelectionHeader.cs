using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MasterFiles;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Customs.DE.Business
{
	public class ExportInventorySelectionHeader : InventorySelectionHeader
	{
		public ExportInventorySelectionHeader(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override void SetFormattedProcedure(JobComInvoiceLine invoiceLine, IWhsDocketLine receiveLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute)
		{
			invoiceLine.JI_FormattedProcedure = GetInventoryInwardProcedureCode(receiveLine) switch
			{
				CustomsProcedureCodeList.Import.ProcedureCode._51 => CustomsProcedureCodeList.Export.ProcedureCode._31 + CustomsProcedureCodeList.Export.PreviousProcedureCode._51,
				_ => CustomsProcedureCodeList.Export.ProcedureCode._31 + CustomsProcedureCodeList.Export.PreviousProcedureCode._71
			};
		}

		protected override void FillInvoiceLineWithInventoryDetails(EU.Business.Declaration.JobComInvoiceLine invoiceLine, IWhsDocketLine whsReceiveLine,
			IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, ZDecimal ratio, WhsInventoryWrapper inventoryWrapper = null)
		{
			base.FillInvoiceLineWithInventoryDetails(invoiceLine, whsReceiveLine, whsBondedWarehouseAttribute, ratio, inventoryWrapper);

			var deInvoiceLine = invoiceLine as JobComInvoiceLine;
			deInvoiceLine.JI_BondedWhsUnitQty = GetBondedWhsUnitQty(deInvoiceLine, whsReceiveLine);
			CreatePreviousProcedure(deInvoiceLine, whsReceiveLine, whsBondedWarehouseAttribute);
			AppendSerialNumberToGoodDescription(deInvoiceLine, whsReceiveLine);
		}

		protected override BaseJobComInvoiceHeader GetMatchingInvoiceHeaderAndPopulateData(InvoiceHeaderGroupingDefinitionProvider invoiceHeaderGroupingDefinitionProvider)
		{
			var invoiceHeader = Declaration.Invoices.FirstOrDefault(x => x.JZ_RX_NKInvoice_Currency == invoiceHeaderGroupingDefinitionProvider.LinePriceCurrency) ?? Declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = invoiceHeaderGroupingDefinitionProvider.LinePriceCurrency;
			return invoiceHeader;
		}

		protected override void FillCharges(EU.Business.Declaration.JobComInvoiceLine invoiceLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, ZDecimal ratio) { }

		bool IsAssembledProduct(IWhsDocketLine whsReceiveLine) => string.IsNullOrEmpty(whsReceiveLine.CustomsData.WB_EntryKey);

		void CreatePreviousProcedure(JobComInvoiceLine invoiceLine, IWhsDocketLine whsReceiveLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute)
		{
			invoiceLine.PreviousProcedures.RemoveAndDeleteAll();

			if (GetInventoryInwardProcedureCode(whsReceiveLine) == CustomsProcedureCodeList.Import.ProcedureCode._51)
			{
				if (IsAssembledProduct(whsReceiveLine))
				{
					AddPreviousProceduresFromAssembledInventory(PreviousProcedureList.Codes._ATAV);
				}
				else
				{
					AddPreviousProcedure(PreviousProcedureList.Codes._ATAV);
				}
			}
			else
			{
				AddPreviousProcedure(PreviousProcedureList.Codes._ATZL);
			}

			ZString GetTariff()
			{
				var result = ZString.Empty;
				var productPK = whsReceiveLine.WE_OP;
				if (productPK.IsValid)
				{
					var product = Factory.Load<OrgSupplierPart>(productPK);
					if (product != null)
					{
						result = product.GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.Germany).SingleOrDefault(x => x.CI_ChildType == Common.Shared.ClassificationTypeList.Codes.Import)?.CI_FormattedTariffNum ?? ZString.Empty;
					}
				}
				return !result.IsEmpty ? result : whsBondedWarehouseAttribute.WB_Tariff;
			}

			void SetAuthorisationNumber(PreviousDocument previousProcedure)
			{
				var authorizationNumbers = invoiceLine.PreviousProcedureMaster.Lookups.AuthorizationNumberList;
				if (authorizationNumbers.Count == 1)
				{
					previousProcedure.AuthorizationNumber = authorizationNumbers.CodesAsString;
				}
			}

			bool IsAtlasEntry(string procedure, ZString referenceNumber) =>
				procedure switch
				{
					PreviousProcedureList.Codes._ATZL => referenceNumber.IsValidAtlasReferenceForBondedWarehouse(),
					PreviousProcedureList.Codes._ATAV => referenceNumber.IsValidAtlasReferenceForInwardProcessing(),
					_ => false,
				};

			void AddPreviousProcedure(ZString procedure)
			{
				var referenceNumber = invoiceLine.JI_PreviousEntryNumber;
				var tariff = GetTariff();

				var previousProcedure = invoiceLine.PreviousProcedures.AddNew();
				previousProcedure.CSI_ReferenceNumber = referenceNumber;
				previousProcedure.CSI_ReferenceNumber2 = Declaration.JE_DeclarationReference;
				previousProcedure.CSI_LineNo = invoiceLine.JI_PreviousEntryLineNumber;
				previousProcedure.CSI_Tariff = tariff;
				previousProcedure.CSI_Description = !string.IsNullOrEmpty(whsReceiveLine.WE_SerialNumber) ? whsReceiveLine.WE_SerialNumber : tariff;
				previousProcedure.Status = IsAtlasEntry(procedure, previousProcedure.CSI_ReferenceNumber);
				previousProcedure.CSI_Quantity2 = invoiceLine.JI_BondedWhsQuantity;
				previousProcedure.CSI_UnitOfQuantity2 = invoiceLine.JI_BondedWhsUnitQty;
				previousProcedure.CSI_Procedure = procedure;
				SetAuthorisationNumber(previousProcedure);
			}

			void AddPreviousProceduresFromAssembledInventory(ZString procedure)
			{
				var components = GetLowestLevelComponents(whsReceiveLine).Select(x => x.Item1);
				foreach (var component in components.Where(x => x.CustomsData?.WB_EntryKey.IsEmpty == false))
				{
					var referenceNumber = component.CustomsData.WB_EntryKey;

					var previousProcedure = invoiceLine.PreviousProcedures.AddNew();
					previousProcedure.CSI_ReferenceNumber = referenceNumber;
					previousProcedure.CSI_ReferenceNumber2 = Declaration.JE_DeclarationReference;
					previousProcedure.CSI_LineNo = component.CustomsData.WB_EntryLineNo;
					previousProcedure.CSI_Tariff = component.CustomsData.WB_Tariff;
					previousProcedure.CSI_Description = !string.IsNullOrEmpty(component.WE_SerialNumber) ? component.WE_SerialNumber : component.CustomsData.WB_Tariff;
					previousProcedure.Status = IsAtlasEntry(procedure, previousProcedure.CSI_ReferenceNumber);
					previousProcedure.CSI_Quantity2 = invoiceLine.JI_BondedWhsQuantity;
					previousProcedure.CSI_UnitOfQuantity2 = invoiceLine.JI_BondedWhsUnitQty;
					previousProcedure.CSI_Procedure = procedure;
					SetAuthorisationNumber(previousProcedure);
				}
			}
		}

		void AppendSerialNumberToGoodDescription(JobComInvoiceLine invoiceLine, IWhsDocketLine whsReceiveLine)
		{
			if (IsAssembledProduct(whsReceiveLine))
			{
				AddSerialNumberFromAssembledInventory();
			}
			else
			{
				AddSerialNumber();
			}

			void AddSerialNumber()
			{
				if (!string.IsNullOrEmpty(whsReceiveLine.WE_SerialNumber))
				{
					var serialNumberText = Res.GetString("3721C0E7-13CB-4938-8BAE-F3F8F9F5D546", ", SN: {0}", whsReceiveLine.WE_SerialNumber);
					AppendToDescription(serialNumberText);
				}
			}

			void AddSerialNumberFromAssembledInventory()
			{
				var components = GetLowestLevelComponents(whsReceiveLine).Select(x => x.Item1);

				if (!string.IsNullOrEmpty(whsReceiveLine.WE_SerialNumber))
				{
					var vnValueText = Res.GetString("BFFD4661-556E-47EC-A9B5-5A7FC7435F8E", ", VN: {0}", whsReceiveLine.WE_SerialNumber);
					AppendToDescription(vnValueText);
				}

				var serialNumbersList = GetSerialNumbersFromComponents(components);
				var serialNumbers = string.Join(", ", serialNumbersList);

				if (!string.IsNullOrEmpty(serialNumbers))
				{
					var serialNumbersText = Res.GetString("AC5CB478-4DF0-4FD4-9972-EC110F79F27A", ", SN: {0}", serialNumbers);
					AppendToDescription(serialNumbersText);
				}
			}

			IEnumerable<ZString> GetSerialNumbersFromComponents(IEnumerable<IWhsDocketLine> components)
			{
				return components
					.Where(x => x.CustomsData?.WB_EntryKey.IsEmpty == false && !string.IsNullOrEmpty(x.WE_SerialNumber))
					.OrderBy(x => x.WE_SerialNumber)
					.Select(x => x.WE_SerialNumber);
			}

			void AppendToDescription(string appendedText)
			{
				invoiceLine.JI_Description += appendedText;
			}
		}

		string GetInventoryInwardProcedureCode(IWhsDocketLine receiveLine)
		{
			var inwardProcedure = GetProcedureCode(receiveLine.CustomsData.WB_InwardProcedure);

			if (string.IsNullOrEmpty(inwardProcedure))
			{
				var componentInwardProcedures = GetLowestLevelComponents(receiveLine)
					.Select(x => GetProcedureCode(x.Item1.CustomsData.WB_InwardProcedure))
					.Where(x => !string.IsNullOrEmpty(x))
					.ToHashSet();

				if (componentInwardProcedures.Count == 1)
				{
					inwardProcedure = componentInwardProcedures.Single();
				}
			}

			return inwardProcedure;

			string GetProcedureCode(string inwardProcedure) => inwardProcedure.LeftOrNull(2);
		}

		[ChildEditable]
		public new InventorySelectionLineCollection SelectionLines => (InventorySelectionLineCollection)base.SelectionLines;

		protected override IInventorySelectionLineCollection<Customs.Business.InventorySelectionLine> GetNewInventorySelectionLineCollection() => new InventorySelectionLineCollection(this);

		ZString GetBondedWhsUnitQty(JobComInvoiceLine invoiceLine, IWhsDocketLine whsReceiveLine)
		{
			return IsAssembledProduct(whsReceiveLine) ? Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItems : invoiceLine.JI_BondedWhsUnitQty;
		}
	}
}
