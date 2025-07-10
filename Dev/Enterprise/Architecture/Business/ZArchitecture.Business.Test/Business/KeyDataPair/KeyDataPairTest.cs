using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(KeyDataPair))]
	sealed class KeyDataPairTest : NonPersistentBusinessObjectTestCase
	{
		public void TestReadOnly()
		{
			var businessObject = this.GetNewBusinessObject();
			Assert("KeyDataPair should be readonly", businessObject.ReadOnly);
			businessObject.ReadOnly = false;
			Assert("KeyDataPair should always be readonly", businessObject.ReadOnly);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new KeyDataPair();
		}

		#endregion
	}
}
