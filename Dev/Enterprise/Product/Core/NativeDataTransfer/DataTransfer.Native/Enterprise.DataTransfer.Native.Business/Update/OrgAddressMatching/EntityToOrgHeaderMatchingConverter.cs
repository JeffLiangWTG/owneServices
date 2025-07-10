using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Business.Update.OrgMatchings.Converters;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgAddressMatching.Converters
{
	/// <summary>
	/// Convert an Entity Object to OrgHeaderForMatching Object
	/// </summary>
	public class EntityToOrgHeaderMatchingConverter : IConverter<IEntity, IOrgHeaderForMatching>
	{
		public EntityToOrgHeaderMatchingConverter(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
		}
		readonly BusinessObjectFactory factory;

		#region IConverter<IEntity, IOrgHeaderForMatching> Members

		public IOrgHeaderForMatching Convert(IEntity entity)
		{
			if (entity.TableName != "OrgAddress")
			{
				return null;
			}

			var result = new OrgHeaderForMatching(factory);

			var orgHeaderEntity = entity.Parents.FirstOrDefault(p => p.EntityName == "OrgHeader");
			if (orgHeaderEntity != null)
			{
				result.OH_Code = (string)orgHeaderEntity["Code"];
			}

			var address = (OrgAddressForMatching)(result.Addresses.FirstOrDefault() ?? result.AddNewAddress());
			ConvertProperties(address, entity);
			ConvertCustomCodes(result, entity);

			return result;
		}

		#endregion

		void ConvertProperties(OrgAddressForMatching result, IEntity entity)
		{
			foreach (var property in entity.Properties)
			{
				property.AssignedTo(result);
			}
		}

		void ConvertCustomCodes(IOrgHeaderForMatching result, IEntity entity)
		{
			var cusCodeEntites = entity.Children.Where(e => e.EntityName == "OrgCusCode");
			if (cusCodeEntites.Any())
			{
				var converter = new EntityToOrgCusCodeMatchingConverter();
				foreach (Entity cusCodeEntity in cusCodeEntites)
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
		}
	}
}
