using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IE;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclaration))]
	class AddInfoJobDeclarationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidation()
		{
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			AssertType<ImportUCC6JobDeclarationValidation>(declaration.Validation);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			AssertType<ExportJobDeclarationValidation>(declaration.Validation);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			AssertType<ExitSummaryJobDeclarationValidation>(declaration.Validation);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			AssertType<ReExportJobDeclarationValidation>(declaration.Validation);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<JobDeclarationValidation>(declaration.Validation);
		}

		public void TestZG_PresentationStartDate_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var resourceStringDataAttribute = declaration.ZG_PresentationStartDateInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Start", resourceStringDataAttribute.Caption);
			AssertEquals("Date and Time of Presentation of the Goods - 15 08 001 000", resourceStringDataAttribute.FullDescription);
		}

		protected override BusinessObject GetNewBusinessObject() => declaration;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;
	}
}
