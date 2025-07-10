using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(DocumentPreviewFormLayoutRegistryDataType))]
	sealed class DocumentPreviewFormLayoutRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DocumentPreviewFormLayoutRegistryDataType>
	{
		protected override DocumentPreviewFormLayoutRegistryDataType GetNewDataType()
		{
			return new DocumentPreviewFormLayoutRegistryDataType();
		}

		protected override bool HasEditor
		{
			get { return false; }
		}

		protected override string ExpectedEditorName
		{
			get { return null; }
		}

		#region Implementation

		protected override void AssertValuesEqual(string message, NonPersistentBusinessObject lhs, NonPersistentBusinessObject rhs)
		{
			DocumentPreviewFormLayout originalValues = (DocumentPreviewFormLayout)lhs;
			DocumentPreviewFormLayout newValues = (DocumentPreviewFormLayout)rhs;

			AssertEquals("IsThumbnailPanelVisible", originalValues.IsThumbnailPanelVisible, newValues.IsThumbnailPanelVisible);
			AssertEquals("ThumbnailPanelPixelWidth", originalValues.ThumbnailPanelPixelWidth, newValues.ThumbnailPanelPixelWidth);
			AssertEquals("ZoomValue", originalValues.ZoomValue, newValues.ZoomValue);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			DocumentPreviewFormLayout documentPreviewFormLayout = new DocumentPreviewFormLayout(true, 1, 2);

			byte[] value = new byte[]
			{
				255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,
				0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,0,45,0,49,
				0,54,0,34,0,63,0,62,0,60,0,68,0,111,0,99,0,117,0,109,0,101,0,110,0,116,0,80,0,114,0,101,0,118,0,105,
				0,101,0,119,0,70,0,111,0,114,0,109,0,76,0,97,0,121,0,111,0,117,0,116,0,62,0,60,0,73,0,115,0,84,0,104,
				0,117,0,109,0,98,0,110,0,97,0,105,0,108,0,80,0,97,0,110,0,101,0,108,0,86,0,105,0,115,0,105,0,98,0,
				108,0,101,0,62,0,84,0,114,0,117,0,101,0,60,0,47,0,73,0,115,0,84,0,104,0,117,0,109,0,98,0,110,0,97,0,
				105,0,108,0,80,0,97,0,110,0,101,0,108,0,86,0,105,0,115,0,105,0,98,0,108,0,101,0,62,0,60,0,84,0,104,0,
				117,0,109,0,98,0,110,0,97,0,105,0,108,0,80,0,97,0,110,0,101,0,108,0,80,0,105,0,120,0,101,0,108,0,87,
				0,105,0,100,0,116,0,104,0,62,0,49,0,60,0,47,0,84,0,104,0,117,0,109,0,98,0,110,0,97,0,105,0,108,0,80,
				0,97,0,110,0,101,0,108,0,80,0,105,0,120,0,101,0,108,0,87,0,105,0,100,0,116,0,104,0,62,0,60,0,90,0,
				111,0,111,0,109,0,86,0,97,0,108,0,117,0,101,0,62,0,50,0,60,0,47,0,90,0,111,0,111,0,109,0,86,0,97,0,
				108,0,117,0,101,0,62,0,60,0,47,0,68,0,111,0,99,0,117,0,109,0,101,0,110,0,116,0,80,0,114,0,101,0,118,
				0,105,0,101,0,119,0,70,0,111,0,114,0,109,0,76,0,97,0,121,0,111,0,117,0,116,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(documentPreviewFormLayout, value)
			};
		}

		#endregion
	}
}
