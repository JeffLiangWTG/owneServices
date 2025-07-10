using System;
using System.Drawing;
using CargoWise.Application;
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
	[TestedType(typeof(PrincipalBranding))]
	public class PrincipalBrandingTest : DocumentBrandingBusinessObjectTest
	{
		#region TestValidateCode_CodeIsInList

		public override void TestValidateCode_CodeIsInList()
		{
			AssertEquals("The 'code list' is empty", "", BizObj.CodeList.CodesAsString);

			BizObj.Code = "";
			BizObj.ValidateCode();
			AssertHasError(BizObj.CodeInfo, "Please enter a Code.");

			BizObj.Code = "BLH";
			AssertNoNotifications(BizObj.CodeInfo);
		}

		#endregion

		#region TestPrincipalValidation

		const string NoPrincipal = "Please enter a Principal.";
		const string InvalidPrincipal = "Enter a valid principal.";
		const string DuplicatePrincipal = "A principal may only appear once in this list.";

		public void TestPrincipalValidation()
		{
			AssertNotNull("lazy load", Principal);

			PrincipalBrandingCollection collection = new PrincipalBrandingCollection(Factory, NewFallBackLevel());

			PrincipalBranding branding1 = collection.AddNew();
			branding1.PrincipalPK = ZGuid.Empty;
			branding1.ValidatePrincipalPK();
			AssertHasError(branding1.PrincipalPKInfo, NoPrincipal);

			branding1.PrincipalPK = ZGuid.NewZGuid();
			AssertNoError(branding1.PrincipalPKInfo, NoPrincipal);
			AssertHasError(branding1.PrincipalPKInfo, InvalidPrincipal);

			branding1.PrincipalPK = Principal.PK;
			AssertNoNotifications(branding1.PrincipalPKInfo);

			PrincipalBranding branding2 = collection.AddNew();
			branding2.PrincipalPK = Principal.PK;
			AssertHasError(branding2.PrincipalPKInfo, DuplicatePrincipal);
		}

		#endregion

		#region Implementation

		protected override byte[] GetSerializedValueThatDoesNotHaveReplaceDomain()
		{
			return new byte[] { 60, 63, 120, 109, 108, 32, 118, 101, 114, 115, 105, 111, 110, 61, 34, 49, 46, 48, 34, 63, 62, 13, 10, 60, 80, 114, 105, 110, 99, 105, 112, 97, 108, 66, 114, 97, 110, 100, 105, 110, 103, 62, 13, 10, 32, 32, 60, 67, 111, 100, 101, 77, 97, 120, 76, 101, 110, 103, 116, 104, 62, 51, 60, 47, 67, 111, 100, 101, 77, 97, 120, 76, 101, 110, 103, 116, 104, 62, 13, 10, 32, 32, 60, 67, 111, 100, 101, 62, 66, 108, 104, 60, 47, 67, 111, 100, 101, 62, 13, 10, 32, 32, 60, 68, 101, 115, 99, 114, 105, 112, 116, 105, 111, 110, 62, 78, 80, 65, 74, 60, 47, 68, 101, 115, 99, 114, 105, 112, 116, 105, 111, 110, 62, 13, 10, 32, 32, 60, 73, 109, 97, 103, 101, 62, 82, 48, 108, 71, 79, 68, 108, 104, 67, 103, 65, 75, 65, 80, 99, 65, 65, 65, 65, 65, 65, 65, 65, 65, 77, 119, 65, 65, 90, 103, 65, 65, 109, 81, 65, 65, 122, 65, 65, 65, 47, 119, 65, 114, 65, 65, 65, 114, 77, 119, 65, 114, 90, 103, 65, 114, 109, 81, 65, 114, 122, 65, 65, 114, 47, 119, 66, 86, 65, 65, 66, 86, 77, 119, 66, 86, 90, 103, 66, 86, 109, 81, 66, 86, 122, 65, 66, 86, 47, 119, 67, 65, 65, 65, 67, 65, 77, 119, 67, 65, 90, 103, 67, 65, 109, 81, 67, 65, 122, 65, 67, 65, 47, 119, 67, 113, 65, 65, 67, 113, 77, 119, 67, 113, 90, 103, 67, 113, 109, 81, 67, 113, 122, 65, 67, 113, 47, 119, 68, 86, 65, 65, 68, 86, 77, 119, 68, 86, 90, 103, 68, 86, 109, 81, 68, 86, 122, 65, 68, 86, 47, 119, 68, 47, 65, 65, 68, 47, 77, 119, 68, 47, 90, 103, 68, 47, 109, 81, 68, 47, 122, 65, 68, 47, 47, 122, 77, 65, 65, 68, 77, 65, 77, 122, 77, 65, 90, 106, 77, 65, 109, 84, 77, 65, 122, 68, 77, 65, 47, 122, 77, 114, 65, 68, 77, 114, 77, 122, 77, 114, 90, 106, 77, 114, 109, 84, 77, 114, 122, 68, 77, 114, 47, 122, 78, 86, 65, 68, 78, 86, 77, 122, 78, 86, 90, 106, 78, 86, 109, 84, 78, 86, 122, 68, 78, 86, 47, 122, 79, 65, 65, 68, 79, 65, 77, 122, 79, 65, 90, 106, 79, 65, 109, 84, 79, 65, 122, 68, 79, 65, 47, 122, 79, 113, 65, 68, 79, 113, 77, 122, 79, 113, 90, 106, 79, 113, 109, 84, 79, 113, 122, 68, 79, 113, 47, 122, 80, 86, 65, 68, 80, 86, 77, 122, 80, 86, 90, 106, 80, 86, 109, 84, 80, 86, 122, 68, 80, 86, 47, 122, 80, 47, 65, 68, 80, 47, 77, 122, 80, 47, 90, 106, 80, 47, 109, 84, 80, 47, 122, 68, 80, 47, 47, 50, 89, 65, 65, 71, 89, 65, 77, 50, 89, 65, 90, 109, 89, 65, 109, 87, 89, 65, 122, 71, 89, 65, 47, 50, 89, 114, 65, 71, 89, 114, 77, 50, 89, 114, 90, 109, 89, 114, 109, 87, 89, 114, 122, 71, 89, 114, 47, 50, 90, 86, 65, 71, 90, 86, 77, 50, 90, 86, 90, 109, 90, 86, 109, 87, 90, 86, 122, 71, 90, 86, 47, 50, 97, 65, 65, 71, 97, 65, 77, 50, 97, 65, 90, 109, 97, 65, 109, 87, 97, 65, 122, 71, 97, 65, 47, 50, 97, 113, 65, 71, 97, 113, 77, 50, 97, 113, 90, 109, 97, 113, 109, 87, 97, 113, 122, 71, 97, 113, 47, 50, 98, 86, 65, 71, 98, 86, 77, 50, 98, 86, 90, 109, 98, 86, 109, 87, 98, 86, 122, 71, 98, 86, 47, 50, 98, 47, 65, 71, 98, 47, 77, 50, 98, 47, 90, 109, 98, 47, 109, 87, 98, 47, 122, 71, 98, 47, 47, 53, 107, 65, 65, 74, 107, 65, 77, 53, 107, 65, 90, 112, 107, 65, 109, 90, 107, 65, 122, 74, 107, 65, 47, 53, 107, 114, 65, 74, 107, 114, 77, 53, 107, 114, 90, 112, 107, 114, 109, 90, 107, 114, 122, 74, 107, 114, 47, 53, 108, 86, 65, 74, 108, 86, 77, 53, 108, 86, 90, 112, 108, 86, 109, 90, 108, 86, 122, 74, 108, 86, 47, 53, 109, 65, 65, 74, 109, 65, 77, 53, 109, 65, 90, 112, 109, 65, 109, 90, 109, 65, 122, 74, 109, 65, 47, 53, 109, 113, 65, 74, 109, 113, 77, 53, 109, 113, 90, 112, 109, 113, 109, 90, 109, 113, 122, 74, 109, 113, 47, 53, 110, 86, 65, 74, 110, 86, 77, 53, 110, 86, 90, 112, 110, 86, 109, 90, 110, 86, 122, 74, 110, 86, 47, 53, 110, 47, 65, 74, 110, 47, 77, 53, 110, 47, 90, 112, 110, 47, 109, 90, 110, 47, 122, 74, 110, 47, 47, 56, 119, 65, 65, 77, 119, 65, 77, 56, 119, 65, 90, 115, 119, 65, 109, 99, 119, 65, 122, 77, 119, 65, 47, 56, 119, 114, 65, 77, 119, 114, 77, 56, 119, 114, 90, 115, 119, 114, 109, 99, 119, 114, 122, 77, 119, 114, 47, 56, 120, 86, 65, 77, 120, 86, 77, 56, 120, 86, 90, 115, 120, 86, 109, 99, 120, 86, 122, 77, 120, 86, 47, 56, 121, 65, 65, 77, 121, 65, 77, 56, 121, 65, 90, 115, 121, 65, 109, 99, 121, 65, 122, 77, 121, 65, 47, 56, 121, 113, 65, 77, 121, 113, 77, 56, 121, 113, 90, 115, 121, 113, 109, 99, 121, 113, 122, 77, 121, 113, 47, 56, 122, 86, 65, 77, 122, 86, 77, 56, 122, 86, 90, 115, 122, 86, 109, 99, 122, 86, 122, 77, 122, 86, 47, 56, 122, 47, 65, 77, 122, 47, 77, 56, 122, 47, 90, 115, 122, 47, 109, 99, 122, 47, 122, 77, 122, 47, 47, 47, 56, 65, 65, 80, 56, 65, 77, 47, 56, 65, 90, 118, 56, 65, 109, 102, 56, 65, 122, 80, 56, 65, 47, 47, 56, 114, 65, 80, 56, 114, 77, 47, 56, 114, 90, 118, 56, 114, 109, 102, 56, 114, 122, 80, 56, 114, 47, 47, 57, 86, 65, 80, 57, 86, 77, 47, 57, 86, 90, 118, 57, 86, 109, 102, 57, 86, 122, 80, 57, 86, 47, 47, 43, 65, 65, 80, 43, 65, 77, 47, 43, 65, 90, 118, 43, 65, 109, 102, 43, 65, 122, 80, 43, 65, 47, 47, 43, 113, 65, 80, 43, 113, 77, 47, 43, 113, 90, 118, 43, 113, 109, 102, 43, 113, 122, 80, 43, 113, 47, 47, 47, 86, 65, 80, 47, 86, 77, 47, 47, 86, 90, 118, 47, 86, 109, 102, 47, 86, 122, 80, 47, 86, 47, 47, 47, 47, 65, 80, 47, 47, 77, 47, 47, 47, 90, 118, 47, 47, 109, 102, 47, 47, 122, 80, 47, 47, 47, 119, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 67, 72, 53, 66, 65, 69, 65, 65, 80, 119, 65, 76, 65, 65, 65, 65, 65, 65, 75, 65, 65, 111, 65, 65, 65, 103, 83, 65, 65, 69, 73, 72, 69, 105, 119, 111, 77, 71, 68, 67, 66, 77, 113, 88, 77, 105, 119, 89, 99, 75, 65, 65, 68, 115, 61, 60, 47, 73, 109, 97, 103, 101, 62, 13, 10, 32, 32, 60, 66, 114, 97, 110, 100, 78, 97, 109, 101, 62, 82, 97, 110, 100, 111, 109, 32, 66, 114, 97, 110, 100, 32, 78, 97, 109, 101, 60, 47, 66, 114, 97, 110, 100, 78, 97, 109, 101, 62, 13, 10, 32, 32, 60, 66, 114, 97, 110, 100, 69, 109, 97, 105, 108, 65, 100, 100, 114, 101, 115, 115, 62, 114, 97, 110, 100, 111, 109, 64, 98, 114, 97, 110, 100, 46, 110, 97, 109, 101, 46, 99, 111, 109, 60, 47, 66, 114, 97, 110, 100, 69, 109, 97, 105, 108, 65, 100, 100, 114, 101, 115, 115, 62, 13, 10, 32, 32, 60, 85, 115, 101, 71, 101, 110, 101, 114, 105, 99, 62, 78, 60, 47, 85, 115, 101, 71, 101, 110, 101, 114, 105, 99, 62, 13, 10, 32, 32, 60, 80, 114, 105, 110, 99, 105, 112, 97, 108, 80, 75, 62, 49, 56, 50, 98, 48, 98, 57, 97, 45, 51, 102, 53, 50, 45, 52, 53, 57, 99, 45, 57, 55, 50, 49, 45, 55, 50, 56, 98, 97, 49, 101, 97, 101, 51, 54, 99, 60, 47, 80, 114, 105, 110, 99, 105, 112, 97, 108, 80, 75, 62, 13, 10, 60, 47, 80, 114, 105, 110, 99, 105, 112, 97, 108, 66, 114, 97, 110, 100, 105, 110, 103, 62 };
		}

		#region Principal

		BusinessObject Principal
		{
			get
			{
				if (principal == null)
				{
					principal = CreatePrincipal("Principal1");
				}

				return principal;
			}
		}

		BusinessObject principal;

		BusinessObject CreatePrincipal(ZString code)
		{
			BusinessObjectFactory dirtyFactory = new BusinessObjectFactory();
			BusinessObject principal = dirtyFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(IOrgHeader)));
			principal[OrgHeaderSchema.Constants.OH_Code] = code;
			principal[OrgHeaderSchema.Constants.OH_IsShippingProvider] = true;

			BusinessObject companyData = (BusinessObject)principal["CompanyData"];
			companyData[OrgCompanyDataSchema.Constants.OB_CRIsShipsAgencyPrincipal] = true;
			dirtyFactory.Save();

			return (BusinessObject)Factory.Load<IOrgHeader>(principal.PK);
		}

		#endregion

		FallbackLevel NewFallBackLevel()
		{
			return new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
		}

		PrincipalBranding GetNewPopulatedBusinessObject()
		{
			PrincipalBrandingCollection collection = new PrincipalBrandingCollection(Factory, NewFallBackLevel());
			PrincipalBranding branding = collection.AddNew();
			branding.PrincipalPK = Principal.PK;
			branding.Code = "Blh";
			branding.Description = (NoResString)"NPAJ";
			branding.BrandName = "Random Brand Name";
			branding.BrandEmailAddress = "random@brand.name.com";
			branding.Image = new Bitmap(10, 10);
			return branding;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			PrincipalBrandingCollection collection = new PrincipalBrandingCollection(Factory, NewFallBackLevel());
			PrincipalBranding branding = collection.AddNew();
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

			PrincipalBranding originalBranding = (PrincipalBranding)originalBusinessObject;
			PrincipalBranding newBranding = (PrincipalBranding)newBusinessObject;

			AssertEquals("PrincipalPK", originalBranding.PrincipalPK, newBranding.PrincipalPK);
			AssertEquals("BrandName", originalBranding.BrandName, newBranding.BrandName);
			AssertEquals("BrandEmailAddress", originalBranding.BrandEmailAddress, newBranding.BrandEmailAddress);
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

		#endregion
	}
}
