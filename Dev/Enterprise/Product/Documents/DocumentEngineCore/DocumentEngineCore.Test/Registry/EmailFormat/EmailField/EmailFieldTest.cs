using System;
using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(EmailField))]
	public class EmailFieldTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestRunPreSaveValidation()
		{
			var bizObj = new EmailField();
			bizObj.ClearAllNotifications();
			AssertNoErrors("Precondition: There should be no errors.", bizObj);

			bizObj.Code = ZString.Empty;
			bizObj.RunPreSaveValidation();
			AssertHasErrors(bizObj.CodeInfo);
		}

		public void TestIndex()
		{
			AssertEquals("Precondition: Index should be empty", true, BizObj.Index.IsEmpty);
			BizObj.Index = "1";
			AssertEquals("Index:", "1", BizObj.Index);

			var newField = BizObj.ParentCollectionInternal.AddNew();
			newField.Index = "2";
			BizObj.Index = "2";
			AssertEquals("Index should have errors", true, BizObj.IndexInfo.HasError("This index has already been used. Please enter one that does not already exist."));

			BizObj.Index = ZString.Empty;
			AssertEquals("Index should have errors", true, BizObj.IndexInfo.HasError("Please enter a numeric value."));

			BizObj.Index = "add";
			AssertEquals("Index should have errors", true, BizObj.IndexInfo.HasError("Index must be a numeric value."));
		}

		public void TestCodeAssignment_UnknownCode()
		{
			var emailField = new EmailFieldDummy();
			emailField.EmailFieldsPairList.AddPairIfNotExist("Test Code", "Test Description");

			emailField.Code = "Some Code";

			AssertEquals("Code should have errors", true, emailField.CodeInfo.HasError("Please select one of the predefined fields."));
			AssertEquals("Description should have errors when assigned code is not in the list", true, emailField.DescriptionInfo.HasError("Please select one of the predefined fields."));
		}

		public void TestCodeAssignment_EmptyEmailFieldsPairList()
		{
			var emailField = new EmailFieldDummy();

			emailField.Code = "Some Code";

			AssertEquals("Code should have errors", true, emailField.CodeInfo.HasError("Please select one of the predefined fields."));
			AssertEquals("Description should have errors when assigned code is not in the list", true, emailField.DescriptionInfo.HasError("Please select one of the predefined fields."));
		}

		public void TestCodeAssignment_KnownCode()
		{
			var emailField = new EmailFieldDummy();
			emailField.EmailFieldsPairList.AddPairIfNotExist("Test Code", "Test Description");

			emailField.Code = "Test Code";

			AssertEquals("Code shouldn't have errors", false, emailField.CodeInfo.HasError("Please select one of the predefined fields."));
			AssertEquals("Description shouldn't have errors when assigned code is valid", false, emailField.DescriptionInfo.HasError("Please select one of the predefined fields."));
			AssertEquals("Code couldn't be set when code is assigned a valid value:", "Test Code", emailField.Code);
			AssertEquals("Description couldn't be set when code is assigned a valid value:", "Test Description", emailField.Description);
		}

		public void TestCodeAssignment_DuplicateCode()
		{
			BizObj.Code = Core.Constants.EmailFormat.EmailFieldCodes.CompanyBrandName;
			EmailField newField = BizObj.ParentCollectionInternal.AddNew();

			newField.Code = Core.Constants.EmailFormat.EmailFieldCodes.CompanyBrandName;

			AssertEquals("Code should have errors", true, newField.CodeInfo.HasError("Each field can only appear once in this list. Please select one that does not already exist."));
			AssertEquals("Description should have errors when assigned code appears more than once", true, newField.DescriptionInfo.HasError("Each field can only appear once in this list. Please select one that does not already exist."));
		}

		// It's not possible to have two EmailField objects, and only later on assign them codes, because SetIndex() will have an exception. This is a gotcha for test development.
		public void TestCodeAssignment_CanHandleMultipleCodeAssignments_Pending()
		{
			AssertExceptionThrown(
				"Pending test fixed",
				typeof(ArgumentOutOfRangeException),
				"Specified argument was out of the range of valid values.",
				() =>
				{
					var newField = BizObj.ParentCollectionInternal.AddNew();
					BizObj.Code = Core.Constants.EmailFormat.EmailFieldCodes.CompanyBrandName;
					newField.Code = Core.Constants.EmailFormat.EmailFieldCodes.CompanyBrandName;
				},
				true
			);
		}

		public void TestCodeAssignment_Empty()
		{
			BizObj.Code = ZString.Empty;

			AssertEquals("Code should have errors", true, BizObj.CodeInfo.HasError("Please select one of the predefined fields."));
			AssertEquals("Description should have errors when assigned code is empty", true, BizObj.DescriptionInfo.HasError("Please select one of the predefined fields."));
		}

		public void TestDescriptionAssignment_UnknownDescription()
		{
			var emailField = new EmailFieldDummy();
			emailField.EmailFieldsPairList.AddPairIfNotExist("Test Code", "Test Description");

			emailField.Description = "Some Wrong Description";

			AssertEquals("Code should have errors when assigned description is not in the list", true, emailField.CodeInfo.HasError("Please select one of the predefined fields."));
			AssertEquals("Description should have errors when assigned description is not in the list", true, emailField.DescriptionInfo.HasError("Please select one of the predefined fields."));
		}

		public void TestDescriptionAssignment_KnownDescription()
		{
			var emailField = new EmailFieldDummy();
			emailField.EmailFieldsPairList.AddPairIfNotExist("Test Code", "Test Description");

			emailField.Description = "Test Description";

			AssertEquals("Code shouldn't have errors when assigned description is one that has been added to the email field pair list", false, emailField.CodeInfo.HasError("Please select one of the predefined fields."));
			AssertEquals("Description shouldn't have errors when assigned description is one that has been added to the email field pair list", false, emailField.DescriptionInfo.HasError("Please select one of the predefined fields."));
			AssertEquals("Code couldn't be set when assigned description is valid", "Test Code", emailField.Code);
			AssertEquals("Description couldn't be set when assigned description is valid", "Test Description", emailField.Description);
		}

		public void TestDescriptionAssignment_DuplicateDescription()
		{
			BizObj.Description = Core.Constants.EmailFormat.EmailFieldDescriptions.CompanyBrandName;
			var newField = BizObj.ParentCollectionInternal.AddNew();

			newField.Description = Core.Constants.EmailFormat.EmailFieldDescriptions.CompanyBrandName;

			AssertEquals("Code should have errors when assigned description appears more than once", true, newField.CodeInfo.HasError("Each field can only appear once in this list. Please select one that does not already exist."));
			AssertEquals("Description should have errors when assigned description appears more than once", true, newField.DescriptionInfo.HasError("Each field can only appear once in this list. Please select one that does not already exist."));
		}

		public void TestDescriptionAssignment_Empty()
		{
			BizObj.Description = ZString.Empty;

			AssertEquals("Code should have errors when assigned description is empty", true, BizObj.CodeInfo.HasError("Please select one of the predefined fields."));
			AssertEquals("Description should have errors when assigned description is empty", true, BizObj.DescriptionInfo.HasError("Please select one of the predefined fields."));
		}

		#region Overrides

		protected override void SetUp()
		{
			base.SetUp();
			SetParentCollection();
		}

		protected virtual void SetParentCollection()
		{
			EmailFieldCollection collection = new EmailFieldCollection();
			collection.Add(BizObj);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return new EmailField();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new EmailField();
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresFactory
		{
			get
			{
				return false;
			}
		}

		protected new EmailField BizObj
		{
			get
			{
				return (EmailField)base.BizObj;
			}
		}
		#endregion

		[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
		class EmailFieldDummy : EmailField
		{
			public EmailFieldDummy() : base() { }

			public override CodeDescriptionPairList EmailFieldsPairList
			{
				get { return emailFieldsPairList ?? (emailFieldsPairList = base.EmailFieldsPairList); }
			}
			CodeDescriptionPairList emailFieldsPairList;
		}
	}
}
