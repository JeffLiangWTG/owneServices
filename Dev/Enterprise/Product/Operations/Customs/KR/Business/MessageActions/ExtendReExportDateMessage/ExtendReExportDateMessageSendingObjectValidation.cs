using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ExtendReExportDateMessageSendingObjectValidation : JobDeclarationMiscMessageSendingObjectCoreValidation
	{
		public ExtendReExportDateMessageSendingObjectValidation(ExtendReExportDateMessageSendingObject parent) : base(parent) { }

		protected new ExtendReExportDateMessageSendingObject Parent => (ExtendReExportDateMessageSendingObject)base.Parent;

		#region ReasonDescription
		public void ValidateReasonDescription()
		{
			ValidateCalculatedProperty(Parent.ReasonDescriptionInfo);
		}
		protected void CheckReasonDescription()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ReasonDescriptionInfo);
		}
		#endregion
		#region NewReExportDate
		public void ValidateNewReExportDate()
		{
			ValidateCalculatedProperty(Parent.NewReExportDateInfo);
		}
		protected void CheckNewReExportDate()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.NewReExportDateInfo);
			TypeValidation.CheckValidZDateTimeWithoutRange(Parent.NewReExportDateInfo);
			TypeValidation.CheckValidZDateTimeRange(Parent.NewReExportDateInfo);
		}
		#endregion
		#region ShouldSend
		protected override void CheckShouldSend()
		{
			base.CheckShouldSend();
			if (Parent.ShouldSend)
			{
				if (Parent.CurrentReExportScheduledDate.IsEmpty)
				{
					Parent.ShouldSendInfo.AddError(Res.GetString("3936E7C0-C114-4717-8075-ADF0329C03C7", "You cannot send a D72 message as there is no scheduled export date."));
				}

				var entryLineListWhichHaveNo5FNAcceptance = GetEntryLinesWhichHaveNo5FNAcceptance();
				if (entryLineListWhichHaveNo5FNAcceptance.Length > 0)
				{
					var orderedList = entryLineListWhichHaveNo5FNAcceptance.OrderBy(x => x.EntryLineNo);
					var stringBuilder = new ZStringBuilder();
					foreach (var entryLine in orderedList)
					{
						stringBuilder.Append(entryLine.EntryLineNo.ToString(Constants.NumberFormatDigit.D3));
					}
					var entryLines = stringBuilder.ToStringWithDelimiterBetweenAppends(", ");
					Parent.ShouldSendInfo.AddMessageError(string.Format(Res.GetString("32457EFA-3A9F-4792-A8B1-FB81FB6D98DF", "There are entry lines ({0}) for which 5FN has not been accepted. If 5FN has not been accepted, you don't have to send this message. Please check."), entryLines));
				}
			}
		}

		MessageSendingEntryLineObject[] GetEntryLinesWhichHaveNo5FNAcceptance()
		{
			var list = new List<MessageSendingEntryLineObject>();
			var entryNumbers = Parent.Header.EntryNumbers.Where(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5FN);
			foreach (MessageSendingEntryLineObject entryLine in Parent.D72EntryLines)
			{
				var entryStatus = entryNumbers.FirstOrDefault(x => x.CE_EntryLineReference == entryLine.EntryLineNo.ToString())?.CE_EntryStatus ?? ZString.Empty;
				if (entryStatus != CustomsMessageStatusTypeList.Codes.OriginalAccepted)
				{
					list.Add(entryLine);
				}
			}
			return list.ToArray();
		}
		#endregion
	}
}
