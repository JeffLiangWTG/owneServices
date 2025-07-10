using System.Linq;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Grid.Internal;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.CH.GUI;

public partial class RestrictionsUserControl : ZUserControl
{
	public RestrictionsUserControl()
	{
		InitializeComponent();
		InitializeAdditionalInformationGridControl();
	}

	void InitializeAdditionalInformationGridControl()
	{
		RestrictionAdditionalInformationGrid.RunAfterBind((s, e) =>
		{
			RestrictionAdditionalInformationGrid.ListManager.CurrentItemChanged += (s, e) =>
			{
				AddAdditionalInformationTextColumnControlChangedHandler();
			};
			AddAdditionalInformationTextColumnControlChangedHandler();
		});
	}

	void AddAdditionalInformationTextColumnControlChangedHandler()
	{
		var column = RestrictionAdditionalInformationGrid.Columns.SingleOrDefault(x => x.ColumnName == RestrictionAdditionalInformation.Schema.CY_Data);
		if (column?.ColumnStyle is ZMultiControlColumnStyle multiControlColumnStyle
			&& multiControlColumnStyle.EditControl is ZMultiCombinationControl multiCombinationControl)
		{
			multiCombinationControl.ControlAdded -= AdditionalInformationTextColumnControlChangedHandler;
			multiCombinationControl.ControlAdded += AdditionalInformationTextColumnControlChangedHandler;
		}
	}

	void AdditionalInformationTextColumnControlChangedHandler(object sender, System.EventArgs e)
	{
		if (sender is ZMultiCombinationControl multiCombinationControl
			&& multiCombinationControl.CurrentEditor is ZGridFindBox gridFindBox)
		{
			gridFindBox.CodeBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
		}
	}
}
