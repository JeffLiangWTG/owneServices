using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestsSubclassesOf(typeof(EntryHeaderConfiguration))]
	public abstract class EntryHeaderConfigurationAbstractTest<EH> : TestCaseWithFactory
		where EH : EntryHeaderConfiguration
	{
		protected override void SetUp()
		{
			base.SetUp();
			configuration = (EH)Activator.CreateInstance(typeof(EH));
		}
		protected EH configuration;
	}

	[TestedType(typeof(EntryHeaderConfiguration))]
	sealed class EntryHeaderConfigurationBaseOnlyTest : EntryHeaderConfigurationAbstractTest<EntryHeaderConfiguration>
	{
		[ExpectNoExceptions]
		public void TestGetValidationDecider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				NUnit.Framework.Assert.That(configuration.GetValidationDecider(entryHeader), NUnit.Framework.Is.EqualTo(default(IEntryHeaderValidationDecider)));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.GetValidationDecider(entryHeader), NUnit.Framework.Is.EqualTo(default(IEntryHeaderValidationDecider)));
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				NUnit.Framework.Assert.That(configuration.GetValidationDecider(entryHeader), NUnit.Framework.Is.EqualTo(default(IEntryHeaderValidationDecider)));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.GetValidationDecider(entryHeader), NUnit.Framework.Is.EqualTo(default(IEntryHeaderValidationDecider)));
			}
		}
	}
}
