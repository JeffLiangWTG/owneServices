using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class PNTSEDIMessage : FREDIMessage
	{
		public PNTSEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypeList.Codes.STO;
		}

		protected override MessageDataObject GenerateMessageDataObject()
		{
			switch (EM_MessageSubType)
			{
				case "016":
					return new IETS016MessageDataObject(this);
				case "028":
					return new IETS028MessageDataObject(this);
				case "029":
					return new IETS029MessageDataObject(this);
				case "030":
					return new IETS030MessageDataObject(this);
				case "095":
					return new IETS095MessageDataObject(this);
				case "410":
					return new IETS410MessageDataObject(this);
				case "460":
					return new IETS460MessageDataObject(this);
				case "928":
					return new IETS928MessageDataObject(this);
				case "906":
					return new IETS906MessageDataObject(this);
				default:
					return null;
			}
		}
	}
}
