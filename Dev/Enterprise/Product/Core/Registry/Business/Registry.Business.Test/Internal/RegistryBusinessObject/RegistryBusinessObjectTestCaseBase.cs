using CargoWise.ComponentModel;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestsSubclassesOf(typeof(RegistryBusinessObject))]
	public abstract class RegistryBusinessObjectTestCaseBase : RegistryBusinessObjectTemplateTestCase
	{
		public void TestCodeInfo()
		{
			AssertNotNull("CodeInfo should not be null", BizObj.CodeInfo);
			AssertEquals("CodeInfo.Name", "Code", BizObj.CodeInfo.Name);
		}

		public void TestDescriptionInfo()
		{
			AssertNotNull("DescriptionInfo should not be null", BizObj.DescriptionInfo);
			AssertEquals("DescriptionInfo.Name", "Description", BizObj.DescriptionInfo.Name);
		}

		public void TestValidateCode_CheckCodeIsNotEmpty()
		{
			string errorMessage = "Please enter a " + CodeDisplayName + ".";

			AssertEquals("Precondition: Code should not have errors", false, BizObj.CodeInfo.HasErrors());

			BizObj.Code = "";
			AssertEquals("Code should have an error if it is empty and mandatory", IsCodeMandatory, BizObj.CodeInfo.HasError(errorMessage));

			BizObj.Code = "!@#";
			AssertEquals("Code should not have an error if it is not empty", false, BizObj.CodeInfo.HasError(errorMessage));
		}

		public void TestValidateCode_CheckCodeIsUnique()
		{
			string errorMessage = string.Format("The {0} has been duplicated and must be unique.", CodeDisplayName);

			DummyRegistryBusinessObjectCollection collection = new DummyRegistryBusinessObjectCollection();

			RegistryBusinessObject bizObj1 = (RegistryBusinessObject)GetNewBusinessObject();
			RegistryBusinessObject bizObj2 = (RegistryBusinessObject)GetNewBusinessObject();
			RegistryBusinessObject bizObj3 = (RegistryBusinessObject)GetNewBusinessObject();

			bizObj1.CodeInfo.HumanReadableName = CodeDisplayName;
			bizObj2.CodeInfo.HumanReadableName = CodeDisplayName;
			bizObj3.CodeInfo.HumanReadableName = CodeDisplayName;

			collection.Add(bizObj1);
			collection.Add(bizObj2);
			collection.Add(bizObj3);

			AssertEquals("Precondition: BizObj1.Code should have no errors", false, bizObj1.CodeInfo.HasErrors());
			AssertEquals("Precondition: BizObj2.Code should have no errors", false, bizObj2.CodeInfo.HasErrors());
			AssertEquals("Precondition: BizObj3.Code should have no errors", false, bizObj1.CodeInfo.HasErrors());

			bizObj1.Code = "ABC";
			bizObj2.Code = "ABC";
			bizObj3.Code = "XYZ";

			if (IsCodeUniqueInCollection)
			{
				AssertEquals("BizObj1.Code should not have errors", false, bizObj1.CodeInfo.HasError(errorMessage));
				AssertEquals("BizObj2.Code should have an error because it is not unique", true, bizObj2.CodeInfo.HasError(errorMessage));
				AssertEquals("BizObj3.Code should not have errors", false, bizObj3.CodeInfo.HasError(errorMessage));

				bizObj2.Code = "POP";
				AssertEquals("BizObj2.Code should not have errors", false, bizObj2.CodeInfo.HasError(errorMessage));
			}
			else
			{
				AssertEquals("BizObj1.Code should not have errors", false, bizObj1.CodeInfo.HasError(errorMessage));
				AssertEquals("BizObj2.Code should not have errors", false, bizObj2.CodeInfo.HasError(errorMessage));
				AssertEquals("BizObj3.Code should not have errors", false, bizObj3.CodeInfo.HasError(errorMessage));
			}
		}

		public void TestValidateDescription_CheckDescriptionIsNotEmpty()
		{
			string errorMessage = "Please enter a " + DescriptionDisplayName + ".";

			AssertEquals("Precondition: Description should not have errors", false, BizObj.DescriptionInfo.HasErrors());

			BizObj.EnglishDescription = "";
			AssertEquals("Description should have an error if it is empty and mandatory", IsDescriptionMandatory, BizObj.DescriptionInfo.HasError(errorMessage));

			BizObj.EnglishDescription = "!@#";
			AssertEquals("Description should not have an error if it is not empty", false, BizObj.DescriptionInfo.HasError(errorMessage));
		}

		#region Implementation

		protected new RegistryBusinessObject BizObj
		{
			get { return (RegistryBusinessObject)base.BizObj; }
		}

		protected virtual bool IsCodeMandatory
		{
			get { return true; }
		}

		protected virtual bool IsCodeUniqueInCollection
		{
			get { return true; }
		}

		protected virtual string CodeDisplayName
		{
			get { return "Code"; }
		}

		protected virtual bool IsDescriptionMandatory
		{
			get { return false; }
		}

		protected virtual string DescriptionDisplayName
		{
			get { return "Description"; }
		}

		#endregion
	}
}
