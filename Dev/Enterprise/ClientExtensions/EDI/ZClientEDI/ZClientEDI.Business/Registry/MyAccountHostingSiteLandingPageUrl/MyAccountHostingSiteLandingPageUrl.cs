using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public partial class MyAccountHostingSiteLandingPageUrl : RegistryBusinessObjectTemplate
	{
		public MyAccountHostingSiteLandingPageUrl()
			: base() { }

		public MyAccountHostingSiteLandingPageUrl(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public MyAccountHostingSiteLandingPageUrl(FallbackLevel fallbackLevel, BusinessObjectFactory factory, MyAccountHostingSiteLandingPageUrlCollection parentCollection)
			: base(fallbackLevel, factory)
		{
			this.ParentCollection = parentCollection;
		}

		internal MyAccountHostingSiteLandingPageUrlCollection ParentCollection;

		#region SuppressResourceStringsCheckRegion

		public static class Schema
		{
			public const string ProductCode = "ProductCode";
			public const string LandingPageAbsoluteUrl = "LandingPageAbsoluteUrl";
		}

		#endregion

		#region Properties

		#region ProductCode

		[List("Lookups.ProductTypes")]
		[ResourceStringData("MyAccountHostingSiteLandingPageUrl|ProductCode", Caption = "Product")]
		[MaxLength(3)]
		public ZString ProductCode
		{
			get { return productCode; }
			set
			{
				SetNonPersistentPropertyValue(ProductCodeInfo, ref productCode, value);
				if (!IsValidationSuspended)
				{
					ValidateEverything();
				}
			}
		}
		ZString productCode;

		public ZPropertyInfo ProductCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ProductCode); }
		}

		#endregion

		#region Url

		[ResourceStringData("MyAccountHostingSiteLandingPageUrl|LandingPageAbsoluteUrl", Caption = "My Account Landing Page")]
		public ZString LandingPageAbsoluteUrl
		{
			get { return landingPageAbsoluteUrl; }
			set
			{
				SetNonPersistentPropertyValue(LandingPageAbsoluteUrlInfo, ref landingPageAbsoluteUrl, value);
				if (!IsValidationSuspended)
				{
					ValidateEverything();
				}
			}
		}
		ZString landingPageAbsoluteUrl;

		public ZPropertyInfo LandingPageAbsoluteUrlInfo
		{
			get { return GetZPropertyInfo(Schema.LandingPageAbsoluteUrl); }
		}

		#endregion

		#region Lookups

		public MyAccountHostingSiteLandingPageUrlLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = GetNewLookups();
				}

				return lookups;
			}
		}

		protected MyAccountHostingSiteLandingPageUrlLookups GetNewLookups()
		{
			return new MyAccountHostingSiteLandingPageUrlLookups(this);
		}

		MyAccountHostingSiteLandingPageUrlLookups lookups;

		#endregion

		#endregion

		#region XMLSerialisation

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			ProductCode = reader.ReadElementString(Schema.ProductCode);
			LandingPageAbsoluteUrl = reader.ReadElementString(Schema.LandingPageAbsoluteUrl);
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ProductCode, ProductCode);
			writer.WriteElementString(Schema.LandingPageAbsoluteUrl, LandingPageAbsoluteUrl.ToString());
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new MyAccountHostingSiteLandingPageUrl(fallbackLevel, factory, null);
		}

		#region Validation

		public void CheckProductCode(MyAccountHostingSiteLandingPageUrl rule)
		{
			rule.ProductCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(rule.ProductCodeInfo);
			ListValidation.ErrorIfInvalidCode(rule.ProductCodeInfo);
		}

		public void CheckLandingPageAbsoluteUrl(MyAccountHostingSiteLandingPageUrl rule)
		{
			rule.LandingPageAbsoluteUrlInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(rule.LandingPageAbsoluteUrlInfo);
			if (!UrlValidation.IsValidUrl(rule.LandingPageAbsoluteUrl))
			{
				rule.LandingPageAbsoluteUrlInfo.AddError(ResString.GetMultilingualString("da7420e9-5d1e-4b11-95ee-c07bb1678bd2", "Please enter a valid URL"));
			}
		}

		public void ValidateEverything()
		{
			HasBaseNotifications();
			CheckDuplicatedProduct();
		}

		bool HasBaseNotifications()
		{
			var hasNotifications = false;
			if (ParentCollection != null)
			{
				foreach (MyAccountHostingSiteLandingPageUrl rule in ParentCollection)
				{
					CheckProductCode(rule);
					CheckLandingPageAbsoluteUrl(rule);

					if (rule.ProductCodeInfo.HasNotifications() || rule.LandingPageAbsoluteUrlInfo.HasNotifications())
					{
						hasNotifications = true;
					}
				}
			}

			return hasNotifications;
		}

		void CheckDuplicatedProduct()
		{
			if (ParentCollection != null)
			{
				var collection = ParentCollection.Cast<MyAccountHostingSiteLandingPageUrl>().OrderBy(m => m.ProductCode);
				MyAccountHostingSiteLandingPageUrl prev = null;
				foreach (MyAccountHostingSiteLandingPageUrl curr in collection)
				{
					if (prev != null && curr.ProductCode == prev.ProductCode)
					{
						SetErrorProductCode(prev, curr);
					}

					prev = curr;
				}
			}
		}

		void SetErrorProductCode(MyAccountHostingSiteLandingPageUrl prev, MyAccountHostingSiteLandingPageUrl curr)
		{
			var errorMessageWaitOverlap = ResString.GetMultilingualString("dd39c3f9-eb39-4168-8ad5-3839ffba6cdf", "Duplicated products are not allowed");
			prev.ProductCodeInfo.AddError(errorMessageWaitOverlap);
			curr.ProductCodeInfo.AddError(errorMessageWaitOverlap);
		}

		#endregion
	}
}
