using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	abstract class CusAuthorizationsUsageLookupsAbstractTest<T> : BusinessObjectLookupsTestCase where T : CusAuthorizationUsageLookups
	{
		[ExpectNoExceptions]
		public void TestCodeList()
		{
			var provider = Customs.Business.CusAuthorisationHeaderProvider.GetByCountryCode(Core.Constants.CountryCodes.EuropeanUnion);
			NUnit.Framework.Assert.That(((CodeDescriptionPairList)lookups.CodeList).CodesAsString, NUnit.Framework.Is.EqualTo(provider.GetAuthorisationTypeList(Factory).CodesAsString));
		}

		[ExpectNoExceptions]
		public void TestCodeList_JobDeclarationDoesNotExist()
		{
			NUnit.Framework.Assert.That(((CodeDescriptionPairList)Factory.New<CusAuthorizationUsage>().Lookups.CodeList).Count, NUnit.Framework.Is.EqualTo(0));
		}

		[ExpectNoExceptions]
		public void TestCodeList_HasSpecificProvider()
		{
			var mockedCusAuthorisationHeaderProviderObjectHandle = new Hashtable { { Core.Constants.CountryCodes.Latvia, new CusAuthorisationHeaderProviderObjectHandleForTest() } };
			using (ObjectFactory.Substitute("CusAuthorisationHeaderProviders", mockedCusAuthorisationHeaderProviderObjectHandle))
			{
				NUnit.Framework.Assert.That(((CodeDescriptionPairList)lookups.CodeList).CodesAsString, NUnit.Framework.Is.EqualTo("XXX, YYY"));
			}
		}

		[ExpectNoExceptions]
		public void TestCodeList_HasSpecificProvider_UCC6()
		{
			jobDeclaration.Delete();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: grouping);
			helper.CreateCusCodeType("AUTH", "Authorisation");
			helper.CreateCusCodeList("EUN", "AUTH", "SAS", "Self-Assessment", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeList("EUN", "AUTH", "DPO", "Deferred", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, "AUTH", "OTH", "Other", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			jobDeclaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(jobDeclaration, "IsUCC6Core", true))
			{
				NUnit.Framework.Assert.That(jobDeclaration.IsUCC6, NUnit.Framework.Is.EqualTo(true), "Prerequisite");
				lookups = GetLookups();
				var mockedCusAuthorisationHeaderProviderObjectHandle = new Hashtable { { Core.Constants.CountryCodes.Latvia, new CusAuthorisationHeaderProviderObjectHandleForTest() } };
				using (ObjectFactory.Substitute("CusAuthorisationHeaderProviders", mockedCusAuthorisationHeaderProviderObjectHandle))
				{
					NUnit.Framework.Assert.That(((CodeDescriptionPairList)lookups.CodeList).CodesAsString, NUnit.Framework.Is.EqualTo("DPO, OTH, SAS"));
				}
			}
		}

		[ExpectNoExceptions]
		public void TestJobDeclaration()
		{
			var cusAuthorizationUsageNotAssingned = Factory.New<CusAuthorizationUsage>();
			NUnit.Framework.Assert.That(cusAuthorizationUsageNotAssingned.Lookups.JobDeclaration, NUnit.Framework.Is.EqualTo(default(JobDeclaration)), "jobDeclaration should be null  as cusAuthorizationUsage has not yet been set. - should be [null]");
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
			var entryInsCusAuthorizationUsage = jobDeclaration.CustomsEntryInstructions.AddNew().CusAuthorizationUsages.AddNew();
			NUnit.Framework.Assert.That(entryInsCusAuthorizationUsage.Lookups.JobDeclaration, NUnit.Framework.Is.EqualTo(jobDeclaration), "The correct jobDeclaration from CustomsEntryInstructions should be linked");
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
			var invoiceLineCusAuthorizationUsage = jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew().CusAuthorizationUsages.AddNew();
			NUnit.Framework.Assert.That(invoiceLineCusAuthorizationUsage.Lookups.JobDeclaration, NUnit.Framework.Is.EqualTo(jobDeclaration), "The correct jobDeclaration from InvoiceLines should be linked");
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
			lookups = GetLookups();
			cusAuthorizationUsage = lookups.Parent;
		}
		protected JobDeclaration jobDeclaration;
		protected CusAuthorizationUsage cusAuthorizationUsage;
		protected T lookups;

		protected abstract T GetLookups();
	}

	class CusAuthorisationHeaderProviderObjectHandleForTest : ObjectHandle
	{
		public override object GetObject(params object[] arguments) => new CusAuthorisationHeaderProviderForTest(arguments[0].ToString());
	}

	class CusAuthorisationHeaderProviderForTest : CusAuthorisationHeaderProvider
	{
		public CusAuthorisationHeaderProviderForTest(ZString dataGroupingCode) : base(dataGroupingCode)
		{
		}

		protected override CodeDescriptionPairList GetAuthorisationTypeListCore(BusinessObjectFactory factory)
		{
			var result = new CodeDescriptionPairList();
			result.AddPair("XXX");
			result.AddPair("YYY");
			return result;
		}
	}
}
