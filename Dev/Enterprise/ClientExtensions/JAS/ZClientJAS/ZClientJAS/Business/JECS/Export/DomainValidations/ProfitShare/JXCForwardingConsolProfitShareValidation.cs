using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations
{
	public class JXCForwardingConsolProfitShareValidation : JXCForwardingConsolValidation
	{
		public JXCForwardingConsolProfitShareValidation(JASForwardingConsol parent)
			: base(parent)
		{
		}

		protected override void CheckJK_RL_NKLoadPort()
		{
			base.CheckJK_RL_NKLoadPort();
			ValidatePortHasIATACode(Parent.JK_RL_NKLoadPortInfo, Parent.LoadPort);
		}

		protected override void CheckJK_RL_NKDischargePort()
		{
			base.CheckJK_RL_NKDischargePort();
			ValidatePortHasIATACode(Parent.JK_RL_NKDischargePortInfo, Parent.DischargePort);
		}

		protected override void CheckJK_MasterBillNum()
		{
			base.CheckJK_MasterBillNum();
			ValidateMasterBillMAWB();
			ValidateMasterBillAirlinePrefix();
		}

		public void ValidateMasterBillMAWB()
		{
			ValidateCalculatedProperty(Parent.MasterBillMAWBInfo);
		}

		protected virtual void CheckMasterBillMAWB()
		{
			ValidationHelper.ValidateNumericTextFieldWithExactLength(Parent.MasterBillMAWBInfo, 8);
		}

		public void ValidateMasterBillAirlinePrefix()
		{
			ValidateCalculatedProperty(Parent.MasterBillAirlinePrefixInfo);
		}

		protected virtual void CheckMasterBillAirlinePrefix()
		{
			ValidationHelper.ValidateNumericTextFieldWithExactLength(Parent.MasterBillAirlinePrefixInfo, 3);
		}

		void ValidatePortHasIATACode(ZPropertyInfo propertyInfo, RefUNLOCO port)
		{
			if (port != null && port.RL_IATA.IsEmpty)
			{
				ValidationHelper.AddJXCWarning(propertyInfo, "Port does not have IATA Code");
			}
		}
	}
}
