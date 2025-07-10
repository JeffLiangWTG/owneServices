using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Licensing;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiBillingTransactionFlattened : AutoEdiBillingTransactionFlattened
	{
		public EdiBillingTransactionFlattened(BusinessObjectFactory factory) : base(factory)
		{
		}

		[List("Lookups.Categories")]
		public override ZString Category { get => base.Category; set => base.Category = value; }

		[List("Lookups.Categories")]
		public override ZString ReportingSource { get => base.ReportingSource; set => base.ReportingSource = value; }

		public ZInt LineNumber { get; set; }

		protected override ZString HumanReadableNameCore => $"Line : {LineNumber}";

		public override void ValidateCategory()
		{
			base.ValidateCategory();
			MandatoryValidation.CheckEntered(CategoryInfo);
			ListValidation.ErrorIfInvalidCode(CategoryInfo);
		}

		public override void ValidatePriceItemCode()
		{
			base.ValidatePriceItemCode();
			MandatoryValidation.CheckEntered(PriceItemCodeInfo);
			if (!Category.IsEmpty && !PriceItemCode.IsEmpty && Lookups.CategoryPriceItemMap.TryGetValue(Category, out var items))
			{
				ListValidation.ErrorIfInvalidCode(PriceItemCodeInfo, items);
			}
		}

		public override void ValidateBillableCount()
		{
			base.ValidateBillableCount();
			MandatoryValidation.CheckEntered(BillableCountInfo);
			MandatoryValidation.CheckNotNegative(BillableCountInfo);
		}

		public override void ValidateReportingSource()
		{
			base.ValidateReportingSource();
			MandatoryValidation.CheckEntered(ReportingSourceInfo);
			ListValidation.ErrorIfInvalidCode(ReportingSourceInfo);
		}

		public override void ValidateServiceOccuredUTC()
		{
			base.ValidateServiceOccuredUTC();
			MandatoryValidation.CheckEntered(ServiceOccuredUTCInfo);
		}

		public override void ValidateClientNumber()
		{
			base.ValidateClientNumber();
			MandatoryValidation.CheckEntered(ClientNumberInfo);

			if (!ClientNumber.IsEmpty)
			{
				var dbNum = ClientNumber.Split('.').First();
				if (!Lookups.ClientNumProductMap.TryGetValue(dbNum, out var product))
				{
					ClientNumberInfo.AddError((NoResString)"Invalid Client Number.");
				}
				else if (!Category.IsEmpty && product != Category)
				{
					ClientNumberInfo.AddError($"This Client does not have Product {Category}.");
				}
			}
		}

		public override void ValidateReference1()
		{
			base.ValidateReference1();
			MandatoryValidation.CheckEntered(Reference1Info);
		}

		#region Lookups

		protected FlattenedLookups Lookups
		{
			get => Factory.GetCachedValue("EdiBillingTransactionFlattened.Lookups", () => new FlattenedLookups(this));
		}

		protected class FlattenedLookups : ZLookups
		{
			public FlattenedLookups(BusinessObject parent) : base(parent)
			{
				var priceLists = EDIDataRegistry.Instance.UsageBillingSettings.Value.PriceLists.OfType<UsageBillingPriceList>();
				var priceCollection = new DynamicBusinessObjectCollection(Factory);
				var priceListCodesParameter = ZSqlParameter.New("@PriceListCodes", priceLists.Select(x => x.PriceListCode).ToArray(), ClientLicencePriceHeaderSchema.L6_SystemCode, isTableValued: true);
				priceCollection.Load(
	@"SELECT DISTINCT L6_SystemCode, L7_Code
FROM dbo.ClientLicencePriceHeader
JOIN dbo.ClientLicencePriceItem ON L7_L6 = L6_PK
WHERE L7_Code <> '' AND L6_SystemCode IN (SELECT Value FROM @PriceListCodes);", new[] { priceListCodesParameter });

				CategoryPriceItemMap = priceCollection.
					Select(x => new { PriceListCode = x["L6_SystemCode"].ToString(), PriceCode = x["L7_Code"].ToString() })
					.Join
					(
						priceLists,
						x => x.PriceListCode,
						y => y.PriceListCode.ToString(),
						(x, y) => new { ProductCode = y.ProductCode.ToString(), x.PriceCode }
					)
					.GroupBy(x => x.ProductCode)
					.ToDictionary(k => k.Key, v =>
					{
						var codes = new CodeDescriptionPairList();
						v.ForEach(x => codes.AddPair(x.PriceCode));
						return codes;
					});

				Categories = new CodeDescriptionPairList();
				CategoryPriceItemMap.Keys.ForEach(x => Categories.AddPair(x));

				var clientCollection = new DynamicBusinessObjectCollection(Factory);
				var productsParameter = ZSqlParameter.New("@Products", priceLists.Select(x => x.ProductCode).ToArray(), ClientLicencePriceHeaderSchema.L6_SystemCode, isTableValued: true);
				clientCollection.Load(
	@"SELECT LD_DatabaseNumber, LD_Product
FROM dbo.LicenceDatabase
WHERE LD_Product NOT IN ('CW1', 'ENT', 'CWN', 'CGW', 'PRW')
AND LD_Product IN (SELECT Value FROM @Products);", new[] { productsParameter });

				ClientNumProductMap = clientCollection.Select(x => new { LD_DatabaseNumber = int.Parse(x["LD_DatabaseNumber"].ToString()), LD_Product = x["LD_Product"].ToString() })
					.Distinct().ToDictionary(k => Base27Encoding.Encode(k.LD_DatabaseNumber), v => v.LD_Product);
			}

			public CodeDescriptionPairList Categories { get; private set; }
			public readonly	Dictionary<string, CodeDescriptionPairList> CategoryPriceItemMap;
			public readonly Dictionary<string, string> ClientNumProductMap;
		}

		#endregion
	}
}


