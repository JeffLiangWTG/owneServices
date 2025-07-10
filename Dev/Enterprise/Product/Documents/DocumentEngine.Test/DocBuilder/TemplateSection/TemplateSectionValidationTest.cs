using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	sealed class TemplateSectionValidationTest : TestCaseWithFactory
	{
		public void TestValidation()
		{
			TemplateSection templateSection = new TemplateSection("", 0, 0);
			templateSection.Validation.ValidateAll();
			AssertHasError(templateSection.TypeCodeInfo, TemplateSectionValidation.ErrorTypeCodeMustBeValid);
			AssertHasError(templateSection.SectionNameInfo, TemplateSectionValidation.ErrorMustSpecifySectionName);
			AssertNoErrors(templateSection.StartingRowNumberInfo);
			AssertHasError(templateSection.RowCountInfo, TemplateSectionValidation.ErrorMustHaveRows);

			templateSection = new TemplateSection("DFD,Fred's Section", 1, 1);
			templateSection.Validation.ValidateAll();
			AssertHasError(templateSection.TypeCodeInfo, TemplateSectionValidation.ErrorTypeCodeMustBeValid);
			AssertNoErrors(templateSection.SectionNameInfo);
			AssertNoErrors(templateSection.StartingRowNumberInfo);
			AssertNoErrors(templateSection.RowCountInfo);

			templateSection = new TemplateSection(ConfigurableSectionTypeList.Codes.ConfigSection + ",Fred's Section", 1, 1);
			templateSection.Validation.ValidateAll();
			AssertNoErrors(templateSection.TypeCodeInfo);
			AssertNoErrors(templateSection.SectionNameInfo);
			AssertNoErrors(templateSection.StartingRowNumberInfo);
			AssertNoErrors(templateSection.RowCountInfo);
		}

		#region WIP
		//public void TestAllFieldsAreValidated()
		//{
		//    TemplateSection templateSection = new TemplateSection(ConfigurableSectionTypeList.Codes.DocumentHeader + ", My Favourite Martian", 0, 0);
		//    ZStringBuilder missingFieldNames = new ZStringBuilder();
		//    List<string> checkMethodNames = new List<string>();
		//    foreach (ZPropertyInfo info in templateSection.ZPropertyInfoHash)
		//    {
		//        string checkMethodName = "Check" + info.Name;
		//        MethodInfo checkMethodInfo = templateSection.Validation.GetType().GetMethod(checkMethodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

		//        if (checkMethodInfo != null && checkMethodInfo.IsVirtual)
		//        {
		//            checkMethodNames.Add(checkMethodName);
		//        }
		//        else
		//        {
		//            missingFieldNames.Append(info.Name);
		//        }
		//    }

		//    if (!missingFieldNames.IsEmpty)
		//    {
		//        Fail("The Following fields on class "
		//            + templateSection.GetType().Name
		//            + " have no CheckXXXX() method in validation: "
		//            + missingFieldNames.ToStringWithDelimiterBetweenAppends(", "));
		//    }

		//    if (checkMethodNames.Count > 0)
		//    {
		//        DynamicMock<TemplateSectionValidation> mockValidator = new DynamicMock<TemplateSectionValidation>(templateSection);
		//        foreach (string checkMethodName in checkMethodNames)
		//        {
		//            mockValidator.Expect(checkMethodName);
		//        }
		//        mockValidator.Object.ValidateAll();
		//        mockValidator.Verify();
		//    }
		//    else
		//    {
		//        Assert("There are no fields to test yet. This is not necessarily a problem.", true);
		//    }
		//}
		#endregion
	}
}
