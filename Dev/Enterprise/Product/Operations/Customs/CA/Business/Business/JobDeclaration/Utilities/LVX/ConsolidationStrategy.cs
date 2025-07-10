using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public interface IConsolidationStrategy
	{
		void AddMatchingFilter(ZQuery query);
		bool IsMatching(JobDeclaration declaration);
		void FillDataForNewDeclaration(JobDeclaration declaration);
	}

	public abstract class ConsolidationStrategy : IConsolidationStrategy
	{
		public ConsolidationStrategy(IConsolidationOptionsWrapper wrapper)
		{
			Wrapper = Argument.NotNull(wrapper, "wrapper");
			fHasAcknowledged = GetHasAcknowledged();
		}

#if DEBUG
		public ConsolidationStrategy(IConsolidationOptionsWrapper wrapper, bool hasAcknowledged)
		{
			Wrapper = Argument.NotNull(wrapper, "wrapper");
			fHasAcknowledged = hasAcknowledged;
		}
#endif

		protected readonly IConsolidationOptionsWrapper Wrapper;
		protected readonly bool fHasAcknowledged;
		public bool HasAcknowledged { get { return fHasAcknowledged; } }

		#region IConsolidationStrategy Members

		public abstract void AddMatchingFilter(ZQuery query);

		public abstract bool IsMatching(JobDeclaration declaration);

		public abstract void FillDataForNewDeclaration(JobDeclaration declaration);

		#endregion

		#region GetEffectiveSetting

		static OrgImpAddInfo GetImporterAddInfo(IConsolidationOptionsWrapper wrapper)
		{
			return wrapper.Importer == null ? null : OrgImpAddInfo.Get(wrapper.Importer);
		}

		protected virtual bool GetSettingFromRegistry()
		{
			return true;
		}

		protected virtual bool GetSettingOfImporterCore(OrgImpAddInfo importerAddInfo)
		{
			return true;
		}

		protected bool GetSettingOfImporter()
		{
			var importerAddInfo = GetImporterAddInfo(Wrapper);
			return importerAddInfo != null && GetSettingOfImporterCore(importerAddInfo);
		}

		protected bool GetEffectiveSetting()
		{
			return GetSettingOfImporter() || GetSettingFromRegistry();
		}

		protected virtual bool GetHasAcknowledged()
		{
			return GetEffectiveSetting();
		}

		#endregion
	}
}
