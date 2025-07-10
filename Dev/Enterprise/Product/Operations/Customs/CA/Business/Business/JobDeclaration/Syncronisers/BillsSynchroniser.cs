using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CA.Business
{
	public class BillsSynchroniser : Customs.Business.BillsSynchroniser
	{
		public BillsSynchroniser(JobDeclaration declaration, Func<IBillDetails, ZString> getBillNumber)
			: base(declaration, getBillNumber)
		{
		}

		protected override ZString ConvertPack(ZString freightPack, ZGuid registryCompanyPK)
		{
			var result = ZString.Empty;
			var caDeclaration = declaration as JobDeclaration;
			if (caDeclaration != null && caDeclaration.IsIID)
			{
				var mappings = CACustomsDataRegistry.Instance.CAPackageTypesMapping.GetValueWithoutFallback(registryCompanyPK.ToGuid(), Guid.Empty, Guid.Empty);
				result = mappings.GetMappedPackageType(freightPack);
			}
			if (result.IsEmpty)
			{
				result = freightPack;
			}
			return result;
		}

		protected override void SynchroniseBill(BillDetailsWrapper sourceBill, Customs.Business.Bill destination)
		{
			base.SynchroniseBill(sourceBill, destination);
			if (!SyncChangesDetected && !DetectEnabled)
			{
				destination.CU_PackType = ConvertPack(sourceBill.GetTypeOfPack(declaration.Shipment), declaration.RegistryCompanyPK);
			}
		}
	}
}
