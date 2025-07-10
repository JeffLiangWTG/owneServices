using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Integration.Customs.IN;
using ResString = Enterprise.Customs.IN.Business.ResString;

namespace Enterprise.Customs.IN.Registry;

public sealed class INCustomsDataRegistry : RegistryItemSet, IINCustomsDataRegistry
{
	public static INCustomsDataRegistry Instance
	{
		get { return instance ??= new INCustomsDataRegistry(); }
	}

	[ThreadStatic]
	static INCustomsDataRegistry instance;

	INCustomsDataRegistry()
	{
	}

	public abstract class Categories : RawDataRegistry.Categories
	{
		public static MultilingualString Customs_India =>
			CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("AFA79425-B7AF-40BA-8F06-A28D866AFDEC", "India"));

		public static MultilingualString Customs_India_CGM =>
			CombineCategories(Customs_India, ResString.GetMultilingualString("F1188C31-D60E-42BA-8147-20F3365EE430", "CGM"));

		public static MultilingualString Customs_India_IGM =>
			CombineCategories(Customs_India, ResString.GetMultilingualString("8826ABE2-E9FF-4054-B5C6-577BCB0EA62C", "IGM"));
	}

	IRegistryItem IINCustomsDataRegistry.INEnableConsolGeneralManifest => INEnableConsolGeneralManifest;

	IRegistryItem IINCustomsDataRegistry.INEnableImportGeneralManifest => INEnableImportGeneralManifest;

	public override bool IsForProductivityWise => false;

	public StringRegistryItem INConsolAgentRegistrationNumber
	{
		get
		{
			return GetItem("INConsolAgentRegistrationNumber", () => new StringRegistryItem(
				"INConsolAgentRegistrationNumber",
				Categories.Customs_India_CGM,
				ResString.GetMultilingualString("BB72BBF7-37EA-4579-85B1-F17D4768D674", "CARN – Consol Agent Registration Number"),
				ResString.GetMultilingualString("EB9656CE-A673-4873-9C72-F451C59E06D0", "This is the Control Agent Registration Number to be used for CGM Custom Message"),
				new AlphaNumericCodeRegistryDataType(0, 16),
				new TextRegistryEditorInfo(TextEditorType.TextBox),
				RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				""));
		}
	}

	public BooleanRegistryItem INEnableConsolGeneralManifest
	{
		get
		{
			return GetItem("INEnableConsolGeneralManifest", () => new BooleanRegistryItem(
				"INEnableConsolGeneralManifest",
				Categories.Customs_India_CGM,
				ResString.GetMultilingualString("1BDE6898-8ECA-4ACA-A89A-5D2DDFE87C4D", "Enable CGM (Consol General Manifest)"),
				ResString.GetMultilingualString("FF09618B-071D-477A-9540-B7EA66E4F608", "Enable CGM (Consol General Manifest)"),
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers,
				false));
		}
	}

	public BooleanRegistryItem INEnableImportGeneralManifest
	{
		get
		{
			return GetItem("INEnableImportGeneralManifest", () => new BooleanRegistryItem(
				"INEnableImportGeneralManifest",
				Categories.Customs_India_IGM,
				ResString.GetMultilingualString("2DEF3AEB-3B6B-47AC-B817-659403E2D367", "Enable IGM (Import General Manifest)"),
				ResString.GetMultilingualString("641C9016-68CD-471C-95A8-58AA8708D933", "Enable IGM (Import General Manifest)"),
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers,
				false));
		}
	}

	public StringRegistryItem INCHALicenseNumber
	{
		get
		{
			return GetItem("INCHALicenseNumber", () => new StringRegistryItem(
				"INCHALicenseNumber",
				Categories.Customs_India,
				ResString.GetMultilingualString("B567BFE91-ADC0-4CA4-82A6-CC74595AB01A", "CHA - Customs House Agent License Number"),
				ResString.GetMultilingualString("26C654BC-B2CD-48E3-837E-11F0A5E5216F", "This is a CHA's License Number for Export and Import Declaration"),
				new StringRegistryDataType(15, 15),
				new TextRegistryEditorInfo(TextEditorType.TextBox),
				RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				""));
		}
	}

	protected override void SetDefaultsForNewItem(IRegistryItem item)
	{
		base.SetDefaultsForNewItem(item);
		item.CountryFilterPKs = CountryFilterPKs.India;
	}
}
