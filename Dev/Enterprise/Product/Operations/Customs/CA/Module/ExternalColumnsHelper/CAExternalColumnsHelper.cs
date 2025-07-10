using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public static class CAExternalColumnsHelper
	{
		#region CAExternalModuleCustomColumnsInitializer

		public class CAExternalModuleCustomColumnsInitializer : ZGridCustomColumnsInitializer
		{
			public CAExternalModuleCustomColumnsInitializer(ZGrid grid, IBusinessObjectCollection collection, ResourceStringData groupName, ICustomPropertyContainer propertyContainer)
				: base(grid, collection, groupName)
			{
				this.propertyContainer = propertyContainer;
			}

			public void AddCustomColumns()
			{
				AddCustomColumns(propertyContainer.CustomProperties);
			}

			protected override PropertyDescriptor GetPropertyDescriptor(ICustomProperty property)
			{
				return new CAExternalModuleCustomPropertyDescriptor(propertyContainer, property);
			}

			#region CAExternalModuleCustomPropertyDescriptor

			class CAExternalModuleCustomPropertyDescriptor : ZCustomPropertyDescriptor
			{
				public CAExternalModuleCustomPropertyDescriptor(ICustomPropertyContainer propertyContainer, ICustomProperty property)
					: base(property.Identifier, property.Info.Type)
				{
					this.propertyContainer = propertyContainer;
				}

				protected override ICustomPropertyContainer GetCustomPropertyContainer(object component)
				{
					return propertyContainer;
				}

				public override bool IsReadOnly
				{
					get { return true; }
				}

				readonly ICustomPropertyContainer propertyContainer;
			}

			#endregion

			readonly ICustomPropertyContainer propertyContainer;
		}

		#endregion

		#region Constants

		public static class Constants
		{
			internal const string AwaitingResponseCode = "AWT";
			internal static string AwaitingResponseDescription
			{
				get { return Res.GetString("966bd301-00d8-418e-9afc-ab223d6d8c2f", "Awaiting Response"); }
			}

			internal const string NotSentCode = "NST";
			internal static string NotSentDescription
			{
				get { return Res.GetString("c7e70207-2f34-45ff-a0c5-536141f1ddda", "Not Sent"); }
			}

			internal const string SentCode = "SNT";
			internal static string SentDescription
			{
				get { return Res.GetString("1ca6181f-1aed-461e-8ef7-ba9a0fede299", "Sent"); }
			}

			internal const string RejectedCode = "REJ";
			internal static string RejectedDescription
			{
				get { return Res.GetString("9711a83a-85c2-40b5-82a2-54c927e60078", "Rejected"); }
			}
		}

		#endregion

		public static ZString GetCodeDescriptionFormatted(ZString code, ZString description)
		{
			var builder = new ZStringBuilder();
			builder.Append(code);
			builder.AppendIfNotEmpty(description);
			return builder.ToStringWithDelimiterBetweenAppends(" - ");
		}

		public static CodeDescriptionPairList GetMessageStatusListForFilter(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("CAMessageStatusListForFilter", () =>
			{
				var result = new CodeDescriptionPairList();
				foreach (ICodeDescription item in factory.GetCachedValue<MessageStatusList>())
				{
					if (string.IsNullOrEmpty(item.Code))
					{
						result.AddPair(Constants.NotSentCode, Constants.NotSentDescription);
					}
					else
					{
						result.Add(item);
					}
				}
				return result;
			});
		}

		public static ZString GetQueryValueForMessageStatusCode(ZString value)
		{
			return value == Constants.NotSentCode ? ZString.Empty : value;
		}
	}
}
