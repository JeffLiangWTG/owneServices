using System.Collections.Generic;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.EU.EMCS.DataTransfer.Testing
{
	class EMCSCustomsContainerDataObjectReaderTest : OrganizationAddressTestHelper
	{
		public void TestFillContainer()
		{
			var declaration = Factory.BOFactory.New<EMCSJobDeclaration>();
			var container = declaration.CusContainers.AddNew();
			container.Delete();
			var containerDataObject = ContainerDataObjectTestHelper.SetupContainerWithVGM();
			containerDataObject.SetAddInfoCollection(() => new List<AddInfo>
			{
				AddInfo.New(Constants.AddInfo.Keys.SealDetails, "SealDetails".PadRight(351, 'X')),
				AddInfo.New(Constants.AddInfo.Keys.Comment, "Comment".PadRight(351, 'X'))
			});

			var reader = new EMCSCustomsContainerDataObjectReader(containerDataObject, new TestErrorLogger(), new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates), declaration);
			var containerBO = (EMCSCusContainer)reader.ReadIntoBusinessObject();

			AssertNotNull(containerBO);

			CombineAssertions(delegate
			{
				ContainerDataObjectTestHelper.AssertContentsWithVGM(containerBO.JobContainer);
				AssertEquals("SealDetails".PadRight(350, 'X'), containerBO.SealDetails);
				AssertEquals("Comment".PadRight(350, 'X'), containerBO.Comment);
			});
		}
	}
}
