namespace Enterprise.Client.EDI.Licencing.Business
{
	using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
	using Enterprise.Licensing;

	public interface ISystemUpdatePacketSender
	{
		void Send(LicenceDatabase db, SystemUpdatePacket packet);
	}
}

