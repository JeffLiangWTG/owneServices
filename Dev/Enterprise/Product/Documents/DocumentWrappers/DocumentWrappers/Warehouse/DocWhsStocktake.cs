using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsStocktake : DocBaseWrapper
	{
		#region Constructors

		protected DocWhsStocktake(WhsStocktake stocktake, BusinessObjectFactory factoryToWrap)
			: base(stocktake, factoryToWrap)
		{
		}

		#endregion

		#region Static

		public static DocWhsStocktake New(WhsStocktake stocktake, BusinessObjectFactory factoryToWrap)
		{
			return (stocktake == null) ? null : new DocWhsStocktake(stocktake, factoryToWrap);
		}

		#endregion

		#region Related Business Objects

		WhsStocktakeLineCollection stocktakeLines;
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

		DocWhsStocktakeLineCollection lines;
		public DocWhsStocktakeLineCollection Lines
		{
			get
			{
				if (lines == null)
				{
					StocktakeLines.ApplySort(new SortStocktakeLines());
					lines = new DocWhsStocktakeLineCollection(Factory);

					foreach (WhsStocktakeLine stocktakeLine in StocktakeLines)
					{
						if (!stocktakeLine.IsClosed)
						{
							lines.Add(DocWhsStocktakeLine.New(stocktakeLine, Factory));
						}
					}
				}
				return lines;
			}
		}

		DocWhsStocktakeLineCollection varianceLines;
		public DocWhsStocktakeLineCollection VarianceLines
		{
			get
			{
				if (varianceLines == null)
				{
					varianceLines = new DocWhsStocktakeLineCollection(Factory);

					if (DocumentName.ToUpper().Contains("LOCATION"))
					{
						StocktakeLines.ApplySort(new SortStocktakeLines());
						foreach (WhsStocktakeLine stocktakeLine in StocktakeLines)
						{
							if (stocktakeLine.WU_SystemUnits != stocktakeLine.CurrentCount)
							{
								varianceLines.Add(DocWhsStocktakeLine.New(stocktakeLine, Factory));
							}
						}
					}
					else
					{
						var tempFactory = new BusinessObjectFactory();
						var tempStocktake = tempFactory.New<WhsStocktake>();
						WhsStocktakeLineCollection stocktakeVarianceLines;

						stocktakeVarianceLines = new WhsStocktakeLineCollection(tempStocktake);
						foreach (WhsStocktakeLine stocktakeLine in StocktakeLines)
						{
							if (stocktakeLine.WU_SystemUnits != stocktakeLine.CurrentCount)
							{
								var fl = false;
								foreach (WhsStocktakeLine varianceLine in stocktakeVarianceLines)
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
						foreach (WhsStocktakeLine stocktakeLine in stocktakeVarianceLines)
						{
							if (stocktakeLine.WU_SystemUnits != stocktakeLine.CurrentCount)
							{
								varianceLines.Add(DocWhsStocktakeLine.New(stocktakeLine, Factory));
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

		public ZString StocktakeNumber
		{
			get { return Stocktake.WS_StocktakeNumber; }
		}

		public MultilingualString WarehouseName
		{
			get { return (Stocktake.Warehouse != null) ? Stocktake.Warehouse.WW_WarehouseNameMultilingual : (NoResString)ZString.Empty; }
		}

		public MultilingualString WarehouseNameAndAddress
		{
			get
			{
				if (Stocktake.Warehouse != null)
				{
					MultilingualString result = Stocktake.Warehouse.WW_WarehouseNameMultilingual;

					if (Stocktake.Warehouse.WarehouseAddress != null)
					{
						DocAddress addressWrapper = DocAddress.New(Stocktake.Warehouse.WarehouseAddress, Factory);

						result = MultilingualString.Join(System.Environment.NewLine, result, (NoResString)addressWrapper.PostalAddress);
					}

					return result;
				}
				else
				{
					return (NoResString)ZString.Empty;
				}
			}
		}

		public ZString ClientNameAndAddress
		{
			get
			{
				ZString result = ZString.Empty;
				if (Stocktake.ClientAddress != null)
				{
					DocAddress addressWrapper = DocAddress.New(Stocktake.ClientAddress, Factory);
					result += addressWrapper.PostalAddress;
				}
				return result;
			}
		}

		public ZString SelectedClient
		{
			get { return (Stocktake.Client != null) ? Stocktake.Client.OH_FullName : ZString.Empty; }
		}

		public ZString SelectedCycle
		{
			get { return Stocktake.WS_StocktakeCycle; }
		}

		public ZString SelectedSupplierPart
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

		public ZString SelectedCommodityCode
		{
			get { return Stocktake.WS_RH_NKCommodityCode; }
		}

		public ZString SelectedPickMethod
		{
			get { return Stocktake.WS_PickMethod; }
		}

		public ZString SelectedRow
		{
			get { return (Stocktake.SelectedRow != null) ? Stocktake.SelectedRow.WR_Name : ZString.Empty; }
		}

		public MultilingualString SelectedArea
		{
			get { return (Stocktake.SelectedArea != null) ? Stocktake.SelectedArea.WA_NameMultilingual : (NoResString)ZString.Empty; }
		}

		public ZString DocumentName
		{
			get { return MenuTitle; }
		}

		#endregion

		#region Implementation

		WhsStocktake Stocktake
		{
			get { return (WhsStocktake)WrappedObject; }
		}

		#endregion
	}
}
