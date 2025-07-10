using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public class CodeDescriptionOptionCollection : NonPersistentBusinessObjectCollection<CodeDescriptionOption>
	{
		public CodeDescriptionOptionCollection(CodeDescriptionOptionCollectionParent parent) : base(parent.Factory)
		{
			Parent = Argument.NotNull(parent, nameof(parent));
			Storage = Parent.Storage;
		}

		public readonly ICodeDescriptionOptionStorage Storage;
		protected readonly CodeDescriptionOptionCollectionParent Parent;

		public CodeDescriptionOption this[string key] => CodeDescriptionOptions.FirstOrDefault(o => o.Code == key);

		public override void Load()
		{
			RemoveAndDeleteAll();
			AddRange(Storage.GetAllOptions().Cast<ICodeDescription>().Select(pair =>
			{
				var newOption = new CodeDescriptionOption(Parent, pair);
				newOption.SelectedInfo.ValueChanged += OptionSelectedChanged;
				return newOption;
			}));

			this.Cast<CodeDescriptionOption>().Where(option => !option.Selected).ForEach(option => option.ValidateSelected());
		}

		void OptionSelectedChanged(object sender, EventArgs e)
		{
			var option = sender as CodeDescriptionOption;
			if (option != null && !option.Code.IsEmpty)
			{
				if (Storage.GetAllOptions() is IGroupedCodeDescriptionPairList groupedCodeDescriptionPairList)
				{
					var exclusiveCodes = groupedCodeDescriptionPairList.GetMutuallyExclusiveCodes(option.Code);
					if (exclusiveCodes?.Any() ?? false)
					{
						var needUnselect = groupedCodeDescriptionPairList.AutoUnselectExclusiveCodes && option.Selected;

						foreach (var exclusive in CodeDescriptionOptions.Where(o => exclusiveCodes.Contains(o.Code.ToString())))
						{
							if (needUnselect && exclusive.Selected)
							{
								exclusive.Selected = false;
							}
							else
							{
								exclusive.ValidateSelected();
							}
						}

						option.ValidateSelected();
					}
				}
			}
		}

		public bool IsSelected(string code)
		{
			return CodeDescriptionOptions.Any(x => x.Code == code && x.Selected);
		}

		public void UnselectAll()
		{
			CodeDescriptionOptions.ForEach(x => x.Selected = false);
		}

		public IEnumerable<ZString> SelectedCodes
		{
			get { return CodeDescriptionOptions.Where(x => x.Selected).Select(x => x.Code); }
			set { CodeDescriptionOptions.ForEach(x => x.Selected = value.Contains(x.Code)); }
		}

		IEnumerable<CodeDescriptionOption> CodeDescriptionOptions => this.Cast<CodeDescriptionOption>();

		#region Overrides of NonPersistentBusinessObjectCollection

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Overrides of BusinessObjectCollection

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override bool AllowSort => false;

		#endregion

		public void RefreshSelectionCollection()
		{
			foreach (var option in this.Where(x => x.Selected))
			{
				if (Storage.FindByCode(option.Code) == null)
				{
					Storage.AddNew(option.Code);
				}
			}
			foreach (var option in this.Where(x => !x.Selected))
			{
				if (Storage.FindByCode(option.Code) is BusinessObject existingCode)
				{
					Storage.RemoveAndDelete(existingCode);
				}
			}
			Storage.AllCodes.Except(CodeDescriptionOptions.Select(option => option.Code)).ToArray()
				.ForEach(redundantCode => Storage.RemoveAndDelete(Storage.FindByCode(redundantCode)));

			var propertyInfo = Storage.SelectedOptionsAsStringPropertyInfo;
			if (propertyInfo != null)
			{
				(propertyInfo.BizObj as IBusinessObjectInternals).Validate(propertyInfo);
				propertyInfo.RefreshBinding();
			}
		}
	}
}
