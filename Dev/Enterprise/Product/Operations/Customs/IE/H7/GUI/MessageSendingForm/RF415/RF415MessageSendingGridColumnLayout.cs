using Enterprise.Customs.IE.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.H7.GUI
{
	class RF415MessageSendingGridColumnLayout : IGridColumnLayoutProvider
	{
		public IGridColumnLayout Layout => layout ??= CreateLayout();
		IGridColumnLayout layout;

		IGridColumnLayout CreateLayout()
		{
			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn<ZCheckBoxColumnStyleInfo>(RF415MessageSendingObject.SchemaShouldSend, 40);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(RF415MessageSendingObject.MovementReferenceNumber), 160);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(RF415MessageSendingObject.BillNumber), 160);
			builder.AddColumn<ZDropEditColumnStyleInfo>(nameof(RF415MessageSendingObject.RefundType), 50);
			builder.AddColumn<ZCodeFindBoxColumnStyleInfo>(nameof(RF415MessageSendingObject.OfficeOfDebt), 160);
			builder.AddColumn<ZCodeFindBoxColumnStyleInfo>(nameof(RF415MessageSendingObject.OfficeOfResponsibility), 160);
			builder.AddColumn<ZDropEditColumnStyleInfo>(nameof(RF415MessageSendingObject.LegalBasis), 160);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(RF415MessageSendingObject.DescriptionOfGrounds), 160);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(RF415MessageSendingObject.BankDetails), 160);
			builder.AddColumn<ZCalcEditColumnStyleInfo>(nameof(RF415MessageSendingObject.Amount), 100);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(RF415MessageSendingObject.AdditionalInformation), 160);

			return builder.Build();
		}
	}
}
