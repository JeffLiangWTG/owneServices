using System;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class CrossTradeDebtorConfigurationRegistryItem : StronglyTypedRegistryItem<CrossTradeDebtorConfigurationHeader>
	{
		public CrossTradeDebtorConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option, object defaultValue)
			: base(new CrossTradeDebtorConfigurationRegistryItemImpl(name, category, caption, hint, storage, option, defaultValue))
		{
		}

		class CrossTradeDebtorConfigurationRegistryItemImpl : RegistryItemImpl
		{
			public CrossTradeDebtorConfigurationRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, object defaultValue)
				: base(name, category, caption, hint, new CrossTradeDebtorConfigurationRegistryDataType(), storage, options, defaultValue)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				var header = new CrossTradeDebtorConfigurationHeader();

				var configuration = header.Configurations.AddNew();
				configuration.JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;
				configuration.DirectionCode = Constants.FreightShipmentDirection.Code.Other;
				configuration.Mode = JobConfigurationSelectorLookups.ModeAdditionalCodes.All;
				configuration.ChargePaymentType = PrepaidCollectFreightForwardingList.Codes.CCX;
				configuration.Debtor = DefaultDebtorList.Codes.CollectBillToParty;

				configuration = header.Configurations.AddNew();
				configuration.JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;
				configuration.DirectionCode = Constants.FreightShipmentDirection.Code.Other;
				configuration.Mode = JobConfigurationSelectorLookups.ModeAdditionalCodes.All;
				configuration.ChargePaymentType = PrepaidCollectFreightForwardingList.Codes.PPD;
				configuration.Debtor = DefaultDebtorList.Codes.PrepaidBillToParty;

				return header;
			}
		}

		[RegistryEditor("Enterprise.Accounting.Registry.GUI.CrossTradeDebtorConfigurationRegistryItemEditor, Enterprise.Accounting.GUI")]
		public class CrossTradeDebtorConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CrossTradeDebtorConfigurationHeader>
		{
			public CrossTradeDebtorConfigurationRegistryDataType()
			{
			}
		}
	}
}
