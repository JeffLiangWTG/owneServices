using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.MasterFiles.Business;
using CusExitControlHeader = Enterprise.Customs.EU.Business.CusExitControlHeader;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.ECS.Testing
{
	class ECSSendMessageIE507Test : TestCaseWithFactory
	{
		public void TestBuild()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var addressPK = SetupDeclarant().MainAddress.PK;
			declaration.JE_OA_DeclarantAddress = addressPK;
			var exitHeader = Factory.New<CusExitControlHeader>();
			exitHeader.CEH_ParentTableCode = "JE";
			exitHeader.CEH_ParentID = declaration.PK;
			exitHeader.CEH_ReferenceNumber = "123";
			exitHeader.CEH_OA_Agent = addressPK;
			var cusExitDetail = Factory.New<CusExitDetail>();
			cusExitDetail.CED_CEH = exitHeader.PK;
			cusExitDetail.CED_MovementReferenceNumber = "20FRD4440053891480";
			cusExitDetail.CED_CustomsOffice = "FR002650";
			cusExitDetail.CED_LocationOfGoods = "39433691100042/1";
			cusExitDetail.CED_Status = "XXX";

			Factory.Save();

			var errCollector = new EU.Business.ErrorCollector();
			var messageBuilderManager = new ECSMessageBuilderManager(cusExitDetail);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSubTypeList.Codes.ARR);
			var message = messageBuilder.GetMessage();

			AssertContains("<EnveloppeMessage><schemaID>MessageIE507</schemaID><schemaVersion>01012012</schemaVersion><partyId>39433691100042</partyId><transactionId>" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + "+ECS+&lt;&lt;INTERCHANGENUMBERPLACEHOLDER&gt;&gt;</transactionId><numseq>0</numseq></EnveloppeMessage>", message);
			AssertContains("<Gen><mrnecs>20FRD4440053891480</mrnecs><bureausortie>FR002650</bureausortie><opeben>40433691100042</opeben><numagrECS>00001909</numagrECS><locagr>39433691100042/1</locagr></Gen>", message);
			AssertContains("<Entete><codact>1</codact>", message);
			Assert("Message should not show any 'TMessage' tag anymore", !message.Contains("TMessage"));
		}

		OrgHeader SetupDeclarant()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var declarantAddress = orgHeader.MainAddress;
			orgHeader.OH_Code = OrgCusCode.FranceCodeTypes.Siret;

			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.FranceCodeTypes.Siret, "39433691100042", Core.Constants.CountryCodes.France);
			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "40433691100042", Core.Constants.CountryCodes.France);
			var collectionFrance = orgHeader.DeltaAgreementNumberCollection;
			var deltaAgreement = collectionFrance.AddNew();
			deltaAgreement.CZ_Code = OrgCusAccountCodeList.Codes.ECS;
			deltaAgreement.CZ_Account = "00001909";
			deltaAgreement.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;

			return orgHeader;
		}
	}
}
