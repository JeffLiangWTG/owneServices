using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.DE.NCTS.Business.DocumentWrappers.Testing
{
	sealed class NctsHeaderForDocumentWrapperTest : NctsHeader
	{
		public NctsHeaderForDocumentWrapperTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString GetFallbackInformationCore() => "Tomorrow is New Year's Eve.";

		public override ZBool FallBackIsActive => IsFallBackActiveForTest;

		public ZBool IsFallBackActiveForTest { get; set; }

		protected override AutologState AutoLoggingState => AutologState.NotLogged;
	}
}
