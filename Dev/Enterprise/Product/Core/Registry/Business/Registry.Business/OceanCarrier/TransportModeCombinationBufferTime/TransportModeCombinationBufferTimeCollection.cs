using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class TransportModeCombinationBufferTimeCollection : RegistryBusinessObjectCollectionTemplate<TransportModeCombinationBufferTime>
	{
		public TransportModeCombinationBufferTimeCollection() : base(null, null)
		{
		}

		public TransportModeCombinationBufferTimeCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowSort => false;

		protected override bool AllowRemoveCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TransportModeCombinationBufferTime();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TransportModeCombinationBufferTimeCollection();
		}

		#region DefaultValue
		public static TransportModeCombinationBufferTimeCollection DefaultValue
		{
			get
			{
				var collection = new TransportModeCombinationBufferTimeCollection();
				var transportModes = new[]
				{
					Core.Constants.TransportModes.Sea,
					Core.Constants.TransportModes.Air,
					Core.Constants.TransportModes.Road,
					Core.Constants.TransportModes.Rail,
					Core.Constants.TransportModes.InlandWaterwayTransport
				};

				foreach (var loadTransportMode in transportModes)
				{
					foreach (var unloadTransportMode in transportModes)
					{
						collection.Add(new TransportModeCombinationBufferTime
						{
							LoadTransportMode = loadTransportMode,
							UnloadTransportMode = unloadTransportMode,
							BufferTimeInHours = 24
						});
					}
				}

				return collection;
			}
		}
		#endregion
	}
}
