namespace Enterprise.Customs.GB.GUI.MessageSending
{
	partial class EnhancedValidationForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EnhancedValidationForm));
            this.TitleLabel = new Enterprise.ZArchitecture.ZLabel();
            this.SelectedLabel = new Enterprise.ZArchitecture.ZLabel();
            this.TakeAMomentLabel = new Enterprise.ZArchitecture.ZLabel();
            this.AboutToSubmitLabel = new Enterprise.ZArchitecture.ZLabel();
            this.CommodityLine1Label = new Enterprise.ZArchitecture.ZLabel();
            this.CommodityLine2Label = new Enterprise.ZArchitecture.ZLabel();
            this.CommodityLine3Label = new Enterprise.ZArchitecture.ZLabel();
            this.CommodityLine4Label = new Enterprise.ZArchitecture.ZLabel();
            this.CommodityLine5Label = new Enterprise.ZArchitecture.ZLabel();
            this.TaxOverridesLabel = new Enterprise.ZArchitecture.ZLabel();
            this.ManualTaxOverrideLabel = new Enterprise.ZArchitecture.ZLabel();
            this.TaxLineLabel = new Enterprise.ZArchitecture.ZLabel();
            this.TaxLine1Label = new Enterprise.ZArchitecture.ZLabel();
            this.TaxLine2Label = new Enterprise.ZArchitecture.ZLabel();
            this.TaxLine3Label = new Enterprise.ZArchitecture.ZLabel();
            this.TaxLine4Label = new Enterprise.ZArchitecture.ZLabel();
            this.TaxLine5Label = new Enterprise.ZArchitecture.ZLabel();
            this.ConfirmLabel = new Enterprise.ZArchitecture.ZLabel();
            this.ClassificationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.CountryCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.ManualTaxCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.ButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.GoBackButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.SubmitButton = new Enterprise.ZArchitecture.GUI.ZButton();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ButtonPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 610, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 24, true);
            this.MainStatusBar.Visible = false;
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Business.EnhancedValidationEntryWrapper);
            // 
            // TitleLabel
            // 
            this.TitleLabel.AutoSize = true;
            this.TitleLabel.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("f17c896c-ce29-403a-a807-f5725d000f61", "HMRC Digital Prompts");
            this.TitleLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.TitleLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Largest;
            this.TitleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.TitleLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.TitleLabel.Name = "TitleLabel";
            this.TitleLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, true);
            this.TitleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 35, true);
            this.TitleLabel.TabIndex = 1;
            this.TitleLabel.UseMnemonic = false;
            // 
            // SelectedLabel
            // 
            this.SelectedLabel.AutoSize = true;
            this.SelectedLabel.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("69853444-3a16-428b-9fba-e002d97538ab", "This submission has been selected for an additional validation process, the purpo" +
        "se of which is to reduce incorrect data entry.");
            this.SelectedLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.SelectedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.SelectedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 35, true);
            this.SelectedLabel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 0, true);
            this.SelectedLabel.Name = "SelectedLabel";
            this.SelectedLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, 4, 8, 4, true);
            this.SelectedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(519, 34, true);
            this.SelectedLabel.TabIndex = 2;
            this.SelectedLabel.UseMnemonic = false;
            // 
            // TakeAMomentLabel
            // 
            this.TakeAMomentLabel.AutoSize = true;
            this.TakeAMomentLabel.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("a9eb05eb-fe9a-4d09-a257-d30b7131c6f6", "Please take a moment to double check that the classification process for commodit" +
        "ies and their origins has been correctly applied.");
            this.TakeAMomentLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.TakeAMomentLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.TakeAMomentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 69, true);
            this.TakeAMomentLabel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 0, true);
            this.TakeAMomentLabel.Name = "TakeAMomentLabel";
            this.TakeAMomentLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, 4, 8, 4, true);
            this.TakeAMomentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(523, 34, true);
            this.TakeAMomentLabel.TabIndex = 3;
            this.TakeAMomentLabel.UseMnemonic = false;
            // 
            // AboutToSubmitLabel
            // 
            this.AboutToSubmitLabel.AutoSize = true;
            this.AboutToSubmitLabel.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("237fc6dc-bf95-4717-825b-672b847ddfb0", "You are about to submit to CDS for the following data (only the first 5 are shown" +
        "). Please tick the boxes at the end of the list to confirm you have followed the" +
        " correct classification process.");
            this.AboutToSubmitLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.AboutToSubmitLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.AboutToSubmitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 103, true);
            this.AboutToSubmitLabel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 0, true);
            this.AboutToSubmitLabel.Name = "AboutToSubmitLabel";
            this.AboutToSubmitLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, 4, 8, 4, true);
            this.AboutToSubmitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(521, 34, true);
            this.AboutToSubmitLabel.TabIndex = 4;
            this.AboutToSubmitLabel.UseMnemonic = false;
            // 
            // CommodityLine1Label
            // 
            this.CommodityLine1Label.AutoSize = true;
			this.CommodityLine1Label.Dock = System.Windows.Forms.DockStyle.Top;
            this.CommodityLine1Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.CommodityLine1Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 137, true);
            this.CommodityLine1Label.Name = "CommodityLine1Label";
            this.CommodityLine1Label.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(32, 4, 8, 0, true);
            this.CommodityLine1Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 17, true);
            this.CommodityLine1Label.TabIndex = 5;
            this.CommodityLine1Label.UseMnemonic = false;
            // 
            // CommodityLine2Label
            // 
            this.CommodityLine2Label.AutoSize = true;
            this.CommodityLine2Label.Dock = System.Windows.Forms.DockStyle.Top;
            this.CommodityLine2Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.CommodityLine2Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 154, true);
            this.CommodityLine2Label.Name = "CommodityLine2Label";
            this.CommodityLine2Label.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(32, 4, 8, 0, true);
            this.CommodityLine2Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 17, true);
            this.CommodityLine2Label.TabIndex = 6;
            this.CommodityLine2Label.UseMnemonic = false;
            // 
            // CommodityLine3Label
            // 
            this.CommodityLine3Label.AutoSize = true;
            this.CommodityLine3Label.Dock = System.Windows.Forms.DockStyle.Top;
            this.CommodityLine3Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.CommodityLine3Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 171, true);
            this.CommodityLine3Label.Name = "CommodityLine3Label";
            this.CommodityLine3Label.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(32, 4, 8, 0, true);
            this.CommodityLine3Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 17, true);
            this.CommodityLine3Label.TabIndex = 7;
            this.CommodityLine3Label.UseMnemonic = false;
            // 
            // CommodityLine4Label
            // 
            this.CommodityLine4Label.AutoSize = true;
            this.CommodityLine4Label.Dock = System.Windows.Forms.DockStyle.Top;
            this.CommodityLine4Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.CommodityLine4Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 188, true);
            this.CommodityLine4Label.Name = "CommodityLine4Label";
            this.CommodityLine4Label.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(32, 4, 8, 0, true);
            this.CommodityLine4Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 17, true);
            this.CommodityLine4Label.TabIndex = 8;
            this.CommodityLine4Label.UseMnemonic = false;
            // 
            // CommodityLine5Label
            // 
            this.CommodityLine5Label.AutoSize = true;
            this.CommodityLine5Label.Dock = System.Windows.Forms.DockStyle.Top;
            this.CommodityLine5Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.CommodityLine5Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 205, true);
            this.CommodityLine5Label.Name = "CommodityLine5Label";
            this.CommodityLine5Label.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(32, 4, 8, 0, true);
            this.CommodityLine5Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 17, true);
            this.CommodityLine5Label.TabIndex = 9;
            this.CommodityLine5Label.UseMnemonic = false;
            // 
            // TaxOverridesLabel
            // 
            this.TaxOverridesLabel.AutoSize = true;
            this.TaxOverridesLabel.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("4d3a1f5f-55d4-4eb0-bb4d-cb4e02af8a69", "Tax overrides");
            this.TaxOverridesLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.TaxOverridesLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Larger;
            this.TaxOverridesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 222, true);
            this.TaxOverridesLabel.Name = "TaxOverridesLabel";
            this.TaxOverridesLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, 20, 8, 4, true);
            this.TaxOverridesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 41, true);
            this.TaxOverridesLabel.TabIndex = 10;
            this.TaxOverridesLabel.UseMnemonic = false;
            // 
            // ManualTaxOverrideLabel
            // 
            this.ManualTaxOverrideLabel.AutoSize = true;
			this.ManualTaxOverrideLabel.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("0c45d6b0-8710-4bf9-b9e4-421b8024ac9f", "Unlike in CHIEF, it is hardly ever necessary to apply a tax override (a manual ta" +
		"x calculation) for CDS. This is mainly because CDS is able to request more data " +
		"from you than CHIEF did, meaning it is more capable of calculating tax itself.");
			this.ManualTaxOverrideLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.ManualTaxOverrideLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.ManualTaxOverrideLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 263, true);
            this.ManualTaxOverrideLabel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 0, true);
            this.ManualTaxOverrideLabel.Name = "ManualTaxOverrideLabel";
            this.ManualTaxOverrideLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, 4, 8, 4, true);
            this.ManualTaxOverrideLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 47, true);
            this.ManualTaxOverrideLabel.TabIndex = 11;
            this.ManualTaxOverrideLabel.UseMnemonic = false;
            // 
            // TaxLineLabel
            // 
            this.TaxLineLabel.AutoSize = true;
            this.TaxLineLabel.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("08262687-dbd5-453a-bd21-a6335a800f03", "You have applied a manual tax override for the following commodities and tax type" +
        "s:");
            this.TaxLineLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.TaxLineLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.TaxLineLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 310, true);
            this.TaxLineLabel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 0, true);
            this.TaxLineLabel.Name = "TaxLineLabel";
            this.TaxLineLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, 4, 8, 4, true);
            this.TaxLineLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(417, 21, true);
            this.TaxLineLabel.TabIndex = 12;
            this.TaxLineLabel.UseMnemonic = false;
            // 
            // TaxLine1Label
            // 
            this.TaxLine1Label.AutoSize = true;
            this.TaxLine1Label.Dock = System.Windows.Forms.DockStyle.Top;
			this.TaxLine1Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.TaxLine1Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 331, true);
            this.TaxLine1Label.Name = "TaxLine1Label";
            this.TaxLine1Label.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(32, 4, 8, 0, true);
            this.TaxLine1Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 17, true);
            this.TaxLine1Label.TabIndex = 13;
            this.TaxLine1Label.UseMnemonic = false;
            // 
            // TaxLine2Label
            // 
            this.TaxLine2Label.AutoSize = true;
            this.TaxLine2Label.Dock = System.Windows.Forms.DockStyle.Top;
            this.TaxLine2Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.TaxLine2Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 348, true);
            this.TaxLine2Label.Name = "TaxLine2Label";
            this.TaxLine2Label.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(32, 4, 8, 0, true);
            this.TaxLine2Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 17, true);
            this.TaxLine2Label.TabIndex = 14;
            this.TaxLine2Label.UseMnemonic = false;
            // 
            // TaxLine3Label
            // 
            this.TaxLine3Label.AutoSize = true;
            this.TaxLine3Label.Dock = System.Windows.Forms.DockStyle.Top;
            this.TaxLine3Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.TaxLine3Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 365, true);
            this.TaxLine3Label.Name = "TaxLine3Label";
            this.TaxLine3Label.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(32, 4, 8, 0, true);
            this.TaxLine3Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 17, true);
            this.TaxLine3Label.TabIndex = 15;
            this.TaxLine3Label.UseMnemonic = false;
            // 
            // TaxLine4Label
            // 
            this.TaxLine4Label.AutoSize = true;
            this.TaxLine4Label.Dock = System.Windows.Forms.DockStyle.Top;
            this.TaxLine4Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.TaxLine4Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 382, true);
            this.TaxLine4Label.Name = "TaxLine4Label";
            this.TaxLine4Label.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(32, 4, 8, 0, true);
            this.TaxLine4Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 17, true);
            this.TaxLine4Label.TabIndex = 16;
            this.TaxLine4Label.UseMnemonic = false;
            // 
            // TaxLine5Label
            // 
            this.TaxLine5Label.AutoSize = true;
            this.TaxLine5Label.Dock = System.Windows.Forms.DockStyle.Top;
            this.TaxLine5Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.TaxLine5Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 399, true);
            this.TaxLine5Label.Name = "TaxLine5Label";
            this.TaxLine5Label.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(32, 4, 8, 0, true);
            this.TaxLine5Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 17, true);
            this.TaxLine5Label.TabIndex = 17;
            this.TaxLine5Label.UseMnemonic = false;
            // 
            // ConfirmLabel
            // 
            this.ConfirmLabel.AutoSize = true;
            this.ConfirmLabel.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("d40e8adf-9479-4313-b47c-5630291f44f6", "I confirm that I have:");
            this.ConfirmLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.ConfirmLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.ConfirmLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 416, true);
            this.ConfirmLabel.Name = "ConfirmLabel";
            this.ConfirmLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, 20, 8, 4, true);
            this.ConfirmLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 37, true);
            this.ConfirmLabel.TabIndex = 18;
            this.ConfirmLabel.UseMnemonic = false;
            // 
            // ClassificationCheckBox
            // 
            this.ClassificationCheckBox.AutoSize = true;
            this.ClassificationCheckBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("3cb8bfa4-1324-4532-874c-405656f3e9e5", "Classified these goods correctly");
            this.ClassificationCheckBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.ClassificationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 453, true);
            this.ClassificationCheckBox.Name = "ClassificationCheckBox";
            this.ClassificationCheckBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(32, 4, 0, 4, true);
            this.ClassificationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 25, true);
            this.ClassificationCheckBox.TabIndex = 19;
            this.ClassificationCheckBox.UseVisualStyleBackColor = true;
            this.ClassificationCheckBox.CheckedChanged += new System.EventHandler(this.CheckBoxCheckedChanged);
            // 
            // CountryCheckBox
            // 
            this.CountryCheckBox.AutoSize = true;
            this.CountryCheckBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("ad09baba-8dd9-4b2f-b5eb-c85d382fa076", "Selected the right origin country by applying the correct rules of origin");
            this.CountryCheckBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.CountryCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 478, true);
            this.CountryCheckBox.Name = "CountryCheckBox";
            this.CountryCheckBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(32, 4, 0, 4, true);
            this.CountryCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 25, true);
            this.CountryCheckBox.TabIndex = 20;
            this.CountryCheckBox.UseVisualStyleBackColor = true;
            this.CountryCheckBox.CheckedChanged += new System.EventHandler(this.CheckBoxCheckedChanged);
            // 
            // ManualTaxCheckBox
            // 
            this.ManualTaxCheckBox.AutoSize = true;
            this.ManualTaxCheckBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("6fe97f82-31de-455e-af10-d76b969c0877", "Not unnecessarily requested that a manual tax calculation is applied");
            this.ManualTaxCheckBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.ManualTaxCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 503, true);
            this.ManualTaxCheckBox.Name = "ManualTaxCheckBox";
            this.ManualTaxCheckBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(32, 4, 0, 4, true);
            this.ManualTaxCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 25, true);
            this.ManualTaxCheckBox.TabIndex = 21;
            this.ManualTaxCheckBox.UseVisualStyleBackColor = true;
            this.ManualTaxCheckBox.CheckedChanged += new System.EventHandler(this.CheckBoxCheckedChanged);
            // 
            // ButtonPanel
            // 
            this.ButtonPanel.Controls.Add(this.GoBackButton);
            this.ButtonPanel.Controls.Add(this.SubmitButton);
            this.ButtonPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.ButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 528, true);
            this.ButtonPanel.Name = "ButtonPanel";
            this.ButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 56, true);
            this.ButtonPanel.TabIndex = 22;
            // 
            // GoBackButton
            // 
            this.GoBackButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.GoBackButton.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("0df021e4-f081-430d-9f27-d9da5f8aaf44", "Go back and double check");
            this.GoBackButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.GoBackButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(244, 17, true);
            this.GoBackButton.Name = "GoBackButton";
            this.GoBackButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 23, true);
            this.GoBackButton.TabIndex = 25;
            this.GoBackButton.ToolTipCaption = null;
            this.GoBackButton.UseVisualStyleBackColor = true;
            // 
            // SubmitButton
            // 
            this.SubmitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SubmitButton.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("6b4075a1-0bcc-4bd0-ad93-41bdc4638210", "Submit");
            this.SubmitButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.SubmitButton.Enabled = false;
            this.SubmitButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 17, true);
            this.SubmitButton.Name = "SubmitButton";
            this.SubmitButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
            this.SubmitButton.TabIndex = 24;
            this.SubmitButton.ToolTipCaption = null;
            this.SubmitButton.UseVisualStyleBackColor = true;
            // 
            // EnhancedValidationForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("9b16b94c-5197-4491-baa7-79bb7128f1d4", "Enhanced Validation");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 634, true);
            this.Controls.Add(this.ButtonPanel);
            this.Controls.Add(this.ManualTaxCheckBox);
            this.Controls.Add(this.CountryCheckBox);
            this.Controls.Add(this.ClassificationCheckBox);
            this.Controls.Add(this.ConfirmLabel);
            this.Controls.Add(this.TaxLine5Label);
            this.Controls.Add(this.TaxLine4Label);
            this.Controls.Add(this.TaxLine3Label);
            this.Controls.Add(this.TaxLine2Label);
            this.Controls.Add(this.TaxLine1Label);
            this.Controls.Add(this.TaxLineLabel);
            this.Controls.Add(this.ManualTaxOverrideLabel);
            this.Controls.Add(this.TaxOverridesLabel);
            this.Controls.Add(this.CommodityLine5Label);
            this.Controls.Add(this.CommodityLine4Label);
            this.Controls.Add(this.CommodityLine3Label);
            this.Controls.Add(this.CommodityLine2Label);
            this.Controls.Add(this.CommodityLine1Label);
            this.Controls.Add(this.AboutToSubmitLabel);
            this.Controls.Add(this.TakeAMomentLabel);
            this.Controls.Add(this.SelectedLabel);
            this.Controls.Add(this.TitleLabel);
            this.DataSourceType = typeof(Enterprise.Customs.GB.Business.EnhancedValidationEntryWrapper);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(540, 0, true);
            this.Name = "EnhancedValidationForm";
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.TitleLabel, 0);
            this.Controls.SetChildIndex(this.SelectedLabel, 0);
            this.Controls.SetChildIndex(this.TakeAMomentLabel, 0);
            this.Controls.SetChildIndex(this.AboutToSubmitLabel, 0);
            this.Controls.SetChildIndex(this.CommodityLine1Label, 0);
            this.Controls.SetChildIndex(this.CommodityLine2Label, 0);
            this.Controls.SetChildIndex(this.CommodityLine3Label, 0);
            this.Controls.SetChildIndex(this.CommodityLine4Label, 0);
            this.Controls.SetChildIndex(this.CommodityLine5Label, 0);
            this.Controls.SetChildIndex(this.TaxOverridesLabel, 0);
            this.Controls.SetChildIndex(this.ManualTaxOverrideLabel, 0);
            this.Controls.SetChildIndex(this.TaxLineLabel, 0);
            this.Controls.SetChildIndex(this.TaxLine1Label, 0);
            this.Controls.SetChildIndex(this.TaxLine2Label, 0);
            this.Controls.SetChildIndex(this.TaxLine3Label, 0);
            this.Controls.SetChildIndex(this.TaxLine4Label, 0);
            this.Controls.SetChildIndex(this.TaxLine5Label, 0);
            this.Controls.SetChildIndex(this.ConfirmLabel, 0);
            this.Controls.SetChildIndex(this.ClassificationCheckBox, 0);
            this.Controls.SetChildIndex(this.CountryCheckBox, 0);
            this.Controls.SetChildIndex(this.ManualTaxCheckBox, 0);
            this.Controls.SetChildIndex(this.ButtonPanel, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ButtonPanel.ResumeLayout(false);
            this.ButtonPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel TitleLabel;
		private ZArchitecture.ZLabel SelectedLabel;
		private ZArchitecture.ZLabel TakeAMomentLabel;
		private ZArchitecture.ZLabel AboutToSubmitLabel;
		private ZArchitecture.ZLabel CommodityLine1Label;
		private ZArchitecture.ZLabel CommodityLine2Label;
		private ZArchitecture.ZLabel CommodityLine3Label;
		private ZArchitecture.ZLabel CommodityLine4Label;
		private ZArchitecture.ZLabel CommodityLine5Label;
		private ZArchitecture.ZLabel TaxOverridesLabel;
		private ZArchitecture.ZLabel ManualTaxOverrideLabel;
		private ZArchitecture.ZLabel TaxLineLabel;
		private ZArchitecture.ZLabel TaxLine1Label;
		private ZArchitecture.ZLabel TaxLine2Label;
		private ZArchitecture.ZLabel TaxLine3Label;
		private ZArchitecture.ZLabel TaxLine4Label;
		private ZArchitecture.ZLabel TaxLine5Label;
		private ZArchitecture.ZLabel ConfirmLabel;
		private ZArchitecture.GUI.ZCheckBox ClassificationCheckBox;
		private ZArchitecture.GUI.ZCheckBox CountryCheckBox;
		private ZArchitecture.GUI.ZCheckBox ManualTaxCheckBox;
		private ZArchitecture.GUI.ZPanel ButtonPanel;
		private ZArchitecture.GUI.ZButton GoBackButton;
		private ZArchitecture.GUI.ZButton SubmitButton;
	}
}
