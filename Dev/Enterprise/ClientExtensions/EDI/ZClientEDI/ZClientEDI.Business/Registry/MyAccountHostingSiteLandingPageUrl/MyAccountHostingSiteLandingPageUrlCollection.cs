using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class MyAccountHostingSiteLandingPageUrlCollection : RegistryBusinessObjectCollectionTemplate
	{
		public MyAccountHostingSiteLandingPageUrlCollection() : base() { }
		public MyAccountHostingSiteLandingPageUrlCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory) { }

		public new MyAccountHostingSiteLandingPageUrl this[int i]
		{
			get { return (MyAccountHostingSiteLandingPageUrl)Elements[i]; }
		}

		protected override bool AllowSort
		{
			get { return false; }
		}

		protected override void OnAdded(BusinessObject bizObjAdded)
		{
			base.OnAdded(bizObjAdded);

			var added = bizObjAdded as MyAccountHostingSiteLandingPageUrl;
			if (added != null && added.ParentCollection == null)
			{
				added.ParentCollection = this;
			}
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			base.RemoveAndDelete(elementToDelete);

			if (elementToDelete is MyAccountHostingSiteLandingPageUrl bizo)
			{
				bizo.ValidateEverything();
			}
		}

		public new MyAccountHostingSiteLandingPageUrl AddNew()
		{
			return (MyAccountHostingSiteLandingPageUrl)base.AddNew();
		}

		public MyAccountHostingSiteLandingPageUrl AddNew(string productCode, string landingPageAbsoluteUrl)
		{
			var rule = this.AddNew();
			using (rule.GetValidationSuspender())
			{
				rule.ProductCode = productCode;
				rule.LandingPageAbsoluteUrl = landingPageAbsoluteUrl;
			}

			return rule;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new MyAccountHostingSiteLandingPageUrl(CurrentFallbackLevel, CurrentFactory, this);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new MyAccountHostingSiteLandingPageUrlCollection(fallbackLevel, factory);
		}
	}
}
