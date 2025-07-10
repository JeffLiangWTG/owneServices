using System;
using CargoWise.Common;
using CargoWise.Customs.AR.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.AR.Manifest.Business
{
	internal class CarrierAuthenticationWrapper : ICarrierAuthentication
	{
		internal CarrierAuthenticationWrapper(AsycudaManifestHeader header)
		{
			this.header = Argument.NotNull(header, "asycudaManifestHeader cannot be null");
		}
		readonly AsycudaManifestHeader header;

		string ICarrierAuthentication.Signatory => header.Carrier?.Header?.OH_FullName ?? ZString.Empty;

		DateTime ICarrierAuthentication.ActualDateTime => ARHelperClass.SafeDateTime(header.AMA_MasterBillIssueDate);

		string ICarrierAuthentication.AuthenticationLocationName => header.Carrier?.Header?.CityName ?? ZString.Empty;
	}
}
