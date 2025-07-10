using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.DeclarationStatusUpdater.Testing
{
	public class DeclarationEntrySubmittedDateUpdaterTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestUpdate()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_EntrySubmittedDate = ZDateTime.Empty;
			DeclarationEntrySubmittedDateUpdater.Update(declaration);
			NUnit.Framework.Assert.That(declaration.JE_EntrySubmittedDate, NUnit.Framework.Is.EqualTo(ZDateTime.Empty));

			CusEntryHeader entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_EntrySubmittedDate = new ZDateTime(2008, 5, 1);
			DeclarationEntrySubmittedDateUpdater.Update(declaration);
			NUnit.Framework.Assert.That(declaration.JE_EntrySubmittedDate, NUnit.Framework.Is.EqualTo(new ZDateTime(2008, 5, 1)));

			CusEntryHeader entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_EntrySubmittedDate = new ZDateTime(2008, 4, 1);
			DeclarationEntrySubmittedDateUpdater.Update(declaration);
			NUnit.Framework.Assert.That(declaration.JE_EntrySubmittedDate, NUnit.Framework.Is.EqualTo(new ZDateTime(2008, 4, 1)));

			CusEntryHeader entry3 = declaration.CustomsEntryHeaders.AddNew();
			entry3.CH_EntrySubmittedDate = new ZDateTime(2008, 6, 1);
			DeclarationEntrySubmittedDateUpdater.Update(declaration);
			NUnit.Framework.Assert.That(declaration.JE_EntrySubmittedDate, NUnit.Framework.Is.EqualTo(new ZDateTime(2008, 4, 1)));
		}
	}
}
