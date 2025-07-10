using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class AvsWebServiceUriCollectionRegistryDataType : NonPersistentBusinessObjectRegistryDataType<AvsWebServiceUriRegistryBusinessObjectCollection>
	{
		protected override void ValidateCore(IRegistryItem registryItem, AvsWebServiceUriRegistryBusinessObjectCollection proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (proposedValue.Primary is null || proposedValue.Secondary is null || proposedValue.Background is null || proposedValue.Count != 3)
			{
				throw new RegistryValidationException(Res.GetString("f1468657-0be2-4179-a544-2b85216437bc", "The number of URIs should be fixed at 3, which are primary, secondary and background URIs."));
			}

			if (proposedValue.OfType<AvsWebServiceUriRegistryBusinessObject>().Any(x => !UrlValidation.IsValidAbsoluteHttpOrHttpsUrl(x.ServiceUri)))
			{
				throw new RegistryValidationException(Res.GetString("e1ebbbff-9841-4926-9b3b-ad8a764ac1c8", "One or more URLs are invalid, please input valid URLs that must be HTTPS or HTTP."));
			}
		}
	}
}
