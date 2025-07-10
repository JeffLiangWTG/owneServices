using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class CusEntryHeaderDocumentSupporterTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestCusEntryHeaderGetsTheRightDocumentSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertType<CusEntryHeader>(entryHeader);
			AssertType<CusEntryHeaderDocumentSupporter>(entryHeader.DocumentSupporter);

			var docSupporter = (CusEntryHeaderDocumentSupporter)entryHeader.DocumentSupporter;
			AssertEquals(true, docSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.SADH)));
			AssertEquals(1, docSupporter.GetDocumentWrappers(Core.Constants.DataContext.SADH, null).Length);
		}

		public void TestGetFilterValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var supporter = new CusEntryHeaderDocumentSupporterForTest(entryHeader);

			CombineAssertions("When using IDD Document", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, nameof(DeclarationConfiguration.UseIDDDocument) + "Core", true))
				{
					AssertEquals("Empty for DocumentFilters.CTYEGSADH", ZString.Empty, supporter.GetFilterValue(DocumentFilters.CTYEGSADH));
					AssertEquals("EUN for DocumentFilters.CTYEGIDD", EconomicGroupList.Codes.EuropeanUnion, supporter.GetFilterValue(DocumentFilters.CTYEGIDD));
					AssertEquals("EUN for DocumentFilters.CTYEG", EconomicGroupList.Codes.EuropeanUnion, supporter.GetFilterValue(DocumentFilters.CTYEG));
					AssertEquals("No exception for DocumentFilters.CTY", Core.Constants.CountryCodes.Latvia, supporter.GetFilterValue(DocumentFilters.CTY));
				}
			});

			CombineAssertions("When not using IDD Document", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, nameof(DeclarationConfiguration.UseIDDDocument) + "Core", false))
				{
					AssertEquals("EUN for DocumentFilters.CTYEGSADH", EconomicGroupList.Codes.EuropeanUnion, supporter.GetFilterValue(DocumentFilters.CTYEGSADH));
					AssertEquals("Empty for DocumentFilters.CTYEGIDD", ZString.Empty, supporter.GetFilterValue(DocumentFilters.CTYEGIDD));
					AssertEquals("EUN for DocumentFilters.CTYEG", EconomicGroupList.Codes.EuropeanUnion, supporter.GetFilterValue(DocumentFilters.CTYEG));
					AssertEquals("No exception for DocumentFilters.CTY", Core.Constants.CountryCodes.Latvia, supporter.GetFilterValue(DocumentFilters.CTY));
				}
			});
		}

		public void TestGetDocumentTitlesForPivot()
		{
			var template = Factory.New<StmTemplate>();
			template.SO_Name = "Blah Blah";
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "SADH C88";

			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menuItem.PK;

			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "Reference";
			var docSupporter = (CusEntryHeaderDocumentSupporter)entryHeader.DocumentSupporter;

			CombineAssertions(() =>
			{
				var result = docSupporter.GetDocumentTitlesForPivot(menuItem.SU_MenuName, entryHeader, pivot);
				AssertEquals("When parentDocumentName is the expected one", "SADH C88 - Reference", result.Title);

				result = docSupporter.GetDocumentTitlesForPivot(menuItem.SU_MenuName, declaration, pivot);
				AssertNull("When parentBusinessObject is not entryHeader", result);

				entryHeader.MovementReferenceNumberSetter("MRNCode");
				result = docSupporter.GetDocumentTitlesForPivot(menuItem.SU_MenuName, entryHeader, pivot);
				AssertEquals("When parentBusinessObject is entryHeader and it has mrn", "SADH C88 - MRNCode", result.Title);
			});
		}

		public void TestGetDocumentTitlesForPivot_Shipment()
		{
			var template = Factory.New<StmTemplate>();
			template.SO_Name = "Blah Blah";
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "SADH C88";

			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menuItem.PK;

			var declaration = Factory.New<JobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "Reference";
			var docSupporter = shipment.DocumentSupporter;

			CombineAssertions(() =>
			{
				var result = docSupporter.GetDocumentTitlesForPivot(menuItem.SU_MenuName, entryHeader, pivot);
				AssertEquals("When parentDocumentName is the expected one", "SADH C88 - Reference", result.Title);

				result = docSupporter.GetDocumentTitlesForPivot(menuItem.SU_MenuName, declaration, pivot);
				AssertNull("When parentBusinessObject is not entryHeader", result);

				entryHeader.MovementReferenceNumberSetter("MRNCode");
				result = docSupporter.GetDocumentTitlesForPivot(menuItem.SU_MenuName, entryHeader, pivot);
				AssertEquals("When parentBusinessObject is entryHeader and it has mrn", "SADH C88 - MRNCode", result.Title);
			});
		}

		#region Implementation

		class CusEntryHeaderDocumentSupporterForTest : CusEntryHeaderDocumentSupporter
		{
			public CusEntryHeaderDocumentSupporterForTest(CusEntryHeader entryHeader)
				: base(entryHeader)
			{
			}
		}

		#endregion
	}
}
