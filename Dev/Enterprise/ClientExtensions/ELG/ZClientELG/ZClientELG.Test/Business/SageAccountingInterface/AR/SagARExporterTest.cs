using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ClientSharedComponents;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.ELG.Testing
{
	[TestedType(typeof(SagARExporter))]
	public class SagARExporterTest : SagAccountsExporterTest
	{
		public void TestHumanReadableName()
		{
			SagARExporterForTest localExporter = new SagARExporterForTest(0, Factory, Notifications);
			AssertEquals("Human Readable Name", "Sage Accounts Receivable Data Export", localExporter.HumanReadableName);
		}

		protected override AccountsExporter Exporter
		{
			get
			{
				return (SagARExporter)GetNewBusinessObject();
			}
		}

		protected override string FileNamePrefix
		{
			get
			{
				return "AR_" + GlbBranch.CurrentBranch.Company.GC_Code;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SagARExporter(0, Factory, Notifications);
		}

		class SagARExporterForTest : SagARExporter
		{
			public SagARExporterForTest(int batchNumber, BusinessObjectFactory factory, NotificationBuffer notifications) : base(batchNumber, factory, notifications)
			{
			}

			public new ZString HumanReadableNameCore
			{
				get
				{
					return base.HumanReadableName;
				}
			}
		}
	}
}
