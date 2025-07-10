using System;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CFIADummyBusinessObject : IPGAHeader
	{
		public ZString GovAgencyIDCode => PGACodes.Codes.CFIA;

		public IHasPGARequirements Parent => null;

		public void CopyPersistentValuesFrom(IPGAHeader source)
		{
			throw new NotImplementedException();
		}
	}
}
