using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.SCP
{
	public class DocSkyLiftRunDocsTest : ClientSpecificRunDocsTest
	{
		public DocSkyLiftRunDocsTest() : base()
		{
		}

		public override BusinessContext BusinessContext
		{
			get
			{
				return BusinessContext.Shipment;
			}
		}

		protected override ZString ClientName
		{
			get
			{
				return "SCP";
			}
		}

		protected ZQuery fFilterForMenuItem;
		public override ZQuery FilterForMenuItem
		{
			get
			{
				return fFilterForMenuItem;
			}

			set
			{
				fFilterForMenuItem = value;
			}
		}

		[ExpectNoExceptions()]
		public void TestSkyLiftHAWB()
		{
			fFilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "SkyLift HAWB");
			RunDocument();
		}
	}
}
