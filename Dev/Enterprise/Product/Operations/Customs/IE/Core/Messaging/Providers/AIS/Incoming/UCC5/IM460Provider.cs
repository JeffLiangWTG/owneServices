using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM460;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public class IM460Provider
	{
		public IM460Provider(Im460 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im460 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.Declaration.Mrn;

		public ZDateTime ControlNotificationDate
		{
			get
			{
				new ZString(xmlObject.Declaration.ControlNotificationDate).TryParseToDate(out var controlNotificationDate);
				return controlNotificationDate;
			}
		}

		public ZDateTime TimeLimitForControl
		{
			get
			{
				new ZString(xmlObject.Declaration.TimeLimitForControl).TryParseToDate(out var limitDate);
				return limitDate;
			}
		}

		public ZString CustomsOfficeLodgement => xmlObject.Declaration.CustomsOffices.CustomsOfficeLodgement;

		public ZString OverallControlTypeCoded => xmlObject.OverallControlType.ControlTypeCoded;

		public IReadOnlyCollection<(ZString ControlTypeCoded, ZString ControlTypeAgency)> ControlTypes => controlTypesCached ??= GetControlTypeDictionary().Select(p => p.Value).ToArray();
		IReadOnlyCollection<(ZString ControlTypeCoded, ZString ControlTypeAgency)> controlTypesCached;

		Dictionary<string, (ZString ControlTypeCoded, ZString ControlTypeAgency)> GetControlTypeDictionary()
		{
			var result = new Dictionary<string, (ZString ControlTypeCoded, ZString ControlTypeAgency)>();
			if (xmlObject.GoodsShipment is Collection<GoodsShipmentItemType> goodsShipment)
			{
				foreach (var item in goodsShipment)
				{
					if (item.ControlType is Collection<ControlsType> controlTypes)
					{
						foreach (var controlType in controlTypes)
						{
							var controlTypeCoded = controlType.ControlTypeCoded;
							var controlAgency = controlType.ControlAgency;
							var key = GetControlTypeKey(controlTypeCoded, controlAgency);
							if (!result.TryGetValue(key, out var existedControlType))
							{
								result.Add(key, (controlTypeCoded, controlAgency));
							}
						}
					}
				}
			}
			return result;
		}

		ZString GetControlTypeKey(ZString controlTypeCoded, ZString controlTypeAgency) => $"{controlTypeCoded}_{controlTypeAgency}";
	}
}
