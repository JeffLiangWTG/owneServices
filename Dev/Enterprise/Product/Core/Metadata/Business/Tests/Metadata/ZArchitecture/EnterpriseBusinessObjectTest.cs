using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Metadata.Business.Tests
{
	public class EnterpriseBusinessObjectTest : TransactionedTestCase
	{
		#region TestNoteTypes

		public void TestNoteTypes()
		{
			var expected = ExpectedNoteTypes();
			var metadata = NewMetadata;
			var actual = metadata.NoteTypes;
			new TestHelper().AssertIListEqualsByElements(expected, actual);
		}

		protected internal virtual NoteTypeCollection ExpectedNoteTypes()
		{
			return new NoteTypeCollection();
		}

		#endregion

		#region Implementation

		protected virtual IMetadata NewMetadata
		{
			get { return new EnterpriseBusinessObject(); }
		}

		#endregion
	}
}
