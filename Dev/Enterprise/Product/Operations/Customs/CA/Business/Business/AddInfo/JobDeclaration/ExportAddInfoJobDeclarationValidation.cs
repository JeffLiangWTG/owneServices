using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class ExportAddInfoJobDeclarationValidation : AddInfoJobDeclarationValidation
	{
		public ExportAddInfoJobDeclarationValidation(AddInfoJobDeclaration parent)
			: base(parent)
		{
		}

		protected override void CheckCA_RX_DeclaredCurr()
		{
			base.CheckCA_RX_DeclaredCurr();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_RX_DeclaredCurrInfo, Res.GetString("7739ae57-6d53-47f6-87d4-3725ec2865c9", "Declared Currency"));
			ListValidation.ErrorIfInvalidPK(Parent.CA_RX_DeclaredCurrInfo, Lookups.DeclaredCurrencies, ResString.GetMultilingualString("5424069a-869f-4302-9783-4fdc9569d2f6", "Declared Currency"));
		}

		protected override void CheckCA_PortOfExit()
		{
			base.CheckCA_PortOfExit();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_PortOfExitInfo, Lookups.CBSAOffices, Res.GetString("4a71713b-c0d6-40a1-94fa-3ed39547ffbf", "Port Of Exit"));
		}

		protected override void CheckCA_PlaceOfReport()
		{
			base.CheckCA_PlaceOfReport();
			if ((Parent.Parent.IsG7ExportDeclaration && !Parent.CA_PlaceOfReport.IsEmpty) || !Parent.Parent.IsG7ExportDeclaration)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_PlaceOfReportInfo, Lookups.CBSAOffices, Res.GetString("c0f347cb-a83e-416a-9f7b-761f01b6915a", "Place Of Report"));
			}
		}

		protected override void CheckCA_TransportDocumentNumber()
		{
			base.CheckCA_TransportDocumentNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_TransportDocumentNumberInfo, Res.GetString("badbe943-58d1-4963-9516-41505f63734e", "Transport Document Number"));
			if (Parent.CA_TransportDocumentNumber.Length < 5)
			{
				Parent.CA_TransportDocumentNumberInfo.AddMessageError(invalidCCNMessage);
			}
		}

		internal static string invalidCCNMessage
		{
			get
			{
				return Res.GetString("f3e10224-2817-459d-8032-1d6ba2732837", @"The transport document number (CCN) is invalid; it should be a valid carrier code followed by either a file or booking reference number.
If a valid carrier code is not known at the time of export declaration indicate 77YY followed by the applicable number.");
			}
		}

		protected override void CheckCA_ReasonForExportCode()
		{
			base.CheckCA_ReasonForExportCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_ReasonForExportCodeInfo, Lookups.ReasonForExportCodes);
		}
	}
}
