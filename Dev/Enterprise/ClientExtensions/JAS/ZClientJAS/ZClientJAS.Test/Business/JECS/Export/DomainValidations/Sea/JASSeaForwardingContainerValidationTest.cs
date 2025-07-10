using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations.Testing
{
	internal class JASSeaForwardingContainerValidationTest : JXCValidationTestCase
	{
		public void TestDomainValidationShouldSubclassFromAutoValidationType()
		{
			AssertEquals(typeof(AutoJobContainerValidation), typeof(JXCForwardingSeaContainerValidation).BaseType);
		}

		public void TestValidateJC_ContainerNum()
		{
			AssertMaxLengthLessOrEqualToForJXC(JXCConstants.CONTFieldBoundaries.ContainerNoMaxLength, JobContainerSchema.JC_ContainerNum);
			AssertHasNoJXCWarnings("Pre-condition", Container.JC_ContainerNumInfo);
			Container.JC_ContainerNum = "123";
			AssertHasNoJXCWarnings(Container.JC_ContainerNumInfo);
			Container.JC_ContainerNum = "";
			AssertHasNotEnteredJXCWarning(Container.JC_ContainerNumInfo);
		}

		public void TestValidateJC_SealNum()
		{
			AssertMaxLengthLessOrEqualToForJXC(JXCConstants.CONTFieldBoundaries.SealNoMaxLength, JobContainerSchema.JC_SealNum);
		}

		protected override void SetUp()
		{
			base.SetUp();
			new JXCDomainValidationManager(Factory).ManageJXCValidations(JXCExportValidationType.Ocean);
		}

		ForwardingContainer Container
		{
			get
			{
				if (fContainer == null)
				{
					fContainer = Factory.New<ForwardingContainer>();
				}

				return fContainer;
			}
		}

		ForwardingContainer fContainer;
	}
}
