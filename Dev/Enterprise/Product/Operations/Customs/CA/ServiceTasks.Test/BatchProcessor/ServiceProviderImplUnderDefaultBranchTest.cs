using System;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.NUnit;

namespace Enterprise.Customs.CA.ServiceTasks.Testing
{
	sealed class ServiceProviderImplUnderDefaultBranchTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetDefaultBranchForServiceTasks()
		{
			var auCompany = Factory.New<GlbCompany>();
			auCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			auCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			auCompany.GC_Code = "ZAU";
			var auBranch = auCompany.Branches.AddNew();
			auBranch.GB_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			auBranch.GB_Code = "ZAU";

			var caCompany = Factory.New<GlbCompany>();
			caCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			caCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			caCompany.GC_Code = "ZCA";
			var caBranch = caCompany.Branches.AddNew();
			caBranch.GB_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			caBranch.GB_Code = "ZCA";
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var serviceTask = new ServiceProviderImplUnderDefaultBranchForTest();
				serviceTask.ServiceLogger = new TestServiceLogger();

				using (CACustomsDataRegistry.Instance.DefaultBranchForServiceTasks.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, auBranch.PK.ToGuid()))
				{
					NUnit.Framework.Assert.That(serviceTask.GetDefaultBranchForServiceTasksExposedForTest(Factory).GB_Code, NUnit.Framework.Is.EqualTo(auBranch.GB_Code), "Returned branch is AU from Registry");

					auBranch.GB_IsActive = false;
					Factory.Save();

					NUnit.Framework.Assert.That(serviceTask.GetDefaultBranchForServiceTasksExposedForTest(Factory).Company.GC_RN_NKCountryCode, NUnit.Framework.Is.EqualTo(Core.Constants.CountryCodes.Canada).Using(CustomComparers.TypeComparison), "Returned branch is CA from All Companies");
				}

				using (CACustomsDataRegistry.Instance.DefaultBranchForServiceTasks.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.NewGuid()))
				{
					NUnit.Framework.Assert.That(serviceTask.GetDefaultBranchForServiceTasksExposedForTest(Factory).Company.GC_RN_NKCountryCode, NUnit.Framework.Is.EqualTo(Core.Constants.CountryCodes.Canada).Using(CustomComparers.TypeComparison), "Returned branch is CA from All Companies");
				}

				using (CACustomsDataRegistry.Instance.DefaultBranchForServiceTasks.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty))
				{
					NUnit.Framework.Assert.That(serviceTask.GetDefaultBranchForServiceTasksExposedForTest(Factory).Company.GC_RN_NKCountryCode, NUnit.Framework.Is.EqualTo(Core.Constants.CountryCodes.Canada).Using(CustomComparers.TypeComparison), "Returned branch is CA from All Companies");

					caBranch.GB_IsActive = false;
					Factory.Save();

					NUnit.Framework.Assert.That(serviceTask.GetDefaultBranchForServiceTasksExposedForTest(Factory), NUnit.Framework.Is.Not.EqualTo(default(GlbBranch)), "Returned branch is valid - should not be [null]");
				}
			}
		}

		sealed class ServiceProviderImplUnderDefaultBranchForTest : ServiceProviderImplUnderDefaultBranch
		{
			protected override void RunTaskMain(CancellationToken token) { }

			internal GlbBranch GetDefaultBranchForServiceTasksExposedForTest(BusinessObjectFactory factory) => GetDefaultBranchForServiceTasks(factory);
		}
	}
}
