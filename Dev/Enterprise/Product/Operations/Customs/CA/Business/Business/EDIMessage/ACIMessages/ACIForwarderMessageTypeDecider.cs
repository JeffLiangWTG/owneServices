using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class ACIForwarderMessageTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			switch (row[EDIMessage.Schema.EM_MessageType].ToString().Trim())
			{
				case MessageTypeList.Codes.ACIForwarderClose:
					return typeof(ACIForwarderCloseMessage);
				case MessageTypeList.Codes.ACIHouseBill:
					return typeof(ACIHouseBillMessage);
				default:
					return typeof(ACIForwarderMessage);
			}
		}

		public override Type GetTypeForBinding()
		{
			return typeof(ACIForwarderMessage);
		}

		public override Type GetTypeForNew()
		{
			return typeof(ACIForwarderMessage);
		}
	}
}
