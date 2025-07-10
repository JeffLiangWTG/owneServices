using CargoWise.Types;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Registry.Testing
{
	[TestedType(typeof(ChaseQueueValidation))]
	public class ChaseQueueValidationTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Validation Test
		public void TestProperties()
		{
			ChaseQueueValidation validation = new ChaseQueueValidation();
			validation.DayOfTheWeek = "BLAH";
			validation.TimeFrom = ZDateTime.Empty;
			AssertHasError(validation.DayOfTheWeekInfo, "Enter a valid selection.");
			AssertHasError(validation.TimeFromInfo, "Please enter a value.");
			validation.DayOfTheWeek = "MONDAY";
			validation.TimeFrom = ZDateTime.Now;
			AssertNoErrors(validation.DayOfTheWeekInfo);
			AssertNoErrors(validation.TimeFromInfo);
		}

		#endregion
		#region Implementation
		protected override void CheckAllPropertiesAreEqual(RegistryBusinessObjectTemplate originalBusinessObject, RegistryBusinessObjectTemplate newBusinessObject, bool isClone)
		{
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			ChaseQueueValidation result = new ChaseQueueValidation();
			result.DayOfTheWeek = "Monday";
			result.TimeFrom = ZDateTime.UtcNow;
			return result;
		}

		protected override bool RequiresFactory
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}
		#endregion
	}
}
