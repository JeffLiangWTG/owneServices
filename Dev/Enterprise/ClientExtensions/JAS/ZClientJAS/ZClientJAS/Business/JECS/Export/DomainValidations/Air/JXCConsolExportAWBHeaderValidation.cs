using Enterprise.Client.JAS.Business.AWB;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations
{
	public class JXCConsolExportAWBHeaderValidation : JXCExportAWBHeaderValidation
	{
		public JXCConsolExportAWBHeaderValidation(JASConsolExportAWBHeader aWBHeader)
			: base(aWBHeader)
		{
		}

		protected void CheckEH_AirlinePrefix()
		{
			ValidationHelper.ValidateNumericTextFieldWithExactLength(Parent.EH_AirlinePrefixInfo, JXCConstants.AWBFieldBoundaries.AirlinePrefixLength);
		}

		protected void CheckEH_AWBSerialNo()
		{
			ValidationHelper.ValidateNumericTextFieldWithExactLength(Parent.EH_AWBSerialNoInfo, JXCConstants.AWBFieldBoundaries.MAWBSerialNoLength);
		}

		protected override void CheckEH_AgentName()
		{
			ValidationHelper.AddJXCWarningIfNotEntered(Parent.EH_AgentNameInfo);
		}
	}
}
