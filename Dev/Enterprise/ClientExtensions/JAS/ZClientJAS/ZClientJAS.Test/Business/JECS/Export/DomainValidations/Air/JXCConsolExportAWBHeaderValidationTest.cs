using System;
using Enterprise.Client.JAS.Business.AWB;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations.Testing
{
	internal class JXCConsolExportAWBHeaderValidationTest : JXCExportAWBHeaderValidationTestCase
	{
		public void TestValidateEH_AirlinePrefix()
		{
			Consol.JK_MasterBillNum = "TES12345678";
			AWBHeader.Validation.ValidateEH_AirlinePrefix();
			AssertHasNumericExactLengthJXCWarning(AWBHeader.EH_AirlinePrefixInfo, JXCConstants.AWBFieldBoundaries.AirlinePrefixLength);
			Consol.JK_MasterBillNum = "08112345678";
			AWBHeader.Validation.ValidateEH_AirlinePrefix();
			AssertHasNoJXCWarnings(AWBHeader.EH_AirlinePrefixInfo);
		}

		public void TestValidateEH_AWBSerialNo()
		{
			Consol.JK_MasterBillNum = "081Invalid1";
			AWBHeader.Validation.ValidateEH_AWBSerialNo();
			AssertHasNumericExactLengthJXCWarning(AWBHeader.EH_AWBSerialNoInfo, JXCConstants.AWBFieldBoundaries.MAWBSerialNoLength);
			Consol.JK_MasterBillNum = "08122222222";
			AWBHeader.Validation.ValidateEH_AWBSerialNo();
			AssertHasNoJXCWarnings(AWBHeader.EH_AWBSerialNoInfo);
		}

		public void TestValidateEH_AgentName()
		{
			AssertMaxLengthLessOrEqualToForJXC(JXCConstants.AWBFieldBoundaries.AgentNameMaxLength, ExportAWBHeader.Constants.AgentNameMaxLength);
			string excessivelyLongString = "12345678901234567890123456789012345678901234567890123456789012345678901234567890";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = excessivelyLongString;
			AWBHeader.Validation.ValidateEH_AgentName();
			AssertHasNoJXCWarnings("Should be trimmed in the get_AgentName", AWBHeader.EH_AgentNameInfo);
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "";
			AWBHeader.Validation.ValidateEH_AgentName();
			AssertHasNotEnteredJXCWarning(AWBHeader.EH_AgentNameInfo);
		}

		protected new JASConsolExportAWBHeader AWBHeader
		{
			get
			{
				return (JASConsolExportAWBHeader)base.AWBHeader;
			}
		}

		JASForwardingConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					fConsol = Factory.New<JASForwardingConsol>();
					fConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
				}

				return fConsol;
			}
		}

		protected override ExportAWBHeader GetNewAWBHeader()
		{
			return Consol.AWBHeader;
		}

		protected override Type ExpectedJXCExportAWBHeaderValidationTypeToTest
		{
			get
			{
				return typeof(JXCConsolExportAWBHeaderValidation);
			}
		}

		protected override void ChangeOverrideWayBillDefaultsFlag(bool shouldOverride)
		{
			Consol.JK_OverrideWaybillDefaults = shouldOverride;
		}

		JASForwardingConsol fConsol;
	}
}
