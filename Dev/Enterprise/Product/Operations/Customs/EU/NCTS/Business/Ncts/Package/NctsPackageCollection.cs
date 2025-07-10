using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsPackageCollection<out TPackage, out TMaster> : IBusinessObjectCollection<TPackage>, Customs.Business.ISequenceNumberHeader
		where TPackage : NctsPackage
		where TMaster : NctsCommonCargoDesc
	{
		new TPackage this[int index] { get; }
		Customs.Business.ShortSequenceNumberGenerator LineNumberGenerator { get; }
		void AddCloneFrom(IBusinessObjectCollection collectionToCloneFrom, BusinessObjectCloneArgs args);
		void SetReadOnlyIncludingChildren(bool readOnly);
	}

	public class NctsPackageCollection<TPackage, TMaster> : Customs.Business.CusInvPackCollection<TPackage, TMaster>, INctsPackageCollection<TPackage, TMaster>
		where TPackage : NctsPackage
		where TMaster : NctsCommonCargoDesc
	{
		public NctsPackageCollection(TMaster master)
			: base(master)
		{
			var header = master.Header;
			if (header != null && header.IsPhase5Departure && header.Configuration.ValidationRuleConfiguration.IsRuleTR0026Active)
			{
				const int maxPackagesAllowed = 99;
				this.EnableMaxCountValidationWithMessageError(maxPackagesAllowed, warnAtHalfway: false, Res.GetString("5B2782CB-2992-4E89-AD92-E9681074B62D", "[{0}] The maximum number of {1} Package Lines has been exceeded.", ValidationRuleCodeConstants.TR0026, maxPackagesAllowed));
			}
		}

		public IEnumerator<TPackage> GetEnumerator() => Elements.Cast<TPackage>().GetEnumerator();

		public void AddCloneFrom(IBusinessObjectCollection collectionToCloneFrom, BusinessObjectCloneArgs args)
		{
			args.AddValueOverride(TypeOfElements, FKSchemaColumnInDependent.Name, Master.PK);
			foreach (var sourcePackage in collectionToCloneFrom)
			{
				var targetPackage = ((BusinessObject)sourcePackage).Clone(args);
				using (targetPackage.GetValidationSuspender())
				{
					Add(targetPackage);
				}
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var package = (TPackage)child;
			if (package.IsPhase5)
			{
				package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
			}
			if (IsDefaultSelectSingleContainer)
			{
				SelectSingleContainer(package);
			}
		}

		protected virtual ZBool IsDefaultSelectSingleContainer => Master.IsPhase5 && Master.Header.IsDepartureMovement;

		void SelectSingleContainer(TPackage package)
		{
			var headerContainers = Master.Header.DepartureHeaderContainers;
			if (package != null && headerContainers.Count == 1)
			{
				package.ContainersPivot.AddPivotFor(headerContainers[0]);
			}
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(CusInvPackSchema.B5_B5_ParentPackage, null);
			return query;
		}

		#region ISequenceNumberHeader
		public IEnumerable<Customs.Business.ISequenceNumberLine> Lines => new TypedEnumerable<Customs.Business.ISequenceNumberLine>(Elements);

		public Customs.Business.ShortSequenceNumberGenerator LineNumberGenerator => lineNumberGenerator ?? (lineNumberGenerator = new Customs.Business.ShortSequenceNumberGenerator(this));
		Customs.Business.ShortSequenceNumberGenerator lineNumberGenerator;
		#endregion
	}
}
