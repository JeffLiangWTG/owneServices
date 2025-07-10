using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocCountryData : DocBaseWrapper
	{
		DocCountryData(OrgCountryData countryData, BusinessObjectFactory factoryForWrapper)
			: base(countryData, factoryForWrapper)
		{
		}

		public static DocCountryData New(OrgCountryData countryData, BusinessObjectFactory factoryForWrapper)
		{
			if (countryData == null)
			{
				return null;
			}
			else
			{
				return new DocCountryData(countryData, factoryForWrapper);
			}
		}

		public ZString ExportPermissionDetails
		{
			get { return countryData.OV_EXExportPermissionDetails; }
		}

		OrgCountryData countryData
		{
			get { return (OrgCountryData)WrappedObject; }
		}
	}
}
