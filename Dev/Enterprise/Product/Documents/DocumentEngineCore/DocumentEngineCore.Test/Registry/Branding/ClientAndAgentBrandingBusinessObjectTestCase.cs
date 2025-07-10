using System;
using System.Drawing;
using System.IO;
using CargoWise.ComponentModel;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestsSubclassesOf(typeof(ClientAndAgentBrandingBusinessObject))]
	public abstract class ClientAndAgentBrandingBusinessObjectTestCase : RegistryBusinessObjectTestCaseBase
	{
		protected abstract byte[] GetSerializedValueThatDoesNotHaveReplaceDomain();

		public void TestSerializationGetsReplaceDomainPropertiesValue()
		{
			var bizo = (ClientAndAgentBrandingBusinessObject)GetBusinessObjectToSerialise();

			bizo.ReplaceDomainNames = false;
			bizo = SertializeAndDeserialize(bizo);

			AssertEquals(false, bizo.ReplaceDomainNames);

			bizo.ReplaceDomainNames = true;
			bizo = SertializeAndDeserialize(bizo);

			AssertEquals(true, bizo.ReplaceDomainNames);
		}

		public void TestWhenReplaceDomainNamesDoesntExistInXmlItReturnsTrue()
		{
			using (var memoryStream = new MemoryStream(GetSerializedValueThatDoesNotHaveReplaceDomain()))
			{
				var serializer = ZXmlSerializer.New(GetExpectedBusinessObjectType());
				var bizo = (ClientAndAgentBrandingBusinessObject)serializer.Deserialize(memoryStream);

				Assert("In existing serializations it should default to true", bizo.ReplaceDomainNames);
			}
		}

		ClientAndAgentBrandingBusinessObject SertializeAndDeserialize(ClientAndAgentBrandingBusinessObject obj)
		{
			var serializer = ZXmlSerializer.New(GetExpectedBusinessObjectType());

			using (var memoryStream = new MemoryStream())
			{
				serializer.Serialize(memoryStream, obj);
				memoryStream.Seek(0, SeekOrigin.Begin);
				return (ClientAndAgentBrandingBusinessObject)serializer.Deserialize(memoryStream);
			}
		}

		public void TestBrandingOptionTitle()
		{
			AssertEquals("BrandingOptionTitle", ExpectedBrandingRegistryItemName, BizObj.BrandingOptionTitleInternal);
		}

		public void TestBrandingOptionRegistryItem()
		{
			AssertEquals("BrandingOptionRegistryItem", ExpectedEnableBrandingRegistryItem, BizObj.BrandingOptionRegistryItemInternal);
		}

		public virtual void TestValidateCode_CheckImageIsNotEmpty()
		{
			string errorMessage = "Please select an Image.";

			AssertEquals("Precondition: Row should not have errors.", false, BizObj.HasRowErrors);
			BizObj.Image = null;
			BizObj.Code = "ABC";
			AssertEquals("Row should have an error if Image is empty.", true, BizObj.RowErrors.Contains(errorMessage));

			BizObj.Image = new Bitmap(1, 1);
			AssertEquals("Row should not have errors.", false, BizObj.RowErrors.Contains(errorMessage));
		}

		public void TestValidateImageMaxSize()
		{
			var bizObj = new ClientAndAgentBrandingBusinessObjectTest.DummyClientAndAgentBrandingBusinessObject();
			string errorMessage = "Image can't be more than 50 Mb";
			AssertEquals("Precondition: Row should not have errors.", false, bizObj.HasRowErrors);
			using (var bm = new Bitmap(500, 500))
			{
				bizObj.Image = bm;
				bizObj.Code = "ABC";
				AssertEquals("Row should have an error if Image size > maxSize.", true, bizObj.RowErrors.Contains(errorMessage));
			}
			using (var bm = new Bitmap(1, 1))
			{
				bizObj.Image = bm;
				AssertEquals("Row should not have errors.", false, bizObj.RowErrors.Contains(errorMessage));
			}
		}

		public virtual void TestValidateCode_CodeIsInList()
		{
			AssertEquals("Precondition: CodeList should not contain \"!@#\"", false, BizObj.CodeList.ContainsCode("!@#"));
			AssertEquals("Precondition: CodeList should not contain \"XYZ\"", false, BizObj.CodeList.ContainsCode("XYZ"));

			try
			{
				BizObj.CodeList.AddPair("XYZ", "");

				BizObj.Image = new Bitmap(1, 2);
				BizObj.CodeInfo.ClearAllNotifications();

				AssertEquals("Precondition: Code should not have errors", false, BizObj.CodeInfo.HasErrors());

				BizObj.Code = "!@#";
				AssertEquals("Code should have an error if it is invalid", true, BizObj.CodeInfo.HasError("Enter a valid selection."));

				BizObj.Code = "XYZ";
				AssertEquals("Code should not have an error if it is valid", false, BizObj.CodeInfo.HasErrors());
			}
			finally
			{
				BizObj.CodeList.RemoveCode("XYZ");
			}
		}

		public void TestValidateCode_CheckBrandingOptionIsEnabled()
		{
			string errorMessage = string.Format("{0} is currently disabled. Please enable {0} first before modifying this Registry Item.", BizObj.BrandingOptionTitleInternal);
			Guid newGuid = Guid.NewGuid();

			BizObj.CurrentFallbackLevel = new FallbackLevel(newGuid, Guid.Empty, Guid.Empty);

			AssertEquals("Precondition: Row should not have Branding Option disabled error.", false, BizObj.RowErrors.Contains(errorMessage));

			BizObj.Code = "ABC";
			BizObj.ValidateRow();

			if (ExpectedEnableBrandingRegistryItem == null)
			{
				AssertEquals("Row should still not have an error as there is no option to check", false, BizObj.RowErrors.Contains(errorMessage));
			}
			else
			{
				AssertEquals("Row should have an error if Branding Option is not enabled.", true, BizObj.RowErrors.Contains(errorMessage));

				BizObj.BrandingOptionRegistryItemInternal.SetValue(newGuid, Guid.Empty, Guid.Empty, true);
				BizObj.Code = "XYZ";
				BizObj.ValidateRow();
				AssertEquals("Row should not have Branding Option disabled error.", false, BizObj.RowErrors.Contains(errorMessage));
			}
		}

		public void TestCopyValuesToCloneDoesNotRunsImageValidation()
		{
			const string errorMessage = "Image can't be more than 50 Mb";

			ClientAndAgentBrandingBusinessObjectTest.DummyClientAndAgentBrandingBusinessObject original = new ClientAndAgentBrandingBusinessObjectTest.DummyClientAndAgentBrandingBusinessObject();
			AssertEquals("Precondition: Row should not have errors.", false, original.HasRowErrors);

			using (Bitmap bitmap = new Bitmap(500, 500))
			{
				original.Image = bitmap;
				original.Code = "ABC";

				AssertEquals("Row should have an error if Image size > maxSize.", true, original.RowErrors.Contains(errorMessage));

				ClientAndAgentBrandingBusinessObjectTest.DummyClientAndAgentBrandingBusinessObject clone = new ClientAndAgentBrandingBusinessObjectTest.DummyClientAndAgentBrandingBusinessObject();
				original.CopyValuesToCloneInternal(clone);

				AssertEquals("Image should be copied to clone", 500, clone.Image.Width);
				AssertEquals("Image should be copied to clone", 500, clone.Image.Height);
				AssertEquals("No image validation should be run.", false, clone.RowErrors.Contains(errorMessage));
			}
		}

		public void TestDescription()
		{
			AssertEquals("Precondition: CodeList should not contain \"ABC\"", false, BizObj.CodeList.ContainsCode("ABC"));
			AssertEquals("Precondition: Description should be empty", true, BizObj.Description.IsEmpty);
			AssertEquals("Description should be ReadOnly", true, BizObj.DescriptionInfo.ReadOnly);

			try
			{
				BizObj.CodeList.AddPair("ABC", "ABC Description");
				BizObj.Code = "ABC";
				AssertEquals("Description", "ABC Description", BizObj.Description);
			}
			finally
			{
				BizObj.CodeList.RemoveCode("ABC");
			}
		}

		#region Implementation

		protected abstract IRegistryItem ExpectedEnableBrandingRegistryItem { get; }
		protected abstract string ExpectedBrandingRegistryItemName { get; }

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			ClientAndAgentBrandingBusinessObject result = (ClientAndAgentBrandingBusinessObject)GetBusinessObjectToSerialise();
			result.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			BizObj.Code = "ABC";
			BizObj.Description = (NoResString)"Desc";
			BizObj.Image = new Bitmap(10, 10);

			return BizObj;
		}

		protected override void CheckAllPropertiesAreEqual(RegistryBusinessObjectTemplate originalBusinessObject, RegistryBusinessObjectTemplate newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);

			ClientAndAgentBrandingBusinessObject original = (ClientAndAgentBrandingBusinessObject)originalBusinessObject;
			ClientAndAgentBrandingBusinessObject @new = (ClientAndAgentBrandingBusinessObject)newBusinessObject;
			AssertEquals("NewBusinessObject.Image should be the same as OriginalBusinessObject.Image", true, Utilities.IsImageEqual(original.Image, @new.Image));
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected new ClientAndAgentBrandingBusinessObject BizObj
		{
			get { return (ClientAndAgentBrandingBusinessObject)base.BizObj; }
		}

		#endregion
	}
}
