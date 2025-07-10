using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map.Testing
{
	[TestedType(typeof(SyntaxAndFormattingWrapperCollection))]
	sealed class SyntaxAndFormattingCommandWrapperCollectionTest : Base.Testing.GenericWrapperCollectionTest<SyntaxAndFormattingWrapperCollection>
	{
		public void TestGotPlentyOfChaptersAndTheyreAllDifferentAndNotEmpty()
		{
			var collection = new SyntaxAndFormattingWrapperCollection(Factory);
			Assert("collection.Count should be > 5", collection.Count > 5);
			var titles = new List<ZString>();
			var lines = new List<ZString>();
			foreach (SyntaxAndFormattingWrapper wrapper in collection)
			{
				AssertEquals("wrapper.TitleText.IsEmpty", false, wrapper.TitleText.IsEmpty);
				AssertEquals("wrapper.LineText.IsEmpty", false, wrapper.LineText.IsEmpty);
				AssertCollectionNotContains("wrapper.TitleText should not be duplicated.\r\n-->" + wrapper.TitleText, wrapper.TitleText, titles);
				AssertCollectionNotContains("wrapper.LineText should not be duplicated.\r\n-->" + wrapper.LineText, wrapper.LineText, lines);
				titles.Add(wrapper.TitleText);
				lines.Add(wrapper.LineText);
			}
		}

		protected override SyntaxAndFormattingWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new SyntaxAndFormattingWrapperCollection(Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new SyntaxAndFormattingWrapper("", "", Factory);
		}
	}
}
