using CargoWise.Customs.FR.MessageDefinitions.TP5.CC057C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class CC057CMessagePrettierTest : NCTSMessagePrettierTest<Cc057CType, CC057CMessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Rejection From Office of Destination<br><strong>MRN: </strong>MRN1<br><strong>Business Rejection Type: </strong>007 - Rejet de la notification d'arrivée<br><strong>Rejection Date and Time: </strong>7/09/2024 12:20:30 PM<br><strong>Rejection Code: </strong>3 - Pas de détournement : itinéraire contraignant et aucun incident notifié<br><strong>Rejection Reason: </strong>rejectionReason1</p><p><strong>Functional Errors: </strong><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr align=""center""><td width=""75px"">Level</td><td width=""100px"">Error Code</td><td width=""100px"">Field Code</td><td width=""150px"">Error Reason</td><td width=""125px"">Original Value</td><td width=""150px"">Remarks</td></tr><tr><td>&nbsp;</td><td>12</td><td>errorPointer1</td><td>errorR1</td><td>originalAttributeValue1</td><td>Remark12</td></tr><tr><td>&nbsp;</td><td>13</td><td>errorPointer2</td><td>errorR2</td><td>originalAttributeValue2</td><td>A mandatory/required element is missing in the received data following the validation of Rule or Condition , e.g. a required element is not present.</td></tr><tr><td>&nbsp;</td><td>14</td><td>errorPointer3</td><td>errorR3</td><td>originalAttributeValue3</td><td>&nbsp;</td></tr></table></p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC057CResponseMessage.xml");

		protected override void SetUp()
		{
			SetUpRefDB(Factory);
		}

		internal static void SetUpRefDB(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL180, "12", "Non respect de la liste de codes.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Remark, "Remark12");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL180, "13", "Non respect des conditions (manquant)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Remark, "A mandatory/required element is missing in the received data following the validation of Rule or Condition , e.g. a required element is not present.");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL180, "14", "Non respect de la liste de codes.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Remark, "");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL227, "3", "Pas de détournement : itinéraire contraignant et aucun incident notifié", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL570, "007", "Rejet de la notification d'arrivée", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();
		}
	}

	sealed class CC057CMessagePrettier_EmptyOriginalAttributeValueTest : NCTSMessagePrettierTest<Cc057CType, CC057CMessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Rejection From Office of Destination<br><strong>MRN: </strong>MRN1<br><strong>Business Rejection Type: </strong>007 - Rejet de la notification d'arrivée<br><strong>Rejection Date and Time: </strong>7/09/2024 12:20:30 PM<br><strong>Rejection Code: </strong>3 - Pas de détournement : itinéraire contraignant et aucun incident notifié<br><strong>Rejection Reason: </strong>rejectionReason1</p><p><strong>Functional Errors: </strong><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr align=""center""><td width=""75px"">Level</td><td width=""100px"">Error Code</td><td width=""100px"">Field Code</td><td width=""150px"">Error Reason</td><td width=""125px"">Original Value</td><td width=""150px"">Remarks</td></tr><tr><td>&nbsp;</td><td>12</td><td>errorPointer1</td><td>errorR1</td><td>&nbsp;</td><td>Remark12</td></tr></table></p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC057CResponseMessage_EmptyOriginalAttributeValue.xml");

		protected override void SetUp()
		{
			CC057CMessagePrettierTest.SetUpRefDB(Factory);
		}
	}
}
