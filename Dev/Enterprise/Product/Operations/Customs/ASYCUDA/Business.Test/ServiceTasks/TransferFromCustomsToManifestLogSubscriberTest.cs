using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(TransferFromCustomsToManifestLogSubscriber))]
	sealed class TransferFromCustomsToManifestLogSubscriberTest : LogSubscriberTest<TransferFromCustomsToManifestLogSubscriber>
	{
		public void TestUxmlCreated()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.Logs.AddNew(Events.TransferFromManifestToCustoms, "M12345");
				declaration.Logs.AddNew(Events.TransferFromCustomsToManifest);
				Factory.Save();

				RunLogWalkerCycleForTest();

				var dexLog = Factory.LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode).AddToFilter(StmALogSchema.SL_Parent, declaration.PK));

				var relatedEDIMessage = dexLog.RelatedEDIMessage;
				var message = relatedEDIMessage.Message;
				var shipment = message.GetEM_MessageTextReader().Parse<Shipment>();
				AssertEquals("M12345", shipment.GetMatchingDataTarget(DataContextType.AsycudaManifest).Key);
				AssertEquals("shipment.DataContext.EventType", Events.TransferFromCustomsToManifestCode, shipment.DataContext.EventType.GetCodeAsUpperCase());
				AssertNull("shipment.DataContext.EventReference", shipment.DataContext.EventReference);
			}
		}

		public void TestUxmlCompanySameAsDeclaration()
		{
			var sgBranch = CreateCompanyAndBranch("SG1", Core.Constants.CountryCodes.Singapore);
			var auBranch = CreateCompanyAndBranch("AU1", Core.Constants.CountryCodes.Australia);
			Factory.Save();

			BaseJobDeclaration declaration;
			using (Environment.DisposableEnvironment.ForBranch(sgBranch.PK.ToGuid()))
			{
				declaration = Factory.New<BaseJobDeclaration>();
				declaration.Logs.AddNew(Events.TransferFromManifestToCustoms, "M12345");
				declaration.Logs.AddNew(Events.TransferFromCustomsToManifest);
				Factory.Save();
			}

			using (Environment.DisposableEnvironment.ForBranch(auBranch.PK.ToGuid()))
			{
				AssertEquals("Precondition declaration.Branch", sgBranch.PK, declaration.Branch.PK);

				RunLogWalkerCycleForTest();

				var dexLog = Factory.LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode).AddToFilter(StmALogSchema.SL_Parent, declaration.PK));

				var message = dexLog.RelatedEDIMessage.Message;
				var shipment = message.GetEM_MessageTextReader().Parse<Shipment>();
				AssertEquals("M12345", shipment.GetMatchingDataTarget(DataContextType.AsycudaManifest).Key);
				AssertEquals("shipment.DataContext.EventType", Events.TransferFromCustomsToManifestCode, shipment.DataContext.EventType.GetCodeAsUpperCase());
				AssertNull("shipment.DataContext.EventReference", shipment.DataContext.EventReference);

				var sgCompany = sgBranch.Company;
				var dataContext = shipment.DataContext as UniversalDataBuss.DataObjects.Universal._2011_11.DataContext;
				AssertEquals("shipment.DataContext.Company.Code", sgCompany.GC_Code, dataContext.Company.Code);
				AssertEquals("shipment.DataContext.Company.Name", sgCompany.GC_Name, dataContext.Company.Name);
				AssertEquals("shipment.DataContext.Company.Country.Code", sgCompany.GC_RN_NKCountryCode, dataContext.Company.Country.Code);
				AssertEquals("shipment.DataContext.Company.Country.Name", "Singapore", dataContext.Company.Country.Name);
				AssertEquals("shipment.DataContext.EventBranch", sgBranch.GB_Code, dataContext.EventBranch.Code);
			}
		}

		GlbBranch CreateCompanyAndBranch(ZString code, ZString countryCode)
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = code;
			company.GC_Name = "TEST COMP " + code;
			company.GC_RN_NKCountryCode = countryCode;
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var branch = company.Branches.AddNew();
			branch.GB_Code = code;
			branch.GB_BranchName = "TEST BRANCH " + code;
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			return branch;
		}
	}
}
