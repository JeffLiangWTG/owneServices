using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

public sealed class ITEDIMessage : EDIMessage, IMessageType
{
	public const string ITMessageNumberPlaceholder = "_IT_MSG_NMBR_PLCHLDR_";

	public ITEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public override ZString EM_MessageText
	{
		get => base.EM_MessageText;
		set
		{
			var oldValue = EM_MessageText;
			base.EM_MessageText = value;
			if (!IsCopying && EM_MessageText != oldValue)
			{
				messageInterpretation = null;
			}
		}
	}

	public override ZString EM_MessageInterpretation
	{
		get => messageInterpretation ?? (messageInterpretation = GetMessageInterpretation());
		set
		{
			base.EM_MessageInterpretation = value;
			messageInterpretation = value;
		}
	}

	ZString GetMessageInterpretation()
	{
		var formatter = PrettyFormatterFactory.GetMessagePrettyFormatter(Factory, this);
		if (formatter != null)
		{
			return formatter.GetFormattedText(EM_MessageText);
		}
		return base.EM_MessageInterpretation;
	}

	public override ZString EM_Status
	{
		get => base.EM_Status;
		set
		{
			var oldValue = EM_Status;
			base.EM_Status = value;
			if (!IsCopying && EM_Status != oldValue)
			{
				SetHeaderAsFailedFromTransmissionIfApplicable();
			}
		}
	}

	public bool IsCancellation => EM_MessageType == EDIMessageTypeList.Codes.Cancellation;

	void SetHeaderAsFailedFromTransmissionIfApplicable()
	{
		if (EM_MessageType == SADConstants.CustomsInterchangeType.IdocR && EM_Status == EDIMessage.Status.Failed && EM_LinkedObject is ICustomsEntryTransmissionFailable transmissionFailureSettable)
		{
			transmissionFailureSettable.SetAsFailedFromTransmission();
		}
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		EM_ApplicationCode = ApplicationCodes.ITCustoms;
	}

	protected override void PopulateMessageNumber()
	{
		base.PopulateMessageNumber();
		ReplaceInterchangeBodyTextMessageNoPlaceHolderForManualProcedure();
	}

	protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
	{
		if (UsesPlaceHolders)
		{
			switch (EM_MessageSubType)
			{
				case SADConstants.MessageSubTypes.NB:
					GetNumberFountainNumbersAndFillInPlaceHoldersForNB(setMessageNumber: true);
					break;

				case SADConstants.MessageSubTypes.NBE:
					GetNumberFountainNumbersAndFillInPlaceHoldersForNB(setMessageNumber: false);
					base.GetNumberFountainNumbersAndFillInPlaceHolders();
					break;

				default:
					base.GetNumberFountainNumbersAndFillInPlaceHolders();
					break;
			}
		}
	}

	#region Implementation

	void GetNumberFountainNumbersAndFillInPlaceHoldersForNB(bool setMessageNumber)
	{
		var regex = new Regex(@"<<MSGNO PLACEHOLDER NB \d>>");
		var matches = regex.Matches(EM_MessageText);
		var numberOfMessages = matches
			.OfType<Match>()
			.Select(m => m.Value)
			.Distinct()
			.Count();

		if (numberOfMessages > 0)
		{
			var batchMessageNumbers = new List<ZString>();
			for (int i = 0; i < numberOfMessages; i++)
			{
				var messageNum = GetMessageReferenceNumber();
				EM_MessageText = EM_MessageText.Replace(MessageNumberPlaceHolderForNB(i + 1), messageNum);
				batchMessageNumbers.Add(messageNum);
			}

			if (setMessageNumber)
			{
				EM_MessageNum = numberOfMessages == 1 ? batchMessageNumbers.First() : (ZString)FormattableString.Invariant($"{batchMessageNumbers.First()}:{batchMessageNumbers.Last()}");
			}
		}
	}

	void ReplaceInterchangeBodyTextMessageNoPlaceHolderForManualProcedure()
	{
		var interchange = Interchange;
		if (EM_Status == EDIMessageStatusList.Codes.Manual && interchange != null && interchange.EI_Status == EDIInterchangeStatusList.Codes.Manual)
		{
			interchange.EI_BodyText = interchange.EI_BodyText.Replace(MessageNumberPlaceHolderOverride, EM_MessageNum);
		}
	}

	#endregion

	public static string MessageNumberPlaceHolderForNB(int indexOfMessage) => FormattableString.Invariant($"<<MSGNO PLACEHOLDER NB {indexOfMessage}>>");

	public ZString GetFileName()
	{
		return Interchange is ITEDIInterchange interchange
			? interchange.GetFileNameFromHeaderText()
			: $"EdiMessage_{EM_ApplicationCode}_{EM_MessageNum}.txt";
	}

	public void Write(Stream stream)
	{
		using (var reader = GetEM_MessageTextReader())
		using (var writer = new StreamWriter(stream))
		{
			if (Interchange != null && Interchange.EI_InterchangeType == SADConstants.CustomsInterchangeType.IdocR)
			{
				var customsMessageHeaderText = MessageProcessorHelper.RetrieveValueOfXmlNode(Interchange.EI_HeaderText, InterchangeHeaderTextBuilder.XmlElementName.Header);
				writer.WriteLine(customsMessageHeaderText);
			}
			reader.CopyTo(writer);
		}
	}

	string messageInterpretation;
}
