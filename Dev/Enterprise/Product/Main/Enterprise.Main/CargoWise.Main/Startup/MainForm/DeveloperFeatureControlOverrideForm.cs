using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.FeatureControl;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Startup;

public partial class DeveloperFeatureControlOverrideForm : ZChildForm
{
	readonly FeatureDataToBind FeatureDatas = new FeatureDataToBind(new FeatureDataCollection());
	bool isReset;
	public DeveloperFeatureControlOverrideForm()
	{
		InitializeComponent();
		SetButtonsText();
	}

	void SetButtonsText()
	{
		this.btnCancel.Text = ResString_Cancel;
		this.btnSave.Text = ResString_Save;
		this.btnReset.Text = ResString_Reset;
		this.btnSelectAll.Text = ResString_SelectAll;
		this.btnSelectNone.Text = ResString_SelectNone;
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		this.Size = new System.Drawing.Size(ControlDpiScalingHelper.ScaleToCurrentDpiX(970), ControlDpiScalingHelper.ScaleToCurrentDpiY(620));
		this.SetDataBinding(this.FeatureDatas, "");
		PopulateDataSource(false);
		foreach (var data in FeatureDatas.Data)
		{
			data.HasChanges = false;
		}
	}

	void SelectAll_Click(object sender, EventArgs e)
	{
		foreach (FeatureData data in FeatureDatas.Data)
		{
			if (!data.Enabled)
			{
				data.Enabled = true;
				data.HasChanges = true;
			}
		}
	}

	void SelectNone_Click(object sender, EventArgs e)
	{
		foreach (FeatureData data in FeatureDatas.Data)
		{
			if (data.Enabled)
			{
				data.Enabled = false;
			}
		}
	}

	void Save_Click(object sender, EventArgs e)
	{
		var hasChanges = DataSourceModified();
		if (isReset && !hasChanges)
		{
			ClearOverrideXML();
		}
		else
		{
			SaveToXmlOverride();
		}
		Close();
		if (hasChanges || isReset)
		{
			ObjectFactory.Get<IProgramRestarter>().ShutdownEnterpriseWithMessage(CargoWise.Main.Res.GetString("237d4eab-f078-409f-b88e-a5fabafb827b", "CargoWise tester must restart application after modify feature!"));
		}
	}

	void Cancel_Click(object sender, EventArgs e)
	{
		Close();
	}

	void Reset_Click(object sender, EventArgs e)
	{
		isReset = true;
		PopulateDataSource(true);
		FeatureDatas.Data.ForEach(s => s.HasChanges = false);
	}

	void PopulateDataSource(bool isReset)
	{
		var featureControlManager = ObjectFactory.Get<IFeatureControlManager>();
		var codes = LicenceFeatureCodeList.GetLicenceFeatureCodePairs();

		var hasOverride = !WebDataRegistry.Instance.FeatureControlRuleContent.Value.IsNullOrEmpty();
		var featureDataCollection = new FeatureDataCollection();
		foreach (var code in codes)
		{
			bool defaultValue;
			string parameter = null;
			if (hasOverride && !isReset)
			{
				var featureData = featureControlManager.GetFeatureData(code.Code);
				defaultValue = featureData != null;
				parameter = featureData?.Parameter;
			}
			else
			{
				defaultValue = code.Stage == LicenceFeatureCodeList.FeatureStage.Active;
			}
			var featureData1 = new FeatureData();
			featureData1.Code = code.Code;
			featureData1.Description = code.Description;
			featureData1.Enabled = defaultValue;
			featureData1.Parameter = parameter;
			featureData1.FeatureStage = code.Stage == LicenceFeatureCodeList.FeatureStage.Development ? CargoWise.Main.ResString.GetMultilingualString("d6ba9d7e-3f1b-433c-900f-382cebabcdab", "Under Development") : CargoWise.Main.ResString.GetMultilingualString("880a082a-6618-45a3-b4b7-a4b60e4a4709", "Ready for Production");
			featureDataCollection.Add(featureData1);
		}
		FeatureDatas.Data.RemoveAll();
		foreach (var featureData in featureDataCollection.OrderBy(s => s.FeatureStage).ThenBy(s => s.Code))
		{
			FeatureDatas.Data.Add(featureData);
		}
	}

	void ClearOverrideXML()
	{
		WebDataRegistry.Instance.FeatureControlRuleContent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
	}

	void DeveloperFeatureControlOverrideForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (DataSourceModified())
		{
			string msg = (NoResString)"Features have been changed. Are you sure you want to close and lose changes?";
			var result = DialogResult.Yes == Globals.Message.Show(msg, (NoResString)"Confirm Close?", MessageBoxButtons.YesNo, MessageBoxIcon.Stop, DialogResult.No);
			if (!result)
			{
				e.Cancel = true;
			}
		}
	}

	#region
	static string ResString_SelectAll => CargoWise.Main.Res.GetString("a18bae64-de51-45c4-b396-412ec7e6486a", "Select All");
	static string ResString_SelectNone => CargoWise.Main.Res.GetString("0f7a983c-67d9-47de-842d-d0bbc8406c47", "Select None");
	static string ResString_Save => CargoWise.Main.Res.GetString("68b51087-f51a-4823-966a-12777c5bcf08", "Save");
	static string ResString_Cancel => CargoWise.Main.Res.GetString("9d4dd6b8-519d-4f6c-92d8-a5a006407603", "Cancel");
	static string ResString_Reset => CargoWise.Main.Res.GetString("5355075e-672c-4255-9bfd-29a4c89b9fdb", "Reset");
	#endregion
#if DEBUG
	public static void EnablePartOfFeaturesForTest()
	{
		var utcNowDate = ZDateTime.UtcNow;
		var featureControlList = new List<FeatureControlRule>();
		var featureControl = new FeatureControl { TimestampUtc = utcNowDate.ToDateTime() };
		featureControlList.Add(AddRule(LicenceFeatureCodeList.Codes.AccountingFrenchEReporting, true, utcNowDate.AddDays(-3).Date, utcNowDate.AddDays(10).Date, string.Empty));
		var base64String = GetCompressedBase64String(featureControl);
		var featureControlRuleRepository = ObjectFactory.Get<IFeatureControlRuleRepository>();
		featureControlRuleRepository.SaveFeatureControlRuleContent(Convert.FromBase64String(base64String));
	}
#endif
	bool DataSourceModified() => FeatureDatas.Data.Any(s => s.HasChanges);

	void SaveToXmlOverride()
	{
		var featureControlRuleRepository = ObjectFactory.Get<IFeatureControlRuleRepository>();

		var utcNowDate = ZDateTime.UtcNow;

		var featureControl = new FeatureControl { TimestampUtc = utcNowDate.ToDateTime() };
		var featureControlRuleList = new List<FeatureControlRule>();

		foreach (FeatureData data in FeatureDatas.Data)
		{
			if (data.Enabled)
			{
				featureControlRuleList.Add(AddRule(data.Code, isGlobal: true, utcNowDate.AddDays(-3).Date, utcNowDate.AddDays(10).Date, data.Parameter));
			}

			data.HasChanges = false;
		}

		featureControl.Rules = featureControlRuleList.ToArray();

		var base64String = GetCompressedBase64String(featureControl);
		featureControlRuleRepository.SaveFeatureControlRuleContent(Convert.FromBase64String(base64String));
	}

	static FeatureControlRule AddRule(string ruleCode, bool isGlobal, ZDate start, ZDate? end, string parameter)
	{
		if (parameter.IsNullOrEmpty())
		{
			parameter = "{}";
		}

		var rule = new FeatureControlRule();
		rule.FCM_FeatureControlCode = ruleCode;
		rule.FCR_RuleType = isGlobal ? FeatureControlRuleFCR_RuleType.GLB : FeatureControlRuleFCR_RuleType.CLI;
		rule.FCR_StartDateUtc = start.ToDateTime();
		if (end.HasValue)
		{
			rule.FCR_EndDateUtc = end.Value.ToDateTime();
		}
		rule.FCR_Parameters = parameter;

		return rule;
	}

	public static string GetCompressedBase64String(FeatureControl featureControl)
	{
		using (var ms = new MemoryStream())
		{
			using (var zipStream = new GZipStream(ms, CompressionMode.Compress))
			{
				var xml = Serialize(featureControl);
				var xmlBytes = Encoding.UTF8.GetBytes(xml);
				zipStream.Write(xmlBytes, 0, xmlBytes.Length);
			}
			return Convert.ToBase64String(ms.ToArray());
		}
	}

	static string Serialize(FeatureControl featureControl)
	{
		using var stream = new MemoryStream();
		var xmlSerializer = new XmlSerializer(typeof(FeatureControl), string.Empty);
		xmlSerializer.Serialize(stream, featureControl);
		return StreamToString(stream);
	}

	static string StreamToString(Stream stream)
	{
		var position = stream.Position;
		if (stream.CanSeek)
		{
			stream.Position = 0;
		}

		using var streamReader = new StreamReader(stream, true);
		var result = streamReader.ReadToEnd();
		stream.Position = position;

		return result;
	}
}
