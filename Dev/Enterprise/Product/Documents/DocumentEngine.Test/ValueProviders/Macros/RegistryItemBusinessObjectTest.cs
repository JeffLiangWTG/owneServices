using System;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(RegistryItemBusinessObject))]
	sealed class RegistryItemBusinessObjectTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			AssertIsResponsibleForReplacing("<RegistryItemBusinessObject(aaa)>");
			AssertIsResponsibleForReplacing("<registryitembusinessobject(aaa)>");
			AssertIsResponsibleForReplacing("< Registry Item Business Object ( aaa ) >");
			AssertIsResponsibleForReplacing("<RegistryItemBusinessObject(Namespace.ClassName.Instance.Property,AssemblyName).Property>");
			AssertIsResponsibleForReplacing("<RegistryItemBusinessObject(Namespace.ClassName.Instance.Property,AssemblyName).Property.Function(\"text \\\" text\\\\\", crap)>");
			AssertIsResponsibleForReplacing("<RegistryItemBusinessObject(ClassName.Instance.Property).Property>");
			AssertIsResponsibleForReplacing("<RegistryItemBusinessObject(ClassName.Instance.Property).Function(\"text\", more, crap)>");

			AssertNotResponsibleForReplacing("<reg istryitembusinessobject(aaa)>");
			AssertNotResponsibleForReplacing("<RegistryItemBusinessObject>");
			AssertNotResponsibleForReplacing("<>");
		}

		public void TestReplacement()
		{
			var brandValue = new PrincipalBrandingCollection();

			brandValue.Add(new PrincipalBranding()
			{
				Code = "BOB",
				Description = (NoResString)"Bob",
				BrandEmailAddress = "blaticus@friednet.org",
				BrandName = "FriedNet",
				Image = new System.Drawing.Bitmap(1, 1),
			});

			brandValue.Add(new PrincipalBranding()
			{
				Code = "MIK",
				Description = (NoResString)"Bob",
				BrandEmailAddress = "blaticus@friednet.org",
				BrandName = "FriedNet",
				Image = new System.Drawing.Bitmap(1, 1),
			});

			DocumentsDataRegistry.Instance.PrincipalDocumentBrand.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, brandValue);

			PrincipalBrandingCollection collection = (PrincipalBrandingCollection)ValueProviderToTest.GetReplacement("<RegistryItemBusinessObject(DocumentsDataRegistry.Instance.PrincipalDocumentBrand)>", Report);
			AssertEquals("assert return registry value", "BOB", collection[0].Code);
			AssertEquals("assert return registry value", "MIK", collection[1].Code);

			AssertIsReplacedWith("Value via property", null, "<RegistryItemBusinessObject(DocumentsDataRegistry.Instance.Watermark).ImageWatermark>");
			AssertIsReplacedWith("Value via property", "BOB", "<RegistryItemBusinessObject(DocumentsDataRegistry.Instance.PrincipalDocumentBrand).Code>");
			AssertIsReplacedWith("Value via format", "BOB, MIK", "<RegistryItemBusinessObject(Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.PrincipalDocumentBrand,Enterprise.DocumentEngineCore).Format(\"{Code}\", Comma)>");

			AssertEquals("Precondition: Errors", ReportErrorManager.HasNoErrors, Report.ErrorManager.ToString());
			AssertIsReplacedWith(null, "<RegistryItemBusinessObject(DocumentsDataRegistry.Instance.PrincipalDocumentBrand).Blaticus>");
			AssertEquals("Errors", "Severity: [Warning (without error report)] Message: [Error in RegistryItemBusinessObject Macro: Unable To Resolve Path: [.Blaticus]]", Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
		}

		public void TestMaskTheReturnValueWhenUsePasswordRegistryMacrosInRegistryItemBusinessObject()
		{
			RawDataRegistry.Instance.SMTPPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestPassWord");
			AssertIsReplacedWith("EditorType is Password in RegistryItemImpl return ***", "***", "<RegistryItemBusinessObject(Enterprise.ZArchitecture.Environment.RawDataRegistry.Instance.SMTPPassword,Enterprise.ZArchitecture.Core)>");
			RawDataRegistry.Instance.AUCCompanyCertificatePassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestPassWord");
			AssertIsReplacedWith("EditorType is Password in RegistryItemWrapper return ***", "***", "<RegistryItemBusinessObject(Enterprise.Accounting.Registry.Business.AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServicePassword,Enterprise.Accounting.Business)>");
		}

		#region Implementation

		protected override ValueProvider GetNewValueProvider()
		{
			return new RegistryItemBusinessObject();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			var brandValue = new PrincipalBrandingCollection();

			brandValue.Add(new PrincipalBranding()
			{
				Code = "WTG",
				Description = (NoResString)"WiseTech Global",
				BrandEmailAddress = "wtgtest@wisetechglobal.com",
				BrandName = "WiseTech Global",
				Image = new System.Drawing.Bitmap(1, 1),
			});

			brandValue.Add(new PrincipalBranding()
			{
				Code = "XYZ",
				Description = (NoResString)"XYZ",
				BrandEmailAddress = "Colonel.Sanders@xyz.com",
				BrandName = "XYZ",
				Image = new System.Drawing.Bitmap(1, 1),
			});

			DocumentsDataRegistry.Instance.PrincipalDocumentBrand.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, brandValue);
		}

		#endregion
	}
}
