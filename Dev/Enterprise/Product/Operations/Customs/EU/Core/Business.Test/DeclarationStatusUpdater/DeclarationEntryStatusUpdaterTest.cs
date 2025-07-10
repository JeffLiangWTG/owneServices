using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.DeclarationStatusUpdater.Testing
{
	public class DeclarationEntryStatusUpdaterTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestUpdate()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_EntryStatus = ZString.Empty;
			DeclarationEntryStatusUpdater.Update(declaration);
			NUnit.Framework.Assert.That(declaration.JE_EntryStatus, NUnit.Framework.Is.EqualTo(ZString.Empty));

			CusEntryHeader entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_EntryStatus = MessageStatusList.Codes.AwaitingResponse;
			DeclarationEntryStatusUpdater.Update(declaration);
			NUnit.Framework.Assert.That(declaration.JE_EntryStatus, NUnit.Framework.Is.EqualTo(MessageStatusList.Codes.AwaitingResponse).Using(CustomComparers.TypeComparison));

			CusEntryHeader entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_EntryStatus = MessageStatusList.Codes.SentAndRejected;
			DeclarationEntryStatusUpdater.Update(declaration);
			NUnit.Framework.Assert.That(declaration.JE_EntryStatus, NUnit.Framework.Is.EqualTo(MessageStatusList.Codes.MultipleStatus).Using(CustomComparers.TypeComparison));

			entry2.CH_EntryStatus = MessageStatusList.Codes.AwaitingResponse;
			DeclarationEntryStatusUpdater.Update(declaration);
			NUnit.Framework.Assert.That(declaration.JE_EntryStatus, NUnit.Framework.Is.EqualTo(MessageStatusList.Codes.AwaitingResponse).Using(CustomComparers.TypeComparison));

			entry2.CH_EntryStatus = ZString.Empty;
			DeclarationEntryStatusUpdater.Update(declaration);
			NUnit.Framework.Assert.That(declaration.JE_EntryStatus, NUnit.Framework.Is.EqualTo(MessageStatusList.Codes.AwaitingResponse).Using(CustomComparers.TypeComparison));
		}
	}
}
