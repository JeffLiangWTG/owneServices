using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing;

public class JobDeclarationDocumentSupportEUOnlyTest : TestCaseWithFactory
{
	public void TestSADH()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var docSupporter = declaration.DocumentSupporter;
		AssertEquals(true, docSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.SADH)));
		AssertEquals(0, docSupporter.GetDocumentWrappers(Core.Constants.DataContext.SADH, null).Length);
		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		AssertEquals(1, docSupporter.GetDocumentWrappers(Core.Constants.DataContext.SADH, null).Length);
		var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
		var result = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.SADH, null);
		AssertEquals(2, result.Length);
		AssertEquals("DocSADH", result[0].GetType().Name);
		AssertEquals(entryHeader1, result[0].WrappedObject);
		AssertEquals("DocSADH", result[1].GetType().Name);
		AssertEquals(entryHeader2, result[1].WrappedObject);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		result = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.SADH, null);
		AssertEquals(2, result.Length);
		AssertEquals("DocSADH", result[0].GetType().Name);
		AssertEquals(entryHeader1, result[0].WrappedObject);
		AssertEquals("DocSADH", result[1].GetType().Name);
		AssertEquals(entryHeader2, result[1].WrappedObject);
	}

	public void TestGetFilterValue()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var supporter = new JobDeclarationDocumentSupporter(declaration);

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
}
