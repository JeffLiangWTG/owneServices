using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test.ExperimentalSettings
{
	[TestedType(typeof(ExperimentalSettingsCollection))]
	public class ExperimentalSettingsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ExperimentalSettingsCollection>
	{
		protected override ExperimentalSettingsCollection GetCollectionToTest()
		{
			return new ExperimentalSettingsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ExperimentalSetting();
		}
	}
}
