using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public enum Module
	{
		Freight,
		Shipping
	}

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class DefaultNumberOfDecimalsCollection : RegistryBusinessObjectCollectionTemplate
	{
		public DefaultNumberOfDecimalsCollection()
			: this(Module.Freight)
		{ }

		public DefaultNumberOfDecimalsCollection(Module module)
			: base()
		{
			this.Module = module;
		}

		public Module Module { get; internal set; }

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			if (Module == Module.Shipping)
			{
				var defaultNumberOfDecimals = (DefaultNumberOfDecimals)child;
				defaultNumberOfDecimals.TransportMode = Core.Constants.TransportModes.Sea;
			}
		}

		public new DefaultNumberOfDecimals this[int i]
		{
			get { return (DefaultNumberOfDecimals)Elements[i]; }
		}

		public new DefaultNumberOfDecimals AddNew()
		{
			return (DefaultNumberOfDecimals)base.AddNew();
		}

		protected override bool AllowNewCore
		{
			get { return true; }
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DefaultNumberOfDecimalsCollection(Module);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DefaultNumberOfDecimals();
		}

		public bool IsDuplicateItem(DefaultNumberOfDecimals itemToCheck)
		{
			return (itemToCheck != null)
				&& (this.Cast<DefaultNumberOfDecimals>()
					.Any((item) => item != itemToCheck
						&& item.TransportMode == itemToCheck.TransportMode
						&& item.UnitOfMeasure == itemToCheck.UnitOfMeasure));
		}

		#region Find

		public int GetNumberOfDecimals(ZString transportMode, ZString unitOfMeasure)
		{
			int result = -1;

			var defaultNumberOfDecimals = GetDefaultValueForModeAndUnit(transportMode, unitOfMeasure);

			if (defaultNumberOfDecimals != null)
			{
				result = defaultNumberOfDecimals.NumberOfDecimals;
			}
			else if (Core.Constants.Weight.ContainsCode(unitOfMeasure) || Core.Constants.Volume.ContainsCode(unitOfMeasure))
			{
				result = DefaultNumberOfDecimals.Schema.DefaultNumberOfDecimalsForWeightAndVolumeUnits;
			}

			return result;
		}

		public ZString GetRoundingMode(ZString transportMode, ZString unitOfMeasure)
		{
			ZString result;

			var defaultNumberOfDecimals = GetDefaultValueForModeAndUnit(transportMode, unitOfMeasure);

			if (defaultNumberOfDecimals != null)
			{
				result = defaultNumberOfDecimals.RoundingMode;
			}
			else
			{
				result = RoundingModes.BankersRounding;
			}

			return result;
		}

		public DefaultNumberOfDecimals GetDefaultValueForModeAndUnit(ZString transportMode, ZString unitOfMeasure)
		{
			return this.Cast<DefaultNumberOfDecimals>()
				.FirstOrDefault((item) => item.TransportMode == transportMode
					&& item.UnitOfMeasure == unitOfMeasure);
		}

		#endregion
	}
}
