using System;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class GACDummyBusinessObject : IPGAHeader
	{
		public ZString GovAgencyIDCode => PGACodes.Codes.GAC;

		public IHasPGARequirements Parent => null;

		public void CopyPersistentValuesFrom(IPGAHeader source)
		{
			throw new NotImplementedException();
		}
	}
}
