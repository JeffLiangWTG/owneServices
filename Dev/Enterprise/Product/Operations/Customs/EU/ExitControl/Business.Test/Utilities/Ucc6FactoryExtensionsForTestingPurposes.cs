using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing;

public static class Ucc6FactoryExtensionsForTestingPurposes
{
	public static CusExitHeader GetUcc6ExitHeader(this BusinessObjectFactory factory) => factory.New<CusExitHeaderUcc6ForTest>();

	public static CusExitHeader LoadUcc6ExitHeader(this BusinessObjectFactory factory, CusExitHeader exitHeader) => factory.Load<CusExitHeaderUcc6ForTest>(exitHeader.PK);

	public static CusExitReportItem GetUcc6ExitReportItem(this BusinessObjectFactory factory) => factory.New<CusExitReportItemUcc6ForTest>();

	public static CusExitConsignment GetUcc6ExitConsignment(this BusinessObjectFactory factory) => factory.New<CusExitConsignmentUcc6ForTest>();

	class CusExitHeaderUcc6ForTest : CusExitHeader
	{
		public CusExitHeaderUcc6ForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override bool IsUCC6Core => true;
	}

	class CusExitReportItemUcc6ForTest : CusExitReportItem
	{
		public CusExitReportItemUcc6ForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override bool IsUCC6Core => true;
	}

	class CusExitConsignmentUcc6ForTest : CusExitConsignment
	{
		public CusExitConsignmentUcc6ForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override bool IsUCC6Core => true;
	}
}

