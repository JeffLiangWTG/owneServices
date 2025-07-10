using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.eAdaptor
{
	public class EAdaptorBillingSystem : TransactionBillingSystem
	{
		public EAdaptorBillingSystem()
		{
			IncludeSubCodeInSystemUsage = true;
			ElementNames = EDIDataRegistry.Instance.EAdaptorElementNames.Value;
		}

		readonly ReadOnlyCodeDescriptionPairList ElementNames;

		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.eAdaptor; }
		}

		protected override SystemUsage[] CreateSystemUsages(ClientChargeableUsage[] chargeableUsages)
		{
			var usageList = new List<EAdaptorUsage>(chargeableUsages.Length);

			// Fast lookup of the parent price item given a price header PK and a module code.
			var priceMaps = new Dictionary<ZGuid, Dictionary<string, ClientLicencePriceItem>>();

			foreach (var dateGroup in chargeableUsages.Where(x => x.U1_UnitCount != 0).GroupBy(x => x.U1_PeriodStart))
			{
				var periodStart = dateGroup.Key;
				UpdateDbUsageList(usageList, priceMaps, dateGroup, periodStart);
			}

			return usageList.ToArray();
		}

		void UpdateDbUsageList(List<EAdaptorUsage> usageList,
													 Dictionary<ZGuid, Dictionary<string, ClientLicencePriceItem>> priceMaps,
													 IGrouping<ZDateTime, ClientChargeableUsage> dateGroup,
													 ZDateTime periodStart)
		{
			foreach (var databaseGroup in dateGroup.Where(s => s.LicenceDatabase != null).GroupBy(s => s.LicenceDatabase))
			{
				// Database level Price Code mapped to SubUsage with a list of all contributing EAdaptorUsage
				var perDatabaseSubUsages = new Dictionary<string, Tuple<SystemUsage.SubUsage, List<EAdaptorUsage>>>();
				UpdateUsageList(usageList, priceMaps, periodStart, databaseGroup, perDatabaseSubUsages);

				if (perDatabaseSubUsages.Count > 0 && periodStart == Context.PeriodStart)
				{
					ApplyDatabaseFeeTypes(perDatabaseSubUsages.Values.Select(x => x.Item1));
					foreach (var dbUsage in perDatabaseSubUsages.Values)
					{
						dbUsage.Item2[0].AddDbUsage(dbUsage);
					}
				}
			}
		}

		void UpdateUsageList(List<EAdaptorUsage> usageList,
												 Dictionary<ZGuid, Dictionary<string, ClientLicencePriceItem>> priceMaps,
												 ZDateTime periodStart,
												 IGrouping<LicenceKeyBuilder.Business.LicenceDatabase, ClientChargeableUsage> databaseGroup,
												 Dictionary<string, Tuple<SystemUsage.SubUsage, List<EAdaptorUsage>>> perDatabaseSubUsages)
		{
			foreach (var orgGroup in databaseGroup.Where(x => x.ClientCompany != null).GroupBy(x => x.ClientCompany))
			{
				var clientCompany = orgGroup.Key;
				var licHeader = clientCompany.UsageOwnerLicence;
				if (licHeader != null)
				{
					var usage = new EAdaptorUsage(Context.Factory, licHeader, periodStart, clientCompany);
					if (Context.IsPreviewOnly)
					{
						usage.SetPreviewOnly(Context.PreviewPriceHeader);
					}

					BuildPriceMaps(new[] { usage }, priceMaps, mapIncludedToParent: false);
					var priceHeader = usage.PriceHeader;
					var items = priceHeader != null ? priceHeader.LocalOrStandardItems : null;
					var subUsageMap = BuildSubUsage(orgGroup, items != null ? priceMaps[items.Master.PK] : null);
					var companySubUsageList = ExtractPerDatabase(subUsageMap, usage, perDatabaseSubUsages);
					ApplyCompanyFeeTypes(companySubUsageList, clientCompany, periodStart);
					usage.SetSubUsage(companySubUsageList);
					usage.ChargeableUsagePKs.AddRange(orgGroup.Select(x => x.PK));
					usageList.Add(usage);
				}
			}
		}

		List<SystemUsage.SubUsage> ExtractPerDatabase(Dictionary<string, SystemUsage.SubUsage> subUsages,
	EAdaptorUsage usage,
	Dictionary<string, Tuple<SystemUsage.SubUsage, List<EAdaptorUsage>>> perDatabaseSubUsages)
		{
			var result = new List<SystemUsage.SubUsage>(subUsages.Count);
			foreach (var subUsage in subUsages.Values)
			{
				if (subUsage.PriceItem != null && BillingConstants.FeeType.IsPerDatabase(subUsage.PriceItem.L7_FeeType))
				{
					Tuple<SystemUsage.SubUsage, List<EAdaptorUsage>> dbUsage;
					if (perDatabaseSubUsages.TryGetValue(subUsage.PriceItemCode, out dbUsage))
					{
						dbUsage.Item1.RawUsageCount += subUsage.RawUsageCount;
					}
					else
					{
						dbUsage = new Tuple<SystemUsage.SubUsage, List<EAdaptorUsage>>(subUsage, new List<EAdaptorUsage>());
						perDatabaseSubUsages.Add(subUsage.PriceItemCode, dbUsage);
					}
					dbUsage.Item2.Add(usage);
				}
				else
				{
					result.Add(subUsage);
				}
			}

			return result;
		}

		Dictionary<string, SystemUsage.SubUsage> BuildSubUsage(IEnumerable<ClientChargeableUsage> usages, Dictionary<string, ClientLicencePriceItem> priceMap)
		{
			var subUsageMap = new Dictionary<string, SystemUsage.SubUsage>();
			foreach (var usage in usages)
			{
				ClientLicencePriceItem priceItem = null;
				if (priceMap != null)
				{
					priceMap.TryGetValue(usage.U1_SubCode, out priceItem);
				}
				var subUsage = new SystemUsage.SubUsage()
				{
					PriceItemCode = usage.U1_SubCode,
					RawUsageCount = usage.U1_UnitCountAsInt,
					UnitCount = usage.U1_UnitCountAsInt,
					PriceItem = priceItem,
					LicenceUnits = priceItem != null ? (decimal)priceItem.L7_LicenceUnits : 0m
				};
				subUsageMap.Add(usage.U1_SubCode, subUsage);
			}

			// add referenced parent modules if needed
			foreach (var subUsage in subUsageMap.Values.Where(x => x.PriceItem != null && !x.PriceItem.L7_ParentCode.IsEmpty).ToArray())
			{
				var parentCode = subUsage.PriceItem.L7_ParentCode;
				SystemUsage.SubUsage parentUsage = null;
				if (!subUsageMap.TryGetValue(parentCode, out parentUsage))
				{
					priceMap.TryGetValue(parentCode, out ClientLicencePriceItem priceItem);
					parentUsage = new SystemUsage.SubUsage()
					{
						PriceItemCode = parentCode,
						PriceItem = priceItem
					};
					subUsageMap.Add(parentCode, parentUsage);
				}

				parentUsage.RawUsageCount += subUsage.RawUsageCount;
			}

			return subUsageMap;
		}

		void ApplyCompanyFeeTypes(List<SystemUsage.SubUsage> subUsageList, ClientCompany clientCompany, ZDateTime periodStart)
		{
			// apply fee types
			foreach (var subUsage in subUsageList.Where(x => x.PriceItem != null))
			{
				var priceItem = subUsage.PriceItem;
				if (priceItem.L7_FeeType == BillingConstants.FeeType.TransactionalModule)
				{
					int userCount = 0;
					var moduleUsers = Context.ModuleUsersService;
					if (moduleUsers != null)
					{
						userCount = moduleUsers.CompanyMonthlyUserCount(clientCompany, priceItem.L7_WebParentCode, periodStart);
					}
					subUsage.IncludedUserCount = userCount;
					subUsage.UnitCount = Math.Max(subUsage.RawUsageCount - (userCount * priceItem.L7_UnitBreak), 0);
				}
				else if (BillingConstants.FeeType.IsPerLicence(priceItem.L7_FeeType))
				{
					subUsage.UnitCount = 1;
					subUsage.RawUsageCount = 1;
				}
				else
				{
					subUsage.UnitCount = subUsage.RawUsageCount;
				}

				subUsage.Amount = subUsage.UnitCount * subUsage.PriceItem.L7_Price;
			}
		}

		void ApplyDatabaseFeeTypes(IEnumerable<SystemUsage.SubUsage> subUsageList)
		{
			foreach (var subUsage in subUsageList.Where(x => x.PriceItem != null))
			{
				var priceItem = subUsage.PriceItem;
				if (priceItem.L7_FeeType == BillingConstants.FeeType.VolumeDatabaseFee)
				{
					var matchingBreakItem = priceItem.Parent.Items
						.FindAllByCode(subUsage.PriceItemCode)
						.OrderByDescending(x => x.L7_UnitBreak)
						.FirstOrDefault(x => x.L7_UnitBreak < subUsage.RawUsageCount);

					subUsage.PriceItem = matchingBreakItem;
				}
				subUsage.UnitCount = 1;
				subUsage.Amount = subUsage.UnitCount * subUsage.PriceItem.L7_Price;
			}
		}

		protected override SystemBill CreateSystemBill()
		{
			return new EAdaptorBill(Context);
		}

		#region Load Raw Usage

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new MultiSectionSystemRawUsage(context, SystemCode);
			string currentItemCode = "";
			string currentClientId = "";
			SummarySection summarySection = null;

			while (reader.Read())
			{
				string clientId = (string)reader["TX_ClientID"];
				string ref1 = (string)reader["TX_Reference1"];
				string ref2 = (string)reader["TX_Reference2"];
				string ref3 = (string)reader["TX_Reference3"];
				string ref4 = (string)reader["TX_Reference4"];
				string priceItemCode = (string)reader["TX_PriceItemCode"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

				SummaryLine newHeader = null;
				if (string.Compare(priceItemCode, currentItemCode, StringComparison.OrdinalIgnoreCase) != 0 ||
					string.Compare(clientId, currentClientId, StringComparison.OrdinalIgnoreCase) != 0)
				{
					summarySection = new SummarySection(context.Factory);
					summarySection.NumberOfColumnsUsed = 4;
					rawUsage.SummarySections.Add(summarySection);
					newHeader = summarySection.Header;
					if (currentClientId.Length == 9)
					{
						newHeader.TopLevelDescription = " Server " + currentClientId.Substring(7);
					}
					currentItemCode = priceItemCode;
					currentClientId = clientId;
				}

				SummaryLine line = summarySection.Lines.AddNew();

				// Column 1 - Tracking ID
				if (newHeader != null)
				{
					newHeader.Column1 = "eHub Tracking ID";
				}

				// Column 4 - Message Time
				if (newHeader != null)
				{
					newHeader.Column4 = "Message Time (UTC)";
				}
				line.Column4 = ToMessageTimeFormat(messageTime);

				string trackingId = PopulateRefColumns(ref1, ref2, ref3, ref4, priceItemCode, newHeader, line, ElementNames);

				line.Column1 = trackingId;
			}

			return rawUsage;
		}

		protected override StlRawUsage LoadStlRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new MultiSectionStlRawUsage(context, SystemCode);
			string currentItemCode = "";
			string currentCompanyCode = "";
			SummarySection summarySection = null;

			while (reader.Read())
			{
				string companyCode = (string)reader["CompanyCode"];
				string ref1 = (string)reader["TX_Reference1"];
				string ref2 = (string)reader["TX_Reference2"];
				string ref3 = (string)reader["TX_Reference3"];
				string ref4 = (string)reader["TX_Reference4"];
				string priceItemCode = (string)reader["TX_PriceItemCode"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

				SummaryLine newHeader = null;
				if (string.Compare(priceItemCode, currentItemCode, StringComparison.OrdinalIgnoreCase) != 0 ||
					string.Compare(companyCode, currentCompanyCode, StringComparison.OrdinalIgnoreCase) != 0)
				{
					summarySection = new SummarySection(context.Factory);
					summarySection.NumberOfColumnsUsed = 4;
					rawUsage.SummarySections.Add(summarySection);
					newHeader = summarySection.Header;
					if (currentCompanyCode.Length > 0)
					{
						newHeader.TopLevelDescription = " Company " + companyCode;
					}
					currentItemCode = priceItemCode;
					currentCompanyCode = companyCode;
				}

				SummaryLine line = summarySection.Lines.AddNew();

				// Column 1 - Tracking ID
				if (newHeader != null)
				{
					newHeader.Column1 = "eHub Tracking ID";
				}

				// Column 9 - Message Time
				if (newHeader != null)
				{
					newHeader.Column9 = "Message Time (UTC)";
				}
				line.Column9 = ToMessageTimeFormat(messageTime);

				string trackingId = PopulateRefColumns(ref1, ref2, ref3, ref4, priceItemCode, newHeader, line, ElementNames);
				line.Column1 = trackingId;
			}

			return rawUsage;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		static string PopulateRefColumns(string ref1, string ref2, string ref3, string ref4, string priceItemCode, SummaryLine newHeader, SummaryLine line, ReadOnlyCodeDescriptionPairList eAdaptorElementNames)
		{
			string trackingId = null;

			if (priceItemCode == "EAO")
			{
				if (newHeader != null)
				{
					newHeader.TopLevelDescription += " - " + "Outbound Messages";
				}
				trackingId = ref1;
			}
			else
			{
				string elementCode = priceItemCode.Substring(2);
				bool isInsert = priceItemCode.Substring(1, 1) == "C";
				string elementDesc = eAdaptorElementNames.GetDescriptionFromCode(elementCode);
				if (newHeader != null)
				{
					newHeader.TopLevelDescription += " - " + (elementDesc ?? priceItemCode) + " - " + (isInsert ? "create" : "update");
				}

				switch (elementCode)
				{
					case "1":
						{
							if (newHeader != null)
							{
								newHeader.Column2 = "Transaction No.";
							}
							line.Column2 = ref1;
							trackingId = ref2;
							break;
						}
					case "2":
						{
							if (newHeader != null)
							{
								newHeader.Column2 = "Container No.";
							}
							line.Column2 = ref1;
							trackingId = ref2;
							break;
						}
					case "3":
						{
							if (newHeader != null)
							{
								newHeader.Column2 = "House Bill";
								newHeader.Column3 = "Mode";
							}
							line.Column2 = ref1;
							line.Column3 = ref2;
							trackingId = ref3;
							break;
						}
					case "4":
						{
							if (newHeader != null)
							{
								newHeader.Column2 = "Bill / Ref No.";
								newHeader.Column3 = "Mode";
							}
							line.Column2 = ref1;
							line.Column3 = ref2;
							trackingId = ref3;
							break;
						}
					case "5":
						{
							if (newHeader != null)
							{
								newHeader.Column2 = "Ref No.";
								newHeader.Column3 = "Job No.";
							}
							line.Column2 = ref1;
							line.Column3 = ref2;
							trackingId = ref3;
							break;
						}
					case "6":
						{
							if (newHeader != null)
							{
								newHeader.Column2 = "Ref No.";
								newHeader.Column3 = "Job No.";
							}
							line.Column2 = ref2;
							line.Column3 = ref3;
							trackingId = ref3;
							break;
						}
					case "7":
						{
							if (newHeader != null)
							{
								newHeader.Column2 = "Invoice Line";
								newHeader.Column3 = "Job No.";
							}
							line.Column2 = ref2 + " / " + ref1;
							line.Column3 = ref3;
							trackingId = ref4;
							break;
						}
					case "8":
						{
							if (newHeader != null)
							{
								newHeader.Column2 = "Invoice Line";
								newHeader.Column3 = "Job No.";
							}
							line.Column2 = ref2 + " / " + ref1;
							line.Column3 = ref3;
							trackingId = ref4;
							break;
						}
					case "9":
						{
							if (newHeader != null)
							{
								newHeader.Column2 = "Order No.";
								newHeader.Column3 = "Buyer";
							}
							line.Column2 = ref1;
							line.Column3 = ref2;
							trackingId = ref3;
							break;
						}
					case "A":
						{
							if (newHeader != null)
							{
								newHeader.Column2 = "Order No.";
								newHeader.Column3 = "Part / Line";
							}
							line.Column2 = ref1;
							line.Column3 = ref3;
							trackingId = ref4;
							break;
						}
					case "B":
						{
							if (newHeader != null)
							{
								newHeader.Column2 = "Consign. Ref";
								newHeader.Column3 = "Type";
							}
							line.Column2 = ref1;
							line.Column3 = ref2;
							trackingId = ref3;
							break;
						}
					case "C":
						{
							if (newHeader != null)
							{
								newHeader.Column2 = "Consign. Ref";
							}
							line.Column2 = ref1;
							trackingId = ref3;
							break;
						}
					case "D": // Spot Quote / Quick Bookings / Quoted Bookings
										// Reference1 = JS_UniqueConsignRef or TH_QuoteNumber, Reference2 = 'Quick Booking'/'Quoted Booking' or 'Spot Quote', Reference3 = EI_SessionGUID
						{
							if (newHeader != null)
							{
								newHeader.Column2 = "Quote";
								newHeader.Column3 = "Type";
							}
							line.Column2 = ref1;
							line.Column3 = ref2;
							trackingId = ref3;
							break;
						}
					case "E": // Shipments (Domestic/Import/Export)
										// Reference1 = JS_UniqueConsignRef, Reference2 = 'Domestic Shipment'/'International Shipment', Reference3 = EI_SessionGUID
						{
							if (newHeader != null)
							{
								newHeader.Column2 = "Consign Ref";
								newHeader.Column3 = "Shipment";
							}
							line.Column2 = ref1;
							line.Column3 = ref2;
							trackingId = ref3;
							break;
						}
					case "F": // Domestic Transport Job / Booking
										// Reference1 = KM_JobID, Reference2 = KB_JobID, Reference3 = EI_SessionGUID
						{
							if (newHeader != null)
							{
								newHeader.Column2 = "Job";
								newHeader.Column3 = "Consolidation";
							}
							line.Column2 = ref1;
							line.Column3 = ref2;
							trackingId = ref3;
							break;
						}
					case "G": // events
						{
							if (newHeader != null)
							{
								newHeader.Column2 = "Event";
								newHeader.Column3 = "Table";
							}
							line.Column2 = ref1;
							line.Column3 = ref2;
							trackingId = ref4;
							break;
						}
					case "H": // 17
					case "I": // 18
						{
							if (newHeader != null)
							{
								newHeader.Column2 = "Docket";
								newHeader.Column3 = "Ref";
							}
							line.Column2 = ref1;
							line.Column3 = ref2;
							trackingId = ref4;
							break;
						}
					case "J":
					case "K":
						{
							if (newHeader != null)
							{
								newHeader.Column2 = "Docket Line";
								newHeader.Column3 = "Warehouse";
							}
							line.Column2 = ref1 + " / " + ref3;
							line.Column3 = ref2;
							trackingId = ref4;
							break;
						}
					case "L": // master files
						{
							if (newHeader != null)
							{
								newHeader.Column2 = "Table";
								newHeader.Column3 = "Record PK";
							}
							line.Column2 = ref2;
							line.Column3 = ref1;
							trackingId = ref3;
							break;
						}
					default: break;
				}
			}
			return trackingId;
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			string currentItemCode = "";

			using (var command = GetRawUsageQuery(context))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					string clientId = (string)reader["TX_ClientID"];
					string companyCode = (string)reader["CompanyCode"];
					string ref1 = (string)reader["TX_Reference1"];
					string ref2 = (string)reader["TX_Reference2"];
					string ref3 = (string)reader["TX_Reference3"];
					string ref4 = (string)reader["TX_Reference4"];
					string priceItemCode = (string)reader["TX_PriceItemCode"];
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

					if (string.Compare(priceItemCode, currentItemCode, StringComparison.OrdinalIgnoreCase) != 0)
					{
						var headerColumns = GetCsvHeaderColumns(priceItemCode, isStlBilling);
						var headerCsvLine = new OCsvLine(headerColumns);
						action(headerCsvLine.ToString());
						currentItemCode = priceItemCode;
					}

					var dataValues = GetCsvDataValues(priceItemCode, isStlBilling, clientId, companyCode, ref1, ref2, ref3, ref4, messageTime);
					var dataCsvLine = new OCsvLine(dataValues);
					action(dataCsvLine.ToString());
				}
			}
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, ICsvUsageReportWriter writer)
		{
			using (var command = GetRawUsageQuery(context))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					string clientId = (string)reader["TX_ClientID"];
					string companyCode = (string)reader["CompanyCode"];
					string ref1 = (string)reader["TX_Reference1"];
					string ref2 = (string)reader["TX_Reference2"];
					string ref3 = (string)reader["TX_Reference3"];
					string ref4 = (string)reader["TX_Reference4"];
					string priceItemCode = (string)reader["TX_PriceItemCode"];
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];
					int billableCount = (int)reader["TX_BillableCount"];

					string client = "";
					if (isStlBilling)
					{
						client = clientId.Length == 9 ? clientId.Substring(7) : "";
					}
					else
					{
						client = companyCode;
					}

					writer.WriteCsvUsageReport(messageTime, client, "", "", string.Concat(ref1, ' ', ref2, ' ', ref3, ' ', ref4).Trim(), priceItemCode, context.PriceItemDescription, billableCount);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		string[] GetCsvHeaderColumns(string priceItemCode, bool isStlBilling)
		{
			var client = isStlBilling ? "Company Code" : "Server Code";

			if (priceItemCode == "EAO")
			{
				return new string[] { "Message Type", client, "eHub Tracking ID", "", "", "Message Time (UTC)" };
			}
			else
			{
				string elementCode = priceItemCode.Substring(2);

				switch (elementCode)
				{
					case "1":
						return new string[] { "Message Type", client, "eHub Tracking ID", "Transaction No.", "", "Message Time (UTC)" };
					case "2":
						return new string[] { "Message Type", client, "eHub Tracking ID", "Container No.", "", "Message Time (UTC)" };
					case "3":
						return new string[] { "Message Type", client, "eHub Tracking ID", "House Bill", "Mode", "Message Time (UTC)" };
					case "4":
						return new string[] { "Message Type", client, "eHub Tracking ID", "Bill / Ref No.", "Mode", "Message Time (UTC)" };
					case "5":
					case "6":
						return new string[] { "Message Type", client, "eHub Tracking ID", "Ref No.", "Job No.", "Message Time (UTC)" };
					case "7":
					case "8":
						return new string[] { "Message Type", client, "eHub Tracking ID", "Invoice Line", "Job No.", "Message Time (UTC)" };
					case "9":
						return new string[] { "Message Type", client, "eHub Tracking ID", "Order No.", "Buyer", "Message Time (UTC)" };
					case "A":
						return new string[] { "Message Type", client, "eHub Tracking ID", "Order No.", "Part / Line", "Message Time (UTC)" };
					case "B":
						return new string[] { "Message Type", client, "eHub Tracking ID", "Consign. Ref", "Type", "Message Time (UTC)" };
					case "C":
						return new string[] { "Message Type", client, "eHub Tracking ID", "Consign. Ref", "", "Message Time (UTC)" };
					case "D": // Spot Quote / Quick Bookings / Quoted Bookings
						return new string[] { "Message Type", client, "eHub Tracking ID", "Quote", "Type", "Message Time (UTC)" };
					case "E": // Shipments (Domestic/Import/Export)
						return new string[] { "Message Type", client, "eHub Tracking ID", "Consign. Ref", "Shipment", "Message Time (UTC)" };
					case "F": // Domestic Transport Job / Booking
						return new string[] { "Message Type", client, "eHub Tracking ID", "Job", "Consolidation", "Message Time (UTC)" };
					case "G": // events
						return new string[] { "Message Type", client, "eHub Tracking ID", "Event", "Table", "Message Time (UTC)" };
					case "H": // 17
					case "I": // 18
						return new string[] { "Message Type", client, "eHub Tracking ID", "Docket", "Ref", "Message Time (UTC)" };
					case "J":
					case "K":
						return new string[] { "Message Type", client, "eHub Tracking ID", "Docket Line", "Warehouse", "Message Time (UTC)" };
					case "L": // master files
						return new string[] { "Message Type", client, "eHub Tracking ID", "Table", "Record PK", "Message Time (UTC)" };
					default:
						return Array.Empty<string>();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		string[] GetCsvDataValues(string priceItemCode, bool isStlBilling, string clientId, string companyCode, string ref1, string ref2, string ref3, string ref4, ZDateTime messageTime)
		{
			string client = "";
			if (isStlBilling)
			{
				client = clientId.Length == 9 ? clientId.Substring(7) : "";
			}
			else
			{
				client = companyCode;
			}

			var messageTimeAsText = ToMessageTimeFormat(messageTime);

			if (priceItemCode == "EAO")
			{
				return new string[] { "Outbound Messages", client, ref1, "", "", messageTimeAsText };
			}
			else
			{
				string elementCode = priceItemCode.Substring(2);

				bool isInsert = priceItemCode.Substring(1, 1) == "C";
				string elementDesc = ElementNames.GetDescriptionFromCode(elementCode);
				var messageType = (elementDesc ?? priceItemCode) + " - " + (isInsert ? "create" : "update");

				switch (elementCode)
				{
					case "1":
					case "2":
						return new string[] { messageType, client, ref2, ref1, "", messageTimeAsText };
					case "3":
					case "4":
					case "5":
						return new string[] { messageType, client, ref3, ref1, ref2, messageTimeAsText };
					case "6":
						return new string[] { messageType, client, ref3, ref2, ref3, messageTimeAsText };
					case "7":
					case "8":
						return new string[] { messageType, client, ref4, ref2 + " / " + ref1, ref3, messageTimeAsText };
					case "9":
						return new string[] { messageType, client, ref3, ref1, ref2, messageTimeAsText };
					case "A":
						return new string[] { messageType, client, ref4, ref1, ref3, messageTimeAsText };
					case "B":
						return new string[] { messageType, client, ref3, ref1, ref2, messageTimeAsText };
					case "C":
						return new string[] { messageType, client, ref3, ref1, "", messageTimeAsText };
					case "D": // Spot Quote / Quick Bookings / Quoted Bookings
										// Reference1 = JS_UniqueConsignRef or TH_QuoteNumber, Reference2 = 'Quick Booking'/'Quoted Booking' or 'Spot Quote', Reference3 = EI_SessionGUID
					case "E": // Shipments (Domestic/Import/Export)
										// Reference1 = JS_UniqueConsignRef, Reference2 = 'Domestic Shipment'/'International Shipment', Reference3 = EI_SessionGUID
					case "F": // Domestic Transport Job / Booking
										// Reference1 = KM_JobID, Reference2 = KB_JobID, Reference3 = EI_SessionGUID
						return new string[] { messageType, client, ref3, ref1, ref2, messageTimeAsText };
					case "G": // events
					case "H": // 17
					case "I": // 18
						return new string[] { messageType, client, ref4, ref1, ref2, messageTimeAsText };
					case "J":
					case "K":
						return new string[] { messageType, client, ref4, ref1 + " / " + ref3, ref2, messageTimeAsText };
					case "L": // master files
						return new string[] { messageType, client, ref3, ref2, ref1, messageTimeAsText };
					default:
						return Array.Empty<string>();
				}
			}
		}

		#endregion

		#region SQL

		protected override string Query_Raw_Usage
		{
			get { return sql_Raw_Usage; }
		}

		const string sql_Raw_Usage = @"
SELECT 
	TX_ClientID,
	CompanyCode,
	TX_PriceItemCode,
	TX_BillableCount,
	TX_Reference1,
	TX_Reference2 = ISNULL(TX_Reference2, ''),
	TX_Reference3 = ISNULL(TX_Reference3, ''),
	TX_Reference4 = ISNULL(TX_Reference4, ''),
	TX_ServiceOccuredUTC
FROM
	EdiGetDetailedUsageEAD(@Period, @DatabaseId, @ClientCompanyPk)
ORDER By
	TX_ClientID, case when TX_PriceItemCode = 'EAO' then 'ZZZ' else TX_PriceItemCode end, TX_ServiceOccuredUTC
";

		#endregion
	}
}

