using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ClientSharedComponents;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.ELG.Testing
{
	[TestedType(typeof(SagAPExporter))]
	public class SagAPExporterTest : SagAccountsExporterTest
	{
		public void TestHumanReadableName()
		{
			SagAPExporterForTest localExporter = new SagAPExporterForTest(0, Factory, Notifications);
			AssertEquals("Human Readable Name", "Sage Accounts Payable Data Export", localExporter.HumanReadableName);
		}

		protected override AccountsExporter Exporter
		{
			get
			{
				return (SagAPExporter)GetNewBusinessObject();
			}
		}

		protected override string FileNamePrefix
		{
			get
			{
				return "AP_" + GlbBranch.CurrentBranch.Company.GC_Code;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SagAPExporter(0, Factory, Notifications);
		}

		class SagAPExporterForTest : SagAPExporter
		{
			public SagAPExporterForTest(int batchNumber, BusinessObjectFactory factory, NotificationBuffer notifications) : base(batchNumber, factory, notifications)
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
