using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IdentityApplication.Business.Testing
{
	[TestedType(typeof(EdiIdentityApplicationValidation))]
	class EdiIdentityApplicationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateApplicationName()
		{
			var app = Factory.New<EdiIdentityApplication>();
			app.IDA_ApplicationName = string.Empty;
			Assert("application should have validation error", app.IDA_ApplicationNameInfo.HasErrors());
			Assert(app.IDA_ApplicationNameInfo.GetErrors().Contains("Please enter an Application Name."));

			app.IDA_ApplicationName = "Test App";
			Assert("application should not have validation error", !app.IDA_ApplicationNameInfo.HasErrors());
		}

		public void TestValidateApplicationType()
		{
			var app = Factory.New<EdiIdentityApplication>();
			app.IDA_ApplicationType = string.Empty;
			Assert("application should not have validation error", !app.IDA_ApplicationTypeInfo.HasErrors());

			app.IDA_ApplicationType = "XXX";
			Assert("application should have validation error", app.IDA_ApplicationTypeInfo.HasErrors());
			Assert(app.IDA_ApplicationTypeInfo.GetErrors().Contains("Enter a valid selection."));

			app.IDA_ApplicationType = "TST";
			Assert("application should not have validation error", !app.IDA_ApplicationTypeInfo.HasErrors());
		}

		public void TestValidateProduct()
		{
			var app = Factory.New<EdiIdentityApplication>();
			app.IDA_Product = string.Empty;
			Assert("application should not have validation error", !app.IDA_ProductInfo.HasErrors());

			app.IDA_Product = "XXX";
			Assert("application should have validation error", app.IDA_ProductInfo.HasErrors());
			Assert(app.IDA_ProductInfo.GetErrors().Contains("Enter a valid selection."));

			app.IDA_Product = "CW1";
			Assert("application should not have validation error", app.IDA_ProductInfo.HasErrors());
			Assert(app.IDA_ProductInfo.GetErrors().Contains("You must have an active license when selecting a CargoWise product."));

			app.IDA_LD = ZGuid.BrettsGuid;
			app.Validation.ValidateIDA_Product();
			Assert("application should not have validation error", !app.IDA_ProductInfo.HasErrors());
		}

		public void TestCustomerApplicationShouldHaveEitherParentApplicationOrParentOrg()
		{
			var application = Factory.New<EdiIdentityApplication>();
			application.IsCustomerApplication = true;
			application.Validation.ValidateAll();
			Assert(application.IDA_IDA_ParentApplicationInfo.HasError("Please enter a Parent Application."));
			Assert(application.IDA_OH_ParentOrgInfo.HasError("Please enter a Parent Org.."));

			var nonCW1App = Factory.New<EdiIdentityApplication>();
			application.IDA_IDA_ParentApplication = nonCW1App.PK;
			application.Validation.ValidateAll();
			Assert(application.IDA_IDA_ParentApplicationInfo.HasError("Please enter a CW1 Application."));
			Assert(!application.IDA_OH_ParentOrgInfo.HasErrors());

			var cw1App = Factory.New<EdiIdentityApplication>();
			cw1App.IDA_LD = Factory.New<LicenceDatabase>().PK;
			application.IDA_IDA_ParentApplication = cw1App.PK;
			application.Validation.ValidateAll();
			Assert(!application.IDA_IDA_ParentApplicationInfo.HasErrors());
			Assert(!application.IDA_OH_ParentOrgInfo.HasErrors());

			var orgHeader = Factory.New<OrgHeader>();
			application.IDA_IDA_ParentApplication = ZGuid.Empty;
			application.IDA_OH_ParentOrg = orgHeader.PK;
			application.Validation.ValidateAll();
			Assert(!application.IDA_IDA_ParentApplicationInfo.HasErrors());
			Assert(!application.IDA_OH_ParentOrgInfo.HasErrors());
		}
	}
}
