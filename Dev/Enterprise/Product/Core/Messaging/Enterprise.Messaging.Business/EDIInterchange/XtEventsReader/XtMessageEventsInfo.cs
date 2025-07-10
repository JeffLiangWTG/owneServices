using System;
using System.Threading;
using CargoWise.Application;
using CargoWise.eHub.Common;
using Enterprise.Messaging.Integration;
using Xware.Xt.Grpc.Config;

namespace Enterprise.Messaging.Business
{
	public class XtMessageEventsInfo : IDisposable
	{
		public XtMessageEventsInfo(EDIInterchange interchange)
		{
			this.interchange = interchange;
			this.xtMsgId = (ulong)interchange.EI_XTInternalMsgID;
		}

		readonly ulong xtMsgId;
		readonly EDIInterchange interchange;
		IXtMessageEventsReaderClientProvider provider;

		public string MsgState
		{
			get
			{
				if (string.IsNullOrEmpty(_MsgState))
				{
					try
					{
						var client = InitializeXtClient();
						_MsgState = GetMsgState(client);
					}
					catch
					{
						_MsgState = string.Empty;
					}
				}
				return _MsgState;
			}
		}
		string _MsgState;

		public XtMessageEventsCollection XtMessageEvents
		{
			get
			{
				if (_xtMessageEvents == null)
				{
					try
					{
						var client = InitializeXtClient();
						_xtMessageEvents = new XtMessageEventsCollection(interchange);
						_xtMessageEvents.LoadCollection(client, true);
					}
					catch (Exception ex)
					{
						if (_xtMessageEvents != null)
						{
							_xtMessageEvents.RemoveAll();
						}
						else
						{
							_xtMessageEvents = new XtMessageEventsCollection(interchange);
						}
						_xtMessageEvents.AddSpecificLog(ex.Message, xtMsgId);
					}
				}
				return _xtMessageEvents;
			}
		}
		XtMessageEventsCollection _xtMessageEvents;

		public void ReLoad(bool shouldRefresh)
		{
			if (shouldRefresh)
			{
				_MsgState = null;
			}
			_xtMessageEvents?.LoadCollection(_client, shouldRefresh);
		}

		public void Dispose()
		{
			provider?.TearDown();
		}

		string GetMsgState(IXtMessageEventsReaderClient client)
		{
			var result = string.Empty;
			if (client != null)
			{
				var (state, _) = client.GetMsgState(xtMsgId);
				int.TryParse(state, out var statusCode);

				if (statusCode == xTStateCode.Codes.Finished)
				{
					result = RESULT_FINISHED;
				}
				else if ((statusCode >= xTStateCode.Codes.OutPortTransmissionError && statusCode <= xTStateCode.Codes.ReturningReplyError) || statusCode == 0)
				{
					result = RESULT_ERROR;
				}
				else
				{
					result = RESULT_PROCESSING;
				}
			}

			return result;
		}

		IXtMessageEventsReaderClient InitializeXtClient()
		{
			if (_client == null)
			{
				provider = GetXtMessageEventsReaderClientProvider();
				_client = provider?.XtMessageEventsReaderClient;
			}
			return _client;
		}
		IXtMessageEventsReaderClient _client;

		protected virtual IXtMessageEventsReaderClientProvider GetXtMessageEventsReaderClientProvider()
		{
			var config = GetConfiguration();

			if (config == null)
			{
				return null;
			}

			return ObjectFactory.Get<IXtMessageEventsReaderClientProvider>("IXtMessageEventsReaderClientProvider", config, CancellationToken.None);
		}

		Configuration GetConfiguration()
		{
			var configurationProvider = (IXtConfigurationProvider)ObjectFactory.Get("IXtConfigurationProvider");
			return configurationProvider.GetConfiguration();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const string")]
		const string RESULT_FINISHED = "Finished";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const string")]
		const string RESULT_ERROR = "Error";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const string")]
		const string RESULT_PROCESSING = "In Processing";
	}
}
