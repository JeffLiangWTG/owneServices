using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Integration;

namespace Enterprise.Client.EDI.eRouter.Business
{
	public class eRouterEdiEnterpriseCommunication : AutoeRouterEdiEnterpriseCommunication
	{
		public eRouterEdiEnterpriseCommunication(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void Delete()
		{
			throw new NotSupportedException("Deleting eRouterEdiEnterpriseCommunication is not supported");
		}

		public override bool IsSavedByFactory
		{
			get { return false; }
		}

		#region Test Data
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			EC_ApplicationCode = "ZAC";
			EC_CompanyCode = "ECC";
			EC_Data = "DUMMY TEST DATA";
			EC_Direction = ReceiveTransmitList.Codes.Receive;
			EC_EnterpriseCode = "ZEC";
			EC_Identifier = "ID123456";
			EC_Recipient = "DUMMYRECIPIENT";
			EC_Sender = "DUMMYSENDER";
			EC_TransmitDate = new ZDateTime(2007, 10, 13, 14, 23, 32);
			EC_Type = "ZTY";
		}

#endif
		#endregion
	}
}

