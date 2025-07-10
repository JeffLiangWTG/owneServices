using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EDIMessageTypeDecider : TypeDecider, Integration.Customs.AU.IEDIMessageTypeDecider
	{
		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForNew() => null;

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			// TODO: Put other AU message type case here
			switch (row[EDIMessage.Schema.EM_ApplicationCode].ToString().Trim())
			{
				case EDIMessage.ApplicationCodes.COLS:
					if (row[EDIMessage.Schema.EM_MessageType].ToString().Trim() == AUCOLSMessageTypeList.Codes.XtMessageError)
					{
						return typeof(COLSXtErrorResponseMessage);
					}
					else
					{
						return typeof(COLSMessage);
					}
				default:
					return typeof(EDIMessage);
			}
		}
	}
}
