using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public static class MessageHostedServiceRequirement
	{
		public static ZString CheckCLSetupSMSMessageSendingConfig()
		{
			if (!checkCLSetupSMSMessageSendingConfig.HasValue)
			{
				var companyPKs = GlbCompany.GetActiveCompanies(CountryCodes.Chile).Select(x => x.PK);
				foreach (var companyPK in companyPKs)
				{
					var smsMessageSending = CLCustomsDataRegistry.Instance.CLSMSMessageSending.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty);
					if (smsMessageSending != null && !smsMessageSending.MachineName.IsEmpty)
					{
						return ZString.Empty;
					}
				}
			}
			return ResString.GetMultilingualString("61508FAA-46D9-4E38-8FED-EDA9FE7DB4A3", "There is no SMS Message Sending configuration on Chilean companies.");
		}

		[ThreadStatic]
		static ZString? checkCLSetupSMSMessageSendingConfig;
	}
}
