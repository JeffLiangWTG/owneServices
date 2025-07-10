using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class DeclarationFormLayoutProvider : IDeclarationFormLayoutProvider
	{
		IPanelLayoutProvider IDeclarationFormLayoutProvider.GetDeclarationDetailsLayout() => null;

		IPanelLayoutProvider IDeclarationFormLayoutProvider.GetDeclarationOrganisationsLayout() => new OrganisationsLayout();

		IPanelLayoutProvider IDeclarationFormLayoutProvider.GetDeclarationShipmentTypeLayout() => new ShipmentTypeLayout();

		IPanelLayoutProvider IDeclarationFormLayoutProvider.GetDeclarationShipmentDetailsLayout() => new ShipmentDetailsLayout();

		IPanelLayoutProvider IDeclarationFormLayoutProvider.GetDeclarationTransportDetailsLayout(BaseJobDeclaration declaration)
		{
			var jobDeclaration = (JobDeclaration)declaration;

			switch (declaration.JE_MessageType)
			{
				case Common.KR.KRJobMessageTypeList.Codes.Export:
					return new EXPDeclarationTransportDetailsLayout(jobDeclaration);
				case Common.KR.KRJobMessageTypeList.Codes.LocalExport:
					return new LEXDeclarationTransportDetailsLayout(jobDeclaration);
				case Common.KR.KRJobMessageTypeList.Codes.Import:
					return new IMPDeclarationTransportDetailsLayout(jobDeclaration);
				default:
					return null;
			}
		}

		IPanelLayoutProvider IDeclarationFormLayoutProvider.GetMiscOptionsLayout(BaseJobDeclaration declaration)
		{
			switch (declaration.JE_MessageType)
			{
				case Common.KR.KRJobMessageTypeList.Codes.Export:
					return new MiscOptionsLayout();
				case Common.KR.KRJobMessageTypeList.Codes.LocalExport:
					return new LocalExportMiscOptionsLayout();
				case Common.KR.KRJobMessageTypeList.Codes.Import:
					return new ImportMiscOptionsLayout();
				default:
					return null;
			}
		}
		IPanelLayoutProvider IDeclarationFormLayoutProvider.GetCommercialInvoiceDetailsLayout(BaseJobDeclaration declaration)
		{
			switch (declaration.JE_MessageType)
			{
				case Common.KR.KRJobMessageTypeList.Codes.Export:
					return new ExportCommercialInvoiceDetailsLayout();
				case Common.KR.KRJobMessageTypeList.Codes.LocalExport:
					return new LocalExportCommercialInvoiceDetailsLayout();
				case Common.KR.KRJobMessageTypeList.Codes.Import:
					return new ImportCommercialInvoiceDetailsLayout();
				default:
					return null;
			}
		}

		IPanelLayoutProvider IDeclarationFormLayoutProvider.GetInstructionDetailsLayoutProvider(BaseJobDeclaration declaration) => null;

		IPanelLayoutProvider IDeclarationFormLayoutProvider.GetInvoiceLineCalculationsPanelLayout(BaseJobDeclaration declaration) => null;

		IGridColumnLayoutProvider IDeclarationFormLayoutProvider.GetInvoiceChargesGridColumnLayout(BaseJobDeclaration declaration) => null;

		IGridColumnLayoutProvider IDeclarationFormLayoutProvider.GetApportionedInvoiceChargesGridColumnLayout(BaseJobDeclaration declaration) => null;

		IGridColumnLayoutProvider IDeclarationFormLayoutProvider.GetGroupChargesGridColumnLayout(BaseJobDeclaration declaration) => null;
	}
}
