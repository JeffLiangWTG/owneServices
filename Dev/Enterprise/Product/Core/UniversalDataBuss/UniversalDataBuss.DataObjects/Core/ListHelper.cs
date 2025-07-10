using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public static class ListHelper
	{
		public static string GetDescription(ZString code, ICodeDescriptionPairList list)
		{
			return code.IsEmpty ? null : list.GetDescriptionFromCode(code);
		}

		public static T GetWithDescription<T>(ZString code, IFindBoxListProvider list) where T : class, ICodeDescriptionDataObject, new()
		{
			var result = new T() { Code = code };
			string description = list == null ? null : list.DescriptionFromCode(code);
			if (description != null)
			{
				result.Description = description;
			}
			return result;
		}

		public static T GetWithDescription<T>(ZString code, ICodeDescriptionPairList list) where T : class, ICodeDescriptionDataObject, new()
		{
			var result = new T() { Code = code };
			string description = list == null ? null : list.GetDescriptionFromCode(code);
			if (description != null)
			{
				result.Description = description;
			}
			return result;
		}

		public static T GetWithDescription<T>(ZGuid pk, IFindBoxListProvider list) where T : class, ICodeDescriptionDataObject, new()
		{
			T result = new T();
			if (!pk.IsEmpty)
			{
				ZString code = list.CodeFromPrimaryKey(pk);
				if (!code.IsEmpty)
				{
					result.Code = code;
					string description = list == null ? null : list.DescriptionFromPrimaryKey(pk);
					if (description != null)
					{
						result.Description = description;
					}
				}
			}
			return result;
		}

		public static T GetWithName<T>(ZString code, IFindBoxListProvider list) where T : class, ICodeNameDataObject, new()
		{
			var result = new T() { Code = code };
			string name = list == null ? null : list.DescriptionFromCode(code);
			if (name != null)
			{
				result.Name = name;
			}
			return result;
		}

		public static UNLOCO GetWithName(ZString code, IFindBoxListProvider list)
		{
			var result = new UNLOCO() { Code = code };
			string name = list == null ? null : list.DescriptionFromCode(code);
			if (name != null)
			{
				result.Name = name;
			}
			return result;
		}
	}
}