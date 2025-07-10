using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Integration.Customs.AU;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineDeliveryInstructions : DeliveryInstructions, IQuarantineDeliveryInstructions
	{
		public QuarantineDeliveryInstructions()
			: base()
		{
		}

		public QuarantineDeliveryInstructions(DocumentPack pack)
			: base(pack)
		{
		}

		protected override MasterFiles.Business.DocDeliveryContactCollection GetNewDocDeliveryContactCollection(IStmMenuItem menuItem)
		{
			return new DocDeliveryContactCollection(menuItem, null, Factory);
		}
	}
}
