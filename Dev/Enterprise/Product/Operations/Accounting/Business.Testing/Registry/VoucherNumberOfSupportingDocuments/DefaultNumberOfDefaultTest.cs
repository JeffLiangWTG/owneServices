using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(DefaultNumberOfSupportingDocuments))]
	public class DefaultNumberOfDefaultTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateDescription()
		{
			BizObj.RunPreSaveValidation();
			BizObj.Description = (NoResString)"General Journal";
			AssertEquals("Description should not have any errors", false, BizObj.DescriptionInfo.HasErrors());
		}

		public void TestValidateNumberOfDefault()
		{
			BizObj.RunPreSaveValidation();
			AssertEquals("NumberOfDefault should not have any errors", false, BizObj.NumberOfDefaultInfo.HasErrors());
			BizObj.NumberOfDefault = 1;
			AssertEquals("NumberOfDefault should not have any errors", false, BizObj.NumberOfDefaultInfo.HasErrors());
		}

		public void TestNumberOfDefaultVisible()
		{
			DefaultNumberOfSupportingDocumentsCollection settings = new DefaultNumberOfSupportingDocumentsCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Guid.Empty), Factory) { BizObj };
			DefaultNumberOfSupportingDocuments bizObj = settings.AddNew();
			ZString ccode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.China;
			Assert("NumberOfDefaultVisible should be true", bizObj.NumberOfDefaultVisible);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Australia;
			Assert("NumberOfDefaultVisible should be false", !bizObj.NumberOfDefaultVisible);

			GlbCompany fNonCurrentCompany = Factory.NewWithValidTestData<GlbCompany>();
			fNonCurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Australia;
			Factory.Save();
			bizObj.CurrentFallbackLevel = new FallbackLevel(fNonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			Assert("NumberOfDefaultVisible should be false", !bizObj.NumberOfDefaultVisible);

			fNonCurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.China;

			Assert("NumberOfDefaultVisible should be true", bizObj.NumberOfDefaultVisible);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = ccode;
		}
		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new DefaultNumberOfSupportingDocuments(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Guid.Empty), Factory);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			DefaultNumberOfSupportingDocuments bizObj = new DefaultNumberOfSupportingDocuments();
			bizObj.Code = "GLJNL";
			bizObj.DefaultDescription = (NoResString)"Number of Documents for GL";
			bizObj.Description = (NoResString)"Number of Documents for GL";
			bizObj.NumberOfDefault = 1;

			return bizObj;
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DefaultNumberOfSupportingDocuments();
		}

		protected new DefaultNumberOfSupportingDocuments BizObj
		{
			get { return (DefaultNumberOfSupportingDocuments)base.BizObj; }
		}

		#endregion
	}
}
