using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.Quotation;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RatingEnums;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocRateEntry : DocBaseWrapper
	{
		protected DocRateEntry(RateEntry entry, BusinessObjectFactory factoryToWrap)
			: base(entry, factoryToWrap) { }

		public static DocRateEntry New(RateEntry entry, BusinessObjectFactory factoryToWrap)
		{
			return entry == null ? null : new DocRateEntry(entry, factoryToWrap);
		}

		public override string ToString()
		{
			return "";
		}

		public ZString Origin
		{
			get { return Entry.Origin() == null ? ZString.Empty : Entry.Origin().Description; }
		}

		public ZString Destination
		{
			get { return Entry.Destination() == null ? ZString.Empty : Entry.Destination().Description; }
		}

		public ZString Via
		{
			get { return Entry.Via?.Description ?? ZString.Empty; }
		}

		public ZString ContainerCode
		{
			get { return Entry.Container == null ? ZString.Empty : Entry.Container.RC_Code; }
		}

		public ZString OriginDestination
		{
			get
			{
				ZString result;

				if (!Origin.IsEmpty)
				{
					if (!Destination.IsEmpty)
					{
						result = Res.GetString("a6d670ac-61d0-4dda-a505-e8eb1e592771", "{0} - {1}", Origin, Destination);
					}
					else
					{
						result = Res.GetString("a3fbce0e-0ab6-4e4e-bfe3-7152578c599f", "From {0}", Origin);
					}
				}
				else if (!Destination.IsEmpty)
				{
					result = Res.GetString("ad6425b8-c421-440a-a9ff-15690cba6f94", "To {0}", Destination);
				}
				else
				{
					result = ZString.Empty;
				}

				return AddVia(result);
			}
		}

		public ZString OverseasPort
		{
			get
			{
				if (Entry.OverseasPort == null || Entry.OverseasPort is RefCountry)
				{
					return ZString.Empty;
				}
				else
				{
					ZString result = Entry.OverseasPort.Description;

					if (Entry.IsQuote() && !Entry.TI_QuotePageIncoTerm.IsEmpty)
					{
						result = Entry.TI_QuotePageIncoTerm + " " + result;
					}

					return result;
				}
			}
		}

		public ZString OverseasPortVia
		{
			get { return AddVia(OverseasPort); }
		}

		public ZString OverseasCountry
		{
			get
			{
				if (Entry.IsCrossTrade())
				{
					if (Entry.Origin() != null && Entry.Destination() != null && Entry.Origin().Country != null && Entry.Destination().Country != null)
					{
						return Entry.Origin().Country.Description + " - " + Entry.Destination().Country.Description;
					}
					else
					{
						return ZString.Empty;
					}
				}
				else if (Entry.IsDomestic())
				{
					return ZString.Empty;
				}
				else
				{
					return Entry.OverseasPort == null || Entry.OverseasPort.Country == null ? ZString.Empty : Entry.OverseasPort.Country.Description;
				}
			}
		}

		public ZString CommodityCode
		{
			get { return Entry.CommodityCode != null ? Entry.CommodityCode.RH_DescriptionMultilingual : ZString.Empty; }
		}

		public ZString ServiceLevel
		{
			get
			{
				if (Entry.IsCosting())
				{
					return Entry.CarrierServiceLevel != null ? Entry.CarrierServiceLevel.PL_CarrierServiceLevelDescriptionMultilingual : ZString.Empty;
				}
				else
				{
					return Entry.ServiceLevel_NI != null ? Entry.ServiceLevel_NI.RS_DescriptionMultilingual : ZString.Empty;
				}
			}
		}

		public ZString Currency
		{
			get { return MatrixedRateLine != null ? MatrixedRateLine.TL_RX_NKCurrency : Entry.TI_RX_NKCurrency; }
		}

		public ZString Carrier
		{
			get
			{
				ZString result = ZString.Empty;

				if (Entry.TransportProvider != null)
				{
					if ((Entry.IsAir() && DocumentsDataRegistry.Instance.AirFreightIncludeTransportProviderOnQuotation.Value) ||
						(Entry.IsSea() && DocumentsDataRegistry.Instance.SeaFreightIncludeTransportProviderOnQuotation.Value))
					{
						result = Entry.TransportProvider.OH_FullName;
					}
				}

				return result;
			}
		}

		public ZString TransitTime
		{
			get
			{
				switch (Entry.TI_TransitTime)
				{
					case RatingConstants.TransitTimes.Overnight:
						return Res.GetString("3a59274e-5bdb-496e-af68-236113f33aab", "Overnight");

					case RatingConstants.TransitTimes.SameDay:
						return Res.GetString("8e58de7c-711a-463f-be22-14fd9a5a31fb", "Same Day");

					case "1":
						return Res.GetString("b5836876-435d-4f95-b1f5-5ec1f6b4e1bb", "1 Day");

					case "":
						return "";

					default:
						return Res.GetString("428a1832-773d-46ee-81f8-5bd804a3d225", "{0} Days", Entry.TI_TransitTime);
				}
			}
		}

		public ZString Frequency
		{
			get
			{
				if (Entry.TI_Frequency != 0)
				{
					switch (Entry.TI_FrequencyUnit.ToUpper())
					{
						case RatingConstants.FrequencyUnits.Daily:
							return Res.GetString("3876049b-fb5e-4a73-a476-f6264c67a27b", "{0} per Day", Entry.TI_Frequency);

						case RatingConstants.FrequencyUnits.Days:
							return Res.GetString("0f5572a3-96e7-4f44-a8d7-5ffd0d7f02f9", "Every {0}", Entry.TI_Frequency) + " " + ((Entry.TI_Frequency > 1) ? Res.GetString("22813d4a-9fbb-4b6a-a1f6-e363d745a2fe", "Day") : Res.GetString("aa046247-e6df-43ec-b514-35d3839319e0", "Days"));

						case RatingConstants.FrequencyUnits.Week:
							return Res.GetString("a5e28ccd-dea0-4829-92e8-599172490957", "{0} per Week", Entry.TI_Frequency);

						case RatingConstants.FrequencyUnits.Fortnight:
							return Res.GetString("f5d735a3-38d0-42fe-8aec-5ff65d10d347", "{0} per Fortnight", Entry.TI_Frequency);

						case RatingConstants.FrequencyUnits.Monthly:
							return Res.GetString("b38e2b38-bbac-433a-ba69-b64bc71835cd", "{0} per Month", Entry.TI_Frequency);
					}
				}
				return ZString.Empty;
			}
		}

		public ZString TransitTimeFrequency
		{
			get
			{
				ZString result = TransitTime;

				if (!result.IsEmpty)
				{
					result += "\n";
				}

				result += Frequency;

				return result;
			}
		}

		public ZString ValidFrom
		{
			get { return Entry.TI_RateStartDate.ToString("d"); }
		}

		public ZString ValidUntil
		{
			get { return Entry.TI_RateEndDate.ToString("d"); }
		}

		public ZString Validity
		{
			get { return ValidFrom + "-" + ValidUntil; }
		}

		public ZBool ViewAgentRates { get; set; }

		public ZString ChargeCode
		{
			get { return MatrixedRateLine != null ? MatrixedRateLine.TL_RateDesc : ZString.Empty; }
		}

		public ZBool IsOtherChargesEntry
		{
			get { return AirLCLType == AirLCLDocRateEntryTypes.OtherChargesLine; }
		}

		public ZString OtherCharges
		{
			get
			{
				if (IsSuitableToShowOtherCharges)
				{
					ZStringBuilder result = new ZStringBuilder();

					if (!HasColumn((NoResString)"Transit Time") && !TransitTime.IsEmpty)
					{
						result.Append(Res.GetString("59ce1f82-c7f1-4eb5-ace9-2e465341513d", "Transit Time: {0}", TransitTime));
					}

					if (!HasColumn("Freq.") && !Frequency.IsEmpty)
					{
						result.Append(Res.GetString("8895b746-88b4-42af-b016-b00615918929", "Frequency: {0}", Frequency));
					}

					if (Entry.Consignor != null)
					{
						result.Append(Res.GetString("3191bf49-149e-4eb3-97d2-5aaf42d88a6d", "Consignor: {0}", Entry.Consignor.OH_FullName));
					}

					if (Entry.Consignee != null)
					{
						result.Append(Res.GetString("321185fe-8933-439d-926d-f583e90105b5", "Consignee: {0}", Entry.Consignee.OH_FullName));
					}

					bool createModeHeading = !Entry.IsAirFreight() && LCLEntry != null && FirstContainerisedEntry != null;
					ZString prevMode = ZString.Empty;
					IEnumerable<DocRateLineItem> otherChargesLines;

					if (Parent != null && Parent.HasAirLCLMarkup)
					{
						otherChargesLines = FreightDocRateLineItems.Where(item => !MatrixCompatibleRateLines.Contains(item.QuotationLine.Master) && RateLinesIncludingRelated(Entry).Contains(item.QuotationLine.Master));
					}
					else
					{
						otherChargesLines = FreightDocRateLineItems.Where(item => !item.IsDefaultFreight);
					}

					foreach (DocRateLineItem item in otherChargesLines)
					{
						if (createModeHeading && prevMode != item.Mode)
						{
							result.Append(item.Mode);
							prevMode = item.Mode;
						}

						ZStringBuilder descCurrAmountUnits = new ZStringBuilder(item.Description + ":");
						descCurrAmountUnits.AppendIfNotEmpty(item.Currency);
						descCurrAmountUnits.AppendIfNotEmpty(item.Amount);
						descCurrAmountUnits.AppendIfNotEmpty(item.Units);

						result.Append(descCurrAmountUnits.ToStringWithDelimiterBetweenAppends(" "));
					}

					return result.ToStringWithNewLineBetweenAppends();
				}

				return ZString.Empty;
			}
		}

		public ZString IncoTerm
		{
			get { return Entry.TI_QuotePageIncoTerm; }
		}

		public ZString ConversionFactor
		{
			get
			{
				if (MatrixedRateLine != null)
				{
					var freightEntry = Entry.IsAirFreight() ? Entry : (LCLEntry ?? Entry);

					return freightEntry.TI_RateCategory != RatingConstants.RateCategory.FCL && !MatrixedRateLine.ConversionFactorForDocumentPrintingOnly.IsEmpty
						? (ZString)MatrixedRateLine.ConversionFactorForDocumentPrintingOnly.ToString()
						: ZString.Empty;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZString ContractNumber
		{
			get { return Entry.TI_ContractNumber; }
		}

		public ZInt LCLHeaderGroupIndex { get; set; }

		public ZString GroupLCLHeader4
		{
			get { return GetGroupLCLHeader(4); }
		}

		public ZString GroupLCLHeader5
		{
			get { return GetGroupLCLHeader(5); }
		}

		public ZString GroupLCLHeader6
		{
			get { return GetGroupLCLHeader(6); }
		}

		public ZString GroupLCLHeader7
		{
			get { return GetGroupLCLHeader(7); }
		}

		public ZString GroupLCLHeader8
		{
			get { return GetGroupLCLHeader(8); }
		}

		public ZString GroupLCLHeader9
		{
			get { return GetGroupLCLHeader(9); }
		}

		public ZString GroupLCLHeader10
		{
			get { return GetGroupLCLHeader(10); }
		}

		public ZString GroupLCLHeader11
		{
			get { return GetGroupLCLHeader(11); }
		}

		public ZString Header1
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[0].HeaderName; }
		}

		public ZString Header2
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[1].HeaderName; }
		}

		public ZString Header3
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[2].HeaderName; }
		}

		public ZString Header4
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[3].HeaderName; }
		}

		public ZString Header5
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[4].HeaderName; }
		}

		public ZString Header6
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[5].HeaderName; }
		}

		public ZString Header7
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[6].HeaderName; }
		}

		public ZString Header8
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[7].HeaderName; }
		}

		public ZString Header9
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[8].HeaderName; }
		}

		public ZString Header10
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[9].HeaderName; }
		}

		public ZString Header11
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[10].HeaderName; }
		}

		public ZString Header12
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[11].HeaderName; }
		}

		public ZString Header13
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[12].HeaderName; }
		}

		public ZString Header14
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[13].HeaderName; }
		}

		public ZString Header15
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[14].HeaderName; }
		}

		public ZString Column1
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[0].GetValue(this); }
		}

		public ZString Column2
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[1].GetValue(this); }
		}

		public ZString Column3
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[2].GetValue(this); }
		}

		public ZString Column4
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[3].GetValue(this); }
		}

		public ZString Column5
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[4].GetValue(this); }
		}

		public ZString Column6
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[5].GetValue(this); }
		}

		public ZString Column7
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[6].GetValue(this); }
		}

		public ZString Column8
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[7].GetValue(this); }
		}

		public ZString Column9
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[8].GetValue(this); }
		}

		public ZString Column10
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[9].GetValue(this); }
		}

		public ZString Column11
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[10].GetValue(this); }
		}

		public ZString Column12
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[11].GetValue(this); }
		}

		public ZString Column13
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[12].GetValue(this); }
		}

		public ZString Column14
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[13].GetValue(this); }
		}

		public ZString Column15
		{
			get { return columnDefinitions == null ? ZString.Empty : columnDefinitions[14].GetValue(this); }
		}

		public static List<RateEntry> UniqueFreightEntriesIgnoringMode(IEnumerable<RateEntry> entryCollection)
		{
			List<RateEntry> results = new List<RateEntry>();

			foreach (RateEntry entry in entryCollection)
			{
				if (!entry.IsFreightEntry())
				{
					continue;
				}

				bool isDuplicate = false;

				for (int i = 0; i < results.Count; i++)
				{
					if (entry.IsDuplicateForPricingPageGrouping(results[i], true))
					{
						isDuplicate = true;
						break;
					}
				}

				if (!isDuplicate)
				{
					results.Add(entry);
				}
			}

			return results;
		}

		#region Implementation

		internal enum AirLCLDocRateEntryTypes
		{
			WeightBreaksMatrixLine,
			OtherChargesLine,
			ComboLine
		}

		internal class ColumnDefinition
		{
			public ColumnDefinition() { }

			public ColumnDefinition(ZString headerName, ZString columnValue)
			{
				HeaderName = headerName;
				value = columnValue;
			}

			public ColumnDefinition(ZString headerName, ZString containerCode, ZString calculatorType)
			{
				this.HeaderName = headerName;
				this.containerCode = containerCode;
				this.calculatorType = calculatorType;
			}

			public ZString HeaderName { get; }

			public virtual ZString GetValue(DocRateEntry docRateEntry)
			{
				if (!containerCode.IsEmpty)
				{
					return docRateEntry.GetContainerizedValue(containerCode, calculatorType);
				}

				return value;
			}

			readonly ZString value;
			readonly ZString calculatorType;
			readonly ZString containerCode;
		}

		internal RateEntry Entry
		{
			get { return (RateEntry)WrappedObject; }
		}

		internal ZString GetContainerizedValue(ZString containerCode, ZString calculatorType)
		{
			RateLine valueLine = null;
			RateEntry[] valueEntries;

			if (EntriesTable.TryGetValue(containerCode, out valueEntries))
			{
				foreach (RateEntry valueEntry in valueEntries)
				{
					if (valueEntry != null)
					{
						if (containerCode == RatingConstants.RateCategory.LCL)
						{
							valueLine = RateLinesIncludingRelated(valueEntry).FirstOrDefault(rateLine => rateLine.TL_AC == FreightChargeCode && rateLine.Parent.Container == null);
						}
						else
						{
							valueLine = RateLinesIncludingRelated(valueEntry).FirstOrDefault(rateLine => rateLine.TL_AC == FreightChargeCode && rateLine.Parent.Container != null && rateLine.Parent.Container.RC_Code == containerCode);

							if (valueLine == null)
							{
								ZString containerClass = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerCode).RC_FreightRateClass;
								if (!containerClass.IsEmpty)
								{
									valueLine = RateLinesIncludingRelated(valueEntry).FirstOrDefault(rateLine => rateLine.Parent.Container != null && rateLine.Parent.Container.RC_FreightRateClass == containerClass);
								}
							}
						}

						if (valueLine != null)
						{
							break;
						}
					}
				}
			}

			return GetValue(valueLine, calculatorType, ZDecimal.Zero);
		}

		protected ZString GetRelevantValue(RateLineItem lineItem)
		{
			if (lineItem == null)
			{
				return ZString.Empty;
			}
			else if (ViewAgentRates && !lineItem.TM_AgentDeclaredRate.IsEmpty)
			{
				return lineItem.TM_AgentDeclaredRate != 0m ? GetFormattedValue(lineItem.TM_AgentDeclaredRate) : ZString.Empty;
			}
			else if (!lineItem.TM_FlatAmount.IsEmpty)
			{
				return GetFormattedValue(lineItem.TM_Value) + System.Environment.NewLine + GetFormattedValue(lineItem.TM_FlatAmount);
			}
			else
			{
				return lineItem.TM_Value != 0m ? GetFormattedValue(lineItem.TM_Value) : ZString.Empty;
			}
		}

		internal AirLCLDocRateEntryTypes AirLCLType { get; set; }

		internal RateEntry LCLEntry
		{
			get
			{
				RateEntry[] result;
				return EntriesTable.TryGetValue(RatingConstants.RateCategory.LCL, out result) ? result[0] : null;
			}
		}

		internal RateEntry FirstContainerisedEntry
		{
			get
			{
				foreach (KeyValuePair<ZString, RateEntry[]> dictEntry in EntriesTable)
				{
					if (dictEntry.Key != RatingConstants.RateCategory.LCL && dictEntry.Value[0] != null)
					{
						return dictEntry.Value[0];
					}
				}

				return null;
			}
		}

		internal RateLine MatrixedRateLine { get; set; }

		internal List<RateLine> MatrixCompatibleRateLines
		{
			get
			{
				if (rateLinesWithMatrixCompatibleCalculators == null)
				{
					rateLinesWithMatrixCompatibleCalculators = new List<RateLine>();

					if (Parent != null && Parent.HasAirLCLMarkup)
					{
						foreach (RateLine line in RateLinesIncludingRelated(Entry))
						{
							if (line.Calculator.CanBePrintedUsing6StandardOperatorColumnHeaders)
							{
								rateLinesWithMatrixCompatibleCalculators.Add(line);
							}
						}
					}
				}
				return rateLinesWithMatrixCompatibleCalculators;
			}
		}
		List<RateLine> rateLinesWithMatrixCompatibleCalculators;

		List<RateLine> RateLinesIncludingRelated(RateEntry entry)
		{
			if (rateLinesIncludingRelated == null)
			{
				rateLinesIncludingRelated = new Dictionary<RateEntry, List<RateLine>>();
			}

			List<RateLine> result;

			if (!rateLinesIncludingRelated.TryGetValue(entry, out result))
			{
				result = entry.RateLinesIncludingRelated(Parent.Page);
				rateLinesIncludingRelated.Add(entry, result);
			}

			return result;
		}
		Dictionary<RateEntry, List<RateLine>> rateLinesIncludingRelated;

		internal RateLine LCLHeaderGroupRateLine { get; set; }

		internal void SetAirColumnDefinitions()
		{
			if (Entry.IsQuote())
			{
				#region Quote Entry Air Columns

				columnDefinitions = new ColumnDefinition[15]
				{
					OriginDestinationColumn,
					new ColumnDefinition((NoResString)"Airline", Carrier),
					new ColumnDefinition("", Currency),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					Entry.IsLooseFreight() ? new ColumnDefinition((NoResString)"W/V conv.", ConversionFactor) : new ColumnDefinition(),
					new ColumnDefinition((NoResString)"Transit Time", TransitTime),
					new ColumnDefinition("Freq.", Frequency),
					new ColumnDefinition(),
				};

				#endregion
			}
			else
			{
				#region Client Rates/Company Tariff/Costing Entry Air Columns

				columnDefinitions = new ColumnDefinition[15]
				{
					new ColumnDefinition((NoResString)"Origin", Origin),
					new ColumnDefinition((NoResString)"Destination", Destination),
					new ColumnDefinition((NoResString)"Airline", Carrier),
					new ColumnDefinition((NoResString)"Serv. Level", ServiceLevel),
					new ColumnDefinition((NoResString)"Comm. Code", CommodityCode),
					new ColumnDefinition("", Currency),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition((NoResString)"W/V conv.", ConversionFactor),
					new ColumnDefinition((NoResString)"Validity", Validity),
				};

				#endregion
			}

			if (Entry.IsULD())
			{
				int firstContainerColumnIndex = Entry.IsQuote() ? 3 : 6;
				int lastContainerColumnIndex = firstContainerColumnIndex + Math.Min(4, Parent.ContainerList.Count - 1);

				for (int i = firstContainerColumnIndex, j = 0; i <= lastContainerColumnIndex; i++, j++)
				{
					columnDefinitions[i] = new ColumnDefinition(Parent.ContainerList[j].RC_Code, Parent.ContainerList[j].RC_Code, Calculator.Items.Operator.UNT);
				}
			}
			else if (MatrixedRateLineItems.Count > 0)
			{
				int firstBreakColumnIndex = Entry.IsQuote() ? 3 : 6;
				int lastBreakColumnIndex = firstBreakColumnIndex + Math.Min((Entry.IsQuote() ? 7 : 6), MatrixedRateLineItems.Count - 1);

				for (int i = firstBreakColumnIndex, j = 0; i <= lastBreakColumnIndex; i++, j++)
				{
					columnDefinitions[i] = new ColumnDefinition(GetRateLineItemDescription(MatrixedRateLineItems[j]), GetRelevantValue(GetCorrespondingRateLineItem(MatrixedRateLineItems[j])));
				}
			}
		}

		internal void SetSeaColumnDefinitions()
		{
			if (Entry.IsQuote())
			{
				#region Quote Entry Sea Columns

				columnDefinitions = new ColumnDefinition[15]
				{
					OriginDestinationColumn,
					new ColumnDefinition(Res.GetString("0ff84be2-9fa9-40e5-891c-4a5fd8f43940", "Shipping Line"), Carrier),
					new ColumnDefinition("", Currency),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition((NoResString)"Transit Time", TransitTime),
					new ColumnDefinition("Freq.", Frequency),
					new ColumnDefinition((NoResString)"W/V conv.", ConversionFactor),
					new ColumnDefinition((NoResString)"Contract No.", ContractNumber),
					new ColumnDefinition(),
				};

				#endregion
			}
			else
			{
				#region Client Rates/Company Tariff/Costing Entry Air Columns

				columnDefinitions = new ColumnDefinition[15]
				{
					new ColumnDefinition((NoResString)"Origin", Origin),
					new ColumnDefinition((NoResString)"Destination", Destination),
					new ColumnDefinition((NoResString)"Shipping Line", Carrier),
					new ColumnDefinition((NoResString)"Serv. Level", ServiceLevel),
					new ColumnDefinition((NoResString)"Comm. Code", CommodityCode),
					new ColumnDefinition("", Currency),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition((NoResString)"W/V conv.", ConversionFactor),
					new ColumnDefinition((NoResString)"Validity", Validity),
				};

				#endregion
			}

			bool fCLExists = Parent != null ? Parent.FirstFCLEntry != null : Entry.TI_RateCategory == RatingConstants.RateCategory.FCL;
			bool lCLExists = Parent != null ? Parent.FirstLCLEntry != null : Entry.IsLCLFreight();

			int firstContainerColumn = Entry.IsQuote() ? 3 : 6;

			if (fCLExists)
			{
				int maxContainerColumn = firstContainerColumn + 4 + (lCLExists ? 0 : 2);
				int lastContainerColumn = Parent != null ? Math.Min(maxContainerColumn, firstContainerColumn + Parent.ContainerList.Count - 1) : 1;

				for (int i = firstContainerColumn, j = 0; i <= lastContainerColumn; i++, j++)
				{
					columnDefinitions[i] = new ColumnDefinition(Parent.ContainerList[j].RC_Code, Parent.ContainerList[j].RC_Code, Calculator.Items.Operator.UNT);
				}
			}

			if (lCLExists)
			{
				int firstLCLColumn = fCLExists ? firstContainerColumn + 5 : firstContainerColumn;
				string lclHeaderPrefix = Entry.IsRoad() ? "FTL/LTL" : "LCL";

				columnDefinitions[firstLCLColumn] = new ColumnDefinition(lclHeaderPrefix + (NoResString)" Minimum", RatingConstants.RateCategory.LCL, Calculator.Items.Operator.MIN);
				columnDefinitions[firstLCLColumn + 1] = new ColumnDefinition(lclHeaderPrefix + (NoResString)" per M3", RatingConstants.RateCategory.LCL, Calculator.Items.Operator.UNT);
			}
		}

		internal void SetShippingColumnDefinitions()
		{
			if (Entry.IsQuote())
			{
				#region Quote Entry Sea Columns

				columnDefinitions = new ColumnDefinition[15]
				{
					OriginDestinationColumn,
					new ColumnDefinition((NoResString)"Principal", Carrier),
					new ColumnDefinition("", Currency),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition((NoResString)"Transit Time", TransitTime),
					new ColumnDefinition("Freq.", Frequency),
					new ColumnDefinition((NoResString)"W/V conv.", ConversionFactor),
					new ColumnDefinition((NoResString)"Contract No.", ContractNumber),
					new ColumnDefinition(),
				};

				#endregion
			}
			else
			{
				#region Client Rates/Company Tariff/Costing Entry Air Columns

				columnDefinitions = new ColumnDefinition[15]
				{
					new ColumnDefinition((NoResString)"Origin", Origin),
					new ColumnDefinition((NoResString)"Destination", Destination),
					new ColumnDefinition((NoResString)"Principal", Carrier),
					new ColumnDefinition((NoResString)"Serv. Level", ServiceLevel),
					new ColumnDefinition((NoResString)"Comm. Code", CommodityCode),
					new ColumnDefinition("", Currency),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition(),
					new ColumnDefinition((NoResString)"W/V conv.", ConversionFactor),
					new ColumnDefinition((NoResString)"Validity", Validity),
				};

				#endregion
			}

			bool fclExists = Parent != null ? Parent.FirstFCLEntry != null : Entry.TI_RateCategory == RatingConstants.RateCategory.FCL;
			bool lclExists = Parent != null ? Parent.FirstLCLEntry != null : Entry.IsLCLFreight();

			int firstContainerColumn = Entry.IsQuote() ? 3 : 6;

			if (fclExists)
			{
				int maxContainerColumn = firstContainerColumn + 4 + (lclExists ? 0 : 2);
				int lastContainerColumn = Parent != null ? Math.Min(maxContainerColumn, firstContainerColumn + Parent.ContainerList.Count - 1) : 1;

				for (int i = firstContainerColumn, j = 0; i <= lastContainerColumn; i++, j++)
				{
					columnDefinitions[i] = new ColumnDefinition(Parent.ContainerList[j].RC_Code, Parent.ContainerList[j].RC_Code, Calculator.Items.Operator.UNT);
				}
			}

			if (lclExists)
			{
				int firstLCLColumn = fclExists ? firstContainerColumn + 5 : firstContainerColumn;
				columnDefinitions[firstLCLColumn] = new ColumnDefinition((NoResString)"LCL Minimum", RatingConstants.RateCategory.LCL, Calculator.Items.Operator.MIN);
				columnDefinitions[firstLCLColumn + 1] = new ColumnDefinition((NoResString)"LCL per M3", RatingConstants.RateCategory.LCL, Calculator.Items.Operator.UNT);
			}
		}

		ZString AddVia(ZString portName)
		{
			if (Entry.Via != null)
			{
				if (portName.IsEmpty)
				{
					portName += (NoResString)"Via ";
				}
				else
				{
					portName += (NoResString)" via ";
				}

				portName += Entry.Via.Description;
			}

			return portName;
		}

		ZString GetValue(RateLine valueLine, ZString calculatorType, ZDecimal slidingBreak)
		{
			if (valueLine != null)
			{
				foreach (RateLineItem lineItem in valueLine.RateLineItems)
				{
					if (lineItem.TM_Type == calculatorType && lineItem.TM_Break == slidingBreak)
					{
						return GetRelevantValue(lineItem);
					}
				}

				if (calculatorType == Calculator.Items.Operator.Plus || calculatorType == Calculator.Items.Operator.Minus)
				{
					RateLineItem lineItem = valueLine.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT);

					if (lineItem != null)
					{
						return GetRelevantValue(lineItem);
					}
				}
			}

			return ZString.Empty;
		}

		ZString GetFormattedValue(ZDecimal value)
		{
			return value.ToString(value.DecimalPlaces < 3 ? 2 : 3);
		}

		void AddToEntriesHashTable(RateEntryCollection entryCollection)
		{
			foreach (RateEntry containerEntry in entryCollection)
			{
				if (containerEntry.IsAirFreight() && !containerEntry.IsULD())
				{
					continue;
				}

				if (containerEntry.TI_OriginLRC != Entry.TI_OriginLRC)
				{
					continue;
				}

				if (containerEntry.TI_DestinationLRC != Entry.TI_DestinationLRC)
				{
					continue;
				}

				if (containerEntry.TI_ViaLRC != Entry.TI_ViaLRC)
				{
					continue;
				}

				if (containerEntry.TI_OH_TransportProvider != Entry.TI_OH_TransportProvider)
				{
					continue;
				}

				if (containerEntry.TI_OH_Supplier != Entry.TI_OH_Supplier)
				{
					continue;
				}

				if (containerEntry.TI_RS_NKServiceLevel_NI != Entry.TI_RS_NKServiceLevel_NI)
				{
					continue;
				}

				if (containerEntry.TI_PL_NKCarrierServiceLevel != Entry.TI_PL_NKCarrierServiceLevel)
				{
					continue;
				}

				if (containerEntry.TI_RH_NKCommodityCode != Entry.TI_RH_NKCommodityCode)
				{
					continue;
				}

				if (containerEntry.TI_TransitTime != Entry.TI_TransitTime)
				{
					continue;
				}

				if (containerEntry.TI_Frequency != Entry.TI_Frequency)
				{
					continue;
				}

				if (containerEntry.TI_FrequencyUnit != Entry.TI_FrequencyUnit)
				{
					continue;
				}

				if (containerEntry.TI_RateStartDate != Entry.TI_RateStartDate)
				{
					continue;
				}

				if (containerEntry.TI_RateEndDate != Entry.TI_RateEndDate)
				{
					continue;
				}

				if (containerEntry.TI_OH_Consignor != Entry.TI_OH_Consignor)
				{
					continue;
				}

				if (containerEntry.TI_OH_Consignee != Entry.TI_OH_Consignee)
				{
					continue;
				}

				if (containerEntry.IsShipping() != Entry.IsShipping())
				{
					continue;
				}

				AddContainersToEntryTable(containerEntry);
			}
		}

		List<ZString> GetContainers(RefContainer entryContainer, bool matchClass)
		{
			List<ZString> containers = new List<ZString>();
			containers.Add(entryContainer?.RC_Code ?? RatingConstants.RateCategory.LCL);

			if (matchClass && entryContainer != null)
			{
				foreach (RefContainer container in entryContainer.ContainersInSameFreightRateClass)
				{
					containers.Add(container.RC_Code);
				}
			}

			return containers;
		}

		void AddContainersToEntryTable(RateEntry containerEntry)
		{
			var containers = GetContainers(containerEntry.Container, containerEntry.TI_MatchContainerRateClass);
			for (int i = 0; i < containers.Count; i++)
			{
				if (!entriesTable.TryGetValue(containers[i], out RateEntry[] entries))
				{
					entries = new RateEntry[2];
					entriesTable.Add(containers[i], entries);
				}

				entries[i == 0 ? 0 : 1] = containerEntry;
			}
		}

		bool IsSuitableToShowOtherCharges
		{
			get { return Parent == null || !Parent.HasAirLCLMarkup || AirLCLType == AirLCLDocRateEntryTypes.OtherChargesLine || AirLCLType == AirLCLDocRateEntryTypes.ComboLine; }
		}

		bool HasColumn(ZString headerName)
		{
			if (columnDefinitions != null)
			{
				foreach (ColumnDefinition column in columnDefinitions)
				{
					if (column.HeaderName == headerName)
					{
						return true;
					}
				}
			}

			return false;
		}

		IEnumerable<DocRateLineItem> FreightDocRateLineItems
		{
			get
			{
				if (freightDocRateLineItems == null)
				{
					var entries = new List<RateEntry>();

					if (Entry != null && Parent != null)
					{
						entries.Add(Entry);

						if (!Entry.IsAirFreight())
						{
							var secondEntry = Entry.TI_RateCategory == RatingConstants.RateCategory.FCL || Entry.TI_RateCategory == RatingConstants.RateCategory.SCO ? LCLEntry : FirstContainerisedEntry;

							if (secondEntry != null)
							{
								entries.Add(secondEntry);
							}
						}
					}

					DocQuotationLineCollection result = new DocQuotationLineCollection(Parent, new PricingPageRateLineFactory(EntryTypes.Freight), entries, Factory);
					result.Load();

					freightDocRateLineItems = result.Cast<DocRateLineItem>();
				}

				return freightDocRateLineItems;
			}
		}
		IEnumerable<DocRateLineItem> freightDocRateLineItems;

		Dictionary<ZString, RateEntry[]> EntriesTable
		{
			get
			{
				if (entriesTable == null)
				{
					entriesTable = new Dictionary<ZString, RateEntry[]>();

					if (Entry.Parent != null)
					{
						foreach (RateEntryCollection entryCollection in Entry.Parent.EntryCollectionsExcludingSummary.Values)
						{
							if (entryCollection.Count != 0 && entryCollection[0].IsFreightEntry())
							{
								AddToEntriesHashTable(entryCollection);
							}
						}
					}
				}

				return entriesTable;
			}
		}
		Dictionary<ZString, RateEntry[]> entriesTable;

		ZGuid FreightChargeCode
		{
			get { return Env.Registry.GetFreightChargeCode(Entry.Company().PK.ToGuid()); }
		}

		ZString GetGroupLCLHeader(int headerNumber)
		{
			if (Parent != null && Parent.HasAirLCLMarkup)
			{
				if (headerNumber < 4 || headerNumber > (Entry.IsQuote() ? 11 : 10))
				{
					if (columnDefinitions != null && columnDefinitions.Length >= headerNumber)
					{
						return columnDefinitions[headerNumber - 1].HeaderName;
					}
				}
				else if (MatrixedRateLineItems.Count > 0)
				{
					if (MatrixedRateLineItems.Count > headerNumber - 4)
					{
						return GetRateLineItemDescription(MatrixedRateLineItems[headerNumber - 4]);
					}
				}
			}

			return ZString.Empty;
		}

		RateLineItem GetCorrespondingRateLineItem(RateLineItem headerItem)
		{
			if (MatrixedRateLine != null && headerItem.Parent != MatrixedRateLine)
			{
				foreach (RateLineItem lineItem in MatrixedRateLine.RateLineItems)
				{
					if (DocAirLCLRateEntryCollection.RateLineItemsCorrespondOneWay(headerItem, lineItem))
					{
						return lineItem;
					}
				}
			}
			else if (headerItem.Parent == MatrixedRateLine)
			{
				return headerItem;
			}

			return null;
		}

		ZString GetRateLineItemDescription(RateLineItem item)
		{
			if (item.RateOperatorIsMinus() || item.RateOperatorIsPlus() || item.RateOperatorIsUNT())
			{
				string result;

				if (item.RateOperatorIsMinus() || item.RateOperatorIsPlus())
				{
					result = item.TM_Type;
					result += item.TM_Break.ToString("g0", Culture.Invariant) + " ";
				}
				else
				{
					result = string.Empty;
				}

				return result + (NoResString)"per " + (item.TM_BreakWeightVolume.IsEmpty ? item.Parent.UnitMultipleAsString + (NoResString)" " + item.Parent.TL_WeightVolume : (string)item.TM_BreakWeightVolume).Trim();
			}
			else if (item.RateOperatorIsBAS() && !string.IsNullOrEmpty(Env.Registry.Rating.FlatFeeText))
			{
				return Env.Registry.Rating.FlatFeeText;
			}
			else
			{
				return item.TM_Type;
			}
		}

		List<RateLineItem> MatrixedRateLineItems
		{
			get { return matrixedRateLineItems ?? (matrixedRateLineItems = GetMatrixCompatibleOperatorItems(LCLHeaderGroupRateLine)); }
		}
		List<RateLineItem> matrixedRateLineItems;

		List<RateLineItem> GetMatrixCompatibleOperatorItems(RateLine matrixedLine)
		{
			List<RateLineItem> result = new List<RateLineItem>();

			if (matrixedLine != null)
			{
				foreach (RateLineItem item in matrixedLine.RateLineItems)
				{
					if (item.RateOperatorIsBAS() || item.RateOperatorIsMIN() || item.RateOperatorIsMAX() || item.RateOperatorIsUNT() || item.RateOperatorIsMinus() || item.RateOperatorIsPlus())
					{
						result.Add(item);
					}
				}
			}

			result.Sort((x, y) =>
			{
				if (x.TM_Type == y.TM_Type)
				{
					return x.TM_Break.CompareTo(y.TM_Break);
				}

				if (x.RateOperatorIsMIN())
				{
					return -1;
				}

				if (y.RateOperatorIsMIN())
				{
					return 1;
				}

				if (x.RateOperatorIsBAS())
				{
					return -1;
				}

				if (y.RateOperatorIsBAS())
				{
					return 1;
				}

				if (x.RateOperatorIsUNT())
				{
					return -1;
				}

				if (y.RateOperatorIsUNT())
				{
					return 1;
				}

				if (x.RateOperatorIsMAX())
				{
					return -1;
				}

				if (x.RateOperatorIsMAX())
				{
					return 1;
				}

				if (x.RateOperatorIsMinus())
				{
					return -1;
				}

				if (y.RateOperatorIsMinus())
				{
					return 1;
				}

				if (x.RateOperatorIsPlus())
				{
					return -1;
				}

				if (y.RateOperatorIsPlus())
				{
					return 1;
				}

				return 0;
			});

			return result;
		}

		ColumnDefinition OriginDestinationColumn
		{
			get
			{
				switch (Entry.JobDirection)
				{
					case Directions.Import:
						return new ColumnDefinition((NoResString)"Origin", OverseasPortVia);

					case Directions.Export:
						return new ColumnDefinition((NoResString)"Destination", OverseasPortVia);

					default:
						return new ColumnDefinition((NoResString)"Origin - Destination", OriginDestination);
				}
			}
		}

		DocTableQuotation Parent
		{
			get
			{
				if (parent == null)
				{
					foreach (var collection in ParentCollections.OfType<DocRateEntryCollection>())
					{
						parent = collection.Parent;
						break;
					}
				}
				return parent;
			}
		}
		DocTableQuotation parent;

		ColumnDefinition[] columnDefinitions;

		#endregion
	}
}
