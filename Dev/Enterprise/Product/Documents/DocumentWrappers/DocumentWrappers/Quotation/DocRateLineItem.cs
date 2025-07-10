using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration.RollUpSort;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Environment.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.Quotation
{
	public class DocRateLineItem : DocBaseWrapper
		, ISortableDocLine
	{
		#region Constructors

		protected DocRateLineItem(QuotationLine quotationLine, DocQuotation quotation, BusinessObjectFactory factoryToWrap)
			: base(quotationLine, factoryToWrap)
		{
			this.Quotation = quotation;
		}

		public static DocRateLineItem New(QuotationLine quotationLine, DocQuotation quotation, BusinessObjectFactory factoryToWrap)
		{
			return quotationLine != null ? new DocRateLineItem(quotationLine, quotation, factoryToWrap) : null;
		}

		public static DocRateLineItem New(QuotationLine quotationLine, BusinessObjectFactory factoryToWrap)
		{
			return quotationLine != null ? new DocRateLineItem(quotationLine, null, factoryToWrap) : null;
		}

		#endregion

		public QuotationLine QuotationLine
		{
			get { return (QuotationLine)WrappedObject; }
		}

		protected readonly DocQuotation Quotation;

		public override string ToString()
		{
			return "";
		}

		public DocChargeCode ChargeCode
		{
			get { return DocChargeCode.New(QuotationLine.Master.ChargeCode, Factory); }
		}

		public ZString Description
		{
			get { return QuotationLine.GetDescription(IncoTerm); }
		}

		public ZString Currency
		{
			get { return GetCurrencyCore(); }
		}

		public ZInt DecimalPlaces => QuotationLine.DecimalPlaces;

		public ZString Amount
		{
			get { return QuotationLine.Amount; }
		}

		public ZBool ChargeIsGSTApplicable
		{
			get { return QuotationLine.Master.MayGSTBeApplicable(IncoTerm); }
		}

		public ZString Units
		{
			get { return QuotationLine.Unit; }
		}

		public ZString Mode
		{
			get { return QuotationLine.Mode; }
		}

		public ZString Origin
		{
			get { return ItemOrigin(QuotationLine.Master); }
		}

		public ZString Destination
		{
			get { return ItemDestination(QuotationLine.Master); }
		}

		public ZString Via
		{
			get { return ItemVia(QuotationLine.Master); }
		}

		public ZString LocalChargesHeading
		{
			get
			{
				ZString result;

				if (Quotation.IsCrossTrade || Quotation.IsDomestic || Quotation.IsSupplementary)
				{
					if (QuotationLine.Master.Parent.IsDestinationEntry())
					{
						result = Res.GetString("860180fb-7501-41ea-aa94-06e2affbfd01", "Destination");
					}
					else
					{
						result = Res.GetString("8e0cc493-c5e9-41a1-9cea-c66345ec1b21", "Origin");
					}
				}
				else
				{
					result = Res.GetString("7764a9bb-4791-43f8-ade1-f3e3a82fad76", "Local");
				}

				return Res.GetString("a8a17306-5a62-4c3f-a183-73a5e926e0c8", "{0} Charges", result) + ChargesHeading;
			}
		}

		public ZString OverseasChargesHeading
		{
			get
			{
				ZString result;

				if (Quotation.IsCrossTrade || Quotation.IsDomestic || Quotation.IsSupplementary)
				{
					if (QuotationLine.Master.Parent.IsOriginEntry())
					{
						result = Res.GetString("8e0cc493-c5e9-41a1-9cea-c66345ec1b21", "Origin");
					}
					else
					{
						result = Res.GetString("860180fb-7501-41ea-aa94-06e2affbfd01", "Destination");
					}
				}
				else
				{
					result = Res.GetString("5e9cc21a-2015-4753-b1e8-83fd88fb4a56", "Overseas");
				}

				return Res.GetString("a8a17306-5a62-4c3f-a183-73a5e926e0c8", "{0} Charges", result) + ChargesHeading;
			}
		}

		public ZString OriginChargesHeading
		{
			get
			{
				if (QuotationLine.Master.Parent.IsCFS())
				{
					return Res.GetString("135bd391-9873-4ace-beb7-7492386a3e86", "Packing Charges") + ChargesHeading;
				}
				else if (QuotationLine.Master.Parent.IsShippingExportDetention() || QuotationLine.Master.Parent.IsShippingImportDetention())
				{
					return Res.GetString("fd21f881-8e42-4b0f-8b18-7cc42a5acf10", "Export Detention Charges") + ChargesHeading;
				}
				else
				{
					return Res.GetString("b9258163-223c-412a-a8a1-5879ccacc7bb", "Origin Charges") + ChargesHeading;
				}
			}
		}

		public ZString DestinationChargesHeading
		{
			get
			{
				if (QuotationLine.Master.Parent.IsCFS())
				{
					return Res.GetString("95568a0f-aae8-412f-807a-81ae2c65a68f", "Unpacking Charges") + ChargesHeading;
				}
				else if (QuotationLine.Master.Parent.IsShippingExportDetention() || QuotationLine.Master.Parent.IsShippingImportDetention())
				{
					return Res.GetString("4c68dcbe-b6df-4d63-abd6-ea4287b27034", "Import Detention Charges") + ChargesHeading;
				}
				else
				{
					return Res.GetString("7f2e7aa2-bc2b-4ec4-a53f-2ee8fc208f01", "Destination Charges") + ChargesHeading;
				}
			}
		}

		public ZBool IsDefaultFreight
		{
			get
			{
				return QuotationLine.Master.TL_AC == Env.Registry.GetFreightChargeCode(QuotationLine.Master.Parent.Company().PK.ToGuid()) &&
					   (QuotationLine.Master.TL_RateCalculator == UnitCalculator.Code || QuotationLine.Master.TL_RateCalculator == MinimumOrPerUnitCalculator.Code);
			}
		}

		public ZString ServiceLevel
		{
			get
			{
				RateEntry entry = QuotationLine.Master.Parent;
				if (entry != null)
				{
					if (entry.IsCosting())
					{
						return entry.CarrierServiceLevel != null ? entry.CarrierServiceLevel.PL_CarrierServiceLevelDescriptionMultilingual : ZString.Empty;
					}
					else
					{
						return entry.ServiceLevel_NI != null ? entry.ServiceLevel_NI.RS_DescriptionMultilingual : ZString.Empty;
					}
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZString CommodityCode
		{
			get
			{
				RateEntry entry = QuotationLine.Master.Parent;
				if (entry != null && entry.CommodityCode != null)
				{
					return entry.CommodityCode.RH_DescriptionMultilingual;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZString ServiceLevelCommodityCode
		{
			get { return ServiceLevel + " / " + CommodityCode; }
		}

		public ZString Validity
		{
			get { return Quotation.IsQuote ? ZString.Empty : QuotationLine.Validity; }
		}

		public ZString TransitTime
		{
			get
			{
				switch (QuotationLine.Master.Parent.TI_TransitTime)
				{
					case RatingConstants.TransitTimes.Overnight:
						return Res.GetString("ff1ac6c0-b3da-4a8b-9135-fbf5876cf99f", "Overnight");

					case RatingConstants.TransitTimes.SameDay:
						return Res.GetString("97d48a48-5085-4ed0-b01f-4dbcbfe00882", "Same Day");

					case "1":
						return Res.GetString("54330a10-18e2-4360-8a86-58e504170b34", "1 Day");

					case "":
						return "";

					default:
						return Res.GetString("daec37dd-cd22-41ea-b7fa-5e4d72ee67a9", "{0} Days", QuotationLine.Master.Parent.TI_TransitTime);
				}
			}
		}

		public ZString Warehouse
		{
			get
			{
				RateEntry entry = QuotationLine.Master.Parent;
				if (entry != null)
				{
					if (!entry.TI_WW_Warehouse.IsEmpty)
					{
						var whs = Factory.Load<WhsWarehouse>(entry.TI_WW_Warehouse);
						if (whs != null)
						{
							return whs.WW_WarehouseNameMultilingual;
						}
					}
					else
					{
						return Res.GetString("3e2baf96-00e4-4609-b388-15baa42505c7", "All Warehouses");
					}
				}

				return ZString.Empty;
			}
		}

		public ZString Product
		{
			get
			{
				OrgSupplierPart part = QuotationLine.Master.ProductNumber;
				if (part != null)
				{
					return part.OP_PartNum + " (" + part.OP_Desc + ")";
				}

				return ZString.Empty;
			}
		}

		public ZInt NumOfIndents
		{
			get
			{
				return QuotationLine.NumberOfTabs;
			}
		}

		#region ISortableDocLine Members

		int ISortableDocLine.OrgLevelSortOrder
		{
			get
			{
				int result = 0;
				if (Quotation != null && Quotation.Client != null && Quotation.Client.InvoiceOrders != null)
				{
					foreach (AccClientInvoiceOrder invoiceOrder in Quotation.Client.InvoiceOrders)
					{
						if ((invoiceOrder.AI_InvoiceType == AccClientInvoiceOrderLookups.InvoiceTypes.All.Code || invoiceOrder.AI_InvoiceType.IsEmpty) && ChargeCode != null && invoiceOrder.AI_AC == ChargeCode.ChargeCodePK)
						{
							result = invoiceOrder.AI_PrintOrder;
							break;
						}
					}
				}
				return result;
			}
		}

		int ISortableDocLine.ChargePrintSeqSortOrder
		{
			get { return ChargeCode != null ? (int)ChargeCode.Sequence : 0; }
		}

		int ISortableDocLine.UserEnteredSortOrder
		{
			get { return 0; }
		}

		string ISortableDocLine.AlphabeticalSortOrder
		{
			get { return ChargeCode != null ? ChargeCode.Code : ZString.Empty; }
		}

		#endregion

		#region Implementation

		protected virtual ZString GetCurrencyCore()
		{
			return QuotationLine.Currency;
		}

		ZString IncoTerm
		{
			get
			{
				DocEntryQuotation entryQuotation = Quotation as DocEntryQuotation;

				if (entryQuotation != null && !entryQuotation.IncoTerm.IsEmpty)
				{
					return entryQuotation.IncoTerm;
				}
				else
				{
					return Res.GetString("afe2e121-5da2-416d-ab93-5109c268ddf3", "ALL");
				}
			}
		}

		ZString ChargesHeading
		{
			get
			{
				ZString result = ZString.Empty;

				RateEntry entry = QuotationLine.Master.Parent;
				if (entry != null && entry.IsSupplementaryEntry())
				{
					if (entry.TI_IsCrossTrade && entry.Destination() == null && entry.Origin() == null)
					{
						result += " " + Res.GetString("a26fadc3-c650-4121-90b1-898ede8ac5e0", "- Cross Trade");
					}
					else
					{
						if (entry.IsDestinationEntry())
						{
							if (entry.Destination() != null)
							{
								result += " - " + entry.Destination().Description;
							}

							if (entry.Origin() != null)
							{
								result += " " + Res.GetString("f421b20f-6808-445f-9c8f-64a267697d48", "from {0}", entry.Origin().Description);
							}
						}
						else if (entry.IsOriginEntry())
						{
							if (entry.Origin() != null)
							{
								result += " - " + entry.Origin().Description;
							}

							if (entry.Destination() != null)
							{
								result += " " + Res.GetString("e49dc672-58fa-4675-8952-a47b7fd0d88e", "to {0}", entry.Destination().Description);
							}
						}
					}

					if (entry.TransportProvider != null)
					{
						result += " (" + entry.TransportProvider.OH_FullName + ")";
					}
				}

				return result;
			}
		}

		ZString ItemOrigin(RateLine line)
		{
			DocEntryQuotation entryQuotation = Quotation as DocEntryQuotation;

			if (entryQuotation != null)
			{
				return line.ItemOrigin(entryQuotation.OriginObj);
			}
			else
			{
				return ZString.Empty;
			}
		}

		ZString ItemDestination(RateLine line)
		{
			DocEntryQuotation entryQuotation = Quotation as DocEntryQuotation;

			if (entryQuotation != null)
			{
				return line.ItemDestination(entryQuotation.DestinationObj);
			}
			else
			{
				return ZString.Empty;
			}
		}

		ZString ItemVia(RateLine line)
		{
			DocEntryQuotation entryQuotation = Quotation as DocEntryQuotation;

			if (entryQuotation != null)
			{
				return line.ItemVia(entryQuotation.ViaObj);
			}
			else
			{
				return ZString.Empty;
			}
		}

		#endregion
	}
}
