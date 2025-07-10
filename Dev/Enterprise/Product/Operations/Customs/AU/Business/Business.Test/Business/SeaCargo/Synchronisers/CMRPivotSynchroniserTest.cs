using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRPivotSynchroniserTest : PivotSynchroniserTest
	{
		#region Implementation

		protected override void SetUp()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			base.SetUp();
			expectedUnpackedOrPackedCode = CMRPackageTypes.Codes.UnpackedOrPacked;
		}

		protected override SeaCargoSynchroniser GetSeaCargoSynchroniser(ForwardingConsol consol)
		{
			return new CMRSeaCargoSynchroniser(consol);
		}

		protected override PivotSynchroniser GetPivotSynchroniser(CusSCAPivot pivot, PackLine packLine)
		{
			return new CMRPivotSynchroniser(null, pivot, packLine, packLine.Shipment);
		}

		#endregion
	}
}
