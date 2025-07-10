using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ClientSharedComponents.DataExport.Testing
{
	public class XmlDataExporterTestCase : TestCaseWithFactory
	{
		public void TestExportSendsEmailWhenNotifyHasErrors()
		{
			SetNotificationGroupEmail();
			DummyBusinessObject dummyObject = Factory.New<DummyBusinessObject>();
			ZString fileName = Path.Combine(Env.TempPath, dummyObject.HumanReadableName + ".xml");
			XmlDataExporterForTest exporter = new XmlDataExporterForTest(false);
			try
			{
				exporter.Export(dummyObject, Notify);
				AssertEmailCreated();
			}
			finally
			{
				DeleteIfExists(fileName);
			}
		}

		public void TestExportSendsEmailWhenIOException()
		{
			SetNotificationGroupEmail();
			XmlDataExporterForTest exporter = new XmlDataExporterForTest(true);
			DummyBusinessObject @object = Factory.New<DummyBusinessObject>();
			exporter.Export(@object, Notify);
			AssertEmailCreated();
		}

		void AssertEmailCreated()
		{
			Assert(Env.OutgoingMailManager.EmailsCreated.Count > 0);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Error email Subject", "DummyBizo: Error exporting", email.Subject);
		}

		void SetNotificationGroupEmail()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();
			GlbGroup postmasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			postmasterGroup.Staff[0].GS_EmailAddress = "test@edi.com.au";
			Factory.Save();
		}

		readonly NotificationBuffer Notify = new NotificationBuffer();

		#region XmlDataExporterForTest

		class XmlDataExporterForTest : XmlDataExporter
		{
			public XmlDataExporterForTest(bool withIOException)
			{
				this.WithIOException = withIOException;
			}

			protected override void DoExport(Stream fileName, BusinessObject bizOToExport, INotifications notify)
			{
				notify.Notify(new ErrorNotification(ErrorType.Error, "ErrorMessage"));
			}

			protected override ZString FileNamePostfix
			{
				get { return ""; }
			}

			protected override ZString EmailSubject
			{
				get { return "Error exporting"; }
			}

			protected override IRegistryItem NotificationGroup
			{
				get
				{
					return Env.Registry.RawRegistry.NotificationGroup;
				}
			}

			protected override Guid NotificationGroupGuid
			{
				get
				{
					BusinessObjectFactory factory = new BusinessObjectFactory();
					GlbGroup postmasterGroup = factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
					return postmasterGroup.PK.ToGuid();
				}
			}

			protected override ZString GetBizOReference(BusinessObject bizOToExport)
			{
				return bizOToExport.HumanReadableName;
			}

			protected override IValueObjectDataAdapter GetValueObjectDataAdapter()
			{
				return null;
			}

			protected override ZString ExportDirectory
			{
				get { return (WithIOException) ? Env.TempPath + @"\NonExistentDirectory\A" : Env.TempPath; }
			}

			readonly bool WithIOException;
		}

		#endregion
	}
}
