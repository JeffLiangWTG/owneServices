using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations
{
	public class JXCForwardingSeaConsolValidation : JXCForwardingConsolValidation
	{
		public JXCForwardingSeaConsolValidation(JASForwardingConsol consol)
			: base(consol)
		{
		}

		protected override void CheckJK_RL_NKLoadPort()
		{
			base.CheckJK_RL_NKLoadPort();

			ValidationHelper.AddJXCWarningIfNotEntered(Parent.JK_RL_NKLoadPortInfo);
			ValidationHelper.AddJXCWarningIfInvalidCode(Parent.JK_RL_NKLoadPortInfo, Parent.Lookups.LoadPorts);
		}

		protected override void CheckJK_RL_NKDischargePort()
		{
			base.CheckJK_RL_NKDischargePort();

			ValidationHelper.AddJXCWarningIfNotEntered(Parent.JK_RL_NKDischargePortInfo);
			ValidationHelper.AddJXCWarningIfInvalidCode(Parent.JK_RL_NKDischargePortInfo, Parent.Lookups.DischargePorts);
		}

		protected override void CheckJK_OA_ShippingLineAddress()
		{
			base.CheckJK_OA_ShippingLineAddress();

			ValidationHelper.AddJXCWarningIfNotEntered(Parent.JK_OA_ShippingLineAddressInfo);
			if (Parent.ShippingLine != null)
			{
				CheckFullName(Parent.JK_OA_ShippingLineAddressInfo, Parent.ShippingLine);
				if (Parent.ShippingLine.JASWWMappedCode.IsEmpty)
				{
					ValidationHelper.AddJXCWarning(Parent.JK_OA_ShippingLineAddressInfo, "JAS SSL Code does not exist for this Carrier. Please use the Config tab in the Organisation screen to setup the SSL code mapping");
				}
			}
		}

		void CheckFullName(ZPropertyInfo propertyInfo, OrgHeader orgHeader)
		{
			if (orgHeader.OH_FullName.IsEmpty)
			{
				ValidationHelper.AddJXCWarning(propertyInfo, "Organisation Name cannot be blank");
			}
		}

		protected override void CheckJK_MasterBillNum()
		{
			base.CheckJK_MasterBillNum();
			ValidationHelper.ValidateFreeTextField(Parent.JK_MasterBillNumInfo, JXCConstants.OHBLFieldBoundaries.BillOfLadingMaxLength);
		}
	}
}
