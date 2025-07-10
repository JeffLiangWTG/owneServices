using CargoWise.Common;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CL.Manifest.Business
{
	internal class BLSendChileWrapperLocation : IDocumentLocation
	{
		internal BLSendChileWrapperLocation(AsycudaBill bill, ZString locationName)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
			Name = locationName;
		}
		readonly AsycudaBill bill;

		IDocumentLocation documentLocation => this;

		public string Name { get; }

		string IDocumentLocation.Code => BLSendChileHelper.GetLocationCode(bill, documentLocation.Name);

		string IDocumentLocation.Description => bill.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, documentLocation.Code)?.RL_PortName.ToString() ?? ZString.Empty;
	}
}
