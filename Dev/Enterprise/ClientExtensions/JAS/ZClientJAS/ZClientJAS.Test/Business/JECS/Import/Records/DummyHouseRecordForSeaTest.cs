using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class DummyHouseRecordForSeaTest : DummyHouseRecordTestCase
	{
		protected override ZString ExpectedLineType
		{
			get
			{
				return JXCConstants.LineTypes.DOHB;
			}
		}
	}
}
