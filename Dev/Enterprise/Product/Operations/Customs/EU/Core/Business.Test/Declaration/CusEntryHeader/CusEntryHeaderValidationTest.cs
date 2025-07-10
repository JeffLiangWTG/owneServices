using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class CusEntryHeaderValidationTest : Customs.Business.Testing.CusEntryHeaderValidationTest
	{
		public void TestParent()
		{
			CusEntryHeader parent = Factory.New<CusEntryHeader>();
			AssertEquals(parent.Validation.Parent, parent);
		}

		public virtual void TestValidationDecider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				AssertNull(entryHeader.Validation.ValidationDecider);
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertNull(entryHeader.Validation.ValidationDecider);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				AssertNull(entryHeader.Validation.ValidationDecider);
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertNull(entryHeader.Validation.ValidationDecider);
			}
		}
	}
}
