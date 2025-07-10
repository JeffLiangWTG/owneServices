using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.CH.DataTransfer.Testing;

partial class UniversalCustomsDataObjectProviderTest : TestCaseWithFactoryAndMessagingHelpers
{
	UniversalCustomsDataObjectProvider universalCustomsDataObjectProvider;

	protected override void SetUp()
	{
		base.SetUp();

		universalCustomsDataObjectProvider = new UniversalCustomsDataObjectProvider();
	}

	public void TestUniversalCustomsDataObjectProviderDefaults()
	{
		AssertNull(universalCustomsDataObjectProvider.TableSpecificCusAddInfoTypeList(ZString.Empty, string.Empty));
		AssertNull(universalCustomsDataObjectProvider.TableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList(ZString.Empty, string.Empty));
		AssertNull(universalCustomsDataObjectProvider.TableSpecificCusCodeDataTypeList(ZString.Empty, string.Empty));
		AssertNull(universalCustomsDataObjectProvider.TableSpecificCusCodeDataCodeList(ZString.Empty, string.Empty));
		AssertNull(universalCustomsDataObjectProvider.TableSpecificCusReferenceTypeList(ZString.Empty, string.Empty));
		AssertType<UniversalDataObjectReaderHelper>(universalCustomsDataObjectProvider.GetNewUniversalDataObjectReaderHelper(new UniversalObjectFactory(new CargoWise.EntityFramework.BusinessObjectFactory()), Core.Constants.CountryCodes.Switzerland, string.Empty));
		var airManifestDataObjectReaders = universalCustomsDataObjectProvider.GetNewAirManifestDataObjectReaders(null, null, null, null, false);
		AssertNotNull(airManifestDataObjectReaders);
		AssertEquals(0, airManifestDataObjectReaders.Count());
		AssertNull(universalCustomsDataObjectProvider.GetNewAirManifestDataObjectWriter(null));
		AssertNull(universalCustomsDataObjectProvider.GetNewAirManifestLineDataObjectWriter(null, null));
		AssertNull(universalCustomsDataObjectProvider.GetNewCusSCAOceanBillDataObjectReaders(null, null, null, null));
		AssertNull(universalCustomsDataObjectProvider.GetNewCusSCAOceanBillDataObjectWriter(null));
		AssertNull(universalCustomsDataObjectProvider.GetNewStandaloneCommercialInvoiceDataObjectReader(null, null, null, null));
		AssertNull(universalCustomsDataObjectProvider.GetNewStandaloneCommercialInvoiceDataObjectWriter(null));
	}

	public void TestGetNewDeclarationDataObjectWriterShouldReturnDeclarationDataObjectWriter()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		var dataWritingManager = new DataWritingManager(new ActionInfo(RecipientRoleType.AAD, jobDeclaration));

		var declarationDataObjectWriter = universalCustomsDataObjectProvider.GetNewDeclarationDataObjectWriter(dataWritingManager);

		AssertNotNull(declarationDataObjectWriter);
		AssertType<DeclarationDataObjectWriter>(declarationDataObjectWriter);
	}

	public void TestGetNewJobDeclarationDataObjectReader()
	{
		var provider = new UniversalCustomsDataObjectProvider();
		var reader = provider.GetNewJobDeclarationDataObjectReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), new TestErrorLogger(), Factory, null);
		AssertType<DeclarationDataObjectReader>(reader);
	}
}
