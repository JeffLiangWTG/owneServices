using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.Customs.FR.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaD.Testing
{
	class DDD2MSendImpMessageBuilderTest : DDSendMessageBuilderTest<DDD2MSendImpMessageBuilder>
	{
		protected override ZBool IsImport => true;

		protected override ZString MessageType => EntryActionCodeList.Codes.D2M;

		protected override ZString[] ItemsNotContains => new ZString[] { "<Gen>", "<Articles>", "<MetaData>", "<Motivation>", "<OperateurComp>", "<conteneurtra>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>2</codact>", "<GenComp>", "<ArticlesComp><ArticleComp>", "<dispopart>INC</dispopart>", "<dispopart>IsC</dispopart>", "<Document><doc>0001</doc><indd48>1</indd48><mntd48>1</mntd48><deld48>10</deld48></Document>", "<codtax>V905</codtax><typtax>0</typtax><quotax>1</quotax><asstax>100</asstax><montanttax>100</montanttax>" };

		protected override void FillSomeFieldsForTheEntry(CusEntryHeader entry)
		{
			var charge = entry.Charges.AddNew();
			charge.C1_ChargeType = "V905";
			charge.C1_ChargeAmount = 100;
			charge.C1_MethodOfPayment = "XXX";
			charge.C1_RateOverrideReasonCode = RateOverrideReasonList.Codes.Precalcule;
		}

		public void TestElementsValeurGenIsNotPresentWhenValuationByPassCodeIsNotEmpty()
		{
			CreateDeclarationMock(MessageType, IsImport);
			myDeclGen.Setup(m => m.ValuationBypassCode).Returns(ZString.Empty);
			declarationMock.Setup(m => m.CusProcedure).Returns(myDeclGen.Object);
			var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			var message = messageBuilder.GetMessage();
			AssertContains("<ElementsValeurGen>", message);

			myDeclGen.Setup(m => m.ValuationBypassCode).Returns(ValuationBypassCodeList.Codes.VBC_A);
			messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			message = messageBuilder.GetMessage();
			AssertNotContains("<ElementsValeurGen>", message);
		}

		public void TestFraisArticleAjoutAndFraisArticleDeduit_PresentWhenValuationByPassCodeAndTariffByPassCodeAreEmpty()
		{
			CreateDeclarationMock(MessageType, IsImport);
			myDeclGenArticle1.Setup(m => m.Resale).Returns(AmountAndCurrencyMock(new ZDecimal("25.00"), "USD"));
			myDeclGenArticle1.Setup(m => m.AssemblyCosts).Returns(AmountAndCurrencyMock(new ZDecimal("25.00"), "USD"));
			myDeclGen.Setup(m => m.ValuationBypassCode).Returns(ZString.Empty);
			myDeclGen.Setup(m => m.VATOrganization).Returns("VATOrganization");
			declarationMock.Setup(m => m.CusProcedure).Returns(myDeclGen.Object);
			var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			var message = messageBuilder.GetMessage();
			AssertContains("ValuationByPassCode and TariffByPassCode are Empty", "<FraisArticleAjout>", message);
			AssertContains("ValuationByPassCode and TariffByPassCode are Empty", "<FraisArticleDeduit>", message);

			myDeclGen.Setup(m => m.ValuationBypassCode).Returns(ValuationBypassCodeList.Codes.VBC_A);
			declarationMock.Setup(m => m.CusProcedure).Returns(myDeclGen.Object);
			messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			message = messageBuilder.GetMessage();
			AssertNotContains("ValuationByPassCode is NOT empty but TariffByPassCode is Empty", "<FraisArticleAjout>", message);
			AssertNotContains("ValuationByPassCode is NOT empty but TariffByPassCode is Empty", "<FraisArticleDeduit>", message);

			var myImpAlternateCalcValue = new Mock<IAlternateCalcValue> { CallBase = true };
			myImpAlternateCalcValue.Setup(m => m.CalcValue).Returns("E");
			myImpAlternateCalcValue.Setup(m => m.Motivation).Returns(new ZString(null));
			myDeclGenArticle1.Setup(m => m.AlternateCalcValue).Returns(myImpAlternateCalcValue.Object);
			declarationMock.Setup(m => m.CusProcedure).Returns(myDeclGen.Object);
			messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			message = messageBuilder.GetMessage();
			AssertNotContains("ValuationByPassCode is NOT empty and TariffByPassCode is NOT Empty", "<FraisArticleAjout>", message);
			AssertNotContains("ValuationByPassCode is NOT empty and TariffByPassCode is NOT Empty", "<FraisArticleDeduit>", message);

			myDeclGen.Setup(m => m.ValuationBypassCode).Returns(ZString.Empty);
			declarationMock.Setup(m => m.CusProcedure).Returns(myDeclGen.Object);
			messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			message = messageBuilder.GetMessage();
			AssertNotContains("ValuationByPassCode is empty but TariffByPassCode is NOT Empty", "<FraisArticleAjout>", message);
			AssertNotContains("ValuationByPassCode is empty but TariffByPassCode is NOT Empty", "<FraisArticleDeduit>", message);
		}

		public void TestVatCustomsStatisticalValues()
		{
			var deltaHelper = new DeltaMessageBuilderHelper();
			var entry = deltaHelper.CreateEntryDeclarationForTest(false, true);
			entry.EntryInstruction.ZG_BypassCode = ZString.Empty;

			var sendingObject = new DeltaGJobDeclarationMessageSendingObject(entry);
			sendingObject.MessageType = MessageType;
			var errCollector = new EU.Business.ErrorCollector();
			var messageBuilderManager = new DeltaGMessageBuilderManager(errCollector);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			var message = messageBuilder.GetMessage();

			AssertNotContains("<valdou>", message);
			AssertNotContains("<valstat>", message);
			AssertNotContains("<asstva>", message);

			entry.EntryInstruction.ZG_BypassCode = ValuationBypassCodeList.Codes.VBC_A;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			message = messageBuilder.GetMessage();
			AssertContains("<valdou>50</valdou>", message);
			AssertContains("<valstat>50</valstat>", message);
			AssertContains("<asstva>50</asstva>", message);
		}

		public void TestPopulateDDSendMessageBuilderArticles_DepartmentIsNotWrittenOutForDROMCountries()
		{
			foreach (var countryCode in Core.Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				if (countryCode != Core.Constants.CountryCodes.France)
				{
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
					{
						CreateDeclarationMock(MessageType, IsImport);

						var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
						var message = messageBuilder.GetMessage();
						AssertContains("Parent node of <depliv> is successfully populated.", "<ArticlesComp>", message);
						AssertNotContains("<depliv> is not populated for DROMs", "<depliv>", message);
					}
				}
			}
		}

		public void TestPopulateDDSendMessageBuilderArticles_DepartmentIsWrittenOutForFrance()
		{
			AssertEquals("Pre-requisite, country code if current company is FR", Core.Constants.CountryCodes.France, GlbCompany.CurrentCompany.Country.Code);

			CreateDeclarationMock(MessageType, IsImport);

			var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			var message = messageBuilder.GetMessage();
			AssertContains("Parent node of <depliv> is successfully populated.", "<ArticlesComp>", message);
			AssertContains("<depliv> is populated for FR", "<depliv>", message);
		}

		public void TestNoNetWeightIn2ndMessage()
		{
			var deltaHelper = new DeltaMessageBuilderHelper();
			var entry = deltaHelper.CreateEntryDeclarationForTest(false, true);
			entry.EntryInstruction.ZG_BypassCode = ZString.Empty;
			var message = deltaHelper.GetMessage(MessageType, entry);
			AssertNotContains("<msn>", message);
		}
	}
}
