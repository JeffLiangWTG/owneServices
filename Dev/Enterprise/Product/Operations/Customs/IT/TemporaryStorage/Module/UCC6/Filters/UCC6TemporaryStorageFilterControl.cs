using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

public partial class UCC6TemporaryStorageFilterControl : EU.TemporaryStorage.Module.UCC6TemporaryStorageFilterControl
{
	static class ITColumnNameConstants
	{
		public const string DeclarantCode = "Declarant+Header+OH_Code";
		public const string DeclarantName = "Declarant+CompanyName";
		public const string LocationOfGoodsAuthorizationNo = "GoodsLocation+Address+AuthorisationNumber";
		public const string LocationOfGoodsOrganizationAddress = "GoodsLocation+Address+IdentificationHolder+MainAddress+OA_Address1";
		public const string LocationOfGoodsOrganizationCode = "GoodsLocation+Address+IdentificationHolder+OH_Code";
		public const string LocationOfGoodsOrganizationName = "GoodsLocation+Address+IdentificationHolder+OH_FullNameTruncated";
		public const string LocationOfGoodsPlaceId = "GoodsLocation+CGL_AdditionalIdentifier";
		public const string RepresentativeCode = "Representative+Header+OH_Code";
		public const string RepresentativeName = "Representative+CompanyName";
		public const string CustomsStatusDate = "CustomsStatusDate";
		public const string CustomsStatusDescription = "CustomsStatusDescription";
		public const string MessageStatusDescription = "MessageStatusDescription";
	}

	[Obsolete("Use the constructor that takes a collection and filter strip, this constructor is just for the designer", true)]
	public UCC6TemporaryStorageFilterControl()
	{
		InitializeComponent();
	}

	public UCC6TemporaryStorageFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
	{
		InitializeComponent();
		UpdateGrid();
	}

	#region Grid Modification Routines & Helper Subroutines

	void UpdateGrid()
	{
		ModifyColumns();
		grid.ColumnStyles.AddRange(NewColumns);
		RemoveColumns();
	}

	void ModifyColumns()
	{
		var colMap = ModifiedColumns.ToDictionary(x => x.ColumnName);
		for (var i = 0; i < grid.ColumnStyles.Count; i++)
		{
			if (grid.ColumnStyles[i] is ZGridColumnInfo colInfo && colMap.TryGetValue(colInfo.ColumnName, out var newColInfo))
			{
				grid.ColumnStyles[i] = newColInfo;
			}
		}
	}

	void RemoveColumns()
	{
		RemoveColumn(CusEntryNumberTypes.Standard.LocalReferenceNumber);
		RemoveColumn(CusEntryNumberTypes.Standard.MovementReferenceNumber);
	}

	void RemoveColumn(string columnName)
	{
		var column = FindColumn(columnName);
		if (column is not null)
		{
			grid.ColumnStyles.Remove(column);
		}
	}

	ZGridColumnInfo FindColumn(string colName) => grid.ColumnStyles.OfType<ZGridColumnInfo>().FirstOrDefault(c => c.ColumnName == colName);

	int ScaledWidth(int w) => CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(w);

	#endregion

	#region Column Group Names

	ResourceStringData DeclarantGroupName => Res.GetData("A1B2C3D4-E5F6-4789-ABCD-1234567890EF", "Declarant");
	ResourceStringData LocationOfGoodsOrganizationGroupName => Res.GetData("843C218B-5CAC-4D6D-9586-55DCD4E26191", "Location of Goods: Organization");
	ResourceStringData RepresentativeGroupName => Res.GetData("B2C3D4E5-F678-49AB-BCDE-2345678901FA", "Representative");

	#endregion

	#region New columns to add into the grid

	List<ZGridColumnInfo> NewColumns =>
	[
		new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("C1D2E3F4-5678-9ABC-DEF1-1234567890A1", "Declarant Code"),
			ColumnName = ITColumnNameConstants.DeclarantCode,
			GroupName = DeclarantGroupName,
			Width = ScaledWidth(120)
		},
		new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("D2E3F4G5-6789-ABCD-EF12-2345678901B2", "Declarant Name"),
			ColumnName = ITColumnNameConstants.DeclarantName,
			GroupName = DeclarantGroupName,
			Width = ScaledWidth(120)
		},
		new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("E3F4G5H6-789A-BCDE-F123-3456789012C3", "Representative Code"),
			ColumnName = ITColumnNameConstants.RepresentativeCode,
			GroupName = RepresentativeGroupName,
			Width = ScaledWidth(120)
		},
		new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("F4G5H6I7-89AB-CDEF-1234-4567890123D4", "Representative Name"),
			ColumnName = ITColumnNameConstants.RepresentativeName,
			GroupName = RepresentativeGroupName,
			Width = ScaledWidth(120)
		},
		new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("C67CDAF1-8E1D-4835-A41B-D8D308E335E4", "Organization Code"),
			ColumnName = ITColumnNameConstants.LocationOfGoodsOrganizationCode,
			GroupName = LocationOfGoodsOrganizationGroupName,
			Width = ScaledWidth(120)
		},
		new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("E63E1BF8-5EC5-4F9A-9C81-7C096F60DB49", "Organization Name"),
			ColumnName = ITColumnNameConstants.LocationOfGoodsOrganizationName,
			GroupName = LocationOfGoodsOrganizationGroupName,
			Width = ScaledWidth(120)
		},
		new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("17108822-276A-48CB-B86E-C3E6AA0B9E25", "Organization Address"),
			ColumnName = ITColumnNameConstants.LocationOfGoodsOrganizationAddress,
			GroupName = LocationOfGoodsOrganizationGroupName,
			Width = ScaledWidth(120)
		},
		new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("17952401-47BF-4742-BF50-28E2B474873F", "Authorization No."),
			ColumnName = ITColumnNameConstants.LocationOfGoodsAuthorizationNo,
			Width = ScaledWidth(120)
		},
		new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("C5F47499-1258-42A7-A879-72204D762323", "Place ID"),
			ColumnName = ITColumnNameConstants.LocationOfGoodsPlaceId,
			Width = ScaledWidth(120)
		},
		new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("F4G5H6I7-89AB-CDEF-1234-4567890123K1", "Message Status"),
			ColumnName = AsycudaManifestHeaderSchema.Constants.AMA_MessageStatus,
			Width = ScaledWidth(100)
		},
		new ZArchitecture.ZDateEditColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("F4G5H6I7-89AB-CDEF-1234-456789012J87", "Customs Status Date"),
			ColumnName = ITColumnNameConstants.CustomsStatusDate,
			Width = ScaledWidth(100),
			DateTimeFormat = ZDateTimePickerFormat.Short
		},
		new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("F4G5H6I7-89AB-CDEF-1234-4567890123L9", "Customs Status Description"),
			ColumnName = ITColumnNameConstants.CustomsStatusDescription,
			Width = ScaledWidth(120)
		},
		new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("F4G5H6I7-89AB-CDEF-1234-4567390128P9", "Message Status Description"),
			ColumnName = ITColumnNameConstants.MessageStatusDescription,
			Width = ScaledWidth(120)
		},
		new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("2C74AF9D-689C-4AF4-A8E2-3E6DFEC10C28", "Repres. Qual."),
			ColumnName = AsycudaManifestHeaderSchema.Constants.AMA_AgentType,
			Width = ScaledWidth(120)
		}
	];

	#endregion

	#region Existing columns to update in the grid

	List<ZGridColumnInfo> ModifiedColumns =>
	[
		new ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("E5F6789A-BCDE-4CDE-EFGH-5678901234FD", "Declarant Address"),
			ColumnName = AsycudaManifestHeaderSchema.Constants.AMA_OA_Declarant,
			GroupName = DeclarantGroupName,
			Width = ScaledWidth(120)
		},
		new ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("2B3C4D5E-6789-ABCD-EF01-2345678901BB", "Representative Address"),
			ColumnName = AsycudaManifestHeaderSchema.Constants.AMA_OA_Representative,
			GroupName = RepresentativeGroupName,
			Width = ScaledWidth(120)
		},
		new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("E5F6789A-BCDE-4CDE-EFGH-5678901234CI", "Supervising Customs Office"),
			ColumnName = AsycudaManifestHeaderSchema.Constants.AMA_CustomsOffice,
			Width = ScaledWidth(120)
		}
	];

	#endregion
}
