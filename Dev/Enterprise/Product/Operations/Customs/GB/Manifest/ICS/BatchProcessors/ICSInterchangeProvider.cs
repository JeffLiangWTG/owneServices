using System;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.CDS;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.ICS
{
	public class ICSInterchangeProvider : CDSInterchangeProvider
	{
		public ICSInterchangeProvider(LoggingInformation logger, NonDependentEDIMessageCollection messages) : base(logger, messages)
		{
		}

		protected override string GetCollationKey(EDIMessage message) => message.PK.ToString();

		protected override ZString GetInterchangeFooter(int messageCount) => ZString.Empty;

		protected override Type InterchangeType => typeof(ICSInterchange);

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			var gvmsRequestMessage = messages.Cast<EDIMessage>().Single();
			var dataProvider = ICSExtensions.GetRequestDataProvider(gvmsRequestMessage);
			var from = dataProvider?.Credentials.Key ?? ZString.Empty;
			SetInterchangeValuesForTransmit(interchange, messages, gvmsRequestMessage.EM_MessageSubType, CDS.Constants.EDIInterchange.GBCustoms, from);
			gvmsRequestMessage.EM_EI = interchange.PK;
			interchange.EI_HeaderText = GetHeaderText(gvmsRequestMessage);
		}

		static ZString GetHeaderText(EDIMessage message)
		{
			return ICSExtensions.NewGBCustomsRequest(message)?.Serialize() ?? ZString.Empty;
		}

		protected override void MarkMessagesInThisInterchangeAsFailed(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			foreach (EDIMessage message in messages.ToArray())
			{
				var errorMessage = new ZStringBuilder();

				if (interchange.EI_To.IsEmpty)
				{
					errorMessage.Append(string.Format(CultureInfo.InvariantCulture, "There is no customs interchange recipient id (Company - {0}, Branch - {1}).", GlbCompany.CurrentCompany.GC_Code, GlbBranch.CurrentBranch.GB_Code) + "\r\n");
				}

				if (interchange.EI_From.IsEmpty)
				{
					errorMessage.Append(string.Format(CultureInfo.InvariantCulture, "There is no customs interchange sender id."));
				}

				logger.LogError(string.Format(CultureInfo.InvariantCulture, "Failed to create interchange for ICS Message #" + message.EM_MessageNum + " : " + errorMessage.ToString()));
				message.EM_Status = EDIMessage.Status.Failed;
			}
		}
	}
}
