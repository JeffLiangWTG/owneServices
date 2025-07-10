using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclaration))]
	public class AddInfoJobDeclarationBOTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetNewValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertType<ImportJobDeclarationValidation>(declaration.Validation);
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertType<ExportJobDeclarationValidation>(declaration.Validation);
				declaration.JE_MessageType = "";
				AssertType<JobDeclarationValidation>(declaration.Validation);
				declaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
				AssertType<JobDeclarationValidation>(declaration.Validation);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<JobDeclaration>();
	}
}
