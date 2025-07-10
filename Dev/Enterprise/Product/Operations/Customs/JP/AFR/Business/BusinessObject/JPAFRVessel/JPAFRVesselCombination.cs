using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class JPAFRVesselCombination
	{
		public JPAFRVesselCombination(BusinessObjectFactory factory, ZPropertyInfo nameInfo, ZPropertyInfo callSignInfo, ZPropertyInfo countryInfo)
		{
			this.factory = factory;
			this.nameInfo = nameInfo;
			this.callSignInfo = callSignInfo;
			this.countryInfo = countryInfo;
		}

		public void DefaultCallSignAndNationalityIfNeeded(ZString name)
		{
			var vesselsWithName = factory.Load<RefVessel>(new ZQuery(RefVesselSchema.RV_Code, name));
			if (vesselsWithName.Length == 1)
			{
				var vessel = vesselsWithName[0];
				CallSign = vessel.RV_RadioCallSign;
				Country = vessel.RV_RN_NKCountryOfReg;
			}
		}

		public void OnVesselSelected(RefVessel vessel)
		{
			Name = vessel.RV_Code;
			CallSign = vessel.RV_RadioCallSign;
			Country = vessel.RV_RN_NKCountryOfReg;
		}

		readonly BusinessObjectFactory factory;
		readonly ZPropertyInfo nameInfo;
		readonly ZPropertyInfo callSignInfo;
		readonly ZPropertyInfo countryInfo;

		ZString Name
		{
			get => (ZString)nameInfo.Value;
			set => nameInfo.Value = value;
		}

		ZString CallSign
		{
			get => (ZString)callSignInfo.Value;
			set => callSignInfo.Value = value;
		}

		ZString Country
		{
			get => (ZString)countryInfo.Value;
			set => countryInfo.Value = value;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Cache Key")]
		public RefVessel Vessel
		{
			get
			{
				return factory.GetCachedValue($"JPAFRVesselCombination.Vessel.{Name}.{CallSign}.{Country}", () =>
				{
					var query = new ZQuery(RefVesselSchema.RV_Code, Name);
					query.AddToFilter(RefVesselSchema.RV_RadioCallSign, CallSign);
					query.AddToFilter(RefVesselSchema.RV_RN_NKCountryOfReg, Country);
					return factory.LoadTop1<RefVessel>(query);
				});
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Cache Key")]
		public RefVesselCollection Vessels
		{
			get
			{
				return factory.GetCachedValue($"JPAFRVesselCombination.Vessels.{Name}.{CallSign}.{Country}", () =>
				{
					var result = new RefVesselCollection(factory);
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(RefVesselCollection.FilterConstants.VesselName, "Property", Name));
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(RefVesselCollection.FilterConstants.RadioCallSign, "Property", CallSign));
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(RefVesselCollection.FilterConstants.CountryOfRegistration, "Property", Country));
					return result;
				});
			}
		}
	}
}
