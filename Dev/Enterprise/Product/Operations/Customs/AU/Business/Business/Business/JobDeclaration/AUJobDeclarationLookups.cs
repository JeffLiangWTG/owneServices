using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUJobDeclarationLookups : JobDeclarationLookups
	{
		public AUJobDeclarationLookups(JobDeclaration declaration)
			: base(declaration)
		{
		}

		JobDeclaration Declaration
		{
			get { return (JobDeclaration)Parent; }
		}

		public OrgHeaderCollection AllOrganisations
		{
			get { return allOrganisations ?? (allOrganisations = new OrgHeaderCollection(Declaration.Factory)); }
		}
		OrgHeaderCollection allOrganisations;

		public OrgHeaderCollection AQISLoadingEstablishmentLocationOrganisations
		{
			get
			{
				if (aqisLoadingEstablishmentLocationOrganisations == null)
				{
					aqisLoadingEstablishmentLocationOrganisations = new OrgHeaderCollection(Declaration.Factory);
					aqisLoadingEstablishmentLocationOrganisations.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Registration Country/Type", "Property1", (ZString)Core.Constants.CountryCodes.Australia, true));
					aqisLoadingEstablishmentLocationOrganisations.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Registration Country/Type", "Property2", (ZString)OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber, true));
				}
				return aqisLoadingEstablishmentLocationOrganisations;
			}
		}
		OrgHeaderCollection aqisLoadingEstablishmentLocationOrganisations;

		public ICodeDescriptionPairList CountryList
		{
			get
			{
				return Factory.GetCachedValue("CMRICAOCountryCodes", new GetValueDelegate<CodeDescriptionPairList>(delegate
					{
						return new CMRICAOCountryCodes();
					}));
			}
		}

		public ICodeDescriptionPairList Gender
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Gender); }
		}

		public override CodeDescriptionPairList PaymentPartyList
		{
			get
			{
				var key = GetPaymentPartyListKey();
				return Factory.GetCachedValue(
					"AUJobDeclarationLookups.PaymentPartyList" + key,
					delegate
					{
						CodeDescriptionPairList paymentList;
						switch (key)
						{
							case "Drawback":
								paymentList = new CodeDescriptionPairList();
								paymentList.AddPair(JobDeclaration.PaymentMethods.Broker, PaymentPartyCodeDescriptionList.Descriptions.Broker);
								paymentList.AddPair(JobDeclaration.PaymentMethods.DrawbackClaimant, "Drawback Claimant");
								paymentList.AddPair(JobDeclaration.PaymentMethods.SecondBroker, "Second Broker Payment Account");
								break;
							default:
								paymentList = new PaymentPartyCodeDescriptionList();
								paymentList.AddPair(JobDeclaration.PaymentMethods.SecondBroker, "Second Broker Payment Account");
								paymentList.AddPair(JobDeclaration.PaymentMethods.Cash, "Cash Payment over Customs Counter");
								break;
						}
						return paymentList;
					});
			}
		}

		string GetPaymentPartyListKey()
		{
			return Declaration.IsDrawback ? "Drawback" : "Other";
		}

		#region CodeDescriptionPairList

		public override CodeDescriptionPairList EntryStatusList
		{
			get
			{
				return Factory.GetCachedValue("AUDeclarationEntryStatusList" + (Declaration.IsImportCMR ? "Import" : "Other"), () =>
				{
					var statusList = new CodeDescriptionPairList();
					if (Declaration.IsImportCMR)
					{
						statusList.AddRange(Factory.GetCachedValue<CMRImportEntryAdviceList>());
					}
					else
					{
						statusList.AddRange(Factory.GetCachedValue<LegacyCustomsEntryStatusList>());
					}
					statusList.AddRange(Factory.GetCachedValue<ConsolidatedEntryStatusList>());
					return statusList;
				});
			}
		}

		public override CodeDescriptionPairList MessageStatusList
		{
			get
			{
				var key = GetMessageStatusListKey();
				return Factory.GetCachedValue("AUDeclarationMessageStatusList" + key, () =>
				{
					switch (key)
					{
						case "Import":
							return Factory.GetCachedValue<CMRImportMessageStatusList>();
						case "Export":
							return Factory.GetCachedValue<CMRExportOtherMessageStatusList>();
						case "Drawback":
							return Factory.GetCachedValue<LegacyCustomsEntryStatusList>();
						default:
							return Factory.GetCachedValue<CodeDescriptionPairList>();
					}
				});
			}
		}

		string GetMessageStatusListKey()
		{
			var result = "";
			if (Declaration.IsImportCMR)
			{
				result = "Import";
			}
			else if (Declaration.IsExport)
			{
				result = "Export";
			}
			else if (Declaration.IsDrawback)
			{
				result = "Drawback";
			}
			else
			{
				result = "Other";
			}
			return result;
		}

		protected override IEnumerable<ZString> GetNonSupportedMessageTypeCodes()
		{
			if (!Declaration.SupportsBondedWarehousing)
			{
				yield return AUJobMessageTypeList.Codes.WarehousedByExternalAgent;
			}
		}

		public override CodeDescriptionPairList MessageSubTypeList
		{
			get
			{
				var key = GetMessageSubTypeListKey();
				return Factory.GetCachedValue("AUDeclarationMessageSubTypeList" + key, () =>
				{
					var result = new CodeDescriptionPairList();
					switch (key)
					{
						case "ImportCMRPost":
						case "ImportCMROther":
							result.AddPair(JobDeclaration.MessageSubType.FormalEntry, ResString.GetMultilingualString("AU|MessageSubTypeList|15FDE4E9-EB78-41F1-8E90-2155D7F91A2C", "Formal Entry"));
							if (key == "ImportCMROther")
							{
								result.AddPair(JobDeclaration.MessageSubType.SelfAssessedClearance, ResString.GetMultilingualString("AU|MessageSubTypeList|E04DE16C-026C-4D72-9DEC-E3E1AB724738", "Self Assessed Clearance"));
							}
							result.AddPair(JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines, ResString.GetMultilingualString("AU|MessageSubTypeList|7BCE01E8-D74F-4D2E-A5B5-906C1A0CFD83", "Self assessed clearance with lines (for alcohol and tobacco)"));
							result.AddPair(JobDeclaration.MessageSubType.RequestForCargoRelease, ResString.GetMultilingualString("AU|MessageSubTypeList|F8196835-5303-4710-9E58-824D16AECEE1", "Request for Cargo Release (Not Yet Implemented)"));
							result.AddPair(JobDeclaration.MessageSubType.PeriodDeclarationType1, ResString.GetMultilingualString("AU|MessageSubTypeList|C64DE9F9-5B43-414A-8C17-367EACA8CA7A", "Period Declaration Type 1 (Detailed - Not Yet Implemented)"));
							result.AddPair(JobDeclaration.MessageSubType.PeriodDeclarationType2, ResString.GetMultilingualString("AU|MessageSubTypeList|30E353D5-9D07-4544-BAB6-62CA134614E1", "Period Declaration Type 2 (Compressed - Not Yet Implemented)"));
							break;
						case "Import":
							result.AddPair(JobDeclaration.MessageSubType.FormalEntry, ResString.GetMultilingualString("AU|MessageSubTypeList|15FDE4E9-EB78-41F1-8E90-2155D7F91A2C", "Formal Entry"));
							result.AddPair(JobDeclaration.MessageSubType.SimplifiedEntry, ResString.GetMultilingualString("AU|MessageSubTypeList|E22F1140-9292-4D16-B7B6-8109264BF8DF", "Simplified Entry"));
							result.AddPair(JobDeclaration.MessageSubType.RequestForCargoRelease, ResString.GetMultilingualString("AU|MessageSubTypeList|BF59CB25-2CA5-44BC-96D3-93B168607438", "Request for Cargo Release"));
							result.AddPair(JobDeclaration.MessageSubType.PeriodDeclarationType1, ResString.GetMultilingualString("AU|MessageSubTypeList|FAC4BAD0-C0E2-4AC1-912B-A1EF324B2713", "Period Declaration Type 1 (Detailed)"));
							result.AddPair(JobDeclaration.MessageSubType.PeriodDeclarationType2, ResString.GetMultilingualString("AU|MessageSubTypeList|2A2ED5F8-7BB3-4545-B4ED-35819AD19496", "Period Declaration Type 2 (Compressed)"));
							break;
						case "Drawback":
							result.AddPair(JobDeclaration.MessageSubType.FormalEntry, ResString.GetMultilingualString("AU|MessageSubTypeList|15FDE4E9-EB78-41F1-8E90-2155D7F91A2C", "Formal Entry"));
							break;
						default:
							result.AddPair(JobDeclaration.MessageSubType.NonConfirming, ResString.GetMultilingualString("AU|MessageSubTypeList|841086E5-F18C-44DC-906F-61551B8B6673", "Non-Confirming"));
							result.AddPair(JobDeclaration.MessageSubType.Confirming, ResString.GetMultilingualString("AU|MessageSubTypeList|E04345D5-3756-4BC7-9560-5D131637AE00", "Confirming"));
							result.AddPair(JobDeclaration.MessageSubType.Manual, ResString.GetMultilingualString("AU|MessageSubTypeList|E8538867-A959-4A31-8871-094C3937E6FE", "Manual"));
							break;
					}
					return result;
				});
			}
		}

		string GetMessageSubTypeListKey()
		{
			string result = "";
			if (Declaration.IsImportCMR)
			{
				result = Declaration.IsPost ? "ImportCMRPost" : "ImportCMROther";
			}
			else if (Declaration.IsImport)
			{
				result = "Import";
			}
			else if (Declaration.IsDrawback)
			{
				result = "Drawback";
			}
			else
			{
				result = "Other";
			}
			return result;
		}

		public override CodeDescriptionPairList ApplicationCodeList => Factory.GetCachedValue($"AUDeclarationApplicationCodeList", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages, "Send CMR Message");
			result.AddPair(DeclarationApplicationCodeList.Codes.Interfaced, DeclarationApplicationCodeList.Descriptions.Interfaced);
			return result;
		});

		public override CodeDescriptionPairList CargoIdTypeList
		{
			get
			{
				var key = GetCargoIdTypeListKey();
				return Factory.GetCachedValue("AUDeclarationCargoIdTypeList" + key, () =>
				{
					var result = new CodeDescriptionPairList();
					switch (key)
					{
						case "Air":
							result.AddPair(Core.Constants.ContainerModes.AIR, "Air Waybill");
							break;
						case "Post":
							result.AddPair(Core.Constants.ContainerModes.NonContainerised, "Post, No Container");
							break;
						case "OTH_NonContainerised":
							result.AddPair(Core.Constants.ContainerModes.Other, Core.Constants.ContainerModeDescriptions.Other);
							break;
						case "ImportCMR":
							result.AddPair(Core.Constants.ContainerModes.FCL, Core.Constants.ContainerModeDescriptions.FCL);
							result.AddPair(Core.Constants.ContainerModes.LCL, Core.Constants.ContainerModeDescriptions.LCL);
							result.AddPair(Core.Constants.ContainerModes.FCLMixedShipper, Core.Constants.ContainerModeDescriptions.FCLMixedShipper);
							result.AddPair(Core.Constants.ContainerModes.BreakBulk, Core.Constants.ContainerModeDescriptions.BreakBulk);
							result.AddPair(Core.Constants.ContainerModes.Bulk, Core.Constants.ContainerModeDescriptions.Bulk);
							result.AddPair(Core.Constants.ContainerModes.Liquid, Core.Constants.ContainerModeDescriptions.Liquid);
							break;
						case "Import":
							result.AddPair(Enterprise.Core.Constants.ContainerModes.Containerised, Enterprise.Core.Constants.ContainerModeDescriptions.Containerised);
							result.AddPair(Enterprise.Core.Constants.ContainerModes.BreakBulk, Enterprise.Core.Constants.ContainerModeDescriptions.BreakBulk);
							result.AddPair(Enterprise.Core.Constants.ContainerModes.Bulk, Enterprise.Core.Constants.ContainerModeDescriptions.Bulk);
							result.AddPair(Enterprise.Core.Constants.ContainerModes.Liquid, Enterprise.Core.Constants.ContainerModeDescriptions.Liquid);
							break;
						default:
							result.AddPair(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
							result.AddPair(Core.Constants.ContainerModes.NonContainerised, Core.Constants.ContainerModeDescriptions.NonContainerised);
							result.AddPair(Core.Constants.ContainerModes.Combination, Core.Constants.ContainerModeDescriptions.Combination);
							result.AddPair(Core.Constants.ContainerModes.Bulk, Core.Constants.ContainerModeDescriptions.Bulk);
							result.AddPair(Core.Constants.ContainerModes.Liquid, Core.Constants.ContainerModeDescriptions.Liquid);
							break;
					}

					return result;
				});
			}
		}

		string GetCargoIdTypeListKey()
		{
			string result = "";
			if (Declaration.IsAir)
			{
				result = "Air";
			}
			else if (Declaration.IsPost)
			{
				result = "Post";
			}
			else if (Declaration.IsOther)
			{
				result = "OTH_NonContainerised";
			}
			else if (Declaration.IsImport)
			{
				if (Declaration.IsImportCMR || Declaration.JE_ApplicationCode.IsEmpty)
				{
					result = "ImportCMR";
				}
				else
				{
					result = "Import";
				}
			}
			else
			{
				result = "Other";
			}

			return result;
		}

		public override CodeDescriptionPairList TransportTypeList
		{
			get
			{
				return Factory.GetCachedValue("AUDeclarationTransportTypeList" + (Declaration.IsImportCMR ? "ImportCMR" : "Other"), () =>
					{
						var result = new CodeDescriptionPairList(Factory.GetCachedValue<TransportTypeList>());
						if (Declaration.IsImportCMR)
						{
							result.AddPair(Core.Constants.TransportModes.Other, Core.Constants.TransportModeDescriptions.Other);
						}

						return result;
					});
			}
		}

		public override CodeDescriptionPairList ConsolidatedCargoStatusList
		{
			get { return Factory.GetCachedValue<CMRConsolidatedCargoStatuses>(); }
		}

		#endregion

		#region Implementation

		protected override CodeDescriptionPairList GetJE_ExportGoodsType_List()
		{
			return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.ExportGoodsType);
		}

		#endregion

		#region InvoicesToAttach
		protected override InvoiceHeaderWithNoDeclarationCollection GetInvoicesToAttachCore()
		{
			return new AUAttachInvoiceCollection(Declaration);
		}

		public class AUAttachInvoiceCollection : AttachInvoiceCollection
		{
			public AUAttachInvoiceCollection(JobDeclaration declaration)
				: base(declaration)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Core.Constants.AUCustoms.CommercialInvoiceFilter.ExporterReference, "Property", ZString.Empty)); // This is not a database field. It is a property on the FilterBusinessObject which we cannot access using the schema.
			}
		}
		#endregion
	}
}
