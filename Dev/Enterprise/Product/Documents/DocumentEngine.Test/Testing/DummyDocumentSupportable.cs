using System.Data;
using System.Drawing;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.DocumentEngine.Testing
{
	[UserDefinedValues]
	class DummyDocumentSupportable : DummyBaseBusinessObject, IDocumentSupportable
	{
		public DummyDocumentSupportable(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			collection = new ChildDummyBusinessObjectCollection(factory);
		}

		readonly ChildDummyBusinessObjectCollection collection;

		public ChildDummyBusinessObjectCollection Collection
		{
			get { return collection; }
		}

		public ZString ZStringProperty { get; set; }

		public DocumentSupporter DocumentSupporter => documentSupporter ?? (documentSupporter = new DummyDocumentSupporter(this));
		DocumentSupporter documentSupporter;

		protected override IBusinessObjectFetchStrategy GetFetchStrategy() => fetchStrategy ?? (fetchStrategy = new DummyFetchStrategy(this));
		IBusinessObjectFetchStrategy fetchStrategy;

		Image byteArrayAsImage;
		public Image Z0_VarBinaryMaxAsImage
		{
			get
			{
				if (byteArrayAsImage == null)
				{
					using (var stream = new MemoryStream(Z0_VarBinaryMax))
					{
						byteArrayAsImage = Image.FromStream(stream);
					}
				}

				return byteArrayAsImage;
			}
		}
	}
}
