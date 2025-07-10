using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
namespace Enterprise.Customs.FR.DataTransfer.Universal.Testing;

sealed class CustomsSupportingInformationCollectionDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
{
	public void TestCreateNewCustomsSupportingInformationDataObjectReader()
	{
		AssertType<CustomsSupportingInformationDataObjectReader>(reader.CreateNewCustomsSupportingInformationDataObjectReader_Exposed(new CustomsSupportingInformation(), ZGuid.Empty, null, null));
	}

	protected override void SetUp()
	{
		base.SetUp();
		var helper = new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.France);
		jobDeclarationReader = new JobDeclarationDataObjectReader_ForTest(JobDeclarationDataObjectReaderTest.CreateShipment(Factory), new TestErrorLogger(), Factory);
		reader = new CustomsSupportingInformationCollectionDataObjectReader_ForTest(new TestErrorLogger(), helper, parentDeclarationReader: jobDeclarationReader);
	}
	CustomsSupportingInformationCollectionDataObjectReader_ForTest reader;
	JobDeclarationDataObjectReader_ForTest jobDeclarationReader;
}

class CustomsSupportingInformationCollectionDataObjectReader_ForTest : CustomsSupportingInformationCollectionDataObjectReader
{
	public CustomsSupportingInformationCollectionDataObjectReader_ForTest(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, string dataContext = "", JobDeclarationDataObjectReader parentDeclarationReader = null) : base(logger, helper, dataContext, parentDeclarationReader)
	{
	}

	public Customs.DataTransfer.Universal.CustomsSupportingInformationDataObjectReader CreateNewCustomsSupportingInformationDataObjectReader_Exposed(CustomsSupportingInformation customsSupportingInformation, ZGuid parentPK, ZString parentTableCode, Customs.DataTransfer.Universal.CustomsSupportingInformationDataObjectReader.GetMatchingDataPredicate matchExisting) => base.CreateNewCustomsSupportingInformationDataObjectReader(customsSupportingInformation, parentPK, parentTableCode, matchExisting);
}
