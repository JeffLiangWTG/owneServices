using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class CASSChargeCodesRegistryItem : StronglyTypedRegistryItem<CASSChargeCodeCollection>
	{
		public CASSChargeCodesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint)
			: base(new CASSChargeCodesRegistryItemImpl(name, category, caption, hint))
		{
		}

		public ZGuid[] GetChargeCodePKByCASSTypesFromRegistry(ZString cASSType)
		{
			return this.Value.Cast<CASSChargeCode>().Where(x => x.CASSType == cASSType || x.CASSType.ToUpper() == CASSChargeCodeLookups.ALL).Select(x => x.ChargeCodePK).Distinct().ToArray();
		}

		public class CASSChargeCodesRegistryItemImpl : RegistryItemImpl
		{
			public CASSChargeCodesRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint)
				: base(name, category, caption, hint, new CASSChargeCodeListRegistryDataType(), RegistryStorageFlags.Company)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				CASSChargeCodeCollection defaultCASSMaps = new CASSChargeCodeCollection(new FallbackLevel(companyPK, branchPK, departmentPK), Factory);
				var chargeCodePK = new ZGuid(Env.Registry.GetFreightChargeCode(companyPK));
				if (!chargeCodePK.IsEmpty)
				{
					var cASSMap = defaultCASSMaps.AddNew();
					using (cASSMap.GetValidationSuspender())
					{
						cASSMap.CASSType = CASSChargeCodeLookups.ALL;
						cASSMap.CASSComponentCode = CASSChargeCodeLookups.ALL;
						cASSMap.ChargeCodePK = chargeCodePK;
					}
				}
				return defaultCASSMaps;
			}

			BusinessObjectFactory Factory
			{
				get { return factory ?? (factory = new BusinessObjectFactory()); }
			}
			BusinessObjectFactory factory;
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.CASSChargeCodesRegistryEditor, Enterprise.Accounting.GUI")]
	public class CASSChargeCodeListRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CASSChargeCodeCollection>
	{
		public CASSChargeCodeListRegistryDataType()
		{
		}

		protected override void ValidateCore(IRegistryItem registryItem, CASSChargeCodeCollection proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			proposedValue.CurrentFallbackLevel = new FallbackLevel(companyPK, branchPK, departmentPK);
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
		}

		protected override CASSChargeCodeCollection CloneValue(CASSChargeCodeCollection value)
		{
			if (value != null)
			{
				return (CASSChargeCodeCollection)value.Clone(value.CurrentFallbackLevel, value.Factory);
			}

			return value;
		}
	}
}
