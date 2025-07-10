using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public class NFEImportObjectItem : NonPersistentBusinessObject
	{
		public NFEImportObjectItem(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static class Schema
		{
			public const string NfeItemNumber = "NfeItemNumber";
			public const string ProductCode = "ProductCode";
			public const string GoodsDescription = "GoodsDescription";
			public const string TariffCode = "TariffCode";
			public const string InvoiceQuantityUQ = "InvoiceQuantityUQ";
			public const string InvoiceQuantity = "InvoiceQuantity";
			public const string TotalValue = "TotalValue";
			public const string CustomsQuantityUQ = "CustomsQuantityUQ";
			public const string CustomsQuantity = "CustomsQuantity";
			public const string ComplementartDescription = "ComplementartDescription";
			public const string FreteValue = "FreteValue";
			public const string SegValue = "SegValue";
			public const string OutroValue = "OutroValue";
			public const string DescValue = "DescValue";
			public const string TotalNfeValue = "TotalNfeValue";

			public const int ProductCodeMaxLength = 35;
		}

		#region NfeItemNumber

		public ZString NfeItemNumber
		{
			get { return fNfeItemNumber; }
			set { SetNonPersistentPropertyValue(NfeItemNumberInfo, ref fNfeItemNumber, value); }
		}

		ZString fNfeItemNumber;

		public ZPropertyInfo NfeItemNumberInfo => GetZPropertyInfo(Schema.NfeItemNumber);

		#endregion

		#region ProductCode

		public ZString ProductCode
		{
			get { return fProductCode; }
			set { SetNonPersistentPropertyValue(ProductCodeInfo, ref fProductCode, value); }
		}

		ZString fProductCode;

		public ZPropertyInfo ProductCodeInfo => GetZPropertyInfo(Schema.ProductCode);

		#endregion

		#region GoodsDescription

		public ZString GoodsDescription
		{
			get { return fGoodsDescription; }
			set { SetNonPersistentPropertyValue(GoodsDescriptionInfo, ref fGoodsDescription, value); }
		}

		ZString fGoodsDescription;

		public ZPropertyInfo GoodsDescriptionInfo => GetZPropertyInfo(Schema.GoodsDescription);

		#endregion

		#region TariffCode

		public ZString TariffCode
		{
			get { return fTariffCode; }
			set { SetNonPersistentPropertyValue(TariffCodeInfo, ref fTariffCode, value); }
		}

		ZString fTariffCode;

		public ZPropertyInfo TariffCodeInfo => GetZPropertyInfo(Schema.TariffCode);

		#endregion

		#region InvoiceQuantityUQ

		public ZString InvoiceQuantityUQ
		{
			get { return fInvoiceQuantityUQ; }
			set { SetNonPersistentPropertyValue(InvoiceQuantityUQInfo, ref fInvoiceQuantityUQ, value); }
		}

		ZString fInvoiceQuantityUQ;

		public ZPropertyInfo InvoiceQuantityUQInfo => GetZPropertyInfo(Schema.InvoiceQuantityUQ);

		#endregion

		#region InvoiceQuantity

		public ZDecimal InvoiceQuantity
		{
			get { return fInvoiceQuantity; }
			set { SetNonPersistentPropertyValue(InvoiceQuantityInfo, ref fInvoiceQuantity, value); }
		}

		ZDecimal fInvoiceQuantity;

		public ZPropertyInfo InvoiceQuantityInfo => GetZPropertyInfo(Schema.InvoiceQuantity);

		#endregion

		#region LinePrice

		public ZDecimal TotalValue
		{
			get { return fTotalValue; }
			set { SetNonPersistentPropertyValue(TotalValueInfo, ref fTotalValue, value); }
		}

		ZDecimal fTotalValue;

		public ZPropertyInfo TotalValueInfo => GetZPropertyInfo(Schema.TotalValue);

		#endregion

		#region CustomsQuantityUQ

		public ZString CustomsQuantityUQ
		{
			get { return fCustomsQuantityUQ; }
			set { SetNonPersistentPropertyValue(CustomsQuantityUQInfo, ref fCustomsQuantityUQ, value); }
		}

		ZString fCustomsQuantityUQ;

		public ZPropertyInfo CustomsQuantityUQInfo => GetZPropertyInfo(Schema.CustomsQuantityUQ);

		#endregion

		#region CustomsQuantity

		public ZDecimal CustomsQuantity
		{
			get { return fCustomsQuantity; }
			set { SetNonPersistentPropertyValue(CustomsQuantityInfo, ref fCustomsQuantity, value); }
		}

		ZDecimal fCustomsQuantity;

		public ZPropertyInfo CustomsQuantityInfo => GetZPropertyInfo(Schema.CustomsQuantity);

		#endregion

		#region ComplementartDescription

		public ZString ComplementartDescription
		{
			get { return fComplementartDescription; }
			set { SetNonPersistentPropertyValue(ComplementartDescriptionInfo, ref fComplementartDescription, value); }
		}

		ZString fComplementartDescription;

		public ZPropertyInfo ComplementartDescriptionInfo => GetZPropertyInfo(Schema.ComplementartDescription);

		#endregion

		#region FreteValue

		public ZDecimal FreteValue
		{
			get { return fFreteValue; }
			set { SetNonPersistentPropertyValue(FreteValueInfo, ref fFreteValue, value); }
		}

		ZDecimal fFreteValue;

		public ZPropertyInfo FreteValueInfo => GetZPropertyInfo(Schema.FreteValue);

		#endregion

		#region SegValue

		public ZDecimal SegValue
		{
			get { return fSegValue; }
			set { SetNonPersistentPropertyValue(SegValueInfo, ref fSegValue, value); }
		}

		ZDecimal fSegValue;

		public ZPropertyInfo SegValueInfo => GetZPropertyInfo(Schema.SegValue);

		#endregion

		#region OutroValue

		public ZDecimal OutroValue
		{
			get { return fOutroValue; }
			set { SetNonPersistentPropertyValue(OutroValueInfo, ref fOutroValue, value); }
		}

		ZDecimal fOutroValue;

		public ZPropertyInfo OutroValueInfo => GetZPropertyInfo(Schema.OutroValue);

		#endregion

		#region DescValue

		public ZDecimal DescValue
		{
			get { return fDescValue; }
			set { SetNonPersistentPropertyValue(DescValueInfo, ref fDescValue, value); }
		}

		ZDecimal fDescValue;

		public ZPropertyInfo DescValueInfo => GetZPropertyInfo(Schema.DescValue);

		#endregion

		#region LineNfePrice

		public ZDecimal TotalNfeValue
		{
			get { return fTotalNfeValue; }
			set { SetNonPersistentPropertyValue(TotalNfeValueInfo, ref fTotalNfeValue, value); }
		}

		ZDecimal fTotalNfeValue;

		public ZPropertyInfo TotalNfeValueInfo => GetZPropertyInfo(Schema.TotalNfeValue);

		#endregion
	}
}
