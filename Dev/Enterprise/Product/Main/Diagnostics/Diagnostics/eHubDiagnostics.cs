using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Diagnostics
{
	public class eHubDiagnostics : NonPersistentBusinessObject, IObsoleteValidation
	{
		public eHubDiagnostics()
			: base(new BusinessObjectFactory())
		{
		}

		public void Send()
		{
			Interchange = Factory.New<XmlEDIInterchange>();

			Interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			Interchange.EI_From = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			Interchange.EI_To = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			Interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			Interchange.EI_SessionGUID = Interchange.PK;
			Interchange.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			Interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.TST;
			Interchange.EI_HeaderNText = "<EDIDelivery><FileName></FileName><EmailSubject></EmailSubject></EDIDelivery>"; // It's bloody xml
			Interchange.EI_BodyText = (NoResString)@"<ns0:Test xmlns:ns0=""http://www.edi.com.au/EnterpriseService/""><A><a1>a1</a1><a2>a2</a2></A><B><b1>b1</b1><b2>b2</b2></B></ns0:Test>"; // It's bloody xml
			Interchange.EI_Status = EDIInterchange.Status.eHubQueued;
			Interchange.EI_TransportType = EDIInterchange.TransportType.eHub;
			Interchange.EI_IsActive = true;

			var message = Factory.New<XmlEDIMessage>();
			message.EM_EI = Interchange.PK;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_GE = GlbDepartment.CurrentDepartment.PK;
			message.EM_IsTestMessage = true;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.Orders;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.XMS;
			message.EM_ReceiveTransmit = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			message.EM_MessageType = EDIInterchangeTypeList.Codes.TST;
			message.EM_TransportType = Interchange.EI_TransportType;

			Factory.Save();
		}

		public bool Check()
		{
			if (Interchange != null)
			{
				Factory.ClearQueryCache();

				var query = new ZQuery(EDIInterchangeSchema.EI_SessionGUID, Interchange.EI_SessionGUID);
				query.AddToFilter(JoinCondition.And, EDIInterchangeSchema.PK, SQLComparisonOperator.NotEqual, Interchange.PK);
				query.AddToFilter(JoinCondition.And, EDIInterchangeSchema.EI_InterchangeType, EDIInterchangeTypeList.Codes.TST);
				var receivedInterchange = Factory.LoadTop1<EDIInterchange>(query);

				if (receivedInterchange != null)
				{
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
	}
}
