using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.FeatureControl.Business;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class FeatureControlBaseController : TrustedController
	{
		const int MaxRetryCount = 3;

		public FeatureControlBaseController() : base()
		{
		}

		public FeatureControlBaseController(NLogWrapper logger) : base(logger)
		{
		}

		protected void GetFeatureControlRuleCore(TrustedContext<FeatureControlRequest, FeatureControlResponse> context)
		{
			if (!context.Success)
			{
				return;
			}

			var database = context.TrustedSystem?.FindTenantDatabaseByTrustedInfo(context.RequestInfo);
			if (database == null)
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Authorization_ActionNotPermitted, ErrorCodes.Descriptions.Authorization_ActionNotPermitted);
				return;
			}

			for (var i = 0; i < MaxRetryCount; i++)
			{
				try
				{
					var featureControl = FeatureControlExtensions.LoadFromDatabase(context.RequestInfo.RuleTimestampUtc, database.PK);
					context.ResponseInfo = new FeatureControlResponse()
					{
						RuleTimestampUtc = featureControl.TimestampUtc != DateTime.MinValue ? featureControl.TimestampUtc : DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc)
					};

					if (context.ResponseInfo.RuleTimestampUtc == context.RequestInfo.RuleTimestampUtc) //the client has the latest rule already.
					{
						database.LD_FeatureControlRuleLastSyncUtc = ZDateTime.UtcNow;
						database.Factory.Save();
					}
					else
					{
						context.ResponseInfo.RuleContent = featureControl.Compress();
						var xml = featureControl.ToXmlString();
						if (database.LD_FeatureControlRuleLastSyncContent != xml)
						{
							database.LD_FeatureControlRuleLastSyncContent = xml;
							database.Factory.Save();
						}
					}

					break;
				}
				catch (ZSaveConcurrencyException)
				{
					if (i == MaxRetryCount - 1)
					{
						throw;
					}

					database.ReloadSafe();
				}
			}
		}
	}
}
