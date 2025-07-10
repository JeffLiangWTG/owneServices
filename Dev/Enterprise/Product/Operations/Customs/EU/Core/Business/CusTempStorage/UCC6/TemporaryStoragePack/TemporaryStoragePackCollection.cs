using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStoragePackCollection : AsycudaPackCollection<TemporaryStoragePack, TemporaryStorageBill>, ISequenceNumberHeader
	{
		public TemporaryStoragePackCollection(TemporaryStorageBill master) : base(master)
		{
			MaxCountValidationEnable(99);
		}

		protected override bool AllowNewCore => Count < MaxCount;

		public ShortSequenceNumberGenerator SequenceGenerator => fSequenceGenerator ?? (fSequenceGenerator = new ShortSequenceNumberGenerator(this));
		ShortSequenceNumberGenerator fSequenceGenerator;

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => this.Cast<ISequenceNumberLine>();
	}
}
