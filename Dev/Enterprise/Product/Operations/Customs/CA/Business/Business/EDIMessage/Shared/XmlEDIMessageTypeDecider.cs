using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class XmlEDIMessageTypeDecider : Integration.Customs.CA.IXmlEDIMessageTypeDecider
	{
		public Type GetXmlEDIMessageType(DataRow row, BusinessObjectFactory factory)
		{
			var result = typeof(XmlEDIMessage);
			if (row[EDIMessage.Schema.EM_MessageSubType]?.ToString()?.Trim() == EDIMessageSubTypeList.Codes.XmlUniversalEvent
				&& row[EDIMessage.Schema.EM_MessageType]?.ToString()?.Trim() == EDIMessageTypeList.Codes.XDC
				&& GetSystemDefinedValue((Guid)row[EDIMessage.Schema.PK], factory) == UniversalEventMessageTypes.Codes.D4Notices)
			{
				result = typeof(UniversalEventMessage);
			}
			return result;
		}

		ZString GetSystemDefinedValue(ZGuid parentID, BusinessObjectFactory factory)
		{
			var genAddOnQuery = new ZQuery(GenAddOnColumnSchema.XA_ParentID, parentID);
			genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, EDIMessageSchema.Constants.Prefix);
			genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, EDIMessage.Schema.XMLCustomsMessageType);
			var addOnColumn = factory.Load<GenAddOnColumn>(genAddOnQuery).FirstOrDefault();
			return addOnColumn?.XA_Data.Trim() ?? ZString.Empty;
		}
	}
}
