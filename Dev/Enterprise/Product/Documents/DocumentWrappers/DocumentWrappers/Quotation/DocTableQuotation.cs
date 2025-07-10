using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.Quotation;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocTableQuotation : DocQuotation
	{
		protected DocTableQuotation(PricingPage page, BusinessObjectFactory factoryToWrap)
			: base(page, factoryToWrap) { }

		public static new DocTableQuotation New(PricingPage page, BusinessObjectFactory factoryToWrap)
		{
			return page == null ? null : new DocTableQuotation(page, factoryToWrap);
		}

		public ZString PageHeading
		{
			get
			{
				ZString result = GetHeader(FirstEntry);

				if (FirstEntry.IsQuote() && !FirstEntry.IsSupplementaryEntry())
				{
					if (IsImport)
					{
						result += " " + Res.GetString("e754ee0f-5a59-4200-a7a1-4c15b4d570cd", "to") + " ";
					}
					else if (IsExport)
					{
						result += " " + Res.GetString("e06b76e4-3ea6-45ea-8a11-b853cd96c00b", "from") + " ";
					}

					if (IsImport || IsExport)
					{
						result += LocalPort;
					}
				}

				return result;
			}
		}

		public ZString MainHeading
		{
			get
			{
				if (FirstEntry != null)
				{
					if (FirstEntry.IsQuote() && FirstEntry.IsCFS())
					{
						return Res.GetString("7b491423-9238-4ec7-80c4-5b5bb4ace57d", "CFS Rates");
					}
					else if (FirstEntry.IsShippingExportDetention() || FirstEntry.IsShippingImportDetention())
					{
						return Res.GetString("eafe8ca3-51c6-4c4e-ad08-ec1e1dc7f06c", "Shipping Detention Rates");
					}
					else if (Header.Header != null && FirstEntry.IsClientRate())
					{
						return Res.GetString("ce5c3874-31ee-4a34-830f-9f63f5dc5830", "{0} Client Rate", Header.Header.OH_FullName);
					}
					else if (FirstEntry.IsCompanyTariff())
					{
						return Res.GetString("fb8d5577-23ed-40b4-b9ab-9d4c70db2d3f", "Level {0} Company Tariff - {1}", Header.TH_GlobalRateLevel, Header.TH_GlobalRateDescriptionMultilingual);
					}
					else if (FirstEntry.IsCosting())
					{
						if (Header.IsStandardCostRate())
						{
							return Res.GetString("ec9f4152-4b62-4693-a06e-bf6bbc88bf3f", "Standard Costing");
						}
						else if (Header.Header != null)
						{
							return Res.GetString("0954a0da-8767-45ab-bcd0-943c98f6c899", "{0} Costing", Header.Header.OH_FullName);
						}
					}
				}

				return ZString.Empty;
			}
		}

		public ZBool IsAirFreight
		{
			get { return FirstEntry != null && FirstEntry.IsAirFreight(); }
		}

		public DocRateEntryCollection Entries
		{
			get
			{
				if (entries == null)
				{
					entries = HasAirLCLMarkup ? new DocAirLCLRateEntryCollection(this, Page.RateEntries, Factory) : new DocRateEntryCollection(this, Page.RateEntries, Factory);

					if (FirstEntry != null)
					{
						bool viewAgentRates = ReportName.IndexOf("AGENT", StringComparison.OrdinalIgnoreCase) >= 0;

						foreach (DocRateEntry entry in entries)
						{
							entry.ViewAgentRates = viewAgentRates;

							if (FirstEntry.IsShipping())
							{
								entry.SetShippingColumnDefinitions();
							}
							else if (FirstEntry.IsAirFreight())
							{
								entry.SetAirColumnDefinitions();
							}
							else
							{
								entry.SetSeaColumnDefinitions();
							}
						}
					}
				}
				return entries;
			}
		}
		DocRateEntryCollection entries;

		public ZBool HasOtherCharges
		{
			get
			{
				foreach (DocRateEntry entry in Entries)
				{
					if (!entry.OtherCharges.IsEmpty)
					{
						return true;
					}
				}

				return false;
			}
		}

		#region Implementation

		internal static ZString GetHeader(RateEntry entry)
		{
			ZString result = "";

			if (entry.IsQuote() && !entry.IsSupplementaryEntry())
			{
				switch (((IImportExport)entry).JobDirection)
				{
					case Directions.Import:
						result += Res.GetString("0bac7069-28eb-4e51-9a10-c71472b71f72", "Import") + " ";
						break;

					case Directions.Export:
						result += Res.GetString("8e51faea-44d6-40a9-925f-b32c690e6063", "Export") + " ";
						break;

					case Directions.CrossTrade:
						result += Res.GetString("7d0ccf3f-aa6d-44b5-824c-4ae9d8a979af", "Cross Trade") + " ";
						break;

					case Directions.Domestic:
						result += Res.GetString("215e3008-6e27-4d8a-8312-662b1247362e", "Domestic") + " ";
						break;
				}
			}

			if (entry.IsShipping())
			{
				result += Res.GetString("da9d978e-c3ed-4b33-9b72-5b3215ecc5db", "Shipping") + " ";
			}
			else if (entry.IsAir())
			{
				result += Res.GetString("45b737aa-b2fc-4252-a64a-11a492cea6d7", "Air") + " ";
			}
			else if (entry.IsSea())
			{
				result += Res.GetString("29acfcd6-2845-41b4-a7c6-81934ef1cf49", "Sea") + " ";
			}
			else if (entry.IsRoad())
			{
				result += Res.GetString("bf7e54b6-498b-44e5-bf57-c4f92b972b9f", "Road") + " ";
			}
			else if (entry.IsRail())
			{
				result += Res.GetString("38faab3b-b67a-45b3-b2a1-5c5869b1b33d", "Rail") + " ";
			}

			result += Res.GetString("989305af-d0e0-4c6a-96ab-22803768391b", "Freight Rates");

			return result;
		}

		protected override bool IsGSTApplicable
		{
			get
			{
				if (IsFreightGSTApplicable)
				{
					return true;
				}

				foreach (DocRateLineItem item in OriginDocRateLineItems)
				{
					if (item.ChargeIsGSTApplicable)
					{
						return true;
					}
				}

				foreach (DocRateLineItem item in DestinationDocRateLineItems)
				{
					if (item.ChargeIsGSTApplicable)
					{
						return true;
					}
				}

				return false;
			}
		}

		protected override bool IsFreightGSTApplicable
		{
			get
			{
				if (FirstEntry.IsFreightEntry())
				{
					AccChargeCode freightChargeCode = Factory.Load<AccChargeCode>(Env.Registry.GetFreightChargeCode(Header.Company.PK.ToGuid()));

					return
						freightChargeCode != null &&
						freightChargeCode.GSTRate != null &&
						freightChargeCode.GSTRate.GetRateRaw(FirstEntry.TI_RateStartDate) > 0;
				}
				else
				{
					return false;
				}
			}
		}

		public ZBool HasAirLCLMarkup
		{
			get { return ContainerList.Count == 0 && IsAirFreight; }
		}

		internal RateEntry FirstLCLEntry
		{
			get
			{
				foreach (DocRateEntry docRateEntry in Entries)
				{
					if (docRateEntry.LCLEntry != null)
					{
						return docRateEntry.LCLEntry;
					}
				}

				foreach (RateEntry entry in Page.RateEntries)
				{
					if (entry.IsSupplementaryEntry() && entry.IsLCL())
					{
						return entry;
					}
				}

				return null;
			}
		}

		internal RateEntry FirstFCLEntry
		{
			get
			{
				foreach (DocRateEntry docRateEntry in Entries)
				{
					if (docRateEntry.FirstContainerisedEntry != null)
					{
						return docRateEntry.FirstContainerisedEntry;
					}
				}

				foreach (RateEntry entry in Page.RateEntries)
				{
					if (entry.IsSupplementaryEntry() && entry.IsFCL())
					{
						return entry;
					}
				}

				return null;
			}
		}

		#endregion
	}
}
