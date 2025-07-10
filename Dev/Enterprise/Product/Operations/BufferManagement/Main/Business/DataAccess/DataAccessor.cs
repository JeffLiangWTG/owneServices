using System.Runtime.CompilerServices;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Business
{
	public class DataAccessor : ServiceTaskFactoryProviderWrapper
	{
		public DataAccessor(ILogger logger, string serviceTaskCode = null, bool mustCatchZSaveConcurrencyException = true)
				: base(logger, serviceTaskCode, mustCatchZSaveConcurrencyException)
		{
		}

		#region ServiceTaskFactoryProviderWrapper Overrides

		protected override void SaveCore(bool createNew, bool swallowSaveUnsuccessfulException = true, [CallerMemberName] string callerMemberName = "")
		{
			try
			{
				base.SaveCore(createNew, swallowSaveUnsuccessfulException, callerMemberName);
			}
			catch (ZSaveConcurrencyException ex)
			{
				throw new SaveConcurrencyException("Cannot save due to save concurrency error", ex);
			}
		}

		#endregion
	}
}
