using CargoWise.Types;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class SerializableNoteTextBizObjTest : SerializableNoteTextTest<DummyNotes>
	{
		protected override ZString ExpectedResultAsHumanReadableText
		{
			get
			{
				ZStringBuilder builder = new ZStringBuilder();
				builder.Append("Element A: ABC");
				builder.Append("Element B: DEF");
				builder.Append(" ");
				return builder.ToStringWithNewLineBetweenAppends();
			}
		}

		protected override ZString NoteInXmlForTesting
		{
			get { return "<DummyNotes><DummyNote><ElementA>ABC</ElementA><ElementB>DEF</ElementB><ElementC>H</ElementC></DummyNote></DummyNotes>"; }
		}
	}
}
