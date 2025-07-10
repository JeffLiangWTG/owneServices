using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Registry;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(ITEDIInterchange))]
sealed class ITEDIInterchangeTest : EDIInterchangeTest
{
	[TestDate(2020, 09, 14)]
	public void TestReplacePlaceHolderWithFileName()
	{
		var interchange = GetNewInterchange();
		AssertNull("FileNameStrategy is null", interchange.FileNameStrategy);
		Assert("interchange is not in database", !interchange.IsInDatabase);
		Factory.Save();
		AssertContains("FileNameStrategy is null and interchange was not in database --> PLACEHOLDER has not been replaced", "PLACEHOLDER", interchange.EI_HeaderText);

		interchange.FileNameStrategy = new InterchangeFileNameStrategy(account, "R", Factory);
		Assert("interchange is in database", interchange.IsInDatabase);
		Factory.Save();
		AssertContains("FileNameStrategy is not null but interchange was already in database--> PLACEHOLDER has not been replaced", "PLACEHOLDER", interchange.EI_HeaderText);

		interchange = GetNewInterchange();
		interchange.FileNameStrategy = new InterchangeFileNameStrategy(account, "R", Factory);
		Assert("interchange is not in database (it has been recreated)", !interchange.IsInDatabase);
		Factory.Save();
		AssertNotContains("FileNameStrategy is not null and interchange was not in database--> PLACEHOLDER has been replaced", "PLACEHOLDER", interchange.EI_HeaderText);
		AssertContains("Effective file name", "12340914.R00", interchange.EI_HeaderText);

		Assert("interchange is in database (it has been saved again)", interchange.IsInDatabase);
		interchange.EI_HeaderText = interchange.EI_HeaderText.Replace("12340914.R00", "PLACEHOLDER");
		AssertNotContains("File name has been manually replaced with placeholder", "12340914.R00", interchange.EI_HeaderText);
		Factory.Save();
		AssertContains("A new file name has not been generated", "PLACEHOLDER", interchange.EI_HeaderText);
	}

	public void TestGetFileNameFromHeaderText()
	{
		var interchange = Factory.New<ITEDIInterchange>();
		interchange.EI_From = "XXX";
		interchange.EI_To = "YYY";

		void TestGetFileNameFromHeaderText()
		{
			var isTransmit = interchange.IsTransmitInterchange;
			interchange.EI_HeaderText = "";
			AssertEquals("When HeaderText is empty", "", interchange.GetFileNameFromHeaderText());

			interchange.EI_HeaderText = GetHeaderText(isTransmit: isTransmit);
			AssertEquals("When HeaderText is PLACEHOLDER", "PLACEHOLDER", interchange.GetFileNameFromHeaderText());

			interchange.EI_HeaderText = GetHeaderText(filename: "X", isTransmit: isTransmit);
			AssertEquals("When HeaderText is invalid", isTransmit ? "<invalid header>" : "X", interchange.GetFileNameFromHeaderText());

			interchange.EI_HeaderText = GetHeaderText(filename: "845A1201.R02", isTransmit: isTransmit);
			AssertEquals("When HeaderText is ok", "845A1201.R02", interchange.GetFileNameFromHeaderText());
		}

		interchange.EI_ReceiveTransmit = "TRX";
		TestGetFileNameFromHeaderText();

		interchange.EI_ReceiveTransmit = "RCV";
		TestGetFileNameFromHeaderText();
	}

	public void TestDefaultValues()
	{
		var interchange = Factory.New<ITEDIInterchange>();

		AssertEquals("EI_ApplicationCode", "ITM", interchange.EI_ApplicationCode);
	}

	public void TestApplicationCode()
	{
		var interchange = Factory.New<ITEDIInterchange>();

		AssertExceptionThrown<InvalidOperationException>("Exception expected when ApplicationCode value is not ITM", "Invalid Application Code, must be: ITM", () => interchange.EI_ApplicationCode = "XXX");
		AssertNoExceptionThrown("No exception expected when ApplicationCode is ITM", () => interchange.EI_ApplicationCode = "ITM");
	}

	ITEDIInterchange GetNewInterchange()
	{
		var interchange = Factory.New<ITEDIInterchange>();
		interchange.EI_From = "XXX";
		interchange.EI_To = "YYY";
		interchange.EI_HeaderText = GetHeaderText();
		return interchange;
	}

	string GetHeaderText(string staff = null, string node = null, string messageType = null, string accountNumber = null, string filename = null, bool isTransmit = true)
	{
		var stringBuilder = new ZStringBuilder();

		stringBuilder.AppendLine("<ITMessage>");
		stringBuilder.AppendLine($"<Staff>{staff ?? "BOB"}</Staff>");
		stringBuilder.AppendLine($"<Node>{node ?? "1234"}</Node>");
		stringBuilder.AppendLine($"<MessageType>{messageType ?? "R"}</MessageType>");
		stringBuilder.AppendLine($"<AccountNumber>{accountNumber ?? "11111111111-001"}</AccountNumber>");

		filename = filename ?? "PLACEHOLDER";
		stringBuilder.AppendLine(isTransmit
			? $"<Header>1234            {filename}            137100    13149600150     003 00003</Header>"
			: $"<FileName>{filename}</FileName>");
		stringBuilder.AppendLine("</ITMessage>");

		return stringBuilder.ToString();
	}

	protected override void TestBizObjectField(ZPropertyInfo info)
	{
		if (info.Name == ITEDIInterchange.Schema.EI_ApplicationCode)
		{
			AssertEquals("ITM", (ZString)info.Value);
		}
		else
		{
			base.TestBizObjectField(info);
		}
	}

	protected override Dictionary<string, IZType> CachedValueForSettingValueCallsRefreshBindingTestCore
	{
		get
		{
			var result = base.CachedValueForSettingValueCallsRefreshBindingTestCore;
			if (!result.ContainsKey(ITEDIInterchange.Schema.EI_ApplicationCode))
			{
				result.Add(ITEDIInterchange.Schema.EI_ApplicationCode, new ZString("ITM"));
			}
			return result;
		}
	}

	protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest() => Factory.New<ITEDIInterchange>();

	protected override void SetUp()
	{
		base.SetUp();
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		var accountCollection = new AccountCollectionTestBuilder(company.PK.ToGuid())
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build();

		account = accountCollection[0];
	}

	Account account;
}
