using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CA.Business
{
	public class CADMessage : EDIMessage
	{
		public CADMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		internal static CADMessage GetLatestCADResponseMessage(CusEntryHeader entryHeader)
		{
			if (entryHeader == null)
			{
				throw new ArgumentNullException(nameof(entryHeader));
			}

			var result = entryHeader.Messages.OfType<CADMessage>().Where(x =>
					x.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Receive &&
					x.EM_Status == EDIMessageStatusList.Codes.Received &&
					x.IsClearMessage)
				.OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
			return result;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodeList.Codes.CAIMP;
			EM_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
		}

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("M", "IMP", EDIInterchange.ApplicationCodes.CAIMP).GetNextFormatted(Factory);
		}

		public override ZString BatchNumber
		{
			get { return GetVersionID(); }
		}

		public ZBool IsQueryMessage
		{
			get { return EM_MessageSubType == MessageTypeList.Codes.Query; }
		}

		bool IsClearMessage => EM_MessageSubType == MessageStatusList.Codes.ClearOriginal || EM_MessageSubType == MessageStatusList.Codes.ClearChange;

		ZString GetVersionID()
		{
			var applicationReferenceID = new Regex(@"<ApplicationReferenceID>\d+").Match(EM_MessageText);
			var versionID = ZString.Empty;
			if (applicationReferenceID.Success)
			{
				var value = applicationReferenceID.Value.Trim();
				versionID = value.SubstringOrNull(value.Length - 8, 5);
			}
			return versionID;
		}
	}
}
