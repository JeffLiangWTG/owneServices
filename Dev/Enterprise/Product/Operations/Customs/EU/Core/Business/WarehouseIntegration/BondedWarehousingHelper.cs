using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business
{
	public class BondedWarehousingHelper : Customs.Business.BondedWarehousingHelper
	{
		public BondedWarehousingHelper(JobDeclaration declaration) : base(declaration)
		{
		}

		protected new JobDeclaration declaration => (JobDeclaration)base.declaration;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public new static class Constants
		{
			public const string LinePrice = "LinePrice";
			public const string LinePriceCurrency = "LinePriceCurrency";
			public const string CountryOfSupply = "CountryOfSupply";
			public const string ValuationCode = "ValuationCode";
			public const string EstimatedDutyBreakdown = "EstimatedDutyBreakdown";
			public const string EstimatedVATBreakdown = "EstimatedVATBreakdown";
			public const string EstimatedOtherTaxesBreakdown = "EstimatedOtherTaxesBreakdown";
			public const string LineGrossWeight = "LineGrossWeight";
			public const string LineGrossWeightUnit = "LineGrossWeightUnit";
			public const string EntryStatus = "ENTRYSTATUS";
			public const string EntryStatusDescription = "ENTRYSTATUSDESCRIPTION";
			public const string Authorization = "AUTHORIZATION";
			public const string PreviousDocCode = "PREVIOUSDOCCODE";
			public const string PreviousDocReference = "PREVIOUSDOCREFERENCE";
			public const string GuaranteedAmount = "GUARANTEEDAMOUNT";
			public const string CountryOfDestination = "COUNTRYOFDESTINATION";
			public const string ExitDate = "EXITDATE";

			public static class SupplementaryCodeAddInfo
			{
				public const string Type = "SUP";

				public static class AddInfoKeys
				{
					public const string Code = "Code";
					public const string Order = "Order";
				}
			}

			public static class CommercialChargeAddInfo
			{
				public const string Type = "CCT";
				public static class AddInfoKeys
				{
					public const string ChargeType = "ChargeType";
					public const string Currency = "Currency";
					public const string Amount = "Amount";
					public const string IsDutiable = "IsDutiable";
					public const string IsGSTApplicable = "IsGSTApplicable";
					public const string IsIncludedInITOT = "IsIncludedInITOT";
					public const string IsStatisticalValueApplicable = "IsStatisticalValueApplicable";
				}
			}

			public static class PreviousDocumentAddInfo
			{
				public const string Type = "PRE";
				public static class AddInfoKeys
				{
					public const string Type = "Type";
					public const string Reference = "Reference";
				}
			}

			public static class EntryInstructionAuthorisationAddInfo
			{
				public const string Type = "AUT";
				public static class AddInfoKeys
				{
					public const string Code = "Code";
					public const string Number = "Number";
				}
			}
		}

		public static IWhsOrderCollection GetCollectionForWhsOrderSelection(BusinessObjectFactory factory)
		{
			var collection = ObjectFactory.New<IWhsOrderCollection>(factory);
			var filters = ((IFilterBusinessObjectDefaultsProvider)collection).FilterBusinessObjectDefaults;
			filters.Add(new FilterBusinessObjectDefault("Order Status", "Property", (ZString)"DEP", isRemovable: false));

			var orgProxy = GlbBranch.CurrentBranch.OrgProxy;
			if (orgProxy != null)
			{
				var warehouse = orgProxy.MainAddress.GetWhsWarehouse();
				if (warehouse != null)
				{
					filters.Add(new FilterBusinessObjectDefault("Warehouse", "Property", warehouse.PK));
				}
			}

			return collection;
		}

		public static Dictionary<IWhsDocketLine, IEnumerable<IWhsPickLine>> GetPickLinesForOrderLines(BusinessObjectFactory factory, IWhsOrder order)
		{
			var orderLineToPickLines = new Dictionary<IWhsDocketLine, IEnumerable<IWhsPickLine>>();
			var orderLineQuery = new ZDBOnlyQuery(typeof(IWhsDocketLine));
			orderLineQuery.AddToFilter(WhsDocketLineSchema.WE_WD, order.PK);

			var orderLines = factory.Load<IWhsDocketLine>(orderLineQuery);
			foreach (var orderLine in orderLines)
			{
				var pickLineQuery = new ZDBOnlyQuery(typeof(IWhsPickLine));
				pickLineQuery.AddToFilter(WhsPickLineSchema.WZ_WE_TransactionLine, orderLine.PK);

				var orderPickLines = factory.Load<IWhsPickLine>(pickLineQuery);

				orderLineToPickLines.Add(orderLine, orderPickLines);
			}
			return orderLineToPickLines;
		}

		public static IWhsReceiveLine GetReceiveLineForPickLine(BusinessObjectFactory factory, IWhsPickLine pickLine)
		{
			var receiveLineQuery = new ZDBOnlyQuery(typeof(IWhsReceiveLine));
			var inventoryPk = pickLine.WZ_WE_OriginalPickedInventoryLine.IsEmpty ? pickLine.WZ_WE_InventoryLine : pickLine.WZ_WE_OriginalPickedInventoryLine;
			receiveLineQuery.AddToFilter(WhsDocketLineSchema.PK, inventoryPk);

			var receiveLine = factory.LoadTop1<IWhsDocketLine>(receiveLineQuery);

			return receiveLine as IWhsReceiveLine;
		}
	}
}
