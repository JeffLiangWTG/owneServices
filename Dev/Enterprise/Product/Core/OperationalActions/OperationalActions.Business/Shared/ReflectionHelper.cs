using System;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Services.OperationalActions.Business
{
	public static class ReflectionHelper
	{
		public static string PathToFieldText(PropertyInfo[] path)
		{
			StringBuilder builder = new StringBuilder();
			string joiner = "";

			foreach (PropertyInfo info in path)
			{
				builder.Append(joiner);
				builder.Append(info.Name);
				joiner = ReflectionHelper.Classify(info) == PropertyClassification.FollowSingle ? "+" : ".";
			}

			return builder.ToString();
		}

		public static PropertyInfo[] FieldTextToPath(Type rootType, string fieldText)
		{
			if (rootType == null)
			{
				throw new ArgumentNullException(nameof(rootType));
			}

			if (fieldText == null)
			{
				throw new ArgumentNullException(nameof(fieldText), "fieldText cannot be null");
			}

			string[] path = fieldText.Split('.', '+');

			PropertyInfo[] result = new PropertyInfo[path.Length];
			Type currentType = rootType;
			Type currentCollectionType = null;

			for (int i = 0; i < path.Length; i++)
			{
				if (typeof(DynamicBusinessObjectCollection).IsAssignableFrom(currentType))
				{
					result = null;
					break;
				}

				if (typeof(IBusinessObjectCollection).IsAssignableFrom(currentType))
				{
					currentCollectionType = currentType;
					currentType = BusinessObjectCollection.GetElementTypeFromCollectionType(currentType);
				}

				PropertyInfo info = GetPropertyInfo(currentType, path[i]);

				if (info == null || !CanFollow(info))
				{
					if (currentCollectionType != null)
					{
						info = GetPropertyInfo(currentCollectionType, path[i]);
					}

					if (info == null || !CanFollow(info))
					{
						result = null;
						break;
					}
				}
				else
				{
					currentCollectionType = null;
				}

				result[i] = info;
				currentType = ActionFieldFollowAttribute.GetReturnType(info);
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "RegEx fragment string")]
		public static bool IsUnsafePropertyName(string name)
		{
			const string audit = "(.._System(Create|LastEdit)(TimeUtc|User))";
			const string parent = ".*(Parent|Table|Foreign).*";
			const string unique = ".._Unique.*";
			const string unSafeFieldRegexText = "^(" + audit + "|" + parent + "|" + unique + ")$";

			if (unSafeFieldRegex == null)
			{
				unSafeFieldRegex = new Regex(unSafeFieldRegexText, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
			}

			return unSafeFieldRegex.IsMatch(name);
		}

		public static PropertyInfo GetPropertyInfo(Type type, string name)
		{
			PropertyInfo result = null;

			if (type == null)
			{
				return null;
			}

			try
			{
				result = type.GetProperty(name);
			}
			catch (AmbiguousMatchException)
			{
				// Because .NET sux!
				foreach (PropertyInfo info in type.GetProperties())
				{
					if (info.Name != name)
					{
						continue;
					}

					if (result == null || result.DeclaringType.IsAssignableFrom(info.DeclaringType))
					{
						result = info;
					}
				}
			}

			return result;
		}

		public static PropertyClassification Classify(PropertyInfo info)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}
			else if (typeof(IZType).IsAssignableFrom(info.PropertyType))
			{
				return PropertyClassification.Updatable;
			}
			else if (info.CanRead && ActionFieldFollowAttribute.ShouldFollow(info))
			{
				Type returnType = ActionFieldFollowAttribute.GetReturnType(info);

				if (typeof(DynamicBusinessObjectCollection).IsAssignableFrom(returnType) || typeof(DynamicBusinessObject).IsAssignableFrom(returnType) || info.GetIndexParameters().Length > 0)
				{
					return PropertyClassification.Blocked;
				}
				else if (typeof(BusinessObject).IsAssignableFrom(returnType))
				{
					return PropertyClassification.FollowSingle;
				}
				else if (typeof(IBusinessObjectCollectionView).IsAssignableFrom(returnType) && typeof(IBusinessObjectCollection).IsAssignableFrom(info.ReflectedType))
				{
					return PropertyClassification.FollowView;
				}
				else if (typeof(IBusinessObjectCollection).IsAssignableFrom(returnType))
				{
					return PropertyClassification.FollowCollection;
				}
			}

			return PropertyClassification.Blocked;
		}

		static bool CanFollow(PropertyInfo info)
		{
			switch (Classify(info))
			{
				case PropertyClassification.Updatable:
					ActionFieldAttribute att = ActionFieldAttribute.Get(info);
					return att == null || att.FieldType != ActionFieldType.Hidden;

				case PropertyClassification.FollowSingle:
				case PropertyClassification.FollowCollection:
				case PropertyClassification.FollowView:
					return true;

				default:
					return false;
			}
		}

		[ThreadStatic]
		static Regex unSafeFieldRegex;
	}
}
