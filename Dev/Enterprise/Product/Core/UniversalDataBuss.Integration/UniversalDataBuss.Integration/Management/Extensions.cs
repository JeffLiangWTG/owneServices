using System;

namespace Enterprise.UniversalDataBuss.Integration
{
	public static class Extensions
	{
		public static T GetAttribute<T>(this object parent) where T : Attribute
		{
			if (parent == null)
			{
				throw new ArgumentNullException("object parent");
			}

			var parentType = (parent as Type) ?? parent.GetType();

			var attributes = parentType.GetCustomAttributes(typeof(T), true);
			if (attributes.Length > 1)
			{
				throw new InvalidOperationException(string.Format("There should only ever be one {0} applied in an inheritance chain. Error on Type: {1}", typeof(T).Name, parentType.FullName));
			}
			if (attributes.Length == 1)
			{
				return ((T)attributes[0]);
			}

			return null;
		}

		public static bool EqualsAny<T>(this T valueToCompare, params object[] comparedValues)
		{
			foreach (var comparedValue in comparedValues)
			{
				if (valueToCompare.Equals(comparedValue))
				{
					return true;
				}
			}

			return false;
		}

		public static bool HasRecipientRoleDetail(this IDataWritingManager manager, RecipientRoleType type, ServiceCodeType? serviceCode = null)
		{
			return manager.GetRecipientRoleDetail(type, serviceCode).HasValue;
		}

		public static RecipientRoleDetail? GetRecipientRoleDetail(this IDataWritingManager manager, RecipientRoleType type, ServiceCodeType? serviceCode = null)
		{
			RecipientRoleDetail? result = null;
			var recipientRoleDetails = manager?.Action?.RecipientRoleDetails;
			if (recipientRoleDetails != null)
			{
				foreach (var recipientRoleDetail in recipientRoleDetails)
				{
					if (recipientRoleDetail.Type == type && (!serviceCode.HasValue || (recipientRoleDetail.ServiceCode.HasValue && recipientRoleDetail.ServiceCode.Value == serviceCode)))
					{
						result = recipientRoleDetail;
						break;
					}
				}
			}
			return result;
		}
	}
}
