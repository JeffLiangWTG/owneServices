using System.Drawing;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentWrappers.GenericWrappers.Base
{
	[DefaultField("DocumentType")]
	public class eDocWrapper : GenericWrapper
	{
		public eDocWrapper(IeDoc eDoc, BusinessObjectFactory factory)
			: base(null, factory)
		{
			if (eDoc == null)
			{
				documentType = ZString.Empty;
				description = ZString.Empty;
				dateAdded = ZDateTime.Empty;
				imageBytes = System.Array.Empty<byte>();
			}
			else
			{
				documentType = eDoc.DocType;
				description = eDoc.Description;
				dateAdded = eDoc.DateAdded;
				imageBytes = (byte[])eDoc.ImageData;
			}
		}

		public ZString DocumentType
		{
			get { return documentType; }
		}
		readonly ZString documentType;

		public ZString Description
		{
			get { return description; }
		}
		readonly ZString description;

		public ZDateTime DateAdded
		{
			get { return dateAdded; }
		}
		readonly ZDateTime dateAdded;

		public Image ImageData
		{
			get
			{
				if (imageData == null || imageData.IsDisposed())
				{
					imageData = (imageBytes.Length == 0) ? null : Image.FromStream(new MemoryStream(imageBytes));
				}
				return imageData;
			}
		}
		Image imageData;
		readonly byte[] imageBytes;
	}
}
