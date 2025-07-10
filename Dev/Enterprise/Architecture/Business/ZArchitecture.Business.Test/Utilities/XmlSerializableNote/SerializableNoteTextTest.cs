using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.DataMapping.Testing;

namespace Enterprise.ZArchitecture.Testing
{
	public abstract class SerializableNoteTextTest<T> : XmlSerializableTestCase<T> where T : SerializableNoteText
	{
		public void TestHumanReadableText()
		{
			AssertEquals("note as humanReadableText", ExpectedResultAsHumanReadableText, SerializableNoteText.HumanReadableText(NoteForTest, typeof(T)));
		}

		protected StmNote NoteForTest
		{
			get
			{
				if (noteForTest == null)
				{
					DummyEnterpriseBusinessObject noteParent = Factory.New<DummyEnterpriseBusinessObject>();
					noteForTest = noteParent.Notes.AddNew(false, PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description, NoteInXmlForTesting);
				}
				return noteForTest;
			}
		}
		StmNote noteForTest;

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		protected abstract ZString NoteInXmlForTesting { get; }
		protected abstract ZString ExpectedResultAsHumanReadableText { get; }
	}
}
