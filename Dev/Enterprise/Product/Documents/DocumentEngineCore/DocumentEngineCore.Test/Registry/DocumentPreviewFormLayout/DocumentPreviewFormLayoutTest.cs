using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(DocumentPreviewFormLayout))]
	public class DocumentPreviewFormLayoutTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestConstructorWithParameters()
		{
			DocumentPreviewFormLayout documentPreviewFormLayout = new DocumentPreviewFormLayout(true, 123, 456);

			AssertEquals("IsThumbnailPanelVisible", true, documentPreviewFormLayout.IsThumbnailPanelVisible);
			AssertEquals("ThumbnailPanelPixelWidth", 123, documentPreviewFormLayout.ThumbnailPanelPixelWidth);
			AssertEquals("ZoomValue", 456, documentPreviewFormLayout.ZoomValue);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return new DocumentPreviewFormLayout(true, 100, 200);
		}

		protected override void CheckAllPropertiesAreEqual(RegistryBusinessObjectTemplate originalBusinessObject, RegistryBusinessObjectTemplate newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);

			DocumentPreviewFormLayout originalValues = (DocumentPreviewFormLayout)originalBusinessObject;
			DocumentPreviewFormLayout newValues = (DocumentPreviewFormLayout)newBusinessObject;

			AssertEquals("IsThumbnailPanelVisible", originalValues.IsThumbnailPanelVisible, newValues.IsThumbnailPanelVisible);
			AssertEquals("ThumbnailPanelPixelWidth", originalValues.ThumbnailPanelPixelWidth, newValues.ThumbnailPanelPixelWidth);
			AssertEquals("ZoomValue", originalValues.ZoomValue, newValues.ZoomValue);
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
