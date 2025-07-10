using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ApplicationCodeObj))]
	sealed class ApplicationCodeObjTest : RegistryBusinessObjectTemplateTestCase<ApplicationCodeObj>
	{
		public void TestIsUnpurgableDoesNotCauseValidationErrors()
		{
			var applicationCodeObj = new ApplicationCodeObj();
			applicationCodeObj.PurgeType = PurgeTypeList.ApplicationCode;
			applicationCodeObj.IsUnpurgable = true;
			applicationCodeObj.PurgeTime = 0;
			AssertNoErrors(applicationCodeObj.PurgeTimeInfo);

			applicationCodeObj.PurgeTimeUnit = ZGuid.Empty;
			AssertNoErrors(applicationCodeObj.PurgeTimeInfo);

			applicationCodeObj.IsUnpurgable = false;
			applicationCodeObj.PurgeTime = 0;
			AssertHasError(applicationCodeObj.PurgeTimeInfo, "1 week is the minimum allowed.");

			applicationCodeObj.PurgeTimeUnit = ZGuid.Empty;
			AssertHasError(applicationCodeObj.PurgeTimeUnitInfo, "Please enter a value.");
		}

		public void TestSelectedFieldIsReadonlyWhenUnpurgable()
		{
			var applicationCodeObj = new ApplicationCodeObj();
			applicationCodeObj.IsUnpurgable = true;
			Assert("Selected field should be readonly when application code is unpurgable", applicationCodeObj.SelectedInfo.ReadOnly);

			applicationCodeObj.IsUnpurgable = false;
			Assert("Selected field should not be readonly when application code is unpurgable", !applicationCodeObj.SelectedInfo.ReadOnly);
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override ApplicationCodeObj GetBusinessObjectToClone()
		{
			return (ApplicationCodeObj)GetNewBusinessObject();
		}

		protected override ApplicationCodeObj GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ApplicationCodeObj();
		}
	}
}
