using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocRefDocType : DocumentWrapper
	{
		DocRefDocType(RefDocType refDocType, BusinessObjectFactory factoryToWrap)
			: base(refDocType, factoryToWrap)
		{
		}

		public static DocRefDocType New(RefDocType refDocType, BusinessObjectFactory factoryToWrap)
		{
			if (refDocType == null)
			{
				return null;
			}
			else
			{
				return new DocRefDocType(refDocType, factoryToWrap);
			}
		}

		public override string ToString()
		{
			return Description;
		}

		RefDocType RefDocType
		{
			get { return (RefDocType)WrappedObject; }
		}
		public ZString DocType
		{
			get { return RefDocType.RT_DocType; }
		}
		public ZString Description
		{
			get { return RefDocType.RT_DescMultilingual; }
		}
		public ZString ReferenceType
		{
			get { return RefDocType.RT_ReferenceType; }
		}
		public ZBool IsActive
		{
			get { return RefDocType.RT_IsActive; }
		}
		public ZBool IsPublished
		{
			get { return RefDocType.RT_IsPublished; }
		}
		public ZBool IsPublishUpdatable
		{
			get { return RefDocType.RT_IsPublishUpdatable; }
		}
		public ZBool IsSystem
		{
			get { return RefDocType.RT_IsSystem; }
		}

		public ZString UserName
		{
			get { return GlbStaff.CurrentUser.GS_LoginName; }
		}

		public ZString BarcodeText
		{
			get { return Barcode.TextAs128sFontString; }
		}

		public ZString BarcodeLabel
		{
			get { return Barcode.TextToEncode; }
		}

		protected TextBarcode Barcode
		{
			get
			{
				BarcodeGenerator generator = new BarcodeGenerator();
				return generator.CreateDocTypeBarcode(DocType);
			}
		}
	}
}
