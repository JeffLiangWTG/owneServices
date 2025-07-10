using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Customs.ServiceTasks;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.IT.ServiceTasks.Testing;

abstract class MessagingServiceTaskTest<TServiceTask> : ServiceTaskTestCase<TServiceTask> where TServiceTask : MessagingServiceTask, new()
{
	public void TestSingleHostedServiceAttribute()
	{
		var hostedServiceAttributes = GetHostedServiceAttributes();
		AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
		var uniqueAttribute = hostedServiceAttributes.Single();

		CombineAssertions(() =>
		{
			AssertEquals("Category", MessagingServiceTask.MessageServiceTaskCategory, uniqueAttribute.Category);
			AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Italy, uniqueAttribute.RequiresCompanyInCountry);
			Assert("CanRunInAnyBranch", uniqueAttribute.CanRunInAnyBranch);
			AssertEquals("AllowsMultipleInstances", false, uniqueAttribute.AllowsMultipleInstances);
			AssertSpecificHostedServiceAttributeProperties(uniqueAttribute);
		});
	}

	public void TestCompanyBranchBecomeInactiveDuringProcessing()
	{
		var company = Factory.New<GlbCompany>();
		company.GC_Code = "CUS";
		company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
		company.GC_IsActive = true;
		var branch = company.Branches.AddNew();
		branch.GB_Code = "BUS";
		branch.GB_IsActive = true;
		Factory.Save();

		var serviceTask = new TServiceTask();
		ErrorReporter.Clear();

		CombineAssertions(() =>
		{
			InitialiseAndRunTaskSchedule(serviceTask);
			AssertEquals("should no error reported when branch is active both in memory and Db", 0, ErrorReporter.TotalErrorCount);

			Db.Connection.ExecuteNonQuery($"UPDATE dbo.GlbBranch SET GB_IsActive = 0, GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GETUTCDATE() WHERE GB_PK = '{branch.PK}'");

			InitialiseAndRunTaskSchedule(serviceTask);
			AssertEquals("should no error reported when branch is active in memory but is inactive in Db", 0, ErrorReporter.TotalErrorCount);
		});
	}

	public void TestIsRequiredHostedServiceRequirement()
	{
		const string thereAreNoValidCompanyCertificatesMessage = "There are no valid company certificates configured in Italy.";

		var isRequiredMethod = typeof(TServiceTask).GetMethod("IsRequired", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
		AssertNotNull("'IsRequired' public static method", isRequiredMethod);

		var hostedServiceRequirementAttribute = isRequiredMethod.GetCustomAttribute<HostedServiceRequirementAttribute>();
		AssertNotNull("'HostedServiceRequirementAttribute' attribute on 'IsRequired' method", hostedServiceRequirementAttribute);

		CertificateRequirementChecker.ResetForTesting();
		AssertEquals("When there are no company certificates, IsRequired", thereAreNoValidCompanyCertificatesMessage, isRequiredMethod.Invoke(obj: null, parameters: null));

		var companyWrapper = Business.GlbCompanyWrapper.Get(GlbCompany.CurrentCompany);
		var companyCredential = companyWrapper.PasswordCollection.AddNew();
		companyCredential.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
		companyCredential.Factory.Save();
		CertificateRequirementChecker.ResetForTesting();
		AssertEquals("When there are no valid company certificates, IsRequired", thereAreNoValidCompanyCertificatesMessage, isRequiredMethod.Invoke(obj: null, parameters: null));

		companyCredential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
		companyCredential.Factory.Save();
		CertificateRequirementChecker.ResetForTesting();
		AssertEquals("When there is a valid company certificates, IsRequired", "", isRequiredMethod.Invoke(obj: null, parameters: null));
	}

	protected abstract void AssertSpecificHostedServiceAttributeProperties(HostedServiceAttribute attribute);
}
