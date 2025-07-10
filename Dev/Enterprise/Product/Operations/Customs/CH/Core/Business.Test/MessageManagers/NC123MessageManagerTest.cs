using CargoWise.Types;
using Enterprise.Customs.Common.CH;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NC123MessageManager))]
sealed class NC123MessageManagerTest : BasePassarExportDeclarationMessageManagerTest
{
	protected override string DeclarationType => CHJobMessageTypeList.Codes.ExportDeclarationActivation;

	protected override string MessageType => PassarMessageTypeList.Codes.NC123;

	protected override string ExpectedMessageSubType => MessageSubTypeCodeList.Codes.NctsActivationAtDomicile;

	protected override string ExpectedEntryHeaderPhaseStatus => PassarDeclarationPhaseList.Codes.Activation;

	protected override Event ExpectedCustomsCommencedEvent => null;

	protected override Event ExpectedDeclarationSentEvent => Events.DeclarationActivationSent;

	protected override ZString ExpectedDeclarationSentEventReference => MessageType;

	protected override DeclarationMessageManager GetSpecificMessageManager(DeclarationMessageSendingObject sender) => new NC123MessageManager((ExportDeclarationMessageSendingObject)sender);

	public void TestDeclarationUpdated() => CombineAssertions(() =>
	{
		var sendingObject = (ExportDeclarationMessageSendingObject)Sender;
		var sendingObjectParent = sendingObject.SendingObjectParent;
		var declaration = sendingObjectParent.ParentDeclaration;

		sendingObject.MessageType = PassarMessageTypeList.Codes.NC123;
		sendingObjectParent.SendingDeclaration.JE_DeclarationLanguage = "DC";
		sendingObjectParent.SendingDeclaration.JE_LocationOfGoods = "LocOfGoods";
		sendingObjectParent.SendingDeclaration.JE_TransportMode = "OTH";
		sendingObjectParent.SendingDeclaration.JE_TransportMeans = "ME";
		sendingObjectParent.SendingDeclaration.JE_VesselName = "Vessel";
		sendingObjectParent.SendingDeclaration.JE_RN_NKTransportNationality = "TN";
		sendingObjectParent.SendingDeclaration.JE_MasterBill = "Master";
		sendingObjectParent.SendingDeclaration.JE_VoyageFlightNo = "Flight";
		sendingObject.NextProcedure = "1";

		Manager.GenerateMessages();

		AssertEquals($"{sendingObject.MessageType} JE_DeclarationLanguage", "DC", declaration.JE_DeclarationLanguage);
		AssertEquals($"{sendingObject.MessageType} JE_LocationOfGoods", "LocOfGoods", declaration.JE_LocationOfGoods);
		AssertEquals($"{sendingObject.MessageType} JE_TransportMode", "OTH", declaration.JE_TransportMode);
		AssertEquals($"{sendingObject.MessageType} JE_TransportMeans", "ME", declaration.JE_TransportMeans);
		AssertEquals($"{sendingObject.MessageType} JE_VesselName", "Vessel", declaration.JE_VesselName);
		AssertEquals($"{sendingObject.MessageType} JE_RN_NKTrailer1Nationality", "TN", declaration.JE_RN_NKTransportNationality);
		AssertEquals($"{sendingObject.MessageType} JE_MasterBill", "Master", declaration.JE_MasterBill);
		AssertEquals($"{sendingObject.MessageType} JE_VoyageFlightNo", "Flight", declaration.JE_VoyageFlightNo);
		AssertEquals($"{sendingObject.MessageType} CEI_NextProcedure", "1", declaration.CustomsEntryInstructions[0].CEI_NextProcedure);
	});
}
