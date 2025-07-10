using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class LineNumberAssignerTest : TestCaseWithFactory
{
	public void TestGetEntryLineComparerBeforeLineNumbering()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var lineNumberAssigner = new LineNumberAssignerForTest(entryHeader);
		var comparer = lineNumberAssigner.GetEntryLineComparerBeforeLineNumbering_Exposed(entryHeader);

		AssertType<EntryLineComparerAccordingToPackageAndMark>(comparer);
	}

	class LineNumberAssignerForTest : LineNumberAssigner
	{
		public LineNumberAssignerForTest(Customs.Business.CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		public IComparer<Customs.Business.CusEntryLine> GetEntryLineComparerBeforeLineNumbering_Exposed(Customs.Business.CusEntryHeader entry) => base.GetEntryLineComparerBeforeLineNumbering(entry);
	}
}
