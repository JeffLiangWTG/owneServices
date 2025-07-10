using System;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS460;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.FR.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class IETS460ProcessorTest : PNTSBaseProcessorTest<Iets460, IETS460Processor>
	{
		protected override void AddPropertiesForTemporaryStorageHeader(TemporaryStorageHeader header)
		{
			header.MRN = "21BEPT00000000QFU6";
		}

		protected override CusEntryNumber GetCusEntryNumber(TemporaryStorageHeader header) => CusEntryNumber.Load(header, "MRN", Core.Constants.CountryCodes.France);

		protected override ZString GetExpectedErrorTextIfEntryNumberNotFound() => new ZString("Couldn't locate Job using provided MRN# or CRN#");

		protected override void PrepareTestData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TransportDocumentTemporaryStorage, "TD44T");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TransportDocumentTemporaryStorage, "C624", "C624 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			base.PrepareTestData();
		}

		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Intended Control<br><strong>CRN: </strong><br><strong>MRN: </strong>21BEPT00000000QFU6<br><strong>Notification Date: </strong>15/05/2021 12:34:56 PM<br><strong>Scheduled Control Date: </strong>16/05/2021 12:34:56 PM<br><strong>Customs Office Of Control: </strong>BE212000</p><p><strong>Control Type: </strong><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""70%"" class=""table""><tr align=""center""><td width=""50%"">Type</td><td width=""50%"">Description</td></tr><tr align=""center""><td>0 (Total release)</td><td>Libération totale : Tous les éléments du message sont à libérer</td></tr></table></p><strong>Control Data - Master</strong><ul><li><strong>Reference Number: </strong>REF123456789<ul><li><strong>Type: </strong>C624 Desc</li></ul></li></ul>");

		protected override ZString GetExpectedCustomsStatus() => Enterprise.Customs.EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.IntendedControl;

		protected override ZDateTime GetExpectedCustomsStatusDate() => new ZDateTime(2021, 5, 15, 12, 34, 56);

		protected override ZString GetExpectedNewMessageStatus() => PNTSMessageStatusList.Codes.Acknowledged;

		protected override ZString GetExpectedCRN() => ZString.Empty;

		protected override ZString GetExpectedMRN() => "21BEPT00000000QFU6";

		protected override ZString GetExpectedFRN() => ZString.Empty;

		protected override ZString GetMessageSubType() => "460";

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.PNTS_IETS460ResponseMessage.xml");

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
