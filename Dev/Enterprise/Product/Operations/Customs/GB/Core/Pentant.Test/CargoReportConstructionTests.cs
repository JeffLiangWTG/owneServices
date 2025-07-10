using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;
using static Enterprise.Customs.GB.Pentant.CargoReportMessageCreator;

namespace Enterprise.Customs.GB.Pentant.Testing
{
	class CargoReportConstructionTests : TestCaseWithFactory
	{
		[TestDate(2015, 8, 22, 14, 0, 0)]
		public void TestMakeCargoReportForImportCDS()
		{
			var expectedMessage = "BEGINMESSAGE~<<MSGNO PLACEHOLDER>>~SSSS~RRRRR~SHORTSEA~C~I~22/08/2015~14:00~DOV~FSA~FEY12345M~REG123~Trailer123~6~38~Blue, YellowYellowYellowYellowYello~PKGS~N~Newspapers~VEH~DOV~~US~7GB123-B0001~ENDMESSAGE";
			var dec = RunAndAssert("AAA", ApplicationCodeList.Codes.GbCustomsDeclarationServices, expectedMessage, out var creator);

			dec.JE_JS = ZGuid.Empty;
			dec.JE_RL_NKOrigin = "RSATL";
			var transPort = dec.TransportsIncludingRelated.AddNew();
			transPort.JW_RL_NKLoadPort = "AUSYD";
			transPort.JW_RL_NKDiscPort = "GBDVR";
			transPort.JW_VesselForBinding = "BO625GB";

			Assert(creator.CreateCargoReportMessage());
			var entry = dec.CustomsEntryHeaders[0];
			entry.Messages.Load();
			var message = entry.Messages[1];
			AssertEquals("BEGINMESSAGE~<<MSGNO PLACEHOLDER>>~SSSS~RRRRR~SHORTSEA~C~I~22/08/2015~14:00~DOV~FSA~FEY12345M~REG123~BO625GB~6~38~Blue, YellowYellowYellowYellowYello~PKGS~N~Newspapers~VEH~DOV~~RS~7GB123-B0001~ENDMESSAGE", message.EM_MessageText);
		}

		JobDeclaration RunAndAssert(ZString badge, ZString expectedApplicationCode, string expectedMessage, out CargoReportMessageCreator creator, string messagingSystem = "CDS", Action setupExtraBadgeAction = null)
		{
			setupExtraBadgeAction?.Invoke();
			var declaration = SetupImportDeclaration(Factory, badge, messagingSystem);
			creator = new CDSCargoReportMessageCreator(declaration, CargoReportType.CREATE);
			Assert(creator.CreateCargoReportMessage());
			var entry = declaration.CustomsEntryHeaders[0];
			entry.Messages.Load();
			var message = entry.Messages[0];
			AssertEquals(expectedApplicationCode, message.EM_ApplicationCode);
			AssertEquals(PentantConstants.CargoMessageType, message.EM_MessageType);
			AssertEquals(declaration.JE_CustomsProfile, message.EM_MessageOwner);
			AssertEquals("SSSS", message.EM_ApplicationReference);
			AssertEquals(expectedMessage, message.EM_MessageText);
			return declaration;
		}

		internal static JobDeclaration SetupImportDeclaration(BusinessObjectFactory factory, string profile, string messagingSystem = "CDS")
		{
			GBCustomsDataRegistry.Instance.AllowLocationOfGoodsCalculationForNotArrivedGoods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			SetUpPentantCredentials();
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = messagingSystem;
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = "ROA";
			declaration.JE_MasterBill = "  anything  ";
			declaration.JE_ACAReference = "FEY12345M";
			declaration.JE_UCR = "7GB123-B0001";
			declaration.JE_LocationOfGoods = "DOV  ";
			declaration.SubLocation = "FSA";
			declaration.JE_VesselName = "MT10VHB";
			declaration.ZG_Box18TransportID = "REG123";
			var consol = factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			consol.JK_RL_NKDischargePort = "GBMIK";
			shipment.JS_RL_NKDestination = "GBMIK";
			declaration.JE_RL_NKOrigin = "USATL";
			declaration.JE_RL_NKFinalDestination = "GBMIK";
			declaration.JE_JS = shipment.PK;
			consol.MostInterestingTransportForBinding[0].JW_VesselForBinding = "Trailer123";
			declaration.JE_TotalNoOfPacks = 6;

			if (messagingSystem == ApplicationCodeList.Codes.GbCustomsDeclarationServices)
			{
				declaration.CusEntryInstruction.CEI_PackageCount = declaration.JE_TotalNoOfPacks;
			}

			declaration.JE_TotalWeight = 38;
			var pack1 = declaration.Bills[0].PackingGroups[0].Packages.AddNew();
			var pack2 = declaration.Bills[0].PackingGroups[0].Packages.AddNew();
			var pack3 = declaration.Bills[0].PackingGroups[0].Packages.AddNew();
			var pack4 = declaration.Bills[0].PackingGroups[0].Packages.AddNew();
			pack1.CW_PackQty = 1;
			pack1.CW_PackType = "BG";
			pack1.CW_MarksAndNos = "";
			pack2.CW_PackQty = 5;
			pack2.CW_PackType = "BL";
			pack2.CW_MarksAndNos = "Blue";
			pack3.CW_PackQty = 5;
			pack3.CW_PackType = "BL";
			pack3.CW_MarksAndNos = "Blue ";
			pack4.CW_PackQty = 5;
			pack4.CW_PackType = "BL";
			pack4.CW_MarksAndNos = "YellowYellowYellowYellowYellowYellowYellow";
			declaration.JE_GoodsDescription = "Newspapers";
			declaration.JE_CustomsProfile = profile;
			var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			invLine.JI_CL = entry.MergedLines.AddNew().PK;
			entry.AllEntryLines.Load();
			declaration.InvoiceLines[0].PackagesForInvoiceLinesForBindingOnly[1].IsLinked = true;
			declaration.InvoiceLines[0].PackagesForInvoiceLinesForBindingOnly[2].IsLinked = true;
			declaration.InvoiceLines[0].PackagesForInvoiceLinesForBindingOnly[3].IsLinked = true;
			declaration.InvoiceLines[0].PackagesForInvoiceLinesForBindingOnly[4].IsLinked = true;
			return declaration;
		}

		const string ftpUserName = "testuser";
		const string ftpUserPassword = "testpassword";

		static void SetUpPentantCredentials()
		{
			var badges = GBCustomsDataRegistry.Instance.BadgeCodes.Value;

			var badge2 = badges.AddNew();
			badge2.BadgeCode = "AAA";
			badge2.CSPCode = "PNT";
			badge2.ApplicationCode = "CDS";

			var creds = GBCustomsDataRegistry.Instance.Credentials.Value;

			var cred2 = creds.AddNew();
			cred2.BadgeCode = badge2.BadgeCode;
			cred2.Username = ftpUserName;
			cred2.Password = ftpUserPassword;
			cred2.Printer = "wisetechFolderNameCDS";
			cred2.SenderID = "SSSS";
			cred2.ReceiverID = "RRRRR";
			cred2.Company = "WTG";
			cred2.Endpoint = EndpointList.Codes.INV;

			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, creds);
		}
	}
}
