using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Messaging.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaD.Testing
{
	class DDD2MSendExpMessageBuilderTest : DDSendMessageBuilderTest<DDD2MSendExpMessageBuilder>
	{
		protected override ZBool IsImport => false;

		protected override ZString MessageType => Business.EntryActionCodeList.Codes.D2M;

		protected override ZString[] ItemsNotContains => new ZString[] { "<Gen>", "<Articles>", "<MetaData>", "<Motivation>", "<OperateurComp>", "<conteneurtra>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>2</codact>", "<ArticlesComp><ArticleComp>", "<dispopart>INC</dispopart>", "<dispopart>IsC</dispopart>", "<Document><doc>0001</doc><indd48>1</indd48><mntd48>1</mntd48><deld48>10</deld48></Document>" };

		public override void TestAeroportembAndAertratag()
		{
			Assert("Export message doesn't populate Aeroportemb and Aertratag.", true);
		}

		public void TestVatCustomsStatisticalValues()
		{
			var deltaHelper = new DeltaMessageBuilderHelper();
			var entry = deltaHelper.CreateEntryDeclarationForTest(false, false);
			entry.EntryInstruction.ZG_BypassCode = ZString.Empty;

			var sendingObject = new DeltaGJobDeclarationMessageSendingObject(entry);
			sendingObject.MessageType = MessageType;
			var errCollector = new ErrorCollector();
			var messageBuilderManager = new DeltaGMessageBuilderManager(errCollector);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			var message = messageBuilder.GetMessage();

			AssertContains("<valdou>50</valdou>", message);
			AssertNotContains("<valstat>", message);
			AssertNotContains("<asstva>", message);
		}

		public void TestPopulateArticles_DepartmentIsNotWrittenOutForDROMCountries()
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
						AssertContains("Parent node of <depexp> is successfully populated.", "<ArticlesComp>", message);
						AssertNotContains("<depexp> is not populated for DROMs", "<depexp>", message);
					}
				}
			}
		}

		public void TestPopulateArticles_DepartmentNodeIsWrittenOutForFrance()
		{
			AssertEquals("Pre-requisite, country code if current company is FR", Core.Constants.CountryCodes.France, GlbCompany.CurrentCompany.Country.Code);

			CreateDeclarationMock(MessageType, IsImport);

			var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			var message = (messageBuilder).GetMessage();
			AssertContains("Parent node of <depexp> is successfully populated.", "<ArticlesComp>", message);
			AssertContains("<depexp> is populated for FR", "<depexp>", message);
		}

		public void TestNoNetWeightIn2ndMessage()
		{
			var deltaHelper = new DeltaMessageBuilderHelper();
			var entry = deltaHelper.CreateEntryDeclarationForTest(false, false);
			entry.EntryInstruction.ZG_BypassCode = ZString.Empty;
			var message = deltaHelper.GetMessage(MessageType, entry);
			AssertNotContains("<msn>", message);
		}
	}
}
