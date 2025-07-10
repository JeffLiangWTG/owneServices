using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos
{
	public class AdditionalInfoValueSetStrategy : IValueSetStrategy
	{
		public AdditionalInfoValueSetStrategy(AdditionalInfo additionalInfo)
		{
			this.additionalInfo = additionalInfo;
		}

		public void ValueSet(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			switch (valueThatHasChanged.Name)
			{
				case AdditionalInfo.Schema.CSI_Code:
					CheckSetReferenceFromCSI_Code(valueThatHasChanged);
					break;
			}
		}

		void CheckSetReferenceFromCSI_Code(ZPropertyInfo valueThatHasChanged)
		{
			var dec = additionalInfo?.Parent as JobDeclaration;

			if (dec != null)
			{
				if (valueThatHasChanged.Value.ToString() == "PREMS")
				{
					var warehouse = dec.CusEntryInstruction?.WarehouseFor27;
					var warehouseId = dec.CusEntryInstruction?.WarehouseIDFor27;

					if (!string.IsNullOrEmpty(warehouseId) && !warehouseId.GetValueOrDefault().EndsWith(Core.Constants.CountryCodes.UnitedKingdom, StringComparison.CurrentCulture))
					{
						var nameAndAddress = warehouse?.AddressAsASingleLine.ToString();

						if (nameAndAddress.EndsWith(string.Format(CultureInfo.InvariantCulture, " {0}", warehouse?.Country.Code), StringComparison.CurrentCulture))
						{
							nameAndAddress = nameAndAddress.Substring(0, nameAndAddress.Length - 3);    // Remove Duplicate country code (and space)
						}

						if (nameAndAddress.Length > additionalInfo.CSI_DescriptionInfo.MaxLength - 3)
						{
							nameAndAddress = nameAndAddress.Substring(0, additionalInfo.CSI_DescriptionInfo.MaxLength - 3); // cut-off address to leave space for -XX (country code)
						}

						additionalInfo.CSI_Description = string.Format(CultureInfo.InvariantCulture, "{0}-{1}",
							nameAndAddress,
							warehouse?.Country.Code);
					}
				}
			}
		}

		readonly AdditionalInfo additionalInfo;
	}
}
