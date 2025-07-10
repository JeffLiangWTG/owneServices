using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.DE.Business.Declaration.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.DE.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.DE.Business
{
	public class BondedWarehousingHelper : EU.Business.BondedWarehousingHelper
	{
		public BondedWarehousingHelper(JobDeclaration declaration) : base(declaration)
		{
		}

		public new static class Constants
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Keys")]
			public static class WarehouseCustomsLineDetailsAddInfoKeys
			{
				public const string LineNetPrice = "LineNetPrice";
				public const string InvoiceNumber = "InvoiceNumber";
				public const string InvoiceDate = "InvoiceDate";
				public const string IncotermCode = "IncotermCode";
				public const string IncotermPlace = "IncotermPlace";
				public const string TransNature = "TransNature";
				public const string Supplier = "Supplier";
				public const string Importer = "Importer";
				public const string Buyer = "Buyer";
				public const string Seller = "Seller";
				public const string PortOfLoading = "PortOfLoading";
				public const string FirstEUArrival = "FirstEUArrival";
				public const string Transport = "Transport";
				public const string CountryOfSupply = "CountryOfSupply";
				public const string LinePrice = "LinePrice";
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Keys")]
			public static class WarehouseCustomsLineAddInfoSupportingInfoKeys
			{
				public const string Type = "Type";
				public const string Reference = "Reference";
				public const string DateOfIssue = "DateOfIssue";
				public const string Available = "Available";
				public const string Quantity = "Quantity";
				public const string UnitofMeasure = "UnitofMeasure";
			}

			public static class InvoiceSupportingDocumentAddInfoTypes
			{
				public const string InvoiceHeader = "SDH";
				public const string InvoiceLine = "SDL";
			}
		}

		public static IWhsBondedWarehouseAttribute GetBondedWarehouseAttributeFromWhsInventoryWrapper(WhsInventoryWrapper wrapper)
		{
			return GetBondedWarehouseAttributeFromWhsDocketLine(wrapper.ReceiveLine, wrapper.Factory);
		}

		public static IWhsBondedWarehouseAttribute GetBondedWarehouseAttributeFromWhsDocketLine(IWhsDocketLine receiveLine, BusinessObjectFactory factory)
		{
			var attributeQuery = new ZQuery(WhsBondedWarehouseAttributeSchema.WB_ParentID, receiveLine?.PK ?? ZGuid.Empty);
			attributeQuery.AddToFilter(WhsBondedWarehouseAttributeSchema.WB_ParentTableCode, WhsDocketLineSchema.Constants.Prefix);
			return factory.LoadTop1<IWhsBondedWarehouseAttribute>(attributeQuery);
		}

		public static OrgAddress GetOrgAddressFromBondedWarehouseAttributeAddInfo(BusinessObjectFactory factory, ZString addInfoString)
		{
			OrgAddress result = null;
			var splittedAddInfoString = addInfoString.Split(";");
			if (splittedAddInfoString.Length >= 2)
			{
				var orgHeaderCode = splittedAddInfoString[0];
				var orgAddressCode = splittedAddInfoString[1];
				result = factory.GetOrgAddressFromOrgHeaderCodeAndAddressCode(orgHeaderCode, orgAddressCode);
			}
			return result;
		}

		protected override bool IsMarkedForBondedWarehousingCore(BaseJobComInvoiceLine invoiceLine)
		{
			var result = base.IsMarkedForBondedWarehousingCore(invoiceLine);
			if (result && invoiceLine.CusEntryLine is CusEntryLine cusEntryLine && (cusEntryLine.RandomLine.IsImport || cusEntryLine.RandomLine.IsWarehouseAdjustment))
			{
				result = !CustomsStatusAttributeHelper.ShouldCancelBondedWhs(invoiceLine.Factory, cusEntryLine.ZG_CustomsStatus, Core.Constants.CountryCodes.Germany, ZDateTime.UtcToday);
			}

			return result;
		}

		public static ZString PublishShipmentForWHSOutward(JobDeclaration declaration, bool publishAcceptEvent = false)
		{
			UpdateInvoiceLineWhsOrderDetails(declaration);

			var result = declaration.PublishShipmentForWHSOutward();
			if (result.ResultType != UniversalResult.HadErrors && publishAcceptEvent)
			{
				result = declaration.PublishAcceptEventForWHSOutwardAndSaveIfNeeded();
			}

			if (result.ResultType == UniversalResult.HadErrors)
			{
				return result.ErrorMessage;
			}
			return ZString.Empty;
		}

		static void UpdateInvoiceLineWhsOrderDetails(JobDeclaration declaration)
		{
			for (var i = 0; i < declaration.InvoiceLines.Count; i++)
			{
				var invoiceLine = declaration.InvoiceLines[i];
				invoiceLine.JI_BondedWHSOrderLineNumber = (short)(i + 1);
				invoiceLine.JI_BondedWHSOrderNumber = declaration.JE_DeclarationReference.Left(invoiceLine.JI_BondedWHSOrderNumberInfo.MaxLength);
			}
		}

		public static void SetWarehouseTransactionStatusForImportMessageProcessing(CusEntryHeader entry)
		{
			if (entry.IsIntoWarehouseWarehousing)
			{
				if (entry.CH_WarehouseTransactionStatus.IsEmpty)
				{
					entry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreatedPending;
				}
				else if (entry.CH_WarehouseTransactionStatus == WarehouseTransactionStatusList.Codes.InwardUpdated)
				{
					entry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardUpdatedPending;
				}
			}
		}
	}
}
