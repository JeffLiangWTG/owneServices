using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class ChargeCodeForPricingPageSectionsRegistryItem : StronglyTypedRegistryItem<ChargeCodeForPricingPageSectionsConfigurationCollection>
	{
		public ChargeCodeForPricingPageSectionsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ChargeCodeForPricingPageSectionsConfigurationCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ChargeCodeForPricingPageSectionsRegistryDataType(), storage, defaultValue))
		{
		}

		public ChargeCodeForPricingPageSectionsConfiguration GetChargeCodeConfiguration(ZString pricingPage, ZString section)
		{
			return Configurations.FirstOrDefault(x => x.PricingPage == pricingPage && x.Section == section);
		}

		IEnumerable<ChargeCodeForPricingPageSectionsConfiguration> Configurations
		{
			get
			{
				if (!isConfigurationsSet)
				{
					configurations = Value
						.Select(x => x as ChargeCodeForPricingPageSectionsConfiguration)
						.WhereNotNull()
						.ToList();

					isConfigurationsSet = true;
				}

				return configurations;
			}
		}

		bool isConfigurationsSet;
		IEnumerable<ChargeCodeForPricingPageSectionsConfiguration> configurations;

		protected override void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			base.SetValueCore(companyOrOwnerPK, branchPK, departmentPK, newValue);

			isConfigurationsSet = false;
			configurations = null;
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.ChargeCodeForPricingPageSectionsRegistryItemEditor, Enterprise.Registry.GUI")]
	class ChargeCodeForPricingPageSectionsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ChargeCodeForPricingPageSectionsConfigurationCollection>
	{
		public ChargeCodeForPricingPageSectionsRegistryDataType()
		{
		}
	}
}
