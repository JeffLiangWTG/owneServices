using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DummyWithBlob : DummyEnterpriseBusinessObject
	{
		public DummyWithBlob(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZBlob Details
		{
			get { return BlobNoteExposed.Rtf; }
			set { BlobNoteExposed.Rtf = value; }
		}

		public HiddenRtfNoteExposed BlobNoteExposed
		{
			get
			{
				if (fBlobNoteExposed == null)
				{
					fBlobNoteExposed = new HiddenRtfNoteExposed(this);
				}
				return fBlobNoteExposed;
			}
		}

		HiddenRtfNoteExposed fBlobNoteExposed;
	}
}
