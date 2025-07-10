using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations
{
	public class JXCExportHeaderValidationHelper
	{
		public void ValidateSendingForwarder(ZPropertyInfo propertyInfo, IJXCExportHeader exportHeader)
		{
			if (propertyInfo != null && exportHeader != null)
			{
				JASOrgHeader sendingForwarder = exportHeader.SendingForwarder;
				ValidationHelper validationHelper = new ValidationHelper();
				validationHelper.AddJXCWarningIfNotEntered(propertyInfo);
				validationHelper.ValidateJXCForwarder(propertyInfo, sendingForwarder);
				if (sendingForwarder != null && !IsOrganisationControlledOrAProxyToThisBranch(sendingForwarder))
				{
					validationHelper.AddJXCWarning(propertyInfo, "The Sending Forwarder is not controlled by or a proxy to the current Branch. Please configure the Controlling Branch from the Organisation screen or relogin");
				}
			}
		}

		bool IsOrganisationControlledOrAProxyToThisBranch(JASOrgHeader orgHeader)
		{
			return (orgHeader.PK == GlbBranch.CurrentBranch.GB_OH_OrgProxy) || (orgHeader.CompanyData.OB_GB_ControllingBranch == GlbBranch.CurrentBranch.PK);
		}
	}
}
