using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Ccsuk.Declaration;
using Enterprise.Customs.GB.Chief;
using Enterprise.Customs.GB.Chief.CusDec.Testing;
using Enterprise.Customs.GB.Chief.NES.Testing;
using Enterprise.Customs.GB.Registry;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.Testing
{
	class CcsukDeclarationMessageSenderTests : NesDeclarationMessageSenderTests
	{
		protected override ChiefDeclarationMessageSender GetNewDeclarationMessageSender()
		{
			return new CcsukDeclarationMessageSender();
		}

		protected override string CspCode
		{
			get { return Enterprise.Customs.GB.Registry.GatewayList.Codes.CCSUKviaNTMsgGW; }
		}

		[ExpectNoExceptions]
		public void TestValidateDoesNotRaiseExceptionWhenAttachedToShipment()
		{
			var declaration = DeclarationChosererTester.CreateGemsDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportFullDeclaration;
			declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Air;
			declaration.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;
			declaration.JE_LocationOfGoods = "LHR";
			declaration.JE_GoodsDescription = "BLAH";

			var invoiceLine = declaration.Invoices[0].JobComInvoiceLines[0];
			invoiceLine.JI_Description = "BLAH";

			PreviousDocument previousDocument1 = invoiceLine.PreviousDocuments.AddNew();
			previousDocument1.CSI_Code = "703";
			previousDocument1.CSI_SubType = "Z";
			previousDocument1.CSI_ReferenceNumber = "122123";

			CreateAndStoreBadge("AAA");
			MakeCredential();
			declaration.JE_CustomsProfile = "AAA";

			var shipment = Factory.New<Freight.Forwarding.Business.ForwardingShipment>();
			shipment.JS_ActualChargeable = 99999999999.0m;

			declaration.JE_JS = shipment.PK;

			var shutUp = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			var sender = new CcsukDeclarationMessageSender();
			sender.Send(declaration, shutUp, new Customs.Business.CusdecMessageFunction.New());
		}
	}
}
