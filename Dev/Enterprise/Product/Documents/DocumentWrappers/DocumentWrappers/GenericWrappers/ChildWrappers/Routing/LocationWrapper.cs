using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("UNLOCOAndPortName")]
	public class LocationWrapper : GenericWrapper
	{
		public LocationWrapper(ZString uNLOCO, BusinessObjectFactory factory)
			: base(new RefUNLOCO.Loader(factory).Load(uNLOCO), factory)
		{
			this.UNLOCO = uNLOCO;
		}

		public ZString PortName
		{
			get { return WrappedBO == null ? UNLOCO : WrappedBO.RL_PortName; }
		}

		public ZString UNLOCO { get; }

		public ZString State
		{
			get { return WrappedBO == null ? ZString.Empty : WrappedBO.StateDescription; }
		}

		public ZString IATACode
		{
			get { return WrappedBO == null ? UNLOCO.SubstringSafe(2, 3) : WrappedBO.RL_IATA; }
		}

		public ZString UNLOCOAndPortName
		{
			get { return GetCombinedValue(UNLOCO, PortName); }
		}

		public CountryWrapper Country
		{
			get { return new CountryWrapper(UNLOCO.SubstringSafe(0, 2), Factory); }
		}

		protected new RefUNLOCO WrappedBO
		{
			get { return (RefUNLOCO)base.WrappedBO; }
		}
	}
}
