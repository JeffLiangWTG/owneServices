using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
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

		public override CodeDescriptionPairList MessageSubTypeList => Declaration.IsImportSiscomex ? Factory.GetCachedValue<MessageSubTypeList>() : new CodeDescriptionPairList();

		public override CodeDescriptionPairList TransportTypeList => Factory.GetCachedValue<TransportTypeList>();

		public CodeDescriptionPairList CustomsTransportModeList =>
			Factory.GetCachedValue("CustomsTransportModeList_IsImport_" + Declaration.IsImportExcludingLicense, () =>
			{
				var result = new BRTransportModeList();
				if (!Declaration.IsImportExcludingLicense)
				{
					result.RemoveCode(BRTransportModeList.Codes.OTH);
					result.RemoveCode(BRTransportModeList.Codes.FIC);
				}
				return result;
			});

		public override CodeDescriptionPairList IncoTermList => Factory.GetCachedValue<BRIncoTermList>();

		public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<BRMessageStatusList>();

		public OrganisationsFindBoxCollection DeclarantOrganisations
		{
			get { return new OrganisationsFindBoxCollection(Declaration.Factory); }
		}

		public override CodeDescriptionPairList PaymentPartyList =>
			Declaration.IsImportExcludingLicense ? Factory.GetCachedValue("BRPaymentPartyList_Import", () =>
			{
				var result = new CodeDescriptionPairList(base.PaymentPartyList);
				result.RemoveCode(PaymentPartyCodeDescriptionList.Codes.Default);
				return result;
			}) : base.PaymentPartyList;

		public override ICodeDescriptionPairList DeclarantTypeList => Declaration.IsExport ? Factory.GetCachedValue<TypeOfOperationExportList>() : new CodeDescriptionPairList();

		public CodeDescriptionPairList BRDeclarantTypeList => Factory.GetCachedValue<DeclarantTypeList>();

		public CodeDescriptionPairList OperationTypeList => Factory.GetCachedValue<TypeOfOperationImportList>();

		public override ICodeDescriptionPairList CustomsOfficeList => BRRefCusCodeListTypes.GetCustomsOfficeList(Factory);

		public ICodeDescriptionPairList CustomsEnclosureList => BRRefCusCodeListTypes.GetCustomsEnclosureList(Factory);

		public ICodeDescriptionPairList SubLocationOfGoodsList => BRRefCusCodeListTypes.GetWarehousingSectorList(Factory, Declaration.JE_CustomsOffice, Declaration.JE_LocationOfGoods);

		public OrgHeaderCollection InvolvedPartyAddressOrganisations => fInvolvedPartyAddressOrganisations ?? (fInvolvedPartyAddressOrganisations = new OrgHeaderCollection(Factory));

		OrgHeaderCollection fInvolvedPartyAddressOrganisations;

		public OrgHeaderCollection BoardingLocalAddressOrganisations => fBoardingLocalAddressOrganisations ?? (fBoardingLocalAddressOrganisations = new OrgHeaderCollection(Factory));

		OrgHeaderCollection fBoardingLocalAddressOrganisations;

		protected override CodeDescriptionPairList PackingUnitTypesListCore => Declaration.IsImportExcludingLicense ? BRRefCusCodeListTypes.GetPackagesTypesList(Factory) : base.PackingUnitTypesListCore;

		public AccBankAccountCollection BankAccounts => new AccBankAccountCollection(Factory, Parent.Company);

		public override OrgHeaderCollection Consignees => new ConsigneeCollection(Factory);

		public CodeDescriptionPairList BillTypeList => Factory.GetCachedValue<BillTypeList>();

		protected override IEnumerable<ZString> GetNonSupportedMessageTypeCodes()
		{
			if (!Parent.FixedJobMessageType.IsEmpty)
			{
				return Factory.GetCachedValue<BRJobMessageTypeList>().GetAllCodesZString().Except(Parent.FixedJobMessageType);
			}
			else
			{
				switch (Parent.JE_MessageType)
				{
					case BRJobMessageTypeList.Codes.ImportLicense:
						return Factory.GetCachedValue<BRJobMessageTypeList>().GetAllCodesZString().Except(BRJobMessageTypeList.Codes.ImportLicense);
					case BRJobMessageTypeList.Codes.LPCO:
						return Factory.GetCachedValue<BRJobMessageTypeList>().GetAllCodesZString().Except(BRJobMessageTypeList.Codes.LPCO);
					default:
						return new ZString[] { BRJobMessageTypeList.Codes.ImportLicense, BRJobMessageTypeList.Codes.LPCO };
				}
			}
		}

		protected override CodeDescriptionPairList GetEntryStatusListInZZ(ZString codeType, ZString fallback)
		{
			var prefix = Parent.IsExport ? EntryStatusListHelper.ExportEntryStatusPrefix :
				Parent.IsImportLicense ? EntryStatusListHelper.ImportLicenseStatusPrefix :
				Parent.IsImportSiscomex ? EntryStatusListHelper.ImportSiscomexEntryStatusPrefix : null;

			return Factory.GetCachedValue($"EntryStatusListInZZ_{prefix}_{ZDateTime.Today}", () =>
			{
				var entryStatusList = base.GetEntryStatusListInZZ(codeType, fallback);
				return prefix == null ? entryStatusList : entryStatusList.GetEntryStatusList(prefix);
			});
		}

		public CodeDescriptionPairList SpecialTransportModesList => Factory.GetCachedValue<SpecialTransportModesList>();

		public CodeDescriptionPairList DispatchModalityList => Declaration.IsImportSiscomex ? Factory.GetCachedValue<DispatchModalityCodes>() : Factory.GetCachedValue<SpecialDispatchModalityCodes>();

		public CodeDescriptionPairList CargoArrivalDocumentUtilizationList => Factory.GetCachedValue<BRUtilizationList>();

		public CodeDescriptionPairList CargoArrivalDocumentList
		{
			get
			{
				var cacheKey = "BRCargoArrivalDocumentList_" + Declaration.JE_TransportMode;
				return Factory.GetCachedValue(cacheKey, () =>
				{
					var result = new BRCargoArrivalDocList();
					if (Declaration.IsAir)
					{
						result.RemoveCode(BRCargoArrivalDocList.Codes.CargoManifest);
						result.RemoveCode(BRCargoArrivalDocList.Codes.MICDTA);
					}
					else if (Declaration.IsSea || Declaration.IsRiver || Declaration.IsLake || Declaration.IsRail)
					{
						result.RemoveCode(BRCargoArrivalDocList.Codes.EntryTerm);
						result.RemoveCode(BRCargoArrivalDocList.Codes.MICDTA);
					}
					else if (Declaration.IsRoad)
					{
						result.RemoveCode(BRCargoArrivalDocList.Codes.EntryTerm);
					}
					return result;
				});
			}
		}
	}
}
