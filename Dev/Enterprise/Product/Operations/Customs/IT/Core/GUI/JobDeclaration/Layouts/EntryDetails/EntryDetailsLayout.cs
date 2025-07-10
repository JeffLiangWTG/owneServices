using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

sealed class EntryDetailsLayout : IPanelLayoutProvider
{
	#region IPanelLayoutProvider

	PanelLayout IPanelLayoutProvider.Layout => layout ?? (layout = CreateLayout());
	PanelLayout layout;

	#endregion

	#region Implementation

	PanelLayout CreateLayout()
	{
		var builder = new EntryDetailsLayoutBuilder();
		var commonBag = builder.CommonBag;
		var itBag = EntryDetailsControlBag.Instance;
		builder.AddControlBag(itBag);

		builder.AddColumn();
		AddControlsToFirstColumn(builder, commonBag, itBag);

		builder.AddColumn();
		AddControlsToSecondColumn(builder, itBag);

		builder.AddColumn();
		AddControlsToThirdColumn(builder, itBag);

		SetCaptions(builder, itBag);
		return builder.Build();
	}

	void SetCaptions(EntryDetailsLayoutBuilder builder, EntryDetailsControlBag itBag)
	{
		builder.SetCaption(itBag.EntryTypeTextBox, _ => Res.GetData("39EFB855-263F-4D17-93A9-D6F4F578DD65", "Entry Type"));
		builder.SetCaption(itBag.ControlChannelDropEdit, _ => Res.GetData("8F079343-4367-4CFA-AAD3-4ABAB43DE38E", "Control Channel"));
		builder.SetCaption(itBag.RegistrationNumberTextBox, _ => Res.GetData("A585D9BF-962F-4310-A2EF-059BDF20DB9C", "Registration No."));
		builder.SetCaption(itBag.ExitDateDateEdit, _ => Res.GetData("58EEC5BF-5F6D-4C37-A51F-BC38C7E5D480", "Exit Date"));
		builder.SetCaption(itBag.SubmittedDateDateEdit, _ => Res.GetData("8299BCA5-7F1F-42EE-AA91-5749343402CC", "Submitted Date"));
		builder.SetCaption(itBag.MRNTextBox, _ => Res.GetData("111AEE3C-7E80-4880-83B5-9B622C1BA85F", "MRN"));
		builder.SetCaption(itBag.ReleaseDateDateEdit, _ => Res.GetData("9F6831C2-16FD-4624-9AE1-DE6968B29B1B", "Release Date"));
		builder.SetCaption(itBag.EntryStatusDropEdit, _ => Res.GetData("A74F42BB-4C73-47FB-94B1-9B6BC365091C", "Entry Status"));
		builder.SetCaption(itBag.ReleaseCodeTextBox, GetReleaseCodeCaption, d => d.JE_MessageTypeInfo, d => d.MessageVersionInfo);
	}

	ResourceStringData GetReleaseCodeCaption(JobDeclaration declaration)
	{
		return declaration.IsUCC6AndIsExport
			? Res.GetData("7D54E32A-5126-4596-8BFD-49E5BE18B7D7", "Release Code")
			: Res.GetData("3ABBD90E-A158-4676-9B20-D8EB668D22A2", "Reference Number");
	}

	void AddControlsToFirstColumn(CommonEntryDetailsLayoutBuilder<JobDeclaration> builder,
		CommonEntryDetailsControlBag commonBag,
		EntryDetailsControlBag itBag)
	{
		builder.Add(itBag.ReferenceLabel, ControlWidthClass.LongNoCaption);
		builder.Add(itBag.EntryTypeTextBox, ControlWidthClass.Auto);
		builder.Add(itBag.ReferenceNumberTextBox, ControlWidthClass.Auto);
		builder.Add(itBag.IssueDateDateEdit, ControlWidthClass.Auto);
		builder.Add(itBag.IncotermTextBox, ControlWidthClass.Auto);

		builder.Add(itBag.TotalsLabel, ControlWidthClass.LongNoCaption, alignToControl: itBag.CustomsLabel);
		builder.Add(commonBag.NoPacksCalcEdit, ControlWidthClass.Auto);
		builder.Add(itBag.GrossWeightUserControl, ControlWidthClass.Auto);
		builder.Add(itBag.NetWeightUserControl, ControlWidthClass.Auto);
		builder.Add(itBag.CustomsQuantityUserControl, ControlWidthClass.Auto);
		builder.Add(itBag.InvoiceAmountUserControl, ControlWidthClass.Auto);

		builder.Add(itBag.FreightAdjustmentCalcEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.DutyCalcEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.VatCalcEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.EntryLinesCountCalcEdit, ControlWidthClass.Auto);
	}

	void AddControlsToSecondColumn(CommonEntryDetailsLayoutBuilder<JobDeclaration> builder, EntryDetailsControlBag itBag)
	{
		builder.Add(itBag.StatusLabel, ControlWidthClass.LongNoCaption);
		builder.Add(itBag.MessageStatusUserControl, ControlWidthClass.Auto);
		builder.Add(itBag.EntryStatusDropEdit, ControlWidthClass.Auto);
		builder.Add(itBag.ControlChannelDropEdit, ControlWidthClass.Auto);
		builder.Add(itBag.MessageTypeDropEdit, ControlWidthClass.Auto);
		builder.Add(itBag.WarehouseStatusDropEdit, ControlWidthClass.Auto);

		builder.Add(itBag.CustomsLabel, ControlWidthClass.LongNoCaption);
		builder.Add(itBag.RegistrationNumberTextBox, ControlWidthClass.Auto);
		builder.Add(itBag.CustomsOfficeTextBox, ControlWidthClass.Auto);
		builder.Add(itBag.SubmittedDateDateEdit, ControlWidthClass.Auto);
		builder.Add(itBag.MRNTextBox, ControlWidthClass.Auto);
		builder.Add(itBag.ReleaseCodeTextBox, ControlWidthClass.Auto);
		builder.Add(itBag.ReleaseDateDateEdit, ControlWidthClass.Auto);

		builder.Add(itBag.A93Label, ControlWidthClass.LongNoCaption);
		builder.Add(itBag.A93Grid, ControlWidthClass.LongNoCaption);
	}

	void AddControlsToThirdColumn(CommonEntryDetailsLayoutBuilder<JobDeclaration> builder, EntryDetailsControlBag itBag)
	{
		builder.Add(itBag.ExitLabel, ControlWidthClass.LongNoCaption);
		builder.Add(itBag.ExitDateDateEdit, ControlWidthClass.Auto);
		builder.Add(itBag.ExitOfficeUserControl, ControlWidthClass.Auto);
		builder.Add(itBag.ExitStatusUserControl, ControlWidthClass.Auto);
	}

	#endregion
}
