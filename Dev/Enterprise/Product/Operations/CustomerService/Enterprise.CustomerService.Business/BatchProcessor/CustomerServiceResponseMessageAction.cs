using System;
using System.Collections.Generic;

using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

using Enterprise.eHubMessaging.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;

using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.CustomerService.Business
{
	public class CustomerServiceResponseMessageAction : IMessageAction
	{
		public CustomerServiceResponseMessageAction(BusinessObjectFactoryProvider factoryProvider)
		{
			//Save logic is in helper class and rely on it to do concurrency check
			//So here not use FactoryProvider to avoid any unexpected factory save
			Factory = new BusinessObjectFactory();
		}

		readonly BusinessObjectFactory Factory;

		#region Process

		public static bool ProcessAndSave(Xsd.CustomerServiceResponse response)
		{
			var factoryProvider = new BusinessObjectFactoryProvider();
			var action = new CustomerServiceResponseMessageAction(factoryProvider);

			return action.Process(response, null);
		}

		bool Process(Xsd.CustomerServiceResponse response, INotifications notifications)
		{
			bool result = false;

			IncidentApproval incident = LoadOrCreateIncident(response);

			if (incident != null)
			{
				CustomerServiceResponseHelper helper = new CustomerServiceResponseHelper(response, incident, notifications);
				helper.ProcessAndSave();
				result = true;
			}
			else if (notifications != null)
			{
				notifications.AddError(string.Format((NoResString)"Incident {0}/{1} is not found.", response.IncidentNumber, response.ClientReferenceNumber));
			}

			return result;
		}

		bool ExecuteActionCore(EDIMessage message, INotifications notifications)
		{
			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(Xsd.CustomerServiceResponse));
			var response = (Xsd.CustomerServiceResponse)SystemMessage.Deserialize(message, serializer);
			return Process(response, notifications);
		}

		IncidentApproval LoadOrCreateIncident(Xsd.CustomerServiceResponse response)
		{
			IncidentApproval incident = null;

			if (response.Action != IncidentApprovalLookups.Actions.Add)
			{
				ZQuery filter = new ZQuery(IncidentApprovalSchema.IA_ClientReference, response.ClientReferenceNumber);
				incident = Factory.LoadTop1<IncidentApproval>(filter);

				if (incident == null && !response.IncidentNumber.IsEmpty && !response.LicenceCode.IsEmpty)
				{
					filter = new ZQuery(IncidentApprovalSchema.IA_IncidentNumber, response.IncidentNumber);
					filter.AddToFilter(IncidentApprovalSchema.IA_LicenceCode, response.LicenceCode);
					incident = Factory.LoadTop1<IncidentApproval>(filter);
				}

				if (incident != null)
				{
					incident.IsUpdatingByResponseProcessor = true;
				}
			}

			return incident;
		}

		#endregion

		#region IMessageAction implementation

		bool IMessageAction.ExecuteAction(EDIMessage message, INotifications notifications, out List<ITransactionParticipant> participants)
		{
			try
			{
				participants = new List<ITransactionParticipant>(0);
				return ExecuteActionCore(message, notifications);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("CustomerServiceResponseMessageAction", "Message: " + message.EM_MessageTextShort, ex);
				participants = new List<ITransactionParticipant>();
				return false;
			}
		}

		void IMessageAction.SendNotificationEmail(ZString subject, ZString body, INotifications notifications, bool onSuccess)
		{
		}

		#endregion
	}
}
