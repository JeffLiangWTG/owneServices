using System;
using System.Drawing;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(LocalTransportCompanyBranding))]
	public class LocalTransportCompanyBrandingTest : ClientAndAgentBrandingBusinessObjectTestCase
	{
		#region TestLocalTransportCompanyValidation

		public void TestLocalTransportCompanyValidation()
		{
			BusinessObject company1 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IOrgHeader)));
			company1[OrgHeaderSchema.Constants.OH_Code] = "LOC";
			company1[OrgHeaderSchema.Constants.OH_IsLocalTransport] = true;
			company1[OrgHeaderSchema.Constants.OH_IsShippingProvider] = true;

			BusinessObject company2 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IOrgHeader)));
			company2[OrgHeaderSchema.Constants.OH_Code] = "POC";
			company2[OrgHeaderSchema.Constants.OH_IsLocalTransport] = true;

			Factory.Save();

			var collection = new LocalTransportCompanyBrandingCollection(NewFallBackLevel(), Factory);

			var branding1 = collection.AddNew();
			branding1.LocalTransportCompanyPK = ZGuid.Empty;
			branding1.ValidateLocalTransportCompany();
			AssertHasError(branding1.LocalTransportCompanyPKInfo, "Please enter a Local Transport Company.");

			branding1.LocalTransportCompanyPK = company2.PK;
			AssertNoError(branding1.LocalTransportCompanyPKInfo, "Please enter a Local Transport Company.");
			AssertHasError(branding1.LocalTransportCompanyPKInfo, "Enter a valid Local Transport Company.");

			branding1.LocalTransportCompanyPK = company1.PK;
			AssertNoNotifications(branding1.LocalTransportCompanyPKInfo);

			var branding2 = collection.AddNew();
			branding2.LocalTransportCompanyPK = company1.PK;
			AssertHasError(branding2.LocalTransportCompanyPKInfo, "Local Transport Company already exists in the list.");
		}

		#endregion

		#region TestLabelNameValidation

		public void TestLabelNameValidation()
		{
			var collection = new LocalTransportCompanyBrandingCollection(NewFallBackLevel(), Factory);

			var branding1 = collection.AddNew();
			branding1.LabelName = "";
			branding1.ValidateLabelName();
			AssertHasError(branding1.LabelNameInfo, "Please enter a Label Name.");

			branding1.LabelName = "XYZ";
			AssertNoError(branding1.LabelNameInfo, "Please enter a Label Name.");
			AssertHasError(branding1.LabelNameInfo, "Enter a valid Label Name.");

			branding1.LabelName = LabelNames.StarTrackLabel;
			AssertNoNotifications(branding1.LabelNameInfo);
		}

		#endregion

		#region Implementation

		protected override byte[] GetSerializedValueThatDoesNotHaveReplaceDomain()
		{
			return new byte[] { 60, 63, 120, 109, 108, 32, 118, 101, 114, 115, 105, 111, 110, 61, 34, 49, 46, 48, 34, 63, 62, 13, 10, 60, 76, 111, 99, 97, 108, 84, 114, 97, 110, 115, 112, 111, 114, 116, 67, 111, 109, 112, 97, 110, 121, 66, 114, 97, 110, 100, 105, 110, 103, 62, 13, 10, 32, 32, 60, 73, 109, 97, 103, 101, 62, 82, 48, 108, 71, 79, 68, 108, 104, 67, 103, 65, 75, 65, 80, 99, 65, 65, 65, 65, 65, 65, 65, 65, 65, 77, 119, 65, 65, 90, 103, 65, 65, 109, 81, 65, 65, 122, 65, 65, 65, 47, 119, 65, 114, 65, 65, 65, 114, 77, 119, 65, 114, 90, 103, 65, 114, 109, 81, 65, 114, 122, 65, 65, 114, 47, 119, 66, 86, 65, 65, 66, 86, 77, 119, 66, 86, 90, 103, 66, 86, 109, 81, 66, 86, 122, 65, 66, 86, 47, 119, 67, 65, 65, 65, 67, 65, 77, 119, 67, 65, 90, 103, 67, 65, 109, 81, 67, 65, 122, 65, 67, 65, 47, 119, 67, 113, 65, 65, 67, 113, 77, 119, 67, 113, 90, 103, 67, 113, 109, 81, 67, 113, 122, 65, 67, 113, 47, 119, 68, 86, 65, 65, 68, 86, 77, 119, 68, 86, 90, 103, 68, 86, 109, 81, 68, 86, 122, 65, 68, 86, 47, 119, 68, 47, 65, 65, 68, 47, 77, 119, 68, 47, 90, 103, 68, 47, 109, 81, 68, 47, 122, 65, 68, 47, 47, 122, 77, 65, 65, 68, 77, 65, 77, 122, 77, 65, 90, 106, 77, 65, 109, 84, 77, 65, 122, 68, 77, 65, 47, 122, 77, 114, 65, 68, 77, 114, 77, 122, 77, 114, 90, 106, 77, 114, 109, 84, 77, 114, 122, 68, 77, 114, 47, 122, 78, 86, 65, 68, 78, 86, 77, 122, 78, 86, 90, 106, 78, 86, 109, 84, 78, 86, 122, 68, 78, 86, 47, 122, 79, 65, 65, 68, 79, 65, 77, 122, 79, 65, 90, 106, 79, 65, 109, 84, 79, 65, 122, 68, 79, 65, 47, 122, 79, 113, 65, 68, 79, 113, 77, 122, 79, 113, 90, 106, 79, 113, 109, 84, 79, 113, 122, 68, 79, 113, 47, 122, 80, 86, 65, 68, 80, 86, 77, 122, 80, 86, 90, 106, 80, 86, 109, 84, 80, 86, 122, 68, 80, 86, 47, 122, 80, 47, 65, 68, 80, 47, 77, 122, 80, 47, 90, 106, 80, 47, 109, 84, 80, 47, 122, 68, 80, 47, 47, 50, 89, 65, 65, 71, 89, 65, 77, 50, 89, 65, 90, 109, 89, 65, 109, 87, 89, 65, 122, 71, 89, 65, 47, 50, 89, 114, 65, 71, 89, 114, 77, 50, 89, 114, 90, 109, 89, 114, 109, 87, 89, 114, 122, 71, 89, 114, 47, 50, 90, 86, 65, 71, 90, 86, 77, 50, 90, 86, 90, 109, 90, 86, 109, 87, 90, 86, 122, 71, 90, 86, 47, 50, 97, 65, 65, 71, 97, 65, 77, 50, 97, 65, 90, 109, 97, 65, 109, 87, 97, 65, 122, 71, 97, 65, 47, 50, 97, 113, 65, 71, 97, 113, 77, 50, 97, 113, 90, 109, 97, 113, 109, 87, 97, 113, 122, 71, 97, 113, 47, 50, 98, 86, 65, 71, 98, 86, 77, 50, 98, 86, 90, 109, 98, 86, 109, 87, 98, 86, 122, 71, 98, 86, 47, 50, 98, 47, 65, 71, 98, 47, 77, 50, 98, 47, 90, 109, 98, 47, 109, 87, 98, 47, 122, 71, 98, 47, 47, 53, 107, 65, 65, 74, 107, 65, 77, 53, 107, 65, 90, 112, 107, 65, 109, 90, 107, 65, 122, 74, 107, 65, 47, 53, 107, 114, 65, 74, 107, 114, 77, 53, 107, 114, 90, 112, 107, 114, 109, 90, 107, 114, 122, 74, 107, 114, 47, 53, 108, 86, 65, 74, 108, 86, 77, 53, 108, 86, 90, 112, 108, 86, 109, 90, 108, 86, 122, 74, 108, 86, 47, 53, 109, 65, 65, 74, 109, 65, 77, 53, 109, 65, 90, 112, 109, 65, 109, 90, 109, 65, 122, 74, 109, 65, 47, 53, 109, 113, 65, 74, 109, 113, 77, 53, 109, 113, 90, 112, 109, 113, 109, 90, 109, 113, 122, 74, 109, 113, 47, 53, 110, 86, 65, 74, 110, 86, 77, 53, 110, 86, 90, 112, 110, 86, 109, 90, 110, 86, 122, 74, 110, 86, 47, 53, 110, 47, 65, 74, 110, 47, 77, 53, 110, 47, 90, 112, 110, 47, 109, 90, 110, 47, 122, 74, 110, 47, 47, 56, 119, 65, 65, 77, 119, 65, 77, 56, 119, 65, 90, 115, 119, 65, 109, 99, 119, 65, 122, 77, 119, 65, 47, 56, 119, 114, 65, 77, 119, 114, 77, 56, 119, 114, 90, 115, 119, 114, 109, 99, 119, 114, 122, 77, 119, 114, 47, 56, 120, 86, 65, 77, 120, 86, 77, 56, 120, 86, 90, 115, 120, 86, 109, 99, 120, 86, 122, 77, 120, 86, 47, 56, 121, 65, 65, 77, 121, 65, 77, 56, 121, 65, 90, 115, 121, 65, 109, 99, 121, 65, 122, 77, 121, 65, 47, 56, 121, 113, 65, 77, 121, 113, 77, 56, 121, 113, 90, 115, 121, 113, 109, 99, 121, 113, 122, 77, 121, 113, 47, 56, 122, 86, 65, 77, 122, 86, 77, 56, 122, 86, 90, 115, 122, 86, 109, 99, 122, 86, 122, 77, 122, 86, 47, 56, 122, 47, 65, 77, 122, 47, 77, 56, 122, 47, 90, 115, 122, 47, 109, 99, 122, 47, 122, 77, 122, 47, 47, 47, 56, 65, 65, 80, 56, 65, 77, 47, 56, 65, 90, 118, 56, 65, 109, 102, 56, 65, 122, 80, 56, 65, 47, 47, 56, 114, 65, 80, 56, 114, 77, 47, 56, 114, 90, 118, 56, 114, 109, 102, 56, 114, 122, 80, 56, 114, 47, 47, 57, 86, 65, 80, 57, 86, 77, 47, 57, 86, 90, 118, 57, 86, 109, 102, 57, 86, 122, 80, 57, 86, 47, 47, 43, 65, 65, 80, 43, 65, 77, 47, 43, 65, 90, 118, 43, 65, 109, 102, 43, 65, 122, 80, 43, 65, 47, 47, 43, 113, 65, 80, 43, 113, 77, 47, 43, 113, 90, 118, 43, 113, 109, 102, 43, 113, 122, 80, 43, 113, 47, 47, 47, 86, 65, 80, 47, 86, 77, 47, 47, 86, 90, 118, 47, 86, 109, 102, 47, 86, 122, 80, 47, 86, 47, 47, 47, 47, 65, 80, 47, 47, 77, 47, 47, 47, 90, 118, 47, 47, 109, 102, 47, 47, 122, 80, 47, 47, 47, 119, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 67, 72, 53, 66, 65, 69, 65, 65, 80, 119, 65, 76, 65, 65, 65, 65, 65, 65, 75, 65, 65, 111, 65, 65, 65, 103, 83, 65, 65, 69, 73, 72, 69, 105, 119, 111, 77, 71, 68, 67, 66, 77, 113, 88, 77, 105, 119, 89, 99, 75, 65, 65, 68, 115, 61, 60, 47, 73, 109, 97, 103, 101, 62, 13, 10, 32, 32, 60, 76, 111, 99, 97, 108, 84, 114, 97, 110, 115, 112, 111, 114, 116, 67, 111, 109, 112, 97, 110, 121, 80, 75, 62, 97, 51, 48, 97, 101, 102, 100, 51, 45, 97, 54, 97, 55, 45, 52, 56, 50, 102, 45, 56, 102, 53, 57, 45, 102, 101, 56, 51, 48, 53, 98, 100, 102, 101, 51, 48, 60, 47, 76, 111, 99, 97, 108, 84, 114, 97, 110, 115, 112, 111, 114, 116, 67, 111, 109, 112, 97, 110, 121, 80, 75, 62, 13, 10, 32, 32, 60, 76, 97, 98, 101, 108, 78, 97, 109, 101, 62, 68, 69, 76, 60, 47, 76, 97, 98, 101, 108, 78, 97, 109, 101, 62, 13, 10, 60, 47, 76, 111, 99, 97, 108, 84, 114, 97, 110, 115, 112, 111, 114, 116, 67, 111, 109, 112, 97, 110, 121, 66, 114, 97, 110, 100, 105, 110, 103, 62 };
		}

		FallbackLevel NewFallBackLevel()
		{
			return new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
		}

		LocalTransportCompanyBranding GetNewPopulatedBusinessObject()
		{
			BusinessObject company = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IOrgHeader)));
			company[OrgHeaderSchema.Constants.OH_Code] = "TOC";
			company[OrgHeaderSchema.Constants.OH_IsLocalTransport] = true;
			company[OrgHeaderSchema.Constants.OH_IsShippingProvider] = true;

			Factory.Save();

			var collection = new LocalTransportCompanyBrandingCollection(NewFallBackLevel(), Factory);
			var branding = collection.AddNew();
			branding.LocalTransportCompanyPK = company.PK;
			branding.Image = new Bitmap(10, 10);
			return branding;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var collection = new LocalTransportCompanyBrandingCollection(NewFallBackLevel(), Factory);
			var branding = collection.AddNew();
			return branding;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetNewPopulatedBusinessObject();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetNewPopulatedBusinessObject();
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override void CheckAllPropertiesAreEqual(RegistryBusinessObjectTemplate originalBusinessObject, RegistryBusinessObjectTemplate newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);

			var originalBranding = (LocalTransportCompanyBranding)originalBusinessObject;
			var newBranding = (LocalTransportCompanyBranding)newBusinessObject;

			AssertEquals("LocalTransportCompanyPK", originalBranding.LocalTransportCompanyPK, newBranding.LocalTransportCompanyPK);
			AssertEquals("Image", true, Utilities.IsImageEqual(originalBranding.Image, newBranding.Image));
		}

		protected override string ExpectedBrandingRegistryItemName
		{
			get { return ""; }
		}

		protected override IRegistryItem ExpectedEnableBrandingRegistryItem
		{
			get { return null; }
		}

		#region TestValidateCode_CodeIsInList

		public override void TestValidateCode_CodeIsInList()
		{
			AssertEquals("The 'code list' is empty", "", BizObj.CodeList.CodesAsString);
			AssertEquals(false, BizObj.CodeInfo.HasError("Please enter a Code."));
		}

		public override void TestValidateCode_CheckImageIsNotEmpty()
		{
			string errorMessage = "Please select an Image.";

			AssertEquals("Precondition: Row should not have errors.", false, BizObj.HasRowErrors);
			BizObj.Image = null;
			BizObj.Code = "ABC";
			AssertEquals("Row should not have an error if Image is empty for Local Transport Company.", false, BizObj.RowErrors.Contains(errorMessage));
		}

		#endregion

		#endregion
	}
}
