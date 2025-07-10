using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(IndexDuration))]
	sealed class IndexDurationEntryTest : RegistryBusinessObjectTemplateTestCase<IndexDuration>
	{
		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override BusinessObject GetNewBusinessObject() => new IndexDuration();

		protected override IndexDuration GetBusinessObjectToClone()
		{
			return new IndexDuration();
		}

		protected override IndexDuration GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
