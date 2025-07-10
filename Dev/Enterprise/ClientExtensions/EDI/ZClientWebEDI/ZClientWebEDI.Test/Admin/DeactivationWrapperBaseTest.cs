using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(DeactivationWrapperBase))]
	public class DeactivationWrapperBaseTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DeactivationWrapperBaseForTest(Factory);
		}

		class DeactivationWrapperBaseForTest : DeactivationWrapperBase
		{
			public DeactivationWrapperBaseForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			public override ZString LicenceType { get; }

			public override ZString Organisation { get; }

			public override ZString SystemInfo { get; }

			public override int ReferenceNumber { get; }
		}
		#endregion
	}
}
