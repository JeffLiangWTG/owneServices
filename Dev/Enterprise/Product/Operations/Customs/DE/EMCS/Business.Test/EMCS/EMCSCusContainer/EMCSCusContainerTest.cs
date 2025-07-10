using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSCusContainer))]
	class EMCSCusContainerTest : EU.EMCS.Business.Testing.EMCSCusContainerTest<EMCSCusContainer, EMCSJobDeclaration>
	{
		public override void TestContainerValidation()
		{
			AssertType<EMCSCusContainerValidation>("Checking CusContainer.Validation", container.Validation);
		}

		protected override BusinessObject GetNewBusinessObject() => container;

		protected override ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject bo) => new BaseCusContainer.CustomLabelsProvider(((EMCSCusContainer)bo).Declaration);

		protected override BaseJobDeclaration GetJobDeclaration(BusinessObjectFactory factory)
		{
			var declaration = factory.New<EMCSJobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			return declaration;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<EMCSJobDeclaration>();
			declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			container = declaration.CusContainers.AddNew();
		}
		EMCSCusContainer container;
	}
}
