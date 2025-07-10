using CargoWise.ComponentModel;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class AsycudaBillValidationHelper
	{
		public AsycudaBillValidationHelper(AsycudaBill asycudaBill)
		{
			this.bill = asycudaBill;
		}

		readonly AsycudaBill bill;

		public void CheckABL_GoodsDescription()
		{
			if (bill.IsAir)
			{
				ValidateByteLength(bill.ABL_GoodsDescriptionInfo, 21);
			}
		}

		public void CheckABL_ManifestQty()
		{
			var info = bill.ABL_ManifestQtyInfo;

			if (bill.IsAir)
			{
				ValidateMaxValue(info, 999999, string.Empty, string.Empty, Res.GetString("BD164C99-A473-4E48-8F36-FBD0B27F4232", "The maximum allowable quantity is 999,999."), CargoWise.EntityFramework.NotificationType.MessageError);
			}
			else if (bill.IsSea)
			{
				ValidateMaxValue(info, 99999999, string.Empty, string.Empty, Res.GetString("C3527E8D-2E5D-42F6-AE1A-5583CB0B572F", "The maximum allowable quantity is 99,999,999."), CargoWise.EntityFramework.NotificationType.MessageError);
			}
		}

		public void CheckABL_NetWeight()
		{
			var info = bill.ABL_NetWeightInfo;

			if (bill.ABL_NetWeight < 0)
			{
				info.AddError(Res.GetString("C0DB40FB-FF1F-4C73-8944-E3566FD67D8C", "Weight cannot be negative."));
			}
		}

		public void ValidateMaxValue(ZPropertyInfo info, ZDecimal maxValue, string fieldDescription = "", string maxValueDescription = "", string errorMessage = "", INotificationType notificationType = null)
		{
			var value = info.PropertyType == typeof(ZInt) ? new ZDecimal(bill.GetPropertyValue<ZInt>(info.Name)) : bill.GetPropertyValue<ZDecimal>(info.Name);
			notificationType = notificationType ??= CargoWise.ComponentModel.NotificationType.Error;
			if (value > maxValue)
			{
				if (string.IsNullOrEmpty(errorMessage))
				{
					info.AddNotification(notificationType, Res.GetString("5AAB20A4-85A0-4C77-84C2-3993A1720BA2", "The number {0} is too large, the maximum value allowed for {1} is {2}.", value, fieldDescription, maxValueDescription));
				}
				else
				{
					info.AddNotification(notificationType, errorMessage);
				}
			}
		}

		public void CheckABL_ShipperStreet2()
		{
			ValidateByteLength(bill.ABL_ShipperStreet2Info, 35);
		}

		public void CheckABL_ShipperCity()
		{
			ValidateByteLength(bill.ABL_ShipperCityInfo, 35);
		}

		public void CheckABL_ShipperPostcode()
		{
			ValidateByteLength(bill.ABL_ShipperPostcodeInfo, 9);
		}

		public void CheckABL_ShipperPhone()
		{
			ValidatePhoneNumberLength(bill.ABL_ShipperPhoneInfo);
		}

		public void CheckABL_ConsigneeStreet2()
		{
			ValidateByteLength(bill.ABL_ConsigneeStreet2Info, 35);
		}

		public void CheckABL_ConsigneeCity()
		{
			ValidateByteLength(bill.ABL_ConsigneeCityInfo, 35);
		}

		public void CheckABL_ConsigneePostcode()
		{
			ValidateByteLength(bill.ABL_ConsigneePostcodeInfo, 9);
		}

		public void CheckABL_ConsigneePhone()
		{
			ValidatePhoneNumberLength(bill.ABL_ConsigneePhoneInfo);
		}

		public void CheckABL_NotifyPartyStreet2()
		{
			ValidateByteLength(bill.ABL_NotifyPartyStreet2Info, 35);
		}

		public void CheckABL_NotifyPartyCity()
		{
			ValidateByteLength(bill.ABL_NotifyPartyCityInfo, 35);
		}

		public void CheckABL_NotifyPartyPostcode()
		{
			ValidateByteLength(bill.ABL_NotifyPartyPostcodeInfo, 9);
		}

		public void CheckABL_NotifyPartyPhone()
		{
			ValidatePhoneNumberLength(bill.ABL_NotifyPartyPhoneInfo);
		}

		public void ValidateByteLength(ZPropertyInfo info, int maxLength)
		{
			var data = JPMessageUtils.MessageDataEncoding.GetBytes(info.Value.ToString());
			if (data.Length > maxLength)
			{
				var truncatedString = JPMessageUtils.MessageDataEncoding.GetString(data, 0, maxLength);
				info.AddWarning(Res.GetString("FBA922FA-D547-4692-A0AF-D2692CC4F689", "Only the initial {0} bytes will be sent to customs as the {1}. The designated character string sent to customs is \"{2}\".", maxLength, info.HumanReadableName, truncatedString));
			}
		}

		void ValidatePhoneNumberLength(ZPropertyInfo info)
		{
			ZString originalValue = info.Value.ToString();
			if (originalValue.Length > 14)
			{
				var formattedValue = originalValue.RemoveNonNumCharFromPhoneNumber();
				info.AddWarning(Res.GetString("7C94FBC6-88CE-4E43-98B8-3CD699C609C1", "Only 14 bytes will be sent to customs as the {0}. The designated character string sent to customs will be \"{1}\".", info.HumanReadableName, formattedValue));
			}
		}
	}
}
