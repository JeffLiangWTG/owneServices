using System;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	public partial class AttributesUserControl : ZUserControl
	{
		public AttributesUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			using (AttributesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				AttributesGrid.SetAllAvailability(available: false);
				if (DataSource is JobDeclaration declaration)
				{
					if (declaration.IsLPCO)
					{
						AttributesGrid.SetAvailability(available: true, columnsLPCODeclaration);
						AttributesGroupBox.CaptionResourceString = Res.GetData("9D66682F-2C6D-4089-B5D9-61AF21BC973B", "Line Attributes");
					}
					else if (declaration.IsImportOnly)
					{
						var bindingSource = (BindingSource.Current as AttributeCusCodeDataCollection);
						var isTTCE = bindingSource != null && bindingSource.CY_Type == CusCodeDataTypeList.Codes.TaxRegimeAttribute;
						AttributesGrid.SetAvailability(available: true, isTTCE ? columnsTTCEImportDeclaration : columnsNcmAttributes);
					}
					else
					{
						AttributesGrid.SetAvailability(available: true, defaultColumns);
					}
				}
				else if (DataSource is CusGoodsCatalog)
				{
					AttributesGrid.SetAvailability(available: true, columnsNcmAttributes);
				}
				else
				{
					AttributesGrid.SetAvailability(available: true, defaultColumns);
				}
			}
		}

		readonly string[] columnsTTCEImportDeclaration = new[]
		{
			AttributeCusCodeData.Schema.TaxType,
			AttributeCusCodeData.Schema.LegalBase,
			AttributeCusCodeData.Schema.CY_Code,
			AttributeCusCodeData.Schema.Label,
			AttributeCusCodeData.Schema.Content,
			AttributeCusCodeData.Schema.IsMandatory,
			AttributeCusCodeData.Schema.FillOrientation,
			AttributeCusCodeData.Schema.ParentAttributeCode,
		};

		readonly string[] columnsLPCODeclaration = new[]
		{
			AttributeCusCodeData.Schema.CY_Code,
			AttributeCusCodeData.Schema.Label,
			AttributeCusCodeData.Schema.Content,
			AttributeCusCodeData.Schema.IsMandatory,
			AttributeCusCodeData.Schema.FillOrientation,
			AttributeCusCodeData.Schema.Example,
		};

		readonly string[] columnsNcmAttributes = new[]
		{
			AttributeCusCodeData.Schema.CY_Code,
			AttributeCusCodeData.Schema.Label,
			AttributeCusCodeData.Schema.Content,
			AttributeCusCodeData.Schema.IsMandatory,
			AttributeCusCodeData.Schema.FillOrientation,
			AttributeCusCodeData.Schema.ParentAttributeCode,
			AttributeCusCodeData.Schema.ConditionDescription,
			AttributeCusCodeData.Schema.StartDate,
			AttributeCusCodeData.Schema.EndDate,
		};

		readonly string[] defaultColumns = new[]
		{
			AttributeCusCodeData.Schema.CY_Code,
			AttributeCusCodeData.Schema.Label,
			AttributeCusCodeData.Schema.Content,
			AttributeCusCodeData.Schema.IsMandatory,
			AttributeCusCodeData.Schema.FillOrientation
		};
	}
}
