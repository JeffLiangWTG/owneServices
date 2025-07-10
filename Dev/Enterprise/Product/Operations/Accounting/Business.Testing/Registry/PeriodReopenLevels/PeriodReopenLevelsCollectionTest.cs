using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(PeriodReopenLevelsCollection))]
	public class PeriodReopenLevelsCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<PeriodReopenLevelsCollection>
	{
		#region Implementation

		protected override PeriodReopenLevelsCollection GetCollectionToTest()
		{
			return new PeriodReopenLevelsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PeriodReopenLevels();
		}

		protected new PeriodReopenLevelsCollection Collection
		{
			get { return base.Collection; }
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
