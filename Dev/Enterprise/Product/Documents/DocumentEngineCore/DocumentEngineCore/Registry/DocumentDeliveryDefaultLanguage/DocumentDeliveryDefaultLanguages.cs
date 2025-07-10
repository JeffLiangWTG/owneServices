using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class DocumentDeliveryDefaultLanguages : RegistryBusinessObjectTemplate, IDocumentDeliveryDefaultLanguages
	{
		#region Schema

		public abstract class Schema
		{
			public const string Fallback = "Fallback";
			public const string Order = "Order";
		}

		#endregion

		public DocumentDeliveryDefaultLanguages()
		{
		}

		public DocumentDeliveryDefaultLanguages(bool isDefaulting)
		{
			if (isDefaulting)
			{
				SuspendValidation();
			}
		}

		public DocumentDeliveryDefaultLanguages(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DocumentDeliveryDefaultLanguages(fallbackLevel, null);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateFallback();
			ValidateOrder();
		}

		#region Bound Properties

		#region Fallbacks

		[List("Fallbacks")]
		public ZString Fallback
		{
			get { return fallback; }
			set
			{
				SetNonPersistentPropertyValue(FallbackInfo, ref fallback, value);

				if (!IsValidationSuspended)
				{
					ValidateFallback();
				}
			}
		}
		ZString fallback;

		public ZPropertyInfo FallbackInfo => GetZPropertyInfo(Schema.Fallback);

		void ValidateFallback()
		{
			FallbackInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(FallbackInfo);
			ListValidation.ErrorIfInvalidCode(FallbackInfo);
			if (!FallbackInfo.HasErrors())
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(FallbackInfo, (IMultilingualString)ResString.GetMultilingualString("9EB6F20D-784F-49CD-8D75-050DEA1AAE97", "There must be only one '{0}' line.", Fallback));
			}
		}

		#endregion

		#region Order

		public ZInt Order
		{
			get { return order; }
			set
			{
				SetNonPersistentPropertyValue(OrderInfo, ref order, value);

				if (!IsValidationSuspended)
				{
					ValidateOrder();
				}
			}
		}
		ZInt order;

		public ZPropertyInfo OrderInfo => GetZPropertyInfo(Schema.Order);

		void ValidateOrder()
		{
			OrderInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(OrderInfo);
			if (!OrderInfo.HasErrors())
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(OrderInfo, (IMultilingualString)ResString.GetMultilingualString("326DAC95-F4EA-4BB3-A5EF-C5647199C538", "Order sequence can only be used once."));
			}
		}

		#endregion

		#endregion

		#region Lookups

		public CodeDescriptionPairList Fallbacks
		{
			get
			{
				return CurrentFactory.GetCachedValue("DocumentDeliveryDefaultLanguages.Fallbacks",
					delegate
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(Constants.DocumentDeliveryDefaultLanguagesFallbackType.Contact, ResString.GetMultilingualString("3045B644-4CD9-4CE1-AC2A-0874F7F3AD49", "Contact Language"));
						result.AddPair(Constants.DocumentDeliveryDefaultLanguagesFallbackType.Address, ResString.GetMultilingualString("BB54F370-09DF-4285-AE9A-5E29ABECF172", "Address Language"));
						result.AddPair(Constants.DocumentDeliveryDefaultLanguagesFallbackType.Organization, ResString.GetMultilingualString("9864D7B0-0760-45BA-AB79-74CED5619E91", "Organization Language"));
						result.AddPair(Constants.DocumentDeliveryDefaultLanguagesFallbackType.Branch, ResString.GetMultilingualString("0D9771FC-CEA8-4442-BED8-09DEF1C8080F", "Branch Language"));
						result.AddPair(Constants.DocumentDeliveryDefaultLanguagesFallbackType.Company, ResString.GetMultilingualString("F640CBEF-FAB0-4370-99CC-43F4B19D50A9", "Company Language"));
						result.AddPair(Constants.DocumentDeliveryDefaultLanguagesFallbackType.System, ResString.GetMultilingualString("B11DB242-1C8D-4E68-81A6-C794E12BC912", "System Language"));
						return result;
					});
			}
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Fallback, Fallback);
			writer.WriteElementString(Schema.Order, Order.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Fallback = reader.ReadElementString(Schema.Fallback);
			Order = new ZInt(reader.ReadElementString(Schema.Order));
		}

		#endregion
	}
}
