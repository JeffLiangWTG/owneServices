using CargoWise.Types;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	public abstract class NoteWrapperTest : Base.Testing.GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			NoteWrapper wrapperEmpty = (NoteWrapper)GetNewDocumentWrapper();
			AssertEquals("wrapperEmpty.Text", ZString.Empty, wrapperEmpty.Text);
			AssertEquals("wrapperEmpty.Description", ZString.Empty, wrapperEmpty.Description);
			AssertEquals("wrapperEmpty.CreatedDate", ZDateTime.Empty, wrapperEmpty.CreatedDate);
		}

		public abstract void TestWrapperMappingFull();

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Note
======================================================================
Name                                    Type
----------------------------------------------------------------------
CreatedDate                             DateTime
Description                             String
DescriptionInDatabase                   String
Text                                    String
";
			}
		}
	}
}
