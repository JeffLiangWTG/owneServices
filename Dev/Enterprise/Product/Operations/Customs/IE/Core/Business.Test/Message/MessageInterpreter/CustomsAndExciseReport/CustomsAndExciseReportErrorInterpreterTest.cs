using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing;

[TestedType(typeof(CustomsAndExciseReportErrorInterpreter))]
sealed class CustomsAndExciseReportErrorInterpreterTest : CustomsAndExciseReportInboundMessageInterpreterTest<CustomsAndExciseReportErrorInterpreter, CustomsAndExciseReportErrorProvider>
{
	protected override ZString MessageType => CustomsAndExciseReportTypeList.Codes.PSR;

	protected override CustomsAndExciseReportErrorProvider GetProvider(TextReader reader) => new CustomsAndExciseReportErrorProvider(CustomsAndExciseReportInterchangeProcessorTestHelper.CreateErrorMessage());

	protected override ZString ExpectedInterpretation => GetExpectedInterpretation();

	protected override void SetUp()
	{
		base.SetUp();
		CreateRefDataForInterpreter(Factory);
	}

	internal static void CreateRefDataForInterpreter(BusinessObjectFactory factory)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingCusCodeType("IEROS", "Ireland Revenue Online Service Error Code List", Core.Constants.CountryCodes.Ireland);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "IEROS", "111008", "Internal system error has occurred when processing the WS request", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();
	}

	internal static ZString GetExpectedInterpretation() => @"ROS Error<br />
		<br />
		<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
			<tr><td>Error Code</td><td>111008</td></tr>
			<tr><td>Error Code Description</td><td>Internal system error has occurred when processing the WS request</td></tr>
		</table>";
}
