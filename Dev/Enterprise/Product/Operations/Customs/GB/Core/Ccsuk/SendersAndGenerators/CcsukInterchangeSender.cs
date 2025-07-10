using System;
using System.Globalization;
using System.Threading;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Customs.GB.Ccsuk.Declaration
{
	public class CcsukInterchangeSender : GbInterchangeSender
	{
		public CcsukInterchangeSender(ILogger serviceLogger) : base(serviceLogger)
		{
			this.iLogger = serviceLogger;
		}

		protected override int SendOutboundInterchange(string applicationCode, int interchangeNum, int numberOfInterchangesSent, CancellationToken token)
		{
			// We only want to prepare them, not send them.
			SharedFactory.RefreshEnabled = false;
			PrepareInterchanges(new[] { applicationCode });
			ReleaseSharedFactory();

			return numberOfInterchangesSent;
		}

		protected override bool SendInt(EDIInterchange outgoingInterchange)
		{
			throw new NotImplementedException();
		}

		public override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection messages)
		{
			return new CcsukInterchangeProvider(messages, iLogger);
		}

		public override string ApplicationCode
		{
			get { return ApplicationCodeList.Codes.GbCcsuk; }
		}

		protected override bool SendMoreInterchanges()
		{
			bool sendMore = MessageCountInCurrentRun == NumberToBatch;
			if (sendMore)
			{
				iLogger.Log(LogType.Information, string.Format(CultureInfo.CurrentCulture, "We processed the maximum batch size (" + NumberToBatch + ") " + " so we will attempt to fetch some more messages"));
			}
			else
			{
				iLogger.Log(LogType.Information, string.Format(CultureInfo.CurrentCulture, "Finished processing batch of " + MessageCountInCurrentRun + " messages "));
			}
			return sendMore;
		}

		protected override int NumberToBatch
		{
			get
			{
				return GBCustomsDataRegistry.Instance.CcsukInterchangePackagerBatchSize.Value;  // Messages to find per run
			}
		}
		public override int NumberOfMessagesPerInterchange
		{
			get
			{
				return 1;
			}
		}

		readonly ILogger iLogger;
	}
}
