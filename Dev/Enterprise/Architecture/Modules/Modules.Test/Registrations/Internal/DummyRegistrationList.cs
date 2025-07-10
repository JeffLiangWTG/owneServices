namespace Enterprise.ZArchitecture.Modules.Testing
{
	public sealed class DummyRegistrationList : RegistrationList<RegistrationIdentifier, RegistrationInfo>
	{
		public RegistrationInfo DummyAUCustomsInfo = new RegistrationInfo(DummyRegistrationIDs.DummyCustoms, "Enterprise.ZArchitecture.Modules.Testing", "Enterprise.ZArchitecture.Modules.Testing.DummyAUCustoms", "AU");
		public RegistrationInfo DummySGCustomsInfo = new RegistrationInfo(DummyRegistrationIDs.DummyCustoms, "Enterprise.ZArchitecture.Modules.Testing", "Enterprise.ZArchitecture.Modules.Testing.DummySGCustoms", "SG");
		public RegistrationInfo DummyCustomsInfo = new RegistrationInfo(DummyRegistrationIDs.DummyCustoms, "Enterprise.ZArchitecture.Modules.Testing", "Enterprise.ZArchitecture.Modules.Testing.DummyCustoms");
		public RegistrationInfo DummyShipmentInfo = new RegistrationInfo(DummyRegistrationIDs.DummyShipment, "Enterprise.ZArchitecture.Modules.Testing", "Enterprise.ZArchitecture.Modules.Testing.DummyShipment");
		public RegistrationInfo DummyNoSuchTypeInfo = new RegistrationInfo(DummyRegistrationIDs.DummyNoSuchType, "Enterprise.ZArchitecture.NoSuchAssembly", "Enterprise.ZArchitecture.Modules.Testing.NoSuchType");

		public DummyRegistrationList()
		{
			Add(DummyAUCustomsInfo);
			Add(DummySGCustomsInfo);
			Add(DummyCustomsInfo);
			Add(DummyShipmentInfo);
			Add(DummyNoSuchTypeInfo);
		}

		public new void Add(RegistrationInfo info)
		{
			base.Add(info);
		}
	}
}
