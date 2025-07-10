using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public sealed class AdditionalInformationDetailsLayout : IPanelLayoutProvider
	{
		public AdditionalInformationDetailsLayout()
		{
			Layout = CreateLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreateLayout()
		{
			var builder = new AdditionalInformationDetailsLayoutBuilder();
			var commonBag = builder.CommonBag;
			builder.AddColumn();
			builder.Add(commonBag.KindDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.FullTypeCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ReferenceTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.DescriptionTextBox, ControlWidthClass.Long);

			builder.SetCaption(commonBag.KindDropEdit, GetKindDropEditCaption, p => p.CSI_SubTypeInfo);
			builder.SetCaption(commonBag.FullTypeCodeFindBox, GetFullTypeCodeFindBoxCaption, p => p.CSI_SubTypeInfo);
			builder.SetCaption(commonBag.ReferenceTextBox, GetReferenceTextBoxCaption, p => p.CSI_SubTypeInfo);
			builder.SetCaption(commonBag.DescriptionTextBox, GetDescriptionTextBoxCaption, p => p.CSI_SubTypeInfo);

			return builder.Build();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Translated below")]
		const string KindDropEditCaption = "Kind";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Translated below")]
		const string FullTypeCodeFindBoxCaption = "Full Type";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Translated below")]
		const string ReferenceTextBoxCaption = "Reference";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Translated below")]
		const string DescriptionTextBoxCaption = "Description";

		ResourceStringData GetKindDropEditCaption(AdditionalInfo info)
		{
			switch (info.CSI_SubType.ToUpper())
			{
				case AdditionalInfoSubTypeList.Codes.AdditionalReference:
					return Res.GetData("666D0DEF-09C3-4FAC-B077-D1E45F81F5F5", KindDropEditCaption, "Additional Reference – Kind");
				case AdditionalInfoSubTypeList.Codes.TransportDocument:
					return Res.GetData("6AF7ECCF-2B9F-458E-8DC1-2599020D798E", KindDropEditCaption, "Transport Document – Kind");
				case AdditionalInfoSubTypeList.Codes.AdditionalInformation:
					return Res.GetData("F7C227B0-1810-4D12-996B-CA324292E074", KindDropEditCaption, "Additional Information – Kind");
				default:
					return Res.GetData("7553AEF3-3454-41E5-8B76-DFC9A905EAA0", KindDropEditCaption);
			}
		}

		ResourceStringData GetFullTypeCodeFindBoxCaption(AdditionalInfo info)
		{
			switch (info.CSI_SubType.ToUpper())
			{
				case AdditionalInfoSubTypeList.Codes.AdditionalReference:
					return Res.GetData("E880C6C3-8A2D-4DE1-AB98-BD36F6874492", FullTypeCodeFindBoxCaption, "Additional Reference – [12 04 002 000] Type");
				case AdditionalInfoSubTypeList.Codes.TransportDocument:
					return Res.GetData("8C980E1C-914C-4252-8A62-88A2D2093421", FullTypeCodeFindBoxCaption, "Transport Document – [12 05 002 000] Type");
				case AdditionalInfoSubTypeList.Codes.AdditionalInformation:
					return Res.GetData("123B6FCB-867C-4ABE-8015-9DFFF8D4A349", FullTypeCodeFindBoxCaption, "Additional Information – [12 12 008 000] Code");
				default:
					return Res.GetData("658A6920-D8B0-4252-949D-51318AE02EE9", FullTypeCodeFindBoxCaption);
			}
		}

		ResourceStringData GetReferenceTextBoxCaption(AdditionalInfo info)
		{
			switch (info.CSI_SubType.ToUpper())
			{
				case AdditionalInfoSubTypeList.Codes.AdditionalReference:
					return Res.GetData("846EAEDA-3999-4EC3-B530-3702562DAA50", ReferenceTextBoxCaption, "Additional Reference – [12 04 001 000] Reference Number");
				case AdditionalInfoSubTypeList.Codes.TransportDocument:
					return Res.GetData("A5514275-572F-4F84-A057-9F632B3E6F13", ReferenceTextBoxCaption, "Transport Document – [12 05 001 000] Reference Number");
				default:
					return Res.GetData("22D397BF-7235-427B-9C29-7C988231F9F6", ReferenceTextBoxCaption);
			}
		}

		ResourceStringData GetDescriptionTextBoxCaption(AdditionalInfo info)
		{
			switch (info.CSI_SubType.ToUpper())
			{
				case AdditionalInfoSubTypeList.Codes.AdditionalInformation:
					return Res.GetData("F4A7066A-5F23-449C-BA3D-1A0DD6B42140", DescriptionTextBoxCaption, "Additional Information – [12 02 009 000] Text");
				default:
					return Res.GetData("EA4B80EE-707B-4AAE-8119-E44230C0ACA1", DescriptionTextBoxCaption);
			}
		}
	}
}
