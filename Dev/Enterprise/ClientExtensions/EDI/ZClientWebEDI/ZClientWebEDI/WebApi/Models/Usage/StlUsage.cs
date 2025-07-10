using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class StlUsage
	{
		public StlUsage(ZString databaseServerCode, ZGuid databasePK)
		{
			this.DatabaseServerCode = databaseServerCode;
			this.usageDataMap = new Dictionary<ZGuid, UsageData>();
			DatabasePK = databasePK;
		}

		public ZString DatabaseServerCode { get; private set; }
		public readonly ZGuid DatabasePK;
		public IEnumerable<UsageData> UsageData
		{
			get { return usageDataMap.Values.OrderBy(x => x.Order).ThenBy(x => x.Description); }
		}

		readonly Dictionary<ZGuid, UsageData> usageDataMap;

		public UsageData AddUsageData(ZGuid priceItemPk, ZString priceItemDescription, ZShort priceItemOrder)
		{
			UsageData usageData = null;
			if (!usageDataMap.TryGetValue(priceItemPk, out usageData))
			{
				int indentLevel = GetTextIndentLevel(priceItemDescription);
				usageData = new UsageData(priceItemDescription.Trim(), priceItemOrder, indentLevel);
				usageDataMap.Add(priceItemPk, usageData);
				AddParentData(priceItemPk, usageData);
			}
			return usageData;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		void AddParentData(ZGuid priceItemPk, UsageData usageData)
		{
			var priceItem = Factory.Load<ClientLicencePriceItem>(priceItemPk);

			if (priceItem != null)
			{
				int currentIndentLevel = GetTextIndentLevel(priceItem.L7_Description);

				var items = GetPriceItems(priceItem).Where(x => x.L7_Order < priceItem.L7_Order && GetTextIndentLevel(x.L7_Description) < currentIndentLevel).OrderByDescending(x => x.L7_Order);

				foreach (var item in items)
				{
					var itemIndentLevel = GetTextIndentLevel(item.L7_Description);
					if (itemIndentLevel < currentIndentLevel)
					{
						var parentData = new UsageData(item.L7_Description.Trim(), item.L7_Order, itemIndentLevel);

						if (!usageDataMap.ContainsKey(item.PK))
						{
							usageDataMap.Add(item.PK, parentData);
						}

						currentIndentLevel = itemIndentLevel;
						if (currentIndentLevel <= 0)
						{
							break;
						}
					}
				}
			}
			else
			{
				var parentData = new UsageData(usageData.Description, usageData.Order - 1, 0);
				usageDataMap.Add(ZGuid.NewZGuid(), parentData);
				usageData.IndentLevel += 1;
			}
		}

		static int GetTextIndentLevel(ZString text)
		{
			return text.ToString().TakeWhile(char.IsWhiteSpace).Count();
		}

		IEnumerable<ClientLicencePriceItem> GetPriceItems(ClientLicencePriceItem priceItem)
		{
			if (priceListToItems == null)
			{
				priceListToItems = new Dictionary<Guid, ClientLicencePriceItem[]>();
			}

			if (!priceListToItems.TryGetValue(priceItem.L7_L6.ToGuid(), out var priceItems))
			{
				var query = new ZQuery(ClientLicencePriceItemSchema.L7_L6, priceItem.L7_L6);
				priceItems = Factory.Load<ClientLicencePriceItem>(query);
				priceListToItems.Add(priceItem.L7_L6.ToGuid(), priceItems);
			}
			return priceItems;
		}
		Dictionary<Guid, ClientLicencePriceItem[]> priceListToItems;

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;
	}
}
