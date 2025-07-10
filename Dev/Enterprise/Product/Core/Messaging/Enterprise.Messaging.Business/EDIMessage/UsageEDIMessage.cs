using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Enterprise.Messaging.Business
{
	public class UsageEDIMessage : EDIMessage, IUsageEDIMessage
	{
		public UsageEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void InitializeCreationStackTrace()
		{
		}

		public JObject UsageProperties
		{
			get
			{
				return JsonConvert.DeserializeObject<JObject>(EM_MessageData.ToUTF8());
			}
			set
			{
				EM_MessageData = ZBlob.FromUTF8(JsonConvert.SerializeObject(value, Formatting.Indented));
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_IsActive = true;
			EM_Status = EDIMessage.Status.Captured;
			EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			EM_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
			EM_SystemLastEditTimeUtc = EM_SystemCreateTimeUtc;
			EM_SystemLastEditUser = EM_SystemCreateUser;
			EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		}

		protected override void PopulateMessageNumber()
		{
			if (MessageNumberStrategy != null)
			{
				EM_MessageNum = MessageNumberStrategy.GetMessageReferenceNumber();
			}
			else if (EM_MessageNum.IsEmpty)
			{
				EM_MessageNum = Env.NumberFountains.UsageEDIMessageNumber.GetNextFormatted(Factory);
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (!IsTransmitMessage && !IsInDatabase)
			{
				PopulateMessageNumber();
			}
		}

		protected override ZBool ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverride() => true;
	}
}
