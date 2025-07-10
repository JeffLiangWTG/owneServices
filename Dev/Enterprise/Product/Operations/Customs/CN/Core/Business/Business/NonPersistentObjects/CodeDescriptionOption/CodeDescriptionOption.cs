using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public class CodeDescriptionOption : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CodeDescriptionOption(CodeDescriptionOptionCollectionParent parent, ICodeDescription codeDescriptionPair) : base(parent.Factory)
		{
			this.parent = Argument.NotNull(parent, nameof(parent));
			this.codeDescriptionPair = Argument.NotNull(codeDescriptionPair, nameof(codeDescriptionPair));

			Selected = this.parent.Storage.ContainsCode(this.codeDescriptionPair.Code);
			ClearHasChanges();
		}
		readonly CodeDescriptionOptionCollectionParent parent;
		readonly ICodeDescription codeDescriptionPair;

		#region Properties

		public ZBool Selected
		{
			get => fSelected;
			set
			{
				if (fSelected != value)
				{
					SetNonPersistentPropertyValue(SelectedInfo, ref fSelected, value);
					ValidateSelected();
					SelectedInfo.RefreshBinding();
				}
			}
		}
		ZBool fSelected = false;

		public ZPropertyInfo SelectedInfo => GetZPropertyInfo(nameof(Selected));

		public ZString Code => codeDescriptionPair.Code;
		public ZPropertyInfo CodeInfo => GetZPropertyInfo(nameof(Code));

		public ZString Description => codeDescriptionPair.Description;
		public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(nameof(Description));

		#endregion

		#region Validation

		public void ValidateSelected()
		{
			if (!IsValidationSuspended)
			{
				SelectedInfo.ClearAllNotifications();

				parent.Storage.ValidateMutuallyExclusiveCodes(Code, Selected, SelectedInfo, parent.OptionCollection.SelectedCodes);
				parent.Storage.ValidateSeletedOption(Code, Selected, SelectedInfo, parent.OptionCollection.SelectedCodes);
			}
		}

		#endregion
	}
}
