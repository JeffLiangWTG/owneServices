using System;
using CargoWise.Customs.MX.MessageContracts;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.MX.Manifest.Business
{
	internal class ISAWrapper : IISAInterchangeControlHeader
	{
		public ISAWrapper()
		{
		}

		string IISAInterchangeControlHeader.AuthorizationInformationQualifier => SEA309Constants.DoubleZero;

		string IISAInterchangeControlHeader.AuthorizationInformation => ZString.Empty;

		string IISAInterchangeControlHeader.SecurityInformationQualifier => SEA309Constants.DoubleZero;

		string IISAInterchangeControlHeader.SecurityInformation => ZString.Empty;

		string IISAInterchangeControlHeader.InterchangeIDQualifier => SEA309Constants.DoubleZ;

		string IISAInterchangeControlHeader.InterchangeSenderID => GlbCompany.CurrentCompany.GC_CustomsRegistrationNo;

		string IISAInterchangeControlHeader.InterchangeIDQualifier2 => SEA309Constants.DoubleZ;

		string IISAInterchangeControlHeader.InterchangeReceiverID => MXCustomsDataRegistry.Instance.IsMXTestingSystem ? SEA309Constants.SendToTest : SEA309Constants.SendToProd;

		string IISAInterchangeControlHeader.InterchangeDate => currentLocalDateTime.ToString("yyMMdd");

		string IISAInterchangeControlHeader.InterchangeTime => currentLocalDateTime.ToString("HHmm");

		string IISAInterchangeControlHeader.RepetitionSeparator => SEA309Constants.Separator;

		string IISAInterchangeControlHeader.InterControlVersion => SEA309Constants.ControlVersion;

		string IISAInterchangeControlHeader.InterControlNumber => MXMessage.MessageNumberPlaceHolder;

		string IISAInterchangeControlHeader.AcknowledgementRequest => SEA309Constants.ACKRequest;

		string IISAInterchangeControlHeader.UsageIndicator => SEA309Constants.UsageIndicator;

		string IISAInterchangeControlHeader.ComponentElement => SEA309Constants.ComponentElementSeparator;

		readonly DateTime currentLocalDateTime = Env.Time.CurrentLocalDateTime;
	}
}
