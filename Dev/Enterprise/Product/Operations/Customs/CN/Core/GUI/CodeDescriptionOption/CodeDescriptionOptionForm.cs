using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.CN.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI
{
	public partial class CodeDescriptionOptionForm : ZChildForm
	{
		internal CodeDescriptionOptionForm(CodeDescriptionOptionCollectionParent parent, bool hideCode = false) : base(parent)
		{
			optionCollectionParent = Argument.NotNull(parent, nameof(parent));
			originSelectedCodes = parent.OptionCollection.Cast<CodeDescriptionOption>().Where(option => option.Selected).Select(option => option.Code).ToArray();

			InitializeComponent();
			if (hideCode)
			{
				OptionsGrid.SetAvailability(false, nameof(CodeDescriptionOption.Code));
			}
		}

		readonly CodeDescriptionOptionCollectionParent optionCollectionParent;
		readonly IEnumerable<ZString> originSelectedCodes;

		public override string FormVerb => string.Empty;

		void zOKButton_Click(object sender, System.EventArgs e)
		{
			optionCollectionParent.OptionCollection.RefreshSelectionCollection();
			Close();
		}

		void zCancelButton_Click(object sender, System.EventArgs e)
		{
			optionCollectionParent.OptionCollection.SelectedCodes = originSelectedCodes;
			Close();
		}
	}
}
