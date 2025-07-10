using System.Drawing;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappersCore;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DummyBusinessObjectWrapperWithDisposedAlternativeBrandingImage : DocBaseWrapperBaseWithImageSupport
	{
		internal DummyBusinessObjectWrapperWithDisposedAlternativeBrandingImage(object objectToWrap, BusinessObjectFactory factory) : base(objectToWrap, factory)
		{
		}

		protected override ClientAndAgentBrandingBusinessObject AlternativeBranding
		{
			get
			{
				var branding = new PrincipalBranding();
				branding.Image = new Bitmap(3, 3);
				branding.Image.Dispose();
				return branding;
			}
		}
	}
}
