using System;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Business;
using Enterprise.xTMessaging.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xware.Xt.Grpc.Config;
using Res = Enterprise.Main.DiagnosticsAndTesting.Res;

namespace Enterprise.Diagnostics
{
	public class DirectxTDiagnostics : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DirectxTDiagnostics()
			: base(new BusinessObjectFactory())
		{
			CW1License = ObjectFactory.Get<IProductRegistration>()?.Key.DatabaseType;
			Configuration = new CW1RegistryConfigurationProvider().GetConfiguration();
		}

		public bool CheckConnection(out string errMsg)
		{
			if (Configuration == null || !Configuration.IsValid())
			{
				errMsg = string.Format(MessageFormat, CW1License,
					Res.GetString("D149EBC8-F558-4B3B-8C77-4FABD612463C", "Failed to get xT configuration. Please check your registry settings, and ensure the Reference Database service tasks (REF/RDU) are running."));
				return false;
			}

			MsgClientProvider = new MsgClientProvider(Configuration, new CancellationToken());
			var connector = new DirectxTConnector(MsgClientProvider, new Cw1DirectxTMessagingConfig(), new Logger());

			try
			{
				var connectorResult = connector.InitializeIfNeeded();
				errMsg = connectorResult.Item1 ? Res.GetString("15558B7A-5B14-42B4-BE77-6657DC29BAC1", "Connection successfully established with xT endpoint.") : connectorResult.Item2;
				return connectorResult.Item1;
			}
			catch (Exception ex)
			{
				var error = ex.InnerException?.Message ?? MsgClientProvider.ErrorMessage;
				errMsg = string.Format(MessageFormat, CW1License, ParseErrorMessage(error));
				return false;
			}
			finally
			{
				connector.Dispose();
			}
		}

		public string ParseErrorMessage(string errMsg)
		{
			var pattern = (NoResString)@"StatusCode=""(\w+)"".*Detail=""([^""]*)""";
			var match = Regex.Match(errMsg, pattern);
			if (match.Success)
			{
				var statusCode = match.Groups[1].Value;
				var detail = match.Groups[2].Value;
				switch (statusCode)
				{
					case Constants.xTGRPCErrorStatusCodes.Unavailable:
						return Res.GetString("102B2500-7853-45FC-BBC2-F022DB98BFC1", "Unable to form a connection. Please check your connectivity, and ensure Registry settings and Reference Data are up to date. \r\nError Message: (")
							+ detail + ").";
					case Constants.xTGRPCErrorStatusCodes.InvalidArgument:
						return Res.GetString("88DC71B0-4F06-48BB-88A8-1071BFF9420B", "There is a problem with the xT server you are trying to connect to. Please raise an incident or contact WiseTech support. \r\nError Message: (")
							+ detail + ").";
					case Constants.xTGRPCErrorStatusCodes.Internal:
						return Res.GetString("BE14694E-491B-44DB-9D3B-62C5FA3B96F2", "xT services are currently in the process of an upgrade. Please try again later or contact WiseTech support for urgent matters. \r\nError Message: (")
							+ detail + ").";
					default:
						return errMsg;
				}
			}
			return errMsg;
		}

		public void Send()
		{
			Interchange = Factory.New<XmlEDIInterchange>();

			Interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			Interchange.EI_From = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			Interchange.EI_To = NormalizeEI_To(GlbCompany.CurrentCompany.LicenceKeyIdentifier);
			Interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			Interchange.EI_SessionGUID = Interchange.PK;
			Interchange.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			Interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.DirectxT;
			Interchange.EI_HeaderNText = "";
			Interchange.EI_BodyText = $@"<?xml version=""1.0"" ?><DiagnosticMessage><Receiver>{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}</Receiver></DiagnosticMessage>"; // It's the diagnostic xml body
			Interchange.EI_Status = EDIInterchange.Status.Queued;
			Interchange.EI_TransportType = EDIInterchange.TransportType.xT;
			Interchange.EI_IsActive = true;

			var message = Factory.New<XmlEDIMessage>();
			message.EM_EI = Interchange.PK;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_GE = GlbDepartment.CurrentDepartment.PK;
			message.EM_IsTestMessage = true;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.Orders;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.XMS;
			message.EM_ReceiveTransmit = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			message.EM_MessageType = EDIInterchangeTypeList.Codes.DirectxT;
			message.EM_TransportType = Interchange.EI_TransportType;

			Factory.Save();
		}

		string NormalizeEI_To(string originalEI_To)
		{
			return originalEI_To.Length == 9
				? originalEI_To.Substring(0, 3) + originalEI_To.Substring(6, 3)
				: originalEI_To;
		}

		public bool Check()
		{
			if (Interchange != null)
			{
				Factory.ClearQueryCache();

				var query = new ZQuery(EDIInterchangeSchema.EI_SessionGUID, Interchange.EI_SessionGUID);
				query.AddToFilter(JoinCondition.And, EDIInterchangeSchema.PK, SQLComparisonOperator.NotEqual, Interchange.PK);
				query.AddToFilter(JoinCondition.And, EDIInterchangeSchema.EI_InterchangeType, EDIInterchangeTypeList.Codes.DirectxT);
				query.AddToFilter(JoinCondition.And, EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued);
				var receivedInterchange = Factory.LoadTop1<EDIInterchange>(query);

				if (receivedInterchange != null)
				{
					receivedInterchange.EI_Status = EDIInterchange.Status.Received;
					Factory.Save();
					return true;
				}
				else
				{
					return false;
				}
			}

			return false;
		}

		internal EDIInterchange Interchange;

		protected string MessageFormat = (NoResString)@"
CW1 license/DB Type: {0}
Result: {1}
";

		protected string CW1License { get; set; }

		protected virtual Configuration Configuration { get; set; }

		protected virtual IMsgClientProvider MsgClientProvider { get; set; }
	}
}
