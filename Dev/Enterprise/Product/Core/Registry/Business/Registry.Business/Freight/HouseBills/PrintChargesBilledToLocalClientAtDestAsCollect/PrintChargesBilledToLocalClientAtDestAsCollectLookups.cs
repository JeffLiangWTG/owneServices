using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public sealed class PrintChargesBilledToLocalClientAtDestAsCollectLookups : ZLookups
	{
		public PrintChargesBilledToLocalClientAtDestAsCollectLookups(PrintChargesBilledToLocalClientAtDestAsCollect parent, BusinessObjectFactory currentFactory)
			: base(parent)
		{
			if (currentFactory == null)
			{
				throw new ArgumentNullException(nameof(currentFactory));
			}

			this.currentFactory = currentFactory;
		}

		readonly BusinessObjectFactory currentFactory;

		public CodeDescriptionPairList TransportMode_List
		{
			get
			{
				return currentFactory.GetCachedValue("PrintChargesBilledToLocalClientAtDestAsCollectLookups.TransportMode_List", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea);
					result.AddPair(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air);
					return result;
				});
			}
		}

		public IBusinessObjectCollection CountryCollection
		{
			get
			{
				return currentFactory.GetCachedValue("PrintChargesBilledToLocalClientAtDestAsCollectLookups.CountryCollection", () =>
				{
					return (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IRefCountryCollection>(), currentFactory);
				});
			}
		}
	}
}
