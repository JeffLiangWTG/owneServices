using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.CN.DataTransfer.Universal.Testing
{
	class CNContainerDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestPopulateContainerSize()
		{
			Registry.Business.eServices.eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var logger = new TestErrorLogger();
			var helper = new CNDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.China);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "BLT";

			var input = new Container
			{
				ContainerNumber = "CNTCNTCNT",
				CustomsContainerSize = new CodeDescriptionPair2Char
				{
					Code = "XXX"
				}
			};

			var output = new CNContainerDataObjectReader(input, logger, helper, declaration).ReadIntoBusinessObject();
			Assert("Should not read container size from UXML", output.CO_ContainerNumber == "CNTCNTCNT" && output.CO_ContainerSize != "XXX");
		}
	}
}
