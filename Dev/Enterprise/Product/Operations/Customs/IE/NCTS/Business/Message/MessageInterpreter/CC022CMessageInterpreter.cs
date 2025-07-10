using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC022CMessageInterpreter : InboundMessageInterpreter<CC022CProvider>
	{
		public CC022CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC022CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"5B15FEAD-DA42-4A26-B448-C357E995AFF8",
			"A Notification to Amend Declaration (IE022) message has been received for Job {0}.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (Res.GetString("D50AB594-235B-4185-93B0-E70A033F8232", "Amend Notification Date & Time"), provider.AmendmentNotificationDateAndTime.ToLongTimeString());
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			foreach (var functionalError in provider.FunctionalErrors)
			{
				yield return (Res.GetString("793D6C9F-55CD-4579-AE60-8DE0EC4B5329", "Functional Error:"), new (string, string)[]
				{
					(CommonResStrings.SequenceNumber, functionalError.SequenceNumber.ToString()),
					(Res.GetString("1E28D4E0-B738-4603-9EF6-F45BF4E88F0B", "Error Pointer"), functionalError.ErrorPointer),
					(Res.GetString("AA74BF35-CBB7-4772-A92E-C7A13859D63D", "Error Code"), GetCodeAndDescription(functionalError.ErrorCode, UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL180)),
					(Res.GetString("8041A304-42FB-4A65-AD28-43BA4024ABC0", "Error Reason"), functionalError.ErrorReason),
					(Res.GetString("10494B6B-3D52-42EA-81C9-604CDF9FBF99", "Original Value"), functionalError.OriginalAttributeValue),
				});
			}
		}
	}
}
