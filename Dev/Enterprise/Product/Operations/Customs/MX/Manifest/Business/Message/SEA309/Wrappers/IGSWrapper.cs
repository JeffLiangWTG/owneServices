using System;
using CargoWise.Customs.MX.MessageContracts;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.MX.Manifest.Business
{
	internal class GSWrapper : IGSFunctionalGroupHeader
	{
		public GSWrapper()
		{
		}

		string IGSFunctionalGroupHeader.FunctionalIdentifierCode => SEA309Constants.FunctionalIdentifierCode;

		string IGSFunctionalGroupHeader.ApplicationSenderCode => GlbCompany.CurrentCompany.GC_CustomsRegistrationNo;

		string IGSFunctionalGroupHeader.ApplicationReceiverCode => MXCustomsDataRegistry.Instance.IsMXTestingSystem ? SEA309Constants.SendToTest : SEA309Constants.SendToProd;

		string IGSFunctionalGroupHeader.Date => currentLocalDateTime.ToString("yyyyMMdd");

		string IGSFunctionalGroupHeader.Time => currentLocalDateTime.ToString("HHmm");

		string IGSFunctionalGroupHeader.GroupControlNumber => MXMessage.MessageNumberPlaceHolder;

		string IGSFunctionalGroupHeader.ResponsibleAgencyCode => SEA309Constants.AgencyCode;

		string IGSFunctionalGroupHeader.Version => SEA309Constants.VersionGS;

		readonly DateTime currentLocalDateTime = Env.Time.CurrentLocalDateTime;
	}
}
