using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Business.NCTS;
using NctsTransitStatusList = Enterprise.Customs.FR.Business.NctsTransitStatusList;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing
{
	class MessageBuilderTests : TestCaseWithFactory
	{
		public void TestCC014AEnvelope()
		{
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			Factory.Save();
			MessageBuilderUtilities.AssertFullEnvelopeXml(new NctsMessageFunctionSet.DeclarationCancellationRequestMessage("reason", "commentOnTheCancelling"), nctsHeader, "IE014", "PRN-REP", "CC014A");
		}

		public void TestCC007AEnvelope()
		{
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival);
			Factory.Save();
			MessageBuilderUtilities.AssertFullEnvelopeXml(new NctsMessageFunctionSet.ArrivalNotificationMessage(), nctsHeader, "IE007", "DEC-REP", "CC007A");
		}

		public void TestCC141AEnvelope()
		{
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			Factory.Save();
			MessageBuilderUtilities.AssertFullEnvelopeXml(new NctsMessageFunctionSet.InformationAboutNonArrivedMovementMessage(), nctsHeader, "IE141", "PRN-REP", "CC141A");
		}

		public void TestCC013BEnvelope()
		{
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			Factory.Save();

			MessageBuilderUtilities.AssertFullEnvelopeXml(new NctsMessageFunctionSet.DeclarationAmendmentMessage("", ""), nctsHeader, "IE013", "PRN-REP", "CC013B");
		}

		public void TestCC015BEnvelope()
		{
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			Factory.Save();
			MessageBuilderUtilities.AssertFullEnvelopeXml(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, "IE015", "PRN-REP", "CC015B");
			nctsHeader.IsPrelodgedMovement = true;
			MessageBuilderUtilities.AssertFullEnvelopeXml(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, "IE015", "PRN-REP", "CC015B");
			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationRejected;
			MessageBuilderUtilities.AssertFullEnvelopeXml(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, "IE015", "PRN-REP", "CC015B");
			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid;
			MessageBuilderUtilities.AssertFullEnvelopeXml(new NctsMessageFunctionSet.DeclarationDataMessage(), nctsHeader, "IE015", "PRN-REP", "CC015B");
		}

		public void TestCCF15AEnvelope()
		{
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			Factory.Save();
			MessageBuilderUtilities.AssertFullEnvelopeXml(new FRNctsMessageFunctionSet.PrelodgeValidationMessage(), nctsHeader, "IEF15", "PRN-REP", "CCF15A");
		}

		public void TestCC044AEnvelope()
		{
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.UnloadingPermissionGranted;
			Factory.Save();

			MessageBuilderUtilities.AssertFullEnvelopeXml(new NctsMessageFunctionSet.UnloadingRemarksMessage(), nctsHeader, "IE044", "DEC-REP", "CC044A");
		}

		public void TestNoAccentsInMessage()
		{
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			Factory.Save();

			MessageBuilderUtilities.AssertFullEnvelopeXml(new NctsMessageFunctionSet.DeclarationCancellationRequestMessage("le transit a été validé alors qu il n aurait pas dû l être", "commentOnTheCancelling"), nctsHeader, "IE014", "PRN-REP", "CC014A");
		}

		protected override void SetUp()
		{
			base.SetUp();
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			universalHelper.CreateNctsDeclarationTypeList(Core.Constants.CountryCodes.France);
			Factory.Save();
			nctsHeader = Factory.New<NctsHeader>();
		}
		NctsHeader nctsHeader;
	}
}
