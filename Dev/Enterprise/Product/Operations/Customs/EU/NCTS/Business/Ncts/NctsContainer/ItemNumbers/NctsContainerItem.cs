using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsContainerItem : CusCodeData
	{
		public NctsContainerItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(NctsContainer));

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.ItemNumber;
			CY_Code = CusCodeDataTypeList.Codes.ItemNumber;
		}

		[ResourceStringData("2B11D631-0E04-480E-9509-53C7B358686B", Caption = "Item number", MediumCaption = "Item No.", ShortCaption = "No.")]
		[ReadOnlyMember(nameof(IsArrivalNotificationDisabled))]
		public ZInt CY_DataNumeric
		{
			get => ZInt.ParseSafe(CY_Data, 0);
			set => CY_Data = value.ToString();
		}

		public ZWrappedPropertyInfo CY_DataNumericInfo => GetWrappedZPropertyInfo(nameof(CY_DataNumeric), x => CY_DataInfo);

		protected override Customs.Business.CusCodeDataLookups GetNewLookups() => new CusCodeDataLookups(this);

		public bool IsArrivalNotificationDisabled => Parent is NctsContainer container && container.IsArrivalNotificationDisabled;
	}
}
