using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class JobDeclarationLookups : Customs.Business.JobDeclarationLookups
	{
		public JobDeclarationLookups(JobDeclaration parent)
			: base(parent)
		{
		}

		public JobDeclaration Declaration
		{
			get { return Parent; }
		}

		protected new JobDeclaration Parent
		{
			get { return (JobDeclaration)base.Parent; }
		}

		#region Collections

		public OrganisationsFindBoxCollection Organizations
		{
			get { return new OrganisationsFindBoxCollection(Declaration.Factory); }
		}

		public override CodeDescriptionPairList TransportTypeList
		{
			get { return Factory.GetCachedValue<TransportTypeList>(); }
		}

		public CodeDescriptionPairList DLMTransportTypeList
		{
			get { return Factory.GetCachedValue<DLMTransportTypeList>(); }
		}

		public CodeDescriptionPairList CBSATransportTypeList
		{
			get { return Factory.GetCachedValue<CBSATransportTypeList>(); }
		}

		public override CodeDescriptionPairList PaymentPartyList
		{
			get
			{
				if (Parent.IsB3X)
				{
					return Factory.GetCachedValue<B3XPaymentCodeList>();
				}
				else
				{
					return base.PaymentPartyList;
				}
			}
		}

		public ZZRefCarrierCombinedCollection JE_CarrierCodeList => ZZRefCarrierCombinedCollectionExtension.GetCachedCollection(Factory, Parent.JE_TransportMode);

		public override CodeDescriptionPairList EntryStatusList
		{
			get { return Factory.GetCachedValue<EDIReleaseImportEntryStatusList>(); }
		}

		#region MessageStatusList

		public override CodeDescriptionPairList MessageStatusList
		{
			get { return new MessageStatusList(new MessageTypeList().GetMultilingualDescriptionFromCode(GetMessageType())); }
		}

		string GetMessageType()
		{
			var messageType = string.Empty;
			if (Parent.IsExport)
			{
				messageType = CACustomsDataRegistry.Instance.SendG7ExportMessages.Value
								? Business.MessageTypeList.Codes.G7Export
								: Business.MessageTypeList.Codes.DataLoadingModule;
			}
			else if (Parent.IsImport)
			{
				var header = Parent.GetEntryHeaderOfLastSentMessage();
				if (header != null)
				{
					messageType = header.CH_MessageType;
				}
			}
			return messageType;
		}

		#endregion

		public CodeDescriptionPairList EditableMessageTypeList
		{
			get
			{
				var keyBuilder = new ZStringBuilder("EditableMessageTypeList");
				foreach (var code in GetNonSupportedMessageTypeCodes())
				{
					keyBuilder.Append(code);
				}
				return Factory.GetCachedValue(keyBuilder.ToStringWithDelimiterBetweenAppends("-"), () =>
				{
					var result = new CodeDescriptionPairList();
					foreach (ICodeDescription pair in MessageTypeList)
					{
						switch (pair.Code)
						{
							case JobMessageTypeList.Codes.LowValueShipments:
							case JobMessageTypeList.Codes.B2Adjustments:
							case JobMessageTypeList.Codes.LVSForConsolidation:
							case JobMessageTypeList.Codes.ImportCopyforB2:
							case JobMessageTypeList.Codes.XTypeEntry:
								// do nothing
								break;
							default:
								result.Add(pair);
								break;
						}
					}
					return result;
				});
			}
		}

		public override CodeDescriptionPairList MessageSubTypeList
		{
			get
			{
				CodeDescriptionPairList subTypes;
				if (Declaration.IsLVS)
				{
					subTypes = Factory.GetCachedValue<LowValueShipmentsTypes>();
				}
				else if (Declaration.IsImport)
				{
					if (Declaration.IsCADEnabled)
					{
						subTypes = Factory.GetCachedValue("CADEntryTypeList", () =>
						{
							var result = new CADEntryTypeList();
							result.RemoveCode(CADEntryTypeList.Codes.LowValueShipments);
							return result;
						});
					}
					else
					{
						subTypes = Factory.GetCachedValue("CAB3EntryTypeList", () =>
						{
							var result = new B3EntryTypeList();
							result.RemoveCode(B3EntryTypeList.Codes.LowValueShipments);
							return result;
						});
					}
				}
				else
				{
					return new CodeDescriptionPairList();
				}
				return subTypes;
			}
		}

		public override CodeDescriptionPairList CargoIdTypeList
		{
			get
			{
				return Factory.GetCachedValue("CACargoIdTypeList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
					result.AddPair(Core.Constants.ContainerModes.BreakBulk, Core.Constants.ContainerModeDescriptions.BreakBulk);
					result.AddPair(Core.Constants.ContainerModes.Bulk, Core.Constants.ContainerModeDescriptions.Bulk);
					result.AddPair(Core.Constants.ContainerModes.Liquid, Core.Constants.ContainerModeDescriptions.Liquid);
					return result;
				});
			}
		}

		public override CodeDescriptionPairList JE_TotalNoOfPacksPackType_List
		{
			get
			{
				if (Parent.IsIID)
				{
					return Universal.RefCusCodeListTypes.GetCachedListValidBeforeDate(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
						Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, UniversalReferenceConstants.UNPackTypeStartDate);
				}
				else
				{
					return Factory.GetCachedValue<ACROSSPackageTypes>();
				}
			}
		}

		public override CodeDescriptionPairList MergeByList
		{
			get
			{
				if (Parent.IsLVS)
				{
					return Factory.GetCachedValue("CALVSMergeByList", () =>
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(JobMessageTypeList.Codes.LowValueShipments, Res.GetString("9f4d282f-3f91-4392-8c33-40263d99a32d", "LVS Data Merge"));
						return result;
					});
				}
				return Factory.GetCachedValue<B3MergeByList>();
			}
		}

		public ZZRefCusCodeListCombinedCollection CBSAOffices
		{
			get
			{
				return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
			}
		}

		public CACSubLocationCollection SubLocationCodes
		{
			get
			{
				var result = Factory.GetCachedValue("CACSubLocation", () =>
				{
					return new CACSubLocationCollection(Factory);
				});
				result.FilterBusinessObjectDefaults.RemoveAll();
				if (!Parent.JE_LocationOfGoods.IsEmpty)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Sub-Location Code", "Property", Parent.JE_LocationOfGoods, true));
				}
				if (!Parent.JE_CustomsOffice.IsEmpty)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Port", "Property", Parent.JE_CustomsOffice, false));
				}
				return result;
			}
		}

		public CACSubLocationCollection ExamLocationCodes
		{
			get
			{
				var result = Factory.GetCachedValue("ExamLocationCodes", () =>
				{
					return new CACSubLocationCollection(Factory);
				});
				result.FilterBusinessObjectDefaults.RemoveAll();
				if (!Parent.CA_ExamLocationCode.IsEmpty)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Sub-Location Code", "Property", Parent.CA_ExamLocationCode, true));
				}
				if (!Parent.JE_CustomsOffice.IsEmpty)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Port", "Property", Parent.JE_CustomsOffice, false));
				}
				return result;
			}
		}
		#endregion

		#region override

		protected override InvoiceHeaderWithNoDeclarationCollection GetInvoicesToAttachCore()
		{
			return new CAAttachInvoiceCollection(Declaration);
		}

		public class CAAttachInvoiceCollection : AttachInvoiceCollection
		{
			public CAAttachInvoiceCollection(BaseJobDeclaration declaration)
				: base(declaration)
			{
			}
		}

		#endregion
	}
}
