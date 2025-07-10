using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.CustomerService.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.BatchProcessor.Testing
{
	[TestedType(typeof(IncidentEDocLogSubscriber))]
	public class IncidentEDocLogSubscriberTest : LogSubscriberTest<IncidentEDocLogSubscriber>
	{
		public void TestProcessQueuedLogs()
		{
			var webuser = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, User.WebUserCode);
			using (Env.SetTemporaryUserContext(webuser.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
				var reportingContact = lic.Company.Header.Contacts.AddNew();
				reportingContact.OC_ContactName = "Joe";
				reportingContact.OC_Email = "joe@test.org";
				var request1 = Factory.New<IncidentRequest>();
				request1.INC_OC_ReportedBy = reportingContact.PK;
				request1.INC_Criticality = "CR5";
				request1.INC_Details = "how stuff happen?";
				request1.INC_OC_ApprovedBy = reportingContact.PK;
				request1.INC_Summary = "stuff happened";
				request1.INC_Type = "ENT";
				request1.INC_Status = "NEW";
				var request2 = Factory.New<IncidentRequest>();
				request2.INC_OC_ReportedBy = reportingContact.PK;
				request2.INC_Criticality = "CR5";
				request2.INC_Details = "how stuff happen again?";
				request2.INC_OC_ApprovedBy = reportingContact.PK;
				request2.INC_Summary = "stuff happened twice";
				request2.INC_Type = "ENT";
				request2.INC_Status = "NEW";
				request1.DocManagerInfo.UseBusinessEntityFactoryAsInternal = true;
				request2.DocManagerInfo.UseBusinessEntityFactoryAsInternal = true;
				var eDoc1 = (StorageDocsBase)request1.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("Test eDoc"), "stuff.txt", "COR");
				request1.DocManagerInfo.AddLogsForNewDocument(request1, eDoc1);
				var eDoc12 = (StorageDocsBase)request2.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("Test2 eDoc"), "request2.txt", "COR");
				request2.DocManagerInfo.AddLogsForNewDocument(request2, eDoc12);
				var eDoc2 = (StorageDocsBase)request1.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("Test eDoc2"), "stuff2.txt", "COR");
				request1.DocManagerInfo.AddLogsForNewDocument(request1, eDoc2);
				BusinessObjectFactory.SaveTogether(Factory, request1.DocManagerInfo.MasterFactory, request2.DocManagerInfo.MasterFactory);
				var processor = new Mock<ISupportRequestProcessor>();
				ObjectFactory.Substitute(processor.Object);
				processor.Setup(x => x.ProcessNewEdocs(It.IsAny<BusinessObjectFactory>(), request1.PK, It.Is<IEnumerable<ZGuid>>(pks => pks.First() == eDoc1.PK && pks.Last() == eDoc2.PK && pks.Count() == 2)));
				processor.Setup(x => x.ProcessNewEdocs(It.IsAny<BusinessObjectFactory>(), request2.PK, It.Is<IEnumerable<ZGuid>>(pks => pks.First() == eDoc12.PK && pks.Count() == 1)));
				RunLogWalkerCycleForTest();
				processor.VerifyAll();
			}
		}

		public void TestProcessQueuedLogs2()
		{
			GlbStaff webuser = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, User.WebUserCode);
			using (Env.SetTemporaryUserContext(webuser.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
				var reportingContact = lic.Company.Header.Contacts.AddNew();
				reportingContact.OC_ContactName = "Joe";
				reportingContact.OC_Email = "joe@test.org";
				var request1 = Factory.New<IncidentRequest>();
				request1.INC_OC_ReportedBy = reportingContact.PK;
				request1.INC_Criticality = "CR5";
				request1.INC_Details = "how stuff happen?";
				request1.INC_OC_ApprovedBy = reportingContact.PK;
				request1.INC_Summary = "stuff happened";
				request1.INC_Type = "ENT";
				request1.INC_Status = "NEW";
				var request2 = Factory.New<IncidentRequest>();
				request2.INC_OC_ReportedBy = reportingContact.PK;
				request2.INC_Criticality = "CR5";
				request2.INC_Details = "how stuff happen again?";
				request2.INC_OC_ApprovedBy = reportingContact.PK;
				request2.INC_Summary = "stuff happened twice";
				request2.INC_Type = "ENT";
				request2.INC_Status = "NEW";
				request1.DocManagerInfo.UseBusinessEntityFactoryAsInternal = true;
				request2.DocManagerInfo.UseBusinessEntityFactoryAsInternal = true;
				var eDoc1 = (StorageDocsBase)request1.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("Test eDoc"), "stuff.txt", "COR");
				request1.DocManagerInfo.AddLogsForNewDocument(request1, eDoc1);
				var eDoc12 = (StorageDocsBase)request2.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("Test2 eDoc"), "request2.txt", "COR");
				request2.DocManagerInfo.AddLogsForNewDocument(request2, eDoc12);
				var eDoc2 = (StorageDocsBase)request1.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("Test eDoc2"), "stuff2.txt", "COR");
				request1.DocManagerInfo.AddLogsForNewDocument(request1, eDoc2);
				BusinessObjectFactory.SaveTogether(Factory, request1.DocManagerInfo.MasterFactory, request2.DocManagerInfo.MasterFactory);
				using (CreateRealProcessorOverride.TemporarilyOverride())
				{
					RunLogWalkerCycleForTest();
				}

				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		public static class CreateRealProcessorOverride
		{
			public static IDisposable TemporarilyOverride()
			{
				return IncidentEDocLogSubscriber.CreateRealProcessorOverride();
			}
		}
	}
}
