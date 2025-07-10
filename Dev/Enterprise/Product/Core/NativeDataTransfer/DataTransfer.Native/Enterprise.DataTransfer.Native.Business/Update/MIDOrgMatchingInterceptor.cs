using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs.US;

namespace Enterprise.DataTransfer.Native.Business.Update
{
	public abstract class MIDOrgMatchingInterceptor : BaseInterceptor
	{
		protected MIDOrgMatchingInterceptor(IInterceptorSetting setting, AncillaryImportServices sessionServices)
			: base(setting, sessionServices)
		{
			factory = setting.Context.ObjectFactory;
		}
		protected BusinessObjectFactory factory;

		protected OrgAddress CreateMIDOrganizationIfNecessary(IEntity entity)
		{
			OrgAddress manufacturerAddress = null;
			var cusCodeEntities = entity.Children.Where(e => e.EntityName == "OrgCusCode");
			var midCusCode = cusCodeEntities.FirstOrDefault(x => x.Properties.Any(p => p.Name == "CodeType" && p.Value.ToString() == OrgCusCode.USACodeTypes.ManufacturerID) &&
				x.Parents.Any(e => e.EntityName == "CodeCountry" && e.Properties.Any(p => p.Name == "Code" && p.Value.ToString() == Core.Constants.CountryCodes.UnitedStates)));
			if (midCusCode != null)
			{
				var usMid = (ZString)midCusCode.Properties.FirstOrDefault(x => x.Name == "CustomsRegNo")?.Value.ToString();
				if (!usMid.IsEmpty)
				{
					manufacturerAddress = TryCreateMIDOrganization(usMid);
					if (manufacturerAddress != null)
					{
						switch (entity.EntityName)
						{
							case "OrgHeader":
								entity.InternalPK = manufacturerAddress.Header.PK.ToGuid();
								entity["Code"] = manufacturerAddress.Header.OH_Code;
								break;
							default:
								entity.InternalPK = manufacturerAddress.PK.ToGuid();
								entity["Code"] = manufacturerAddress.OA_Code;
								break;
						}
					}
				}
			}

			return manufacturerAddress;
		}

		protected OrgAddress TryCreateMIDOrganization(ZString usMid)
		{
			var manufacturerAddress = ObjectFactory.Get<IMIDOrganisation>().CreateMIDOrganizationIfNecessary(usMid, factory) as OrgAddress;
			if (manufacturerAddress != null)
			{
				sessionServices.Logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "A new MID Organization: {0} was created because no matched Organization was found and 'Registry > Customs > United States of America > Import > Create MID Organization on unmatched import' is on.", manufacturerAddress.Header.OH_Code));

				try
				{
					factory.Save();
				}
				catch (ZSaveException)
				{
					sessionServices.Logger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "There was a problem saving the new MID Organization. This means that it cannot find out the matched manufacturer. You can try to reimport the data."));
				}
			}
			else if (!usMid.IsLettersAndNumbersOnlyOrEmpty)
			{
				sessionServices.Logger.Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture, "There are invalid characters in MID {0}, only alphanumeric characters are allowed.", usMid));
			}

			return manufacturerAddress;
		}
	}
}
