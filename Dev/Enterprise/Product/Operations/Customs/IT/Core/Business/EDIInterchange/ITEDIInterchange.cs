using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageStructure;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

public sealed class ITEDIInterchange : EDIInterchange, Integration.Customs.IT.IEDIInterchange
{
	public ITEDIInterchange(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public IInterchangeFileNameStrategy FileNameStrategy { get; set; }

	public override void OnSaving()
	{
		base.OnSaving();
		ReplacePlaceHolderWithFileName();
	}

	public override ZString EI_ApplicationCode
	{
		get => base.EI_ApplicationCode;
		set
		{
			if (value != ApplicationCodes.ITCustoms)
			{
				throw new InvalidOperationException(Res.GetString("6D6F577A-F099-43A7-9950-9D00BFBE19E3", "Invalid Application Code, must be: {0}", ApplicationCodes.ITCustoms));
			}
			base.EI_ApplicationCode = value;
			EI_ApplicationCodeInfo.RefreshBinding();
		}
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();

		EI_ApplicationCode = ApplicationCodes.ITCustoms;
	}

	void ReplacePlaceHolderWithFileName()
	{
		var headerText = EI_HeaderText;
		if (!IsInDatabase && HeaderTextContainsPlaceHolder(headerText) && FileNameStrategy != null)
		{
			EI_HeaderText = headerText.Replace(Constants.FileNamePlaceHolder, FileNameStrategy.GetFileName());
		}
	}

	bool HeaderTextContainsPlaceHolder(ZString headerText) => headerText.IndexOf(Constants.FileNamePlaceHolder, StringComparison.InvariantCulture) != -1;

	#region Constants

	public static class Constants
	{
		public const string FileNamePlaceHolder = "PLACEHOLDER";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message")]
		public const string InvalidHeader = "<invalid header>";
	}

	#endregion

	public ZString GetFileNameFromHeaderText()
	{
		var fileName = ZString.Empty;
		if (IsTransmitInterchange)
		{
			var customsMessageHeaderText = MessageProcessorHelper.RetrieveValueOfXmlNode(EI_HeaderText, InterchangeHeaderTextBuilder.XmlElementName.Header);
			if (!customsMessageHeaderText.IsEmpty)
			{
				fileName = HeaderTextContainsPlaceHolder(customsMessageHeaderText) ? Constants.FileNamePlaceHolder : LoadHeaderTextAndRetriveFileName(customsMessageHeaderText);
			}
		}
		else
		{
			fileName = MessageProcessorHelper.RetrieveValueOfXmlNode(EI_HeaderText, InterchangeHeaderTextBuilder.XmlElementName.FileName);
		}
		return fileName;
	}

	string LoadHeaderTextAndRetriveFileName(ZString customsMessageHeaderText)
	{
		string fileName;
		try
		{
			var customsInterchangeHeader = CustomsInterchangeHeader.NewFromText(customsMessageHeaderText, ignoreTrasmissionEnviroment: true);
			fileName = customsInterchangeHeader.FileName;
		}
		catch
		{
			fileName = Constants.InvalidHeader;
		}
		return fileName;
	}
}
