using System.Collections.Generic;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStoragePreviousDocumentComparer : IEqualityComparer<TemporaryStoragePreviousDocument>
	{
		public bool Equals(TemporaryStoragePreviousDocument x, TemporaryStoragePreviousDocument y)
		{
			if (ReferenceEquals(x, y))
			{
				return true;
			}

			if (x is null || y is null || x.GetType() != y.GetType())
			{
				return false;
			}

			return x.CSI_Code == y.CSI_Code && x.CSI_ReferenceNumber == y.CSI_ReferenceNumber;
		}

		public int GetHashCode(TemporaryStoragePreviousDocument obj)
		{
			return new { obj.CSI_Code, obj.CSI_ReferenceNumber }.GetHashCode();
		}
	}
}
