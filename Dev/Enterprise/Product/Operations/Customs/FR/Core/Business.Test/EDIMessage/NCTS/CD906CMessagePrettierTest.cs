using CargoWise.Customs.FR.MessageDefinitions.TP5.CD906C;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class CD906CMessagePrettierTest : NCTSMessagePrettierTest<Cd906CType, CD906CMessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Rejected at Origin<br><strong>CRN: </strong>Token1<br><strong>MRN: </strong>MRN1<br><strong>Rejection Type: </strong>Functional Rejection</p><p><strong>Functional Errors: </strong><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr align=""center""><td width=""75px"">Level</td><td width=""100px"">Error Code</td><td width=""150px"">Error Code description</td><td width=""100px"">Field Code</td><td width=""150px"">Error Reason</td><td width=""125px"">Original Value</td><td width=""150px"">Remarks</td></tr><tr><td>&nbsp;</td><td>12</td><td>Non respect de la liste de codes.</td><td>errorPointer</td><td>errorReason</td><td>originalAttributeValue</td><td>Remark12</td></tr></table></p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CD906CResponseMessage.xml");

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL180, "12", "Non respect de la liste de codes.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Remark, "Remark12");
			Factory.Save();
		}
	}
}
