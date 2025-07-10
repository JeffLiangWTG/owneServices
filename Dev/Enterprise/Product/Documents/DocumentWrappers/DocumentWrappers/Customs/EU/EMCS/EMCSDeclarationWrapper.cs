using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using WTG.StaticAnalysis.Annotation;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.Customs.EU.EMCS
{
	[CodeAlive("Item1/2/3 properties used by reflection in EMCSDeclarationWrapperTest.cs AssertItem")]
	public class EMCSDeclarationWrapper : DocBaseWrapper
	{
		public EMCSDeclarationWrapper(EMCSJobDeclaration declaration, BusinessObjectFactory factory) : base(declaration, factory)
		{
			_ = Argument.NotNull(declaration, nameof(declaration));
			this.declaration = declaration;
		}

		public static EMCSDeclarationWrapper New(BusinessObject businessObject, BusinessObjectFactory factory)
		{
			return businessObject is EMCSJobDeclaration declaration ? new EMCSDeclarationWrapper(declaration, factory) : null;
		}

		readonly EMCSJobDeclaration declaration;

		public ZString Box01ConsignorNameAndAddress => EMCSDeclarationWrapperHelper.GetConsignorData(declaration.SupplierDocumentaryAddress);
		public ZString Box01ConsignorEori => EMCSDeclarationWrapperHelper.GetJobDocAddressData(declaration.SupplierDocumentaryAddress, eori: true);
		public ZString Box02ConsignorExciseNumber => EMCSDeclarationWrapperHelper.GetConsignorData(declaration.SupplierDocumentaryAddress, exciseNumber: true);
		public ZString Box03ReferenceNumber => declaration.JE_DeclarationReference;
		public ZString Box04ConsigneeExciseNumber => EMCSDeclarationWrapperHelper.GetConsigneeData(declaration.ImporterDocumentaryAddress, exciseNumber: true);
		public ZString Box05CommercialUse => ZString.Empty;
		public ZString Box06CommercialUse => ZString.Empty;
		public ZString Box07ConsigneeNameAddressAndNumber => EMCSDeclarationWrapperHelper.GetConsigneeData(declaration.ImporterDocumentaryAddress);
		public ZString Box07ConsigneeNameAndAddress => EMCSDeclarationWrapperHelper.GetJobDocAddressData(declaration.ImporterDocumentaryAddress);
		public ZString Box07ConsigneeEori => EMCSDeclarationWrapperHelper.GetJobDocAddressData(declaration.ImporterDocumentaryAddress, eori: true);
		public ZString Box07APlaceOfDelivery => EMCSDeclarationWrapperHelper.GetJobDocAddressData(declaration.DestinationWarehouseDocumentaryAddress);
		public ZString Box07APlaceOfDeliveryEori => EMCSDeclarationWrapperHelper.GetJobDocAddressData(declaration.DestinationWarehouseDocumentaryAddress, eori: true);
		public ZString Box08CustomsOffice => EMCSDeclarationWrapperHelper.GetCustomsOfficeData(declaration, EuOfficeCodesTypes.Codes.CompetentAuthorityOfDispatch);
		public ZString Box09Transporter => EMCSDeclarationWrapperHelper.GetJobDocAddressData(declaration.TransporterDocumentaryAddress);
		public ZString Box09TransporterEori => EMCSDeclarationWrapperHelper.GetJobDocAddressData(declaration.TransporterDocumentaryAddress, eori: true);
		public ZString Box10Guarantee => declaration.ZG_GuarantorType;
		public ZString Box11OtherTransportDetails => EMCSDeclarationWrapperHelper.GetOtherTransportDetails(declaration);
		public ZString Box12CommercialUse => ZString.Empty;
		public ZString Box13CommercialUse => ZString.Empty;
		public ZString Box14Proprietor => EMCSDeclarationWrapperHelper.GetJobDocAddressData(declaration.OwnerDocumentaryAddress);
		public ZString Box15CommercialUse => ZString.Empty;
		public ZString Box16DateOfRemoval => declaration.JE_DateAtOrigin.ToString(EMCSDeclarationWrapperHelper.DateFormat);
		public ZString Box17CommercialUse => ZString.Empty;

		#region Item1
		public ZString Box18APackagesMarksAndDescription => EMCSDeclarationWrapperHelper.GetPackagesData(declaration, 1);
		public ZString Box19ACommodityCode => EMCSDeclarationWrapperHelper.GetItemCommodityCode(declaration, 1);
		public ZString Box20AQuantity => EMCSDeclarationWrapperHelper.GetItemCustomsQuantityAndUnity(declaration, 1);
		public ZString Box21AGrossMass => EMCSDeclarationWrapperHelper.GetItemMass(declaration, 1);
		public ZString Box22ANetMass => EMCSDeclarationWrapperHelper.GetItemMass(declaration, 1, netMass: true);
		public ZString Box23ACustomsStatus => ZString.Empty;
		public ZString Box24ASoldInWarehouseLabel => Box24ASoldInWarehouseLabelCore;
		protected virtual ZString Box24ASoldInWarehouseLabelCore => SoldInWarehouseResString;
		public ZString Box24ASoldInWarehouse => Box24ASoldInWarehouseCore;
		protected virtual ZString Box24ASoldInWarehouseCore => ZBool.False.ToYesNoString();
		public ZString Box24AProducedInUKLabel => Box24AProducedInUKLabelCore;
		protected virtual ZString Box24AProducedInUKLabelCore => UKProducedResString;
		public ZString Box24AProducedInUK => Box24AProducedInUKCore;
		protected virtual ZString Box24AProducedInUKCore => EMCSDeclarationWrapperHelper.GetItemProducedInUK(declaration, 1);
		#endregion

		#region Item2
		public ZString Box18BPackagesMarksAndDescription => EMCSDeclarationWrapperHelper.GetPackagesData(declaration, 2);
		public ZString Box19BCommodityCode => EMCSDeclarationWrapperHelper.GetItemCommodityCode(declaration, 2);
		public ZString Box20BQuantity => EMCSDeclarationWrapperHelper.GetItemCustomsQuantityAndUnity(declaration, 2);
		public ZString Box21BGrossMass => EMCSDeclarationWrapperHelper.GetItemMass(declaration, 2);
		public ZString Box22BNetMass => EMCSDeclarationWrapperHelper.GetItemMass(declaration, 2, netMass: true);
		public ZString Box23BCustomsStatus => ZString.Empty;
		public ZString Box24BSoldInWarehouseLabel => Box24BSoldInWarehouseLabelCore;
		protected virtual ZString Box24BSoldInWarehouseLabelCore => SoldInWarehouseResString;
		public ZString Box24BSoldInWarehouse => Box24BSoldInWarehouseCore;
		protected virtual ZString Box24BSoldInWarehouseCore => ZBool.False.ToYesNoString();
		public ZString Box24BProducedInUKLabel => Box24BProducedInUKLabelCore;
		protected virtual ZString Box24BProducedInUKLabelCore => UKProducedResString;
		public ZString Box24BProducedInUK => Box24BProducedInUKCore;
		protected virtual ZString Box24BProducedInUKCore => EMCSDeclarationWrapperHelper.GetItemProducedInUK(declaration, 2);
		#endregion

		#region Item3
		public ZString Box18CPackagesMarksAndDescription => EMCSDeclarationWrapperHelper.GetPackagesData(declaration, 3);
		public ZString Box19CCommodityCode => EMCSDeclarationWrapperHelper.GetItemCommodityCode(declaration, 3);
		public ZString Box20CQuantity => EMCSDeclarationWrapperHelper.GetItemCustomsQuantityAndUnity(declaration, 3);
		public ZString Box21CGrossMass => EMCSDeclarationWrapperHelper.GetItemMass(declaration, 3);
		public ZString Box22CNetMass => EMCSDeclarationWrapperHelper.GetItemMass(declaration, 3, netMass: true);
		public ZString Box23CCustomsStatus => ZString.Empty;
		public ZString Box24CSoldInWarehouseLabel => Box24CSoldInWarehouseLabelCore;
		protected virtual ZString Box24CSoldInWarehouseLabelCore => SoldInWarehouseResString;
		public ZString Box24CSoldInWarehouse => Box24CSoldInWarehouseCore;
		protected virtual ZString Box24CSoldInWarehouseCore => ZBool.False.ToYesNoString();
		public ZString Box24CProducedInUKLabel => Box24CProducedInUKLabelCore;
		protected virtual ZString Box24CProducedInUKLabelCore => UKProducedResString;
		public ZString Box24CProducedInUK => Box24CProducedInUKCore;
		protected virtual ZString Box24CProducedInUKCore => EMCSDeclarationWrapperHelper.GetItemProducedInUK(declaration, 3);
		#endregion

		public ZString Box25AdditionalInformation => EMCSDeclarationWrapperHelper.GetAdditionalInformation(declaration);
		public ZString Box26ASignatoryCompanyAndPhone => EMCSDeclarationWrapperHelper.GetSignatoryCompanyAndPhone();
		public ZString Box26BSignatoryName => EMCSDeclarationWrapperHelper.GetSignatoryName(declaration);
		public ZString Box26CSignatoryPlaceAndDate => EMCSDeclarationWrapperHelper.GetSignatoryPlaceAndDate();
		public ZString Box26DSignatorySignature => ZString.Empty;

		public ZString Box05InvoiceNumber => declaration.InvoiceNumber;
		public ZString Box06InvoiceDate => declaration.InvoiceDate.ToString(EMCSDeclarationWrapperHelper.DateFormat);
		public ZString Box08DispatcherNameAddress => EMCSDeclarationWrapperHelper.GetJobDocAddressData(declaration.DispatchWarehouseDocumentaryAddress);
		public ZString Box08DispatcherEORI => EMCSDeclarationWrapperHelper.GetJobDocAddressData(declaration.DispatchWarehouseDocumentaryAddress, eori: true);
		public ZString Box10Guarantor => EMCSDeclarationWrapperHelper.GetGuarantor(declaration);
		public ZString Box12CountryDispatch => declaration.JE_GoodsOrigin;
		public ZString Box13CountryDestination => declaration.JE_GoodsDestination;
		public ZString Box14Representative => EMCSDeclarationWrapperHelper.GetRepresentative(declaration);
		public ZString Box15DispatchOffice => EMCSDeclarationWrapperHelper.GetCustomsOfficeData(declaration, EuOfficeCodesTypes.Codes.OfficeOfDispatch);
		public ZString Box17JourneyTime => $"{declaration.JourneyTimeNumericPart}{declaration.JourneyTimeFormatPart}";
		public ZString BoxZSubmissionReference => ZString.Empty;
		public ZString BoxZImportSAD => EMCSDeclarationWrapperHelper.GetImportSADNumbers(declaration);
		public ZString BoxZThirdCountryOrigin => ZString.Empty;
		public ZString BoxZDestinationOffice => EMCSDeclarationWrapperHelper.GetCustomsOfficeData(declaration, EuOfficeCodesTypes.Codes.OfficeOfDelivery);
		public ZString BoxZDateOfArrival => ZString.Empty;
		public ZString BoxZGlobalConclusion => ZString.Empty;

		public ZString BoxZAdministrativeReferenceCode => BoxZAdministrativeReferenceCodeCore;
		protected virtual ZString BoxZAdministrativeReferenceCodeCore => declaration.EADNumber;

		ZString UKProducedResString => Res.GetString("95E8BED9-ACE2-4491-BA69-5F067BC00FFC", "UK produced?");
		ZString SoldInWarehouseResString => Res.GetString("C203FA20-38DC-48A5-A9FA-9CC4D5828FC3", "24a\r\nSold in\r\nwarehouse?");

		public EMCSInvoiceLineCollection Lines => lines ??= new EMCSInvoiceLineCollection(declaration.InvoiceLines, Factory);
		EMCSInvoiceLineCollection lines;
	}
}
