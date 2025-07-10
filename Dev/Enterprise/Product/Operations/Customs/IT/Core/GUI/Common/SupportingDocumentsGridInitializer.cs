using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public class SupportingDocumentsGridInitializer
{
	public SupportingDocumentsGridInitializer(ZGrid grid)
	{
		this.grid = Argument.NotNull(grid, nameof(grid));
	}

	readonly ZGrid grid;

	public void Initialize()
	{
		SetUpCSI_CodeColumn();
		SetUpCSI_ReferenceNumberColumn();
		SetUpCSI_QuantityColumn();
		SetUpCSI_UnitOfQuantityColumn();
		SetUpCSI_YearOfIssueColumn();
		SetUpCSI_RN_NKCountryCodeColumn();

		SetUpCSI_StatusColumn();

		SetUpCSI_DateOfExpiryColumn();
		SetUpCSI_ReferenceNumber2Column();
		SetUpCSI_ValueColumn();
		SetUpCSI_RX_NKCurrencyColumn();
		SetUpCSI_LineNoColumn();
	}

	#region Implementation

	void SetUpCSI_CodeColumn() => SetUpColumn<ZCodeFindBoxColumnStyleInfo>(SupportingDocument.Schema.CSI_Code, 100, CharacterCasing.Upper);

	void SetUpCSI_ReferenceNumberColumn() => SetUpColumn<ZTextBoxColumnStyleInfo>(SupportingDocument.Schema.CSI_ReferenceNumber, 100, CharacterCasing.Normal);

	void SetUpCSI_StatusColumn() => SetUpColumn<ZDropEditColumnStyleInfo>(SupportingDocument.Schema.CSI_Status, 100, CharacterCasing.Upper);

	void SetUpCSI_QuantityColumn() => SetUpColumn<ZCalcEditColumnStyleInfo>(SupportingDocument.Schema.CSI_Quantity, 100, CharacterCasing.Upper);

	void SetUpCSI_UnitOfQuantityColumn() => SetUpColumn<ZTextBoxColumnStyleInfo>(SupportingDocument.Schema.CSI_UnitOfQuantity, 100, CharacterCasing.Upper);

	void SetUpCSI_YearOfIssueColumn() => SetUpColumn<ZTextBoxColumnStyleInfo>(SupportingDocument.Schema.CSI_YearOfIssue, 100, CharacterCasing.Upper);

	void SetUpCSI_RN_NKCountryCodeColumn() => SetUpColumn<ZCodeFindBoxColumnStyleInfo>(SupportingDocument.Schema.CSI_RN_NKCountryCode, 100, CharacterCasing.Upper);

	void SetUpCSI_DateOfExpiryColumn() => SetUpColumn<ZDateEditColumnStyleInfo>(SupportingDocument.Schema.CSI_DateOfExpiry, 100, CharacterCasing.Upper);

	void SetUpCSI_ReferenceNumber2Column() => SetUpColumn<ZTextBoxColumnStyleInfo>(SupportingDocument.Schema.CSI_ReferenceNumber2, 100, CharacterCasing.Upper);

	void SetUpCSI_ValueColumn() => SetUpColumn<ZCalcEditColumnStyleInfo>(SupportingDocument.Schema.CSI_Value, 100, CharacterCasing.Upper);

	void SetUpCSI_RX_NKCurrencyColumn() => SetUpColumn<ZCodeFindBoxColumnStyleInfo>(SupportingDocument.Schema.CSI_RX_NKCurrency, 100, CharacterCasing.Upper);

	void SetUpCSI_LineNoColumn()
	{
		var calcEditColumnStyle = (ZCalcEditColumnStyleInfo)SetUpColumn<ZCalcEditColumnStyleInfo>(SupportingDocument.Schema.CSI_LineNo, 100, CharacterCasing.Upper);
		calcEditColumnStyle.MaxLengthOverride = SupportingDocument.Schema.CSI_LineNoMaxLength;
	}

	protected ZGridColumnInfo SetUpColumn<TColumnInfo>(ZString columnName, ZInt width, CharacterCasing characterCasing)
		where TColumnInfo : ZGridColumnInfo, new()
	{
		var columnStyle = GetColumnStyles().SingleOrDefault(x => x.ColumnName == columnName);
		if (columnStyle == null)
		{
			columnStyle = new TColumnInfo();
			columnStyle.ColumnName = columnName;
			grid.ColumnStyles.Add(columnStyle);
		}
		columnStyle.CharacterCasing = characterCasing;
		columnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(width);
		return columnStyle;
	}

	IEnumerable<ZGridColumnInfo> GetColumnStyles() => grid.ColumnStyles.Cast<ZGridColumnInfo>();

	#endregion
}
