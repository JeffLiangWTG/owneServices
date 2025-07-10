using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public class G5V1TemporaryStorageDetailsLayout : IPanelLayoutProvider
	{
		public G5V1TemporaryStorageDetailsLayout()
		{
			layout = CreateLayout();
		}

		readonly PanelLayout layout;
		PanelLayout IPanelLayoutProvider.Layout => layout;

		PanelLayout CreateLayout()
		{
			var builder = new TemporaryStorageDetailsLayoutBuilder<TemporaryStorageHeader>();
			var esBag = builder.CommonBag;

			builder.AddControlBag(esBag);

			builder.AddColumn();
			builder.Add(esBag.LAMEEntryNumberTextBox, ControlWidthClass.Auto);
			builder.Add(esBag.LRNTextBox, ControlWidthClass.Auto);
			builder.Add(esBag.MRNTextBox, ControlWidthClass.Auto);
			builder.Add(esBag.CustomsStatusDropEdit, ControlWidthClass.Auto);
			builder.Add(esBag.MessageStatusDropEdit, ControlWidthClass.Auto);
			builder.Add(esBag.CircuitTextBox, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(esBag.LAMEEntryDateDateEdit, ControlWidthClass.Auto);
			builder.Add(esBag.AcceptanceDateDateEdit, ControlWidthClass.Auto);
			builder.Add(esBag.ClearanceNumberTextBox, ControlWidthClass.Auto);
			builder.Add(esBag.DsdtSdFormatHasUrlUserControl, ControlWidthClass.Auto);
			builder.Add(esBag.DsdtSdFormatNoUrlUserControl, ControlWidthClass.Auto);
			builder.Add(esBag.DsdtMrnBindingMemberUserControl, ControlWidthClass.Auto);
			builder.Add(esBag.DsdtMrnNumberTextBox, ControlWidthClass.Auto);

			builder.SetVisibility(esBag.MessageStatusDropEdit, h => !h.IsMessageTypeManual, h => h.AMA_MessageTypeInfo);
			builder.SetVisibility(esBag.CircuitTextBox, h => !h.IsMessageTypeManual, h => h.AMA_MessageTypeInfo);
			builder.SetVisibility(esBag.ClearanceNumberTextBox, h => !h.IsMessageTypeManual, h => h.AMA_MessageTypeInfo);
			builder.SetVisibility(esBag.DsdtSdFormatHasUrlUserControl, IsDsdtSdFormatHasUrlUserControlVisible, h => h.AMA_MessageTypeInfo);
			builder.SetVisibility(esBag.DsdtSdFormatNoUrlUserControl, IsDsdtSdFormatNoUrlUserControlVisible, h => h.AMA_MessageTypeInfo);
			builder.SetVisibility(esBag.DsdtMrnBindingMemberUserControl, IsDsdtSdFormatWritableUserControlVisible, h => h.AMA_MessageTypeInfo, h => h.DsdtMrnNumberInfo);

			builder.SetVisibility(esBag.LRNTextBox, h => !h.IsMessageTypeLAM, h => h.AMA_MessageTypeInfo);
			builder.SetVisibility(esBag.MRNTextBox, h => !h.IsMessageTypeLAM, h => h.AMA_MessageTypeInfo);
			builder.SetVisibility(esBag.AcceptanceDateDateEdit, h => !h.IsMessageTypeLAM, h => h.AMA_MessageTypeInfo);

			builder.SetVisibility(esBag.DsdtMrnNumberTextBox, h => !h.IsMessageTypeManual || (h.IsMessageTypeTSM && h.DsdtSummaryDeclarationUrl.IsEmpty), h => h.AMA_MessageTypeInfo, h => h.DsdtMrnNumberInfo);

			builder.SetVisibility(esBag.LAMEEntryNumberTextBox, h => h.IsMessageTypeLAM, h => h.AMA_MessageTypeInfo);
			builder.SetVisibility(esBag.LAMEEntryDateDateEdit, h => h.IsMessageTypeLAM, h => h.AMA_MessageTypeInfo);

			builder.SetCaption(esBag.DsdtMrnNumberTextBox, GetDsdtMrnNumberTextBoxCaption, l => l.AMA_MessageTypeInfo);
			builder.SetCaption(esBag.MRNTextBox, GetMrnTextBoxCaption, l => l.AMA_MessageTypeInfo, l => l.UnionGoodsInfo);
			builder.SetCaption(esBag.AcceptanceDateDateEdit, GetAcceptanceDateDateEditCaption, l => l.AMA_MessageTypeInfo, l => l.UnionGoodsInfo);

			return builder.Build();
		}

		bool IsDsdtSdFormatHasUrlUserControlVisible(TemporaryStorageHeader h) => IsDsdtSdFormatUserControlVisibleBase(h) && !h.DsdtSummaryDeclarationUrl.IsEmpty;

		bool IsDsdtSdFormatNoUrlUserControlVisible(TemporaryStorageHeader h) => IsDsdtSdFormatUserControlVisibleBase(h) && h.DsdtSummaryDeclarationUrl.IsEmpty;

		bool IsDsdtSdFormatUserControlVisibleBase(TemporaryStorageHeader h) => !h.IsMessageTypeManual && h.IsNotReadOnlyMemberWhenMessageTypeTSMAndCustomsStatusEmpty;

		bool IsDsdtSdFormatWritableUserControlVisible(TemporaryStorageHeader h) => !h.IsMessageTypeLAM && (!h.IsNotReadOnlyMemberWhenMessageTypeTSMAndCustomsStatusEmpty || (h.IsMessageTypeTSM && !h.CustomsStatus.IsEmpty)) && !h.DsdtSummaryDeclarationUrl.IsEmpty;

		ResourceStringData GetDsdtMrnNumberTextBoxCaption(TemporaryStorageHeader tsHeader)
		{
			if (!IsDsdtSdFormatUserControlVisibleBase(tsHeader) && !IsDsdtSdFormatWritableUserControlVisible(tsHeader))
			{
				return Res.GetData("A22A8FE5-041D-4C34-A0F9-B61214D54C97", englishCaption: "DSDT (SD Format)", englishMediumCaption: "DSDT (SD Format)", englishShortCaption: "DSDT (SD Format)", englishFullDescription: "DSDT (Summary Declaration Format)");
			}
			else
			{
				return Res.GetData("737A8BC4-CDA7-497A-8D85-7519D28A7D58", englishCaption: "DSDT MRN", englishMediumCaption: "DSDT MRN", englishShortCaption: "DSDT MRN", englishFullDescription: "DSDT MRN (Summary Declaration)");
			}
		}

		ResourceStringData GetMrnTextBoxCaption(TemporaryStorageHeader tsHeader) => tsHeader.IsMessageTypeTSMAndIsUnionGoods
			? Res.GetData("3D3615E0-6E18-4674-9808-B89AC30D2044", "Entry Reference")
			: Res.GetData("7DA23B03-3D15-482A-93FA-C82EE14060E9", "MRN");

		ResourceStringData GetAcceptanceDateDateEditCaption(TemporaryStorageHeader tsHeader) => tsHeader.IsMessageTypeTSMAndIsUnionGoods
			? Res.GetData("5406D23B-61D6-4434-9A20-2918EFF69303", "Entry Date")
			: Res.GetData("B68B1BA8-E259-4031-99C1-B951D738F9E1", "Acceptance Date");
	}
}
