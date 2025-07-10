using System;
using System.IO;
using CargoWise.EntityFramework;

namespace Enterprise.Client.TNT.NZ
{
	class NZQuantumMawbCollection : QuantumMawbCollection
	{
		public NZQuantumMawbCollection(BusinessObjectFactory factory, string fullFileName)
			: base(factory, fullFileName)
		{
			isX2 = new FileInfo(fullFileName).Name.Split('.')[1].Equals("X2", StringComparison.OrdinalIgnoreCase);
		}

		protected bool isX2;

		protected override QuantumRecordFactory GetNewRecordFactory()
		{
			return new QuantumRecordFactory();
		}

		protected override void AddQuantumMawb(BusinessObjectFactory factory, QuantumSegment segment)
		{
			Add(new NZQuantumMawb(factory, segment, isX2));
		}
	}
}
