using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgMatchings.Converters
{
	/// <summary>
	/// Convert an Entity Object to OrgHeaderForMatching Object
	/// </summary>
	public class EntityToOrgMatchingConverter : IConverter<IEntity, IOrgHeaderForMatching>
	{
		public EntityToOrgMatchingConverter(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		#region IConverter<IEntity,IOrgHeaderForMatching> Members

		public IOrgHeaderForMatching Convert(IEntity entity)
		{
			if (entity.TableName != "OrgHeader")
			{
				return null;
			}

			var result = new OrgHeaderForMatching(factory);

			ConvertProperties(result, entity);
			ConvertAddresses(result, entity);
			ConvertCustomCodes(result, entity);
			ConvertBrands(result, entity);
			return result;
		}

		#endregion

		void ConvertProperties(OrgHeaderForMatching result, IEntity entity)
		{
			foreach (var property in entity.Properties)
			{
				property.AssignedTo(result);
			}
		}

		void ConvertAddresses(IOrgHeaderForMatching result, IEntity entity)
		{
			var addressEntities = entity.Children.Where(e => e.EntityName == "OrgAddress");
			var converter = new EntityToOrgAddressMatchingConverter();

			foreach (Entity addressEntity in addressEntities)
			{
				var address = result.AddNewAddress();
				converter.Convert(address, addressEntity);
			}
			// Should do futher investigation for MainAddress
			var addresses = result.Addresses.OfType<OrgAddressForMatching>();

			if (!addresses.Any() || addresses.Any(a => a.IsMainAddress))
			{
				return;
			}

			var firstAddress = addresses.First();
			firstAddress.SetMainAddress();
		}

		void ConvertCustomCodes(IOrgHeaderForMatching result, IEntity entity)
		{
			var cusCodeEntities = entity.Children.Where(e => e.EntityName == "OrgCusCode");
			var converter = new EntityToOrgCusCodeMatchingConverter();

			foreach (Entity cusCodeEntity in cusCodeEntities)
			{
				var cusCode = new OrgCusCodeForMatching();
				converter.Convert(cusCode, cusCodeEntity);
				var codeCountryEntity = cusCodeEntity.ParentCollection.FirstOrDefault(e => e.EntityName == "CodeCountry");
				if (codeCountryEntity != null)
				{
					cusCode.OK_RN_NKCodeCountry = (string)codeCountryEntity["Code"];
				}

				result.CustomsCodes.Add(cusCode);
			}
		}

		void ConvertBrands(OrgHeaderForMatching result, IEntity entity)
		{
			var brandEntities = entity.Children.Where(e => e.EntityName == "OrgBrandOrRelatedName");
			var converter = new EntityToOrgBrandMatchingConverter();

			foreach (Entity brandEntity in brandEntities)
			{
				var brand = new OrgBrandOrRelatedNameForMatching();
				converter.Convert(brand, brandEntity);
				result.Brands.Add(brand);
			}
		}
	}
}
