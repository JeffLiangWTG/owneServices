using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Warehouse.ChildWrappers;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers.Warehouse
{
	public class WarehouseStocktakeWrapper : WarehouseJobGenericWrapper
	{
		#region Constructors

		public WarehouseStocktakeWrapper(WhsStocktake stocktake, BusinessObjectFactory factoryToWrap)
			: base(stocktake, factoryToWrap)
		{
		}

		#endregion

		#region Static

		public static WarehouseStocktakeWrapper New(WhsStocktake stocktake, BusinessObjectFactory factoryToWrap)
		{
			return (stocktake == null) ? null : new WarehouseStocktakeWrapper(stocktake, factoryToWrap);
		}

		#endregion

		#region Related Business Objects

#if DEBUG
		public
#endif
		WhsStocktakeLineCollection StocktakeLines
		{
			get
			{
				if (stocktakeLines == null)
				{
					stocktakeLines = Stocktake.StocktakeLinesForFilter;
				}
				return stocktakeLines;
			}
		}

		WhsStocktakeLineCollection stocktakeLines;

		WarehouseStocktakeLineWrapperCollection Lines
		{
			get
			{
				StocktakeLines.ApplySort(new SortStocktakeLines());
				var lines = new WarehouseStocktakeLineWrapperCollection(Factory);

				foreach (WhsStocktakeLine stocktakeLine in StocktakeLines)
				{
					if (!stocktakeLine.IsClosed)
					{
						lines.Add(new WarehouseStocktakeLineWrapper(stocktakeLine, Factory));
					}
				}
				return lines;
			}
		}

		protected override WarehouseGenericWrapperCollection GetJobLines()
		{
			return Lines;
		}

		protected override WarehouseStocktakeLineWrapperCollection VarianceLinesCore
		{
			get
			{
				if (varianceLines == null)
				{
					varianceLines = new WarehouseStocktakeLineWrapperCollection(Factory);

					if (DocumentTitle.ToUpper().Contains("LOCATION"))
					{
						StocktakeLines.ApplySort(new SortStocktakeLines());
						foreach (WhsStocktakeLine stocktakeLine in StocktakeLines)
						{
							if (stocktakeLine.WU_SystemUnits != stocktakeLine.CurrentCount)
							{
								varianceLines.Add(new WarehouseStocktakeLineWrapper(stocktakeLine, Factory));
							}
						}
					}
					else
					{
						var tempFactory = new BusinessObjectFactory();
						var tempStocktake = tempFactory.New<WhsStocktake>();
						WhsStocktakeLineCollection stocktakeVarianceLines;

						stocktakeVarianceLines = new WhsStocktakeLineCollection(tempStocktake);
						foreach (var stocktakeLine in StocktakeLines.Where(l => l.WU_Status != "EMP"))
						{
							if (stocktakeLine.WU_SystemUnits != stocktakeLine.CurrentCount)
							{
								var fl = false;
								foreach (var varianceLine in stocktakeVarianceLines)
								{
									if (stocktakeLine.WU_OP == varianceLine.WU_OP &&
										stocktakeLine.WU_PartAttrib1 == varianceLine.WU_PartAttrib1 &&
										stocktakeLine.WU_PartAttrib2 == varianceLine.WU_PartAttrib2 &&
										stocktakeLine.WU_PartAttrib3 == varianceLine.WU_PartAttrib3 &&
										stocktakeLine.WU_SerialNumber == varianceLine.WU_SerialNumber &&
										stocktakeLine.WU_Status == varianceLine.WU_Status)
									{
										varianceLine.WU_SystemUnits += stocktakeLine.WU_SystemUnits;
										SetCountValue(varianceLine, varianceLine.WU_TotalCounts, stocktakeLine.CurrentCount);
										fl = true;
									}
								}

								if (!fl)
								{
									var clonedLine = stocktakeVarianceLines.AddNew();

									clonedLine.SuspendValidation();

									clonedLine.WU_OP = stocktakeLine.WU_OP;
									clonedLine.WU_OH_Client = stocktakeLine.WU_OH_Client;
									clonedLine.WU_PartAttrib1 = stocktakeLine.WU_PartAttrib1;
									clonedLine.WU_PartAttrib2 = stocktakeLine.WU_PartAttrib2;
									clonedLine.WU_PartAttrib3 = stocktakeLine.WU_PartAttrib3;
									clonedLine.WU_SerialNumber = stocktakeLine.WU_SerialNumber;
									clonedLine.WU_ExpiryDate = stocktakeLine.WU_ExpiryDate;
									clonedLine.WU_PackingDate = stocktakeLine.WU_PackingDate;
									clonedLine.WU_TotalCounts = stocktakeLine.WU_TotalCounts;
									clonedLine.CurrentCount = stocktakeLine.CurrentCount;
									clonedLine.WU_Status = stocktakeLine.WU_Status;
									clonedLine.WU_SystemUnits = stocktakeLine.WU_SystemUnits;
								}
							}
						}
						stocktakeVarianceLines.ApplySort(new SortByProduct());
						foreach (var stocktakeLine in stocktakeVarianceLines)
						{
							if (stocktakeLine.WU_SystemUnits != stocktakeLine.CurrentCount)
							{
								varianceLines.Add(new WarehouseStocktakeLineWrapper(stocktakeLine, Factory));
							}
						}
					}
				}
				return varianceLines;
			}
		}

		void SetCountValue(WhsStocktakeLine line, ZByte countColumnNumber, ZDecimal countValue)
		{
			line.WU_TotalCounts = countColumnNumber;
			switch (countColumnNumber)
			{
				case 1:
					line.WU_LastCount += countValue;
					break;
				case 2:
					line.WU_Count2 += countValue;
					break;
				case 3:
					line.WU_Count3 += countValue;
					break;
				default:
					throw new NotImplementedException("Count column number not implemented.");
			}
		}

		#endregion

		#region Properties

		protected override ZString SecondaryHeadingCore => Res.GetString("a0f06e44-42d0-4a37-a3eb-e764a676fb5c", "Stocktake Line Details");

		protected override ZString StocktakeNumberCore => Stocktake.WS_StocktakeNumber;

		protected override WarehouseBOWrapper WarehouseCore
		{
			get
			{
				if (warehouseWrapper == null && Stocktake != null)
				{
					var warehouse = Stocktake.Warehouse;
					var warehouseTitle = Res.GetString("a1f18bcb-07e4-4b77-a16d-9b486ee59a15", "Warehouse");

					warehouseWrapper = warehouse != null
							? Factory.GetCachedValue(warehouse.PK.ToString(), () => new WarehouseBOWrapper(warehouseTitle, warehouse, Factory))
							: new WarehouseBOWrapper(warehouseTitle, warehouse, Factory);
				}
				return warehouseWrapper;
			}
		}
		WarehouseBOWrapper warehouseWrapper;

		protected override ZString SelectedClientCore => (Stocktake.Client != null) ? Stocktake.Client.OH_FullName : ZString.Empty;

		protected override ZString SelectedCycleCore => Stocktake.WS_StocktakeCycle;

		protected override ZString SelectedSupplierPartCore
		{
			get
			{
				var result = ZString.Empty;
				if (Stocktake.ProductFilterCollection.Count == 1)
				{
					result = Stocktake.ProductFilterCollection.Single().ProductCode;
				}
				else if (Stocktake.ProductFilterCollection.Count > 1)
				{
					result = Res.GetString("a7203239-5031-4341-ac45-5a4f1495e411", "Many");
				}
				return result;
			}
		}

		protected override ZString SelectedCommodityCodeCore => Stocktake.WS_RH_NKCommodityCode;

		protected override ZString SelectedPickMethodCore => Stocktake.WS_PickMethod;

		protected override ZString SelectedRowCore => (Stocktake.SelectedRow != null) ? Stocktake.SelectedRow.WR_Name : ZString.Empty;

		protected override MultilingualString SelectedAreaCore => (Stocktake.SelectedArea != null) ? Stocktake.SelectedArea.WA_NameMultilingual : (NoResString)ZString.Empty;

		protected override ZString DocumentTitleCore => MenuTitle;

		public override ZString PrimaryBarcodeText
		{
			get
			{
				var barcode = new TextBarcode(StocktakeNumber);
				return barcode.TextAs128sFontString;
			}
		}

		protected override ZString SelectedABCCategoryCore => Stocktake.WS_ABCAnalysisCategory;

		protected override ZString SelectedLocationCore => Stocktake.LocationString;

		protected override ZString SelectedStocktakeTypeCore => Stocktake.WS_StocktakeType;

		protected override ZString JobNumberCore => Stocktake != null ? Stocktake.WS_StocktakeNumber : ZString.Empty;

		#endregion

		#region Implementation

		WhsStocktake Stocktake => (WhsStocktake)WrappedObject;

		#endregion
	}
}
