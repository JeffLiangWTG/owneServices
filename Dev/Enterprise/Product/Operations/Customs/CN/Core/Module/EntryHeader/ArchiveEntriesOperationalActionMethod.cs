using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.CN.Module
{
	public class ArchiveEntriesOperationalActionMethod : OperationalActionMethod
	{
		public ArchiveEntriesOperationalActionMethod() : base(new ZGuid("DD08A2D4-E634-4CC4-8FAC-156BADADBE9A"))
		{
		}

		public override string Name => Res.GetString("6EBD1780-8109-4FD4-BD85-64B8BA07F11C", "Archive Entries");

		public override string Description => Res.GetString("BC80B845-BB33-45CD-8759-3A429446E45B", "Archive Entry (CN)");

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new ArchiveEntriesOperationalActionMethodApplicator(factory);
		}

		public override FilterRequirementList GetFilterRequirements()
		{
			var result = base.GetFilterRequirements();

			result.Add(FilterConstants.Country, new[]
			{
				Core.Constants.CountryCodes.China
			});

			return result;
		}
	}
}
