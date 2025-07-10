using CargoWise.Types;

namespace Enterprise.Customs.ES.Manifest.H7.Business.BusinessObjects.Interfaces
{
	public interface IH7CommonMessageSendingObject
	{
		public ZString Action { get; set; }
		public AsycudaBill Bill { get; }
	}
}
