using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Business.AWB;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations
{
	public class JXCShipmentExportAWBHeaderValidation : JXCExportAWBHeaderValidation
	{
		public JXCShipmentExportAWBHeaderValidation(JASShipmentExportAWBHeader aWBHeader)
			: base(aWBHeader)
		{
		}

		#region Shipper

		protected override void CheckEH_ShipperState()
		{
			base.CheckEH_ShipperState();
			CheckShipperStateOrPostCodeExist(Parent.EH_ShipperStateInfo);
		}

		protected override void CheckEH_ShipperPostCode()
		{
			base.CheckEH_ShipperPostCode();
			CheckShipperStateOrPostCodeExist(Parent.EH_ShipperPostCodeInfo);
		}

		void CheckShipperStateOrPostCodeExist(ZPropertyInfo propertyInfo)
		{
			if (Parent.EH_ShipperState.IsEmpty && Parent.EH_ShipperPostCode.IsEmpty)
			{
				const string ErrorMessage = "Both the Shipper's Post Code and State cannot be blank";
				ValidationHelper.AddJXCWarning(propertyInfo, ErrorMessage);
			}
		}

		#endregion

		#region Consignee

		protected override void CheckEH_ConsigneeState()
		{
			base.CheckEH_ConsigneeState();
			CheckConsigneeStateOrPostCodeExist(Parent.EH_ConsigneeStateInfo);
		}

		protected override void CheckEH_ConsigneePostCode()
		{
			base.CheckEH_ConsigneePostCode();
			CheckConsigneeStateOrPostCodeExist(Parent.EH_ConsigneePostCodeInfo);
		}

		void CheckConsigneeStateOrPostCodeExist(ZPropertyInfo propertyInfo)
		{
			if (Parent.EH_ConsigneeState.IsEmpty && Parent.EH_ConsigneePostCode.IsEmpty)
			{
				const string ErrorMessage = "Both the Consignee's Post Code and State cannot be blank";
				ValidationHelper.AddJXCWarning(propertyInfo, ErrorMessage);
			}
		}

		#endregion

		#region Notify Party

		protected override void CheckEH_AlsoNotifyName()
		{
			base.CheckEH_AlsoNotifyName();
			ValidationHelper.ValidateFreeTextField(Parent.EH_AlsoNotifyNameInfo, JXCConstants.HAWBFieldBoundaries.AlsoNotifyNameMaxLength);
		}

		protected override void CheckEH_AlsoNotifyPlace()
		{
			base.CheckEH_AlsoNotifyPlace();
			ValidationHelper.ValidateFreeTextField(Parent.EH_AlsoNotifyPlaceInfo, JXCConstants.HAWBFieldBoundaries.AlsoNotifyPlaceMaxLength);
		}

		protected override void CheckEH_AlsoNotifyContactDetail()
		{
			base.CheckEH_AlsoNotifyContactDetail();
			ValidationHelper.ValidateFreeTextField(Parent.EH_AlsoNotifyContactDetailInfo, JXCConstants.HAWBFieldBoundaries.PhoneMaxLength);
		}

		#endregion

		#region Freight Declarations

		protected override void CheckEH_HouseDeclaredValueCurrency()
		{
			base.CheckEH_HouseDeclaredValueCurrency();
			ValidationHelper.ValidateCurrencyCode(Parent.Factory, Parent.EH_HouseDeclaredValueCurrencyInfo, Parent.EH_HouseDeclaredValueCurrency);
		}

		protected override void CheckEH_HouseCustomsValueCurrency()
		{
			base.CheckEH_HouseCustomsValueCurrency();
			ValidationHelper.ValidateCurrencyCode(Parent.Factory, Parent.EH_HouseCustomsValueCurrencyInfo, Parent.EH_HouseCustomsValueCurrency);
		}

		#endregion
	}
}

#region Shipper
#endregion
#region Consignee
#endregion
#region Notify Party
#endregion
#region Freight Declarations
#endregion
