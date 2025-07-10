using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

public class EdecContainerDataProviderTest : TestCaseWithFactory
{
	public void TestConstructorNullArgument()
	{
		var declaration = Factory.New<JobDeclaration>();
		var container = declaration.CusContainers.AddNew();

		CombineAssertions(() =>
		{
			AssertNull("Null", EdecContainerDataProvider.New(null));
			AssertNotNull("Not Null", EdecContainerDataProvider.New(container));
		});
	}

	public void TestProvider()
	{
		var declaration = Factory.New<JobDeclaration>();
		var container = declaration.CusContainers.AddNew();

		container.CO_ContainerNumber = "ABCDEFGHI12345678";

		var messageBuilder = EdecContainerDataProvider.New(container);

		CombineAssertions(() =>
		{
			AssertEquals(nameof(messageBuilder.ContainerNumber), "ABCDEFGHI12345678", messageBuilder.ContainerNumber);
		});
	}
}
