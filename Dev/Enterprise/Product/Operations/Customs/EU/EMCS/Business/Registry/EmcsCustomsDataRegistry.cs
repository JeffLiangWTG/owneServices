using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.EMCS.Registry
{
	public sealed class EmcsCustomsDataRegistry : RegistryItemSet, Integration.Customs.EUEMCS.IEmcsCustomsDataRegistry
	{
		public static EmcsCustomsDataRegistry Instance => instance ?? (instance = new EmcsCustomsDataRegistry());

		[ThreadStatic]
		static EmcsCustomsDataRegistry instance;

		EmcsCustomsDataRegistry()
		{
		}

		public override bool IsForProductivityWise => false;

		protected override void SetDefaultsForNewItem(IRegistryItem item)
		{
			base.SetDefaultsForNewItem(item);
			item.CountryFilterPKs = Core.CountryGuids.CountriesUnderEUCustomsJurisdiction;
		}

		public BooleanRegistryItem EnableEmcsFunctions
		{
			get
			{
				return GetItem("EnableEmcsFunctions", delegate
				{
					return new BooleanRegistryItem(
						new RegistryItemImplWithDynamicDefaultValue("EnableEmcsFunctions",
							RawDataRegistry.Categories.Customs_EuropeanUnionCommon_EMCS,
							ResString.GetMultilingualString("C2F375A0-C5F7-4181-A1A0-905928F9E81B", "Enable EMCS Functions"),
							ResString.GetMultilingualString("906A7A82-F364-4A43-AE0D-80602F9807B3", "Set to YES to enable EMCS module"),
							RegistryDataTypes.BoolType,
							RegistryStorageFlags.Company,
							RegistryOptions.IsOnlyForSupport,
							(companyPK, branchPK, departmentPK) => IsGBCompany(companyPK))
					);
				});
			}
		}

		bool IsGBCompany(Guid companyPK)
		{
			var query = new ZDBOnlyQuery(typeof(GlbCompany));
			query.AddToFilter(GlbCompanySchema.PK, companyPK);
			query.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedKingdom);
			var factory = new BusinessObjectFactory();
			var results = factory.Load<GlbCompany>(query)?.FirstOrDefault();
			return results != null;
		}

		IRegistryItem Integration.Customs.EUEMCS.IEmcsCustomsDataRegistry.EnableEmcsFunctions => EnableEmcsFunctions;

		public GroupNotificationRegistryItem<EmcsGroupNotification> EmcsSendMessageErrors
		{
			get
			{
				return GetItem("EmcsSendMessageErrorsRegistry", () =>
				{
					return new GroupNotificationRegistryItem<EmcsGroupNotification>("EmcsSendMessageErrorsRegistry",
						CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_EMCS,
						ResString.GetMultilingualString("5CF77E7F-0811-44A4-9FD2-E485425536EF", "Send EMCS Errors To"),
						ResString.GetMultilingualString("231349C4-0AE7-4293-B873-941688300BC7", "Send EMCS message errors to staff member, nominated group or combination of both"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						new EmcsGroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty));
				});
			}
		}

		public GroupNotificationRegistryItem<EmcsGroupNotification> EmcsSendConsigneeAcknowledgements
		{
			get
			{
				return GetItem("EmcsSendConsigneeAcknowledgementsRegistry", () =>
				{
					return new GroupNotificationRegistryItem<EmcsGroupNotification>("EmcsSendConsigneeAcknowledgementsRegistry",
						CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_EMCS,
						ResString.GetMultilingualString("1DA4592A-6299-41D5-B308-C9B0419BDCFA", "Send EMCS Consignee Acknowledgements To"),
						ResString.GetMultilingualString("B3BAC82C-0C85-4F06-A49B-C59BC5B8CFF6", "Send EMCS consignee acknowledgements to staff member, nominated group or combination of both"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						new EmcsGroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty));
				});
			}
		}

		public GroupNotificationRegistryItem<EmcsGroupNotification> EmcsSendConsignorAcknowledgements
		{
			get
			{
				return GetItem("EmcsSendConsignorAcknowledgementsRegistry", () =>
				{
					return new GroupNotificationRegistryItem<EmcsGroupNotification>("EmcsSendConsignorAcknowledgementsRegistry",
						CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_EMCS,
						ResString.GetMultilingualString("B0942FA6-8AC5-42E7-9B0B-AD054DDFE2E9", "Send EMCS Consignor Acknowledgements To"),
						ResString.GetMultilingualString("54284BB8-9906-4280-9E8E-ECCDA4540EB0", "Send EMCS consignor acknowledgements to staff member, nominated group or combination of both"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						new EmcsGroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty));
				});
			}
		}
	}
}
