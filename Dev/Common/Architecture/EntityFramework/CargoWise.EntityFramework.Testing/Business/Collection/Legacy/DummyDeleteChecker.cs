namespace CargoWise.EntityFramework.Testing
{
	sealed class DummyDeleteChecker : DeleteChecker
	{
		public override DeleteDetails DeleteDetails(BusinessObject businessObjectToBeDeleted)
		{
			return new DeleteDetails.Disallow("Disallow for test");
		}
	}
}
