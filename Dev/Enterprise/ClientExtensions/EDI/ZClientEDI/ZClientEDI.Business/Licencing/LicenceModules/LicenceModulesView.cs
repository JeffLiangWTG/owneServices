using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceModulesView : BusinessObjectCollectionView<LicenceModules>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
		[Flags]
		public enum FilterMode
		{
			ShowAll = 0,
			HideUnlicenced = 1
		}

		public LicenceModulesView(LicenceModulesDependentCollection collectionToFilter)
			: base(collectionToFilter)
		{
			Rebuild();
		}

		public ZBool HideUnlicenced
		{
			get { return (Mode & FilterMode.HideUnlicenced) != 0; }
			set { Mode = (Mode & ~FilterMode.HideUnlicenced) | (value ? FilterMode.HideUnlicenced : 0); }
		}

		public FilterMode Mode
		{
			get { return mode; }
			set
			{
				if (mode != value)
				{
					mode = value;
					RemoveAllButLeaveRelationshipsIntact();
					Rebuild();
					EDIDataRegistry.Instance.LicenceModuleViewMode = (int)value;
				}
			}
		}

		FilterMode mode = (FilterMode)EDIDataRegistry.Instance.LicenceModuleViewMode;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			bool result = true;
			LicenceModules item = (LicenceModules)element;

			if (mode != FilterMode.ShowAll)
			{
				if (HideUnlicenced && (item.LM_LicenceType == LicenceTypes.Codes.NON))
				{
					result = false;
				}
			}

			var checkpoint = Env.Licence.GetCheckpointFromCode(item.LM_GroupModuleCode);
			if (checkpoint is LanguageLicenceChildCheckpoint)
			{
				result = false;
			}

			return result;
		}

		protected override bool AllowSort
		{
			get { return false; }
		}
	}
}
