using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE451;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class IE451MessagePrettierTest : DeltaIEMessagePrettierTest<CC451BType, IE451MessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Decision Date: </strong>2021-05-01<br><strong>Decision Reason: </strong>Goods declared for release<br><strong>Declaration Status: </strong>STA<br><strong>Status Date: </strong>2023-04-19T23:28:57<br><strong>Previous Status: </strong>PRV<br><strong>Event: </strong>DSX</p><style> table, th, td {border: 1px solid black; border-collapse: collapse;} th, td { padding: 10px; text-align: left;}</style><table><tr><td bgcolor=""lightgray"" rowspan=""4"" colspan=""1""><strong><p style=""font-size: 120%"">Control Result</p></strong></td><td rowspan=""1"" colspan=""3"">Code</td><td rowspan=""1"" colspan=""1"">A1 - Satisfying</td></tr><tr><td rowspan=""1"" colspan=""3"">Date</td><td rowspan=""1"" colspan=""1"">2023-04-19T23:28:57</td></tr><tr><td rowspan=""1"" colspan=""3"">Remarks</td><td rowspan=""1"" colspan=""1"">control</td></tr><tr><td rowspan=""1"" colspan=""3"">Pending Sampling Results</td><td rowspan=""1"" colspan=""1"">No</td></tr><tr><td bgcolor=""lightgray"" rowspan=""14"" colspan=""1""><strong><p style=""font-size: 120%"">Control Results</p></strong></td><td rowspan=""1"" colspan=""3"">Sequence Number</td><td rowspan=""1"" colspan=""1"">1</td></tr><tr><td rowspan=""1"" colspan=""3"">Declaration Goods Item Number</td><td rowspan=""1"" colspan=""1"">00001</td></tr><tr><td rowspan=""1"" colspan=""3"">Control Result Code</td><td rowspan=""1"" colspan=""1"">A1 - Satisfying</td></tr><tr><td bgcolor=""lightgray"" rowspan=""11"" colspan=""1""><strong><p style=""font-size: 120%"">Results of Control</p></strong></td><td rowspan=""1"" colspan=""2"">Sequence Number</td><td rowspan=""1"" colspan=""1"">1</td></tr><tr><td rowspan=""1"" colspan=""2"">Risk Area Code</td><td rowspan=""1"" colspan=""1"">1000000 - Safety</td></tr><tr><td rowspan=""1"" colspan=""2"">Control Type</td><td rowspan=""1"" colspan=""1"">10 - Documentary checks</td></tr><tr><td rowspan=""1"" colspan=""2"">Control Date</td><td rowspan=""1"" colspan=""1"">2023-04-19T23:28:57</td></tr><tr><td rowspan=""1"" colspan=""2"">Remarks</td><td rowspan=""1"" colspan=""1"">Missing commercial invoice</td></tr><tr><td bgcolor=""lightgray"" rowspan=""6"" colspan=""1""><strong><p style=""font-size: 120%"">Control Details</p></strong></td><td rowspan=""1"" colspan=""2""></td></tr><tr><td rowspan=""1"" colspan=""1"">Sequence Number</td><td rowspan=""1"" colspan=""1"">1</td></tr><tr><td rowspan=""1"" colspan=""1"">Type Of discrepancies</td><td rowspan=""1"" colspan=""1"">D1 - Additional quantities</td></tr><tr><td rowspan=""1"" colspan=""1"">Attribute Pointer</td><td rowspan=""1"" colspan=""1"">GoodsShipment.InvoiceNumber</td></tr><tr><td rowspan=""1"" colspan=""1"">Corrected Value</td><td rowspan=""1"" colspan=""1"">123</td></tr><tr><td rowspan=""1"" colspan=""1"">Remarks</td><td rowspan=""1"" colspan=""1""></td></tr><table>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE451ResponseMessage.json");

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ControlResult, "CL047");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ControlResult, "A1", "Satisfying", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType, "CL716");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType, "10", "Documentary checks", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.RiskAreaCode, "CL740");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.RiskAreaCode, "1000000", "Safety", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfDiscrepancies, "CL790");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfDiscrepancies, "D1", "Additional quantities", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}
	}
}
