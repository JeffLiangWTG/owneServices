using System.Drawing;
using System.Drawing.Imaging;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using BusinessObject = CargoWise.EntityFramework.BusinessObject;

namespace Enterprise.DocumentEngine.Testing
{
	public static class EDocsTestHelper
	{
		public static RefDocType CreateDocType(BusinessObjectFactory factory, ZString docType, ZString referenceType, ZString description)
		{
			var result = factory.New<RefDocType>();
			result.RT_DocType = docType;
			result.RT_ReferenceType = referenceType;
			result.RT_Desc = description;

			return result;
		}

		internal static SubStreamableStream CreateSimpleBitmapContents()
		{
			var stream = new CargoWise.IO.Shim.SubStreamableStream();
			using (var bitmap = new Bitmap(1, 1))
			{
				bitmap.Save(stream, ImageFormat.Png);
				return stream;
			}
		}

		public static IDocumentFactory GetDocumentFactory(BusinessObjectFactory factory)
		{
			var provider = ObjectFactory.Get<IDocumentFactoryProvider>();
			return provider.GetFactory(factory);
		}

		public static IStorageMain GetOrCreateStorageMain(IDocumentFactory documentFactory, BusinessObject bizO)
		{
			var docManagerInfo = ((IDocManagerSupport)bizO).DocManagerInfo;
			var docManagerCode = docManagerInfo != null ? docManagerInfo.DocManagerCode : ZString.Empty;

			return documentFactory.RetrieveExistingOrCreateStorageMain(bizO, docManagerCode);
		}

		internal static void SetIsSystemGenerated(this IeDoc eDoc, ZBool isSystemGenerated)
		{
			((BusinessObject)eDoc)[StorageDocsSchema.SC_IsSystemGenerated] = isSystemGenerated;
		}

		internal static void SetDate(this IeDoc eDoc, ZDateTime date)
		{
			((BusinessObject)eDoc)[StorageDocsSchema.SC_Date] = date;
		}
	}
}
