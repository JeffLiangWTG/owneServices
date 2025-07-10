using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.ClientSharedComponents.ServiceTasks.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.TEL.ServiceTasks.Testing
{
	[TestedType(typeof(TELConsolShipServiceTask))]
	public class TELConsolShipServiceTaskTest : MailDataImportServiceTaskTestCase<TELConsolShipServiceTask>
	{
		public void TestMinimumPeriod()
		{
			AssertEquals("30seconds", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		EmbeddedResourceRetriever resourceRetriever;
		protected override void SetupValidEnvironment()
		{
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			TELDataRegistry.Instance.ConsolShipManifestEmailSubjectIdentifierItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "bob the builder");
			GlbGroup notificationGroup = Factory.New<GlbGroup>();
			notificationGroup.GG_Code = "NOT";
			GlbStaff user = notificationGroup.Staff.AddNew();
			user.GS_Code = "BOB";
			user.GS_IsActive = true;
			user.GS_EmailAddress = "test@test.com";
			TELDataRegistry.Instance.ConsolShipImportNotificationGroupPKItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, notificationGroup.PK.ToGuid());
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();
			resourceRetriever.Dispose();
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
		protected override ZString ValidTestFile
		{
			get
			{
				return resourceRetriever.SaveResourceToFile("single.xml");
			}
		}

		protected override TELConsolShipServiceTask ServiceTask
		{
			get
			{
				if (fServiceTask == null)
				{
					fServiceTask = new TELConsolShipServiceTask();
				}

				return fServiceTask;
			}
		}

		TELConsolShipServiceTask fServiceTask;
		protected override ZString SubjectIdentifier
		{
			get
			{
				return TELDataRegistry.Instance.ConsolShipManifestEmailSubjectIdentifier;
			}
		}
	}
}
