using CargoWise.Customs.FR.MessageDefinitions.TP5.CC917C;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class CC917CMessagePrettierTest : NCTSMessagePrettierTest<Cc917CType, CC917CMessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p><strong>Errors: </strong><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr align=""left""><td colspan=""6"">Status: Technical Rejection</td></tr><tr align=""left""><td colspan=""6"">LRN: LRN8</td></tr><tr align=""left""><td colspan=""6"">CRN: 56386316</td></tr><tr align=""left""><td colspan=""6"">MRN: N/A</td></tr><tr align=""center""><td width=""100px"">Error Code</td><td width=""100px"">Error Code description</td><td width=""75px"">Field Code</td><td width=""100px"">Error Reason</td><td width=""100px"">Original Value</td><td width=""100px"">Remarks</td></tr><tr><td>51</td><td>Description</td><td>Pointer 1</td><td><br>          Error Text 1<br>        </td><td>NEW COD TRANSIT</td><td>An error reported by Convertor in the Technical Message Structure (EDIFACT level) validation on the downgraded message (ECS-P2/NCTS-P4 message) [Downgrade Conversion from NCTS-P5/AES to NCTS-P4/ECS-P2].</td></tr><tr><td>13</td><td>Error</td><td>Pointer 2</td><td><br>          Error Text 2<br>        </td><td>value 2</td><td>A mandatory/required element is missing in the received data following the validation of Rule or Condition , e.g. a required element is not present.</td></tr></table></p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC917CResponseMessage.xml");

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL180, "13", "Non respect de la liste de codes.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Remark, "A mandatory/required element is missing in the received data following the validation of Rule or Condition , e.g. a required element is not present.");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL180, "51", "Non respect de la liste de codes.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Remark, "An error reported by Convertor in the Technical Message Structure (EDIFACT level) validation on the downgraded message (ECS-P2/NCTS-P4 message) [Downgrade Conversion from NCTS-P5/AES to NCTS-P4/ECS-P2].");
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL030, "Code Description");
			helper.CreateNewOrGetExistingCusCodeList("FR", EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL030, "51", "Description", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList("FR", EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL030, "13", "Error", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();
		}
	}
}
