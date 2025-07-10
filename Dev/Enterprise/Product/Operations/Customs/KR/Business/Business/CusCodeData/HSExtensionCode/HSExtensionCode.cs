using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class HSExtensionCode : CusCodeData
	{
		public HSExtensionCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get
			{
				if (IsChildOfInvoiceLine)
				{
					return new TypeLoaderCollection(typeof(JobComInvoiceLine));
				}
				else
				{
					return new TypeLoaderCollection(typeof(CusClassPartPivot));
				}
			}
		}

		bool IsChildOfInvoiceLine => CY_ParentTableCode == JobComInvoiceLineSchema.Constants.Prefix;

		public override ZShort CY_Order { get => base.CY_Order; set => base.CY_Order = value; }

		[ResourceStringData("F4704352-22B6-4F79-BC57-A38C6A79A53B", Caption = "Category Code")]
		public override ZString CY_Code
		{
			get => base.CY_Code;
			set
			{
				var oldValue = CY_Code;
				base.CY_Code = value;
				if (oldValue != CY_Code && Category == Messaging.Constants.HsExtensionCodes.Code.Category)
				{
					Parent.HSExtensionCodeCollection.UpdateSubAdditionalCode(TariffView, CY_Code);
				}
			}
		}

		[ResourceStringData("3B1E0058-B457-4B4F-8C99-E794718F57AA", Caption = "Classification Type")]
		public ZString ClassificationType
		{
			get
			{
				var result = ZString.Empty;
				if (CY_Order == 0)
				{
					result = Messaging.Constants.HsExtensionCodes.Description.Category;
				}
				else
				{
					result = Messaging.Constants.HsExtensionCodes.Description.SubCategory + " " + CY_Order;
				}
				return result;
			}
		}

		public TariffView TariffView => Parent?.UniversalTariff;
		public new ILineOrProduct Parent => base.Parent as ILineOrProduct;

		public ZString Category => CY_Order == 0 ? Messaging.Constants.HsExtensionCodes.Code.Category : Messaging.Constants.HsExtensionCodes.Code.SubCategory;

		[ResourceStringData("89D48450-E0E0-47EA-9E24-37FF6B549DBC", Caption = "Category Desc.", FullDescription = "Category Description")]
		public ZString CategoryDescription
		{
			get
			{
				return TariffView?.AdditionalCodes.FirstOrDefault(x => x.ZY2_AdditionalCode == CY_Code && x.ZY2_ZY3_NKCategory == Category)?.ZY2_Description ?? ZString.Empty;
			}
		}

		public new HSExtensionCodeLookups Lookups => (HSExtensionCodeLookups)base.Lookups;

		protected override CusCodeDataLookups GetNewLookups() => new HSExtensionCodeLookups(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.HsExtensionCode;
		}
	}
}
