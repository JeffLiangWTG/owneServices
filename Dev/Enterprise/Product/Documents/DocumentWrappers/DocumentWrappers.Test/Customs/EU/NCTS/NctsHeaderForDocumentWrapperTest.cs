using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS.Testing
{
	sealed class NctsHeaderForDocumentWrapperTest : NctsHeader
	{
		public NctsHeaderForDocumentWrapperTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString GetFallbackInformationCore()
		{
			return "Tomorrow is New Year's Eve.";
		}

		public override ZBool FallBackIsActive => IsFallBackActiveForTest;

		public ZBool IsFallBackActiveForTest
		{
			get
			{
				return fallBackIsActive;
			}

			set
			{
				fallBackIsActive = value;
			}
		}
		ZBool fallBackIsActive;
	}
}
