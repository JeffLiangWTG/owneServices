using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.DeclarationStatusUpdater.Testing
{
	public class DeclarationMessageStatusUpdaterTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestUpdate()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageStatus = ZString.Empty;
			DeclarationMessageStatusUpdater.Update(declaration);
			NUnit.Framework.Assert.That(declaration.JE_MessageStatus, NUnit.Framework.Is.EqualTo(ZString.Empty));

			CusEntryHeader entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_Status = MessageStatusList.Codes.AwaitingResponse;
			DeclarationMessageStatusUpdater.Update(declaration);
			NUnit.Framework.Assert.That(declaration.JE_MessageStatus, NUnit.Framework.Is.EqualTo(MessageStatusList.Codes.AwaitingResponse).Using(CustomComparers.TypeComparison));

			CusEntryHeader entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_Status = MessageStatusList.Codes.SentAndRejected;
			DeclarationMessageStatusUpdater.Update(declaration);
			NUnit.Framework.Assert.That(declaration.JE_MessageStatus, NUnit.Framework.Is.EqualTo(MessageStatusList.Codes.MultipleStatus).Using(CustomComparers.TypeComparison));
		}
	}
}
