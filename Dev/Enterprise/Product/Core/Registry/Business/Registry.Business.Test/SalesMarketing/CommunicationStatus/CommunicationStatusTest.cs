using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CommunicationStatus))]
	sealed class CommunicationStatusTest : CodeDescriptionBoolTest
	{
		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new CommunicationStatus();

			result.Code = "AAA";
			result.EnglishDescription = "AAA Description";
			result.Bool = true;
			result.Closed = true;

			return result;
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			AssertEquals("AAA", clone.Code);
			AssertEquals("AAA Description", clone.Description);
			AssertEquals(true, ((CommunicationStatus)clone).Bool);
			AssertEquals(true, ((CommunicationStatus)clone).Closed);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CommunicationStatus();
		}

		#endregion
	}
}
