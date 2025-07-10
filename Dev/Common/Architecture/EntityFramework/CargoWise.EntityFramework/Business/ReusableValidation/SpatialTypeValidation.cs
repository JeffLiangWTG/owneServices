using System.Linq;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class SpatialTypeValidation : ValidationProvider
	{
		public static void ErrorIfSpatialTypeIsNotAllowed(ZPropertyInfo info)
		{
			if (!CheckSpatialTypeInAllowedTypesArray(info))
			{
				info.AddError(GetNotificationMessage(info));
			}
		}

		public static void MessageErrorIfSpatialTypeIsNotAllowed(ZPropertyInfo info)
		{
			if (!CheckSpatialTypeInAllowedTypesArray(info))
			{
				info.AddMessageError(GetNotificationMessage(info));
			}
		}

		public static void WarnIfSpatialTypeIsNotAllowed(ZPropertyInfo info)
		{
			if (!CheckSpatialTypeInAllowedTypesArray(info))
			{
				info.AddWarning(GetNotificationMessage(info));
			}
		}

		static bool CheckSpatialTypeInAllowedTypesArray(ZPropertyInfo info)
		{
			var value = (ZGeography)info.Value;
			if (!TypeValidation.CheckIsValidGeographyValue(value) || value.IsEmpty)
			{
				return true;
			}

			var allowedTypes = (info as ZPropertyInfoGeography)?.AllowedSpatialTypes;
			if (allowedTypes != null)
			{
				var type = GetSpatialType(info);
				return allowedTypes.Contains(type, new ZGeography.SpatialTypeComparer());
			}

			return true;
		}

		static string GetSpatialType(ZPropertyInfo info)
		{
			return ((ZGeography)info.Value).SpatialTypeName;
		}

		public static ZString GetNotificationMessage(ZPropertyInfo info)
		{
			return info.HumanReadableName + " " + Res.GetString("60345445-e843-481a-8507-e67b2d47303b", "does not accept spatial type ") + GetSpatialType(info);
		}
	}
}
