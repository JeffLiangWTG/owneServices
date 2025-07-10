using System;
using CargoWise.Common;
using CargoWise.Customs.MX.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.MX.Manifest.Business
{
	internal class BusinessHeaderDocumentWrapper : IBusinessHeaderDocument
	{
		internal BusinessHeaderDocumentWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
		}
		readonly AsycudaBill bill;

		string IBusinessHeaderDocument.ID => bill.ABL_BillNumber;

		string IBusinessHeaderDocument.SignatoryConsignorAuthentication => bill.Header.ShippingAgentName;

		ICarrierAuthentication IBusinessHeaderDocument.CarrierAuthentication => carrierAuthentication ?? (carrierAuthentication = new CarrierAuthenticationWrapper(bill.Header));
		ICarrierAuthentication carrierAuthentication;
	}

	internal class CarrierAuthenticationWrapper : ICarrierAuthentication
	{
		internal CarrierAuthenticationWrapper(AsycudaManifestHeader header)
		{
			this.header = Argument.NotNull(header, "asycudaManifestHeader cannot be null");
		}
		readonly AsycudaManifestHeader header;

		string ICarrierAuthentication.Signatory => header.Carrier?.Header?.OH_FullName ?? ZString.Empty;

		DateTime ICarrierAuthentication.ActualDateTime => MXMessageHelper.SafeDateTime(header.AMA_MasterBillIssueDate);

		string ICarrierAuthentication.AuthenticationLocationName => header.Carrier?.Header?.CityName ?? ZString.Empty;
	}
}
