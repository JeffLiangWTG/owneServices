using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.H7.Business
{
	public class AsycudaBillSimilarAddressViewModel : NonPersistentBusinessObject, IDisposable
	{
		public AsycudaBillSimilarAddressViewModel(AsycudaBill bill, AsycudaBillAddress.AddressType type)
		{
			this.bill = bill;
			if (type == AsycudaBillAddress.AddressType.Consignee || type == AsycudaBillAddress.AddressType.Shipper)
			{
				addressType = type;
			}
			else
			{
				throw new ArgumentException("Only Consignee and Shipper type are supported.");
			}
		}

		AsycudaBill bill { get; }

		AsycudaBillAddress.AddressType addressType { get; }

		public string BillNumber => bill.ABL_BillNumber;

		public string AddressType => isConsignee ? Importer : Exporter;

		public string OrgName => isConsignee ? bill.ABL_ConsigneeName : bill.ABL_ShipperName;

		public string Address1 => isConsignee ? bill.ABL_ConsigneeStreet1 : bill.ABL_ShipperStreet1;

		public string Address2 => isConsignee ? bill.ABL_ConsigneeStreet2 : bill.ABL_ShipperStreet2;

		public string City => isConsignee ? bill.ABL_ConsigneeCity : bill.ABL_ShipperCity;

		public string State => isConsignee ? bill.ABL_ConsigneeState : bill.ABL_ShipperState;

		public string Postcode => isConsignee ? bill.ABL_ConsigneePostcode : bill.ABL_ShipperPostcode;

		public string Country => isConsignee ? bill.ABL_RN_NKConsigneeCountry : bill.ABL_RN_NKShipperCountry;

		bool isConsignee => addressType == AsycudaBillAddress.AddressType.Consignee;

		public OrgPatternMatchCollection SimilarOrgMatches
		{
			get
			{
				if (temporaryOrganization == null)
				{
					temporaryOrganization = GenerateTemporaryOrganization();
					temporaryOrganization.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(temporaryOrganization);
					temporaryOrganization.SimilarOrgFinder.FindSimilarOrganisations();
				}

				return temporaryOrganization.SimilarOrgMatches;
			}
		}

		OrgHeader temporaryOrganization;

		public ZGuid LinkingAddressPK;

		public void SetLinkingAddress(ZGuid addressPK)
		{
			LinkingAddressPK = addressPK;
			temporaryOrganization?.Delete();
		}

		public void ApplyLinkAddress()
		{
			if (isConsignee)
			{
				bill.ABL_OA_Consignee = LinkingAddressPK;
			}
			else
			{
				bill.ABL_OA_Shipper = LinkingAddressPK;
			}
		}

		OrgHeader GenerateTemporaryOrganization()
		{
			var result = bill.Factory.New<OrgHeader>();
			result.OverrideIsSavedByFactory(false);
			FillOrgHeaderWithConsignmentDetails(result);
			return result;
		}

		public void FillOrgHeaderWithConsignmentDetails(OrgHeader org)
		{
			var mainAddress = org.MainAddress;
			if (isConsignee)
			{
				org.OH_IsConsignee = true;
				org.OH_FullName = bill.ABL_ConsigneeName.Substring(0, org.OH_FullNameInfo.MaxLength);
				mainAddress.OA_Address1 = bill.ABL_ConsigneeStreet1.Substring(0, mainAddress.OA_Address1Info.MaxLength);
				mainAddress.OA_Address2 = bill.ABL_ConsigneeStreet2.Substring(0, mainAddress.OA_Address2Info.MaxLength);
				mainAddress.OA_City = bill.ABL_ConsigneeCity;
				mainAddress.OA_State = bill.ABL_ConsigneeState.Substring(0, mainAddress.OA_StateInfo.MaxLength);
				mainAddress.OA_PostCode = bill.ABL_ConsigneePostcode.Substring(0, mainAddress.OA_PostCodeInfo.MaxLength);
				mainAddress.OA_RN_NKCountryCode = bill.ABL_RN_NKConsigneeCountry;
				mainAddress.OA_Phone = bill.ABL_ConsigneePhone;
				mainAddress.OA_Email = bill.ABL_ConsigneeEmail;
			}
			else
			{
				org.OH_IsConsignor = true;
				org.OH_FullName = bill.ABL_ShipperName.Substring(0, org.OH_FullNameInfo.MaxLength);
				mainAddress.OA_Address1 = bill.ABL_ShipperStreet1.Substring(0, mainAddress.OA_Address1Info.MaxLength);
				mainAddress.OA_Address2 = bill.ABL_ShipperStreet2.Substring(0, mainAddress.OA_Address2Info.MaxLength);
				mainAddress.OA_City = bill.ABL_ShipperCity;
				mainAddress.OA_State = bill.ABL_ShipperState.Substring(0, mainAddress.OA_StateInfo.MaxLength);
				mainAddress.OA_PostCode = bill.ABL_ShipperPostcode.Substring(0, mainAddress.OA_PostCodeInfo.MaxLength);
				mainAddress.OA_RN_NKCountryCode = bill.ABL_RN_NKShipperCountry;
				mainAddress.OA_Phone = bill.ABL_ShipperPhone;
				mainAddress.OA_Email = bill.ABL_ShipperEmail;
			}
		}

		#region IDisposable Members

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				temporaryOrganization?.Delete();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		static string Importer => Res.GetString("df8fd033-280f-43aa-ae6f-ee88b60ae285", "Importer");
		static string Exporter => Res.GetString("42e007b0-e160-42ea-ac96-79a2146c464b", "Exporter");
	}
}
