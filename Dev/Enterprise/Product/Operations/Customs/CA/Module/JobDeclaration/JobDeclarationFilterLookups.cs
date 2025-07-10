using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public class JobDeclarationFilterLookups : Customs.Module.JobDeclarationFilterLookups
	{
		public JobDeclarationFilterLookups(CommonJobDeclarationFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		public CodeDescriptionPairList MessageStatusListForFilter(ZString messageTypeCode)
		{
			return Factory.GetCachedValue($"{messageTypeCode}StatusList", delegate
			{
				var result = new CodeDescriptionPairList();
				result.AddRange(new MessageStatusList(new MessageTypeList().GetMultilingualDescriptionFromCode(messageTypeCode)));
				result.RemoveCode(Common.Shared.MessageStatusList.Codes.NotSent);
				result.RemoveCode(Common.Shared.MessageStatusList.Codes.Unknown);
				result.AddPairIfNotExist(Customs.Module.DeclarationFilterConstants.EntryStatus.NotSentForFilter, Common.Shared.MessageStatusList.Descriptions.NotSent);
				result.Sort();
				return result;
			});
		}

		public CodeDescriptionPairList ReleaseStatusList()
		{
			return Factory.GetCachedValue("ReleaseStatusList", delegate
			{
				var result = new CodeDescriptionPairList();
				result.AddRange(Factory.GetCachedValue<Business.EDIReleaseImportEntryStatusList>());
				result.AddPairIfNotExist(ExtraConstantCodes.Codes.NotReleased, ExtraConstantCodes.Descriptions.NotReleased);
				return result;
			});
		}

		public override CodeDescriptionPairList EntryStatusList()
		{
			return Factory.GetCachedValue("CAEntryStatusList", delegate
			{
				var result = new CodeDescriptionPairList();
				result.AddRange(Factory.GetCachedValue<B3EntryStatusList>());
				if (UniversalReferenceConstants.IsCarmR2)
				{
					result.AddRange(Factory.GetCachedValue<CADEntryStatusList>());
				}
				result.AddPairIfNotExist(ExtraConstantCodes.Codes.NotEntryAccepted, ExtraConstantCodes.Descriptions.NotEntryAccepted);
				return result;
			});
		}

		protected override CodeDescriptionPairList GetMainMessageStatusList() => Factory.GetCachedValue<MessageStatusList>();

		public override CodeDescriptionPairList MessageSubTypeList()
		{
			return Factory.GetCachedValue("CAB3EntryTypeList", delegate
			{
				var result = new B3EntryTypeList();
				result.AddRange(new LowValueShipmentsTypes());
				result.AddRangeOverwriteIfExists(new CADEntryTypeList());
				result.RemoveCode(B3EntryTypeList.Codes.LowValueShipments);
				return result;
			});
		}

		public override CodeDescriptionPairList MessageTypeList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("CAMessageTypeList", delegate
				{
					return new JobMessageTypeList();
				});
			}
		}

		public OrganisationsFindBoxCollection OrganisationList
		{
			get
			{
				return Factory.GetCachedValue("OrganisationList", delegate
				{
					return new OrganisationsFindBoxCollection(Factory);
				});
			}
		}

		public DeclarationJobDocAddressCollection JobDocAddressList
		{
			get
			{
				return Factory.GetCachedValue("JobDocAddressList", delegate
				{
					return new DeclarationJobDocAddressCollection(Factory);
				});
			}
		}

		public ZZRefCusCodeListCombinedCollection CBSAOffices
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		public ZZRefCarrierCombinedCollection CarrierCodes => ZZRefCarrierCombinedCollectionExtension.GetCachedCollection(Factory, ZString.Empty);

		public CACSubLocationCollection SubLocationCodes
		{
			get
			{
				return Factory.GetCachedValue("CACSubLocationCollection", delegate
				{
					return new CACSubLocationCollection(Factory);
				});
			}
		}

		public ACROSSServiceOptions ServiceOptions
		{
			get { return Factory.GetCachedValue<ACROSSServiceOptions>(); }
		}

		public CodeDescriptionPairList B2Types
		{
			get { return Factory.GetCachedValue<B2TypeList>(); }
		}

		public CodeDescriptionPairList DIFMessageStatusList => Factory.GetCachedValue<Common.CA.DIF.StatusList>();

		public virtual RefCountryCollection Countries
		{
			get
			{
				return Factory.GetCachedValue("CARefCountryCollection", delegate
				{
					return new RefCountryCollection(Factory);
				});
			}
		}

		public AVSStatusList AVSStatusCodes
		{
			get { return Factory.GetCachedValue<AVSStatusList>(); }
		}

		public CodeDescriptionPairList CAInitiatedByList
		{
			get { return Factory.GetCachedValue<CAInitiatedByList>(); }
		}

		public CodeDescriptionPairList ExceptionCodes
		{
			get
			{
				return Factory.GetCachedValue("ExceptionCodes", delegate
				{
					var result = new CodeDescriptionPairList();
					result.AddRange(Factory.GetCachedValue<CAExceptionCodeList>());
					result.AddPairIfNotExist(ExtraConstantCodes.Codes.ExceptionNotBlank, ExtraConstantCodes.Descriptions.ExceptionNotBlank);
					return result;
				});
			}
		}

		public CodeDescriptionPairList BondTypeList => Factory.GetCachedValue<BondTypeList>();
	}
}
