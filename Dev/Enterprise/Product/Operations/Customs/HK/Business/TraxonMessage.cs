using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.HK.Business
{
	/// <summary>
	/// Summary description for TraxonMessage.
	/// </summary>
	public class TraxonMessage : EDIMessage
	{
		public TraxonMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.Traxon;
			EM_IsTestMessage = false;
			EM_MessageType = EDIInterchange.ApplicationCodes.Traxon;
			EM_MessageSubType = EDIInterchange.ApplicationCodes.Traxon;
			EM_Status = EDIMessage.Status.Queued;
		}

		protected override string GetMessageReferenceNumber()
		{
			var branch = Branch ?? GlbBranch.CurrentBranch;
			var sender = HKDataRegistry.Instance.HKTraxonSenderID.GetValueWithoutFallback(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty);
			var receiver = "TRAXON"; // This is used as the Sender + Receive Mailboxes are greater then 35 in length.
			return Env.NumberFountains.EDIFACTNumberFountain("M", sender, receiver).GetNextFormatted(Factory) + "00";
		}
	}
}
