using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(NoteTextWrapper))]
	sealed class NoteTextWrapperTest : NoteWrapperTest
	{
		public override void TestWrapperMappingFull()
		{
			NoteTextWrapper wrapper = new NoteTextWrapper("text", (NoResString)"description", new ZDateTime(2012, 1, 1), Factory);
			AssertEquals("text", wrapper.Text);
			AssertEquals("description", wrapper.Description);
			AssertEquals(new ZDateTime(2012, 1, 1), wrapper.CreatedDate);
		}

		#region Implementation

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new NoteTextWrapper("", (NoResString)"", ZDateTime.Empty, Factory);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get { return @"Registry : (No Default Field Value Available on Registry)"; }
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new NoteTextWrapper("text", (NoResString)"description", new ZDateTime(2012, 1, 1), Factory);
		}

		#endregion
	}
}
