using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.Testing
{
	[TestedType(typeof(EDIMessageAttachDependentCollection))]
	sealed class EDIMessageAttachDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefault()
		{
			EDIMessageAttach ediMessageAttach = EDIMessageAttachDependentCollection.AddNew();
			AssertEquals(EDIMessage.PK, ediMessageAttach.EG_EM);
		}

		#region Implementation

		EDIMessageAttachDependentCollection EDIMessageAttachDependentCollection
		{
			get
			{
				if (ediMessageAttachDependentCollection == null)
				{
					ediMessageAttachDependentCollection = new EDIMessageAttachDependentCollection(EDIMessage, Factory);
				}
				return ediMessageAttachDependentCollection;
			}
		}
		EDIMessageAttachDependentCollection ediMessageAttachDependentCollection;

		EDIMessage EDIMessage
		{
			get { return ediMessage ?? (ediMessage = EDIMessageTestFactory.New(Factory)); }
		}
		EDIMessage ediMessage;

		#endregion

		#region Overrides

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return EDIMessageAttachDependentCollection;
		}

		#endregion
	}
}
