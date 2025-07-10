using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Support.Testing
{
	[TestsSubclassesOf(typeof(OperationalActionMethodSettings))]
	public abstract class OperationalActionMethodSettingsTest<SettingsT> : NonPersistentBusinessObjectTestCase where SettingsT : OperationalActionMethodSettings
	{
		#region Implementation
		protected SettingsT Settings
		{
			get
			{
				return settings ?? (settings = (SettingsT)GetNewBusinessObject());
			}
		}

		SettingsT settings;
		#endregion
	}
}
