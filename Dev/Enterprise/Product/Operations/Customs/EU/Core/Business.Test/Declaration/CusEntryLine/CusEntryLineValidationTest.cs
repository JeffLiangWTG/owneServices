using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class CusEntryLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestEntryLine()
		{
			CusEntryLine parent = Factory.New<CusEntryLine>();
			AssertEquals(parent.Validation.EntryLine, parent);
		}

		public virtual void TestValidationDecider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				AssertNull(entryLine.Validation.ValidationDecider);
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertNull(entryLine.Validation.ValidationDecider);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				AssertNull(entryLine.Validation.ValidationDecider);
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertNull(entryLine.Validation.ValidationDecider);
			}
		}
	}
}
