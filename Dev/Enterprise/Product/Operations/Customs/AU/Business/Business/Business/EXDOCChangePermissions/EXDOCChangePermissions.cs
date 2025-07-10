using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public enum EXDOCDataFields
	{
		AmendedInformation,
		CommodityType,
		Compartments,
		DeclarationOfComplianceIndicator,
		DestinationCountry,
		ImportedProductFlag,
		InspectionPort,
		InspectionProcess,
		InspectionRequestedDate,
		LotNumber,
		RFPAuthorisationEstablishmentNumber,
		RFPAuthorisedStartDate,
		RFPAuthorisedEndDate,
		RFPAuthorisingOfficerIdentifier,
		ShipStoresIndicator,
		StorageEstablishmentNumber,
		TrueAndCompleteIndicator,
		CertificateProductDescription,
		CutCode,
		GrowerNumber,
		HalalProductIndicator,
		NatureOfCommodity,
		ProcessDetails,
		ProductCode,
		TreatmentProcess,
		TreatmentType,
		UngradedProductIndicator
	}

	public class EXDOCChangePermissions
	{
		public EXDOCChangePermissions(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		public bool IsChangeAllowed(EXDOCDataFields dataField, ZString product, ZString rFPStatus)
		{
			var result = rFPStatus.IsEmpty;
			if (!result)
			{
				if (ChangePermissionsMatrix.TryGetValue(dataField, out var changePermission))
				{
					if (changePermission.TryGetValue(rFPStatus, out var permissionString))
					{
						result = permissionString.Contains(product);
					}
				}
				else
				{
					result = rFPStatus != EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
				}
			}
			return result;
		}

		ChangePermissions ChangePermissionsMatrix => factory.GetCachedValue("EXDOCChangePermissionsMatrix", GetPermissions);

		ChangePermissions GetPermissions()
		{
			var result = new ChangePermissions();
			result.Add(EXDOCDataFields.AmendedInformation, GetPermission(",,,GH,GH"));
			result.Add(EXDOCDataFields.CommodityType, GetPermission("DEFGHIMSW,DEFGHIMSW,DEFGHIMSW"));
			result.Add(EXDOCDataFields.Compartments, GetPermission("G,G,G"));
			result.Add(EXDOCDataFields.DeclarationOfComplianceIndicator, GetPermission("DEFGHIMSW,DEFGHIMSW,DEFGHIMSW"));
			result.Add(EXDOCDataFields.DestinationCountry, GetPermission("DEFGHIMSW"));
			result.Add(EXDOCDataFields.ImportedProductFlag, GetPermission("DF,DF,DF"));
			result.Add(EXDOCDataFields.InspectionPort, GetPermission("G,G,G"));
			result.Add(EXDOCDataFields.InspectionProcess, GetPermission("DEFGHIMS"));
			result.Add(EXDOCDataFields.InspectionRequestedDate, GetPermission("DEFGHIM,DEFGHIM,DEFGHIM"));
			result.Add(EXDOCDataFields.LotNumber, GetPermission("GH,GH,GH"));
			result.Add(EXDOCDataFields.RFPAuthorisationEstablishmentNumber, GetPermission("DEFGHIMS,HG,HG"));
			result.Add(EXDOCDataFields.RFPAuthorisedStartDate, GetPermission("GH,GH,GH,GH"));
			result.Add(EXDOCDataFields.RFPAuthorisedEndDate, GetPermission("EFGHIMSW,EFGHIMSW,EFGHIMSW,EFGHIMSW"));
			result.Add(EXDOCDataFields.RFPAuthorisingOfficerIdentifier, GetPermission("DEFGHIM,DEFGHIM,DEFGHIM"));
			result.Add(EXDOCDataFields.ShipStoresIndicator, GetPermission("DEFGHIM"));
			result.Add(EXDOCDataFields.StorageEstablishmentNumber, GetPermission("FDEGHIMSW"));
			result.Add(EXDOCDataFields.TrueAndCompleteIndicator, GetPermission("DEFGHIMSW,DEFGHIMSW,DEFGHIMSW"));
			result.Add(EXDOCDataFields.CertificateProductDescription, GetPermission("DEFGHIMSW,DEFGHIMSW,DEFGHIMSW,MFGHSW,MFGHSW"));
			result.Add(EXDOCDataFields.CutCode, GetPermission("DEFGHIMSW"));
			result.Add(EXDOCDataFields.GrowerNumber, GetPermission("H,H,H"));
			result.Add(EXDOCDataFields.HalalProductIndicator, GetPermission("M,M,M"));
			result.Add(EXDOCDataFields.NatureOfCommodity, GetPermission("M,M,M"));
			result.Add(EXDOCDataFields.ProcessDetails, GetPermission("DEFGHIMSW"));
			result.Add(EXDOCDataFields.ProductCode, GetPermission("DEFGHIMSW"));
			result.Add(EXDOCDataFields.TreatmentProcess, GetPermission("DEFGHISW,DEFGHISW,DEFGHISW"));
			result.Add(EXDOCDataFields.TreatmentType, GetPermission("MF,MF,MF"));
			result.Add(EXDOCDataFields.UngradedProductIndicator, GetPermission("M,M,M"));

			return result;
		}

		StatusPermissions GetPermission(ZString permissionList)
		{
			var result = new StatusPermissions();
			var permissions = permissionList.Split(',');
			var element = 0;
			AddPermission(result, permissions, element++, EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder);
			AddPermission(result, permissions, element++, EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial);
			AddPermission(result, permissions, element++, EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal);
			AddPermission(result, permissions, element++, EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InspInspected);
			AddPermission(result, permissions, element++, EXDOCComplianceStatusCodesForCusEntryNumber.Codes.HcrdHealthCertificateReady);
			AddPermission(result, permissions, element++, EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted);
			AddPermission(result, permissions, element++, EXDOCComplianceStatusCodesForCusEntryNumber.Codes.EmhcEmergencyHealthCertificate);
			AddPermission(result, permissions, element++, EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CancCancelled);
			return result;
		}

		void AddPermission(StatusPermissions statusPermissions, ZString[] permissions, int element, ZString applicableStatus)
		{
			if (permissions.Length > element && permissions[element].Length > 0)
			{
				statusPermissions.Add(applicableStatus, permissions[element]);
			}
		}

		public class StatusPermissions : Dictionary<ZString, ZString>
		{
		}

		public class ChangePermissions : Dictionary<EXDOCDataFields, StatusPermissions>
		{
		}
	}
}
