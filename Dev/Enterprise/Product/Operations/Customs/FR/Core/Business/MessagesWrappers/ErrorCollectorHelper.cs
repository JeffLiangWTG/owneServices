using CargoWise.Types;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers
{
	public sealed class ErrorCollectorHelper
	{
		#region Warehouse

		public static string WarehouseAuthorisationNotFoundMessage => Res.GetString("2D23CEF7-A3A8-4C30-A1D1-383395C65DC3", "No matching authorization found for bonded warehouse purposes, please configure it in the Authorization module.");

		public static string AlternativePurposeAuthorisationFound => Res.GetString("DCD7ED0F-988E-49F3-8633-96581487C7B6", "No authorization record has been found (an authorization exists, but of the wrong type, {0}).");

		#endregion

		#region GetExpeditionDepartment
		internal static bool DeliveryDepartmentCantBeDeterminate(OrgAddress address, ref ZString department)
		{
			var postCode = address?.OA_PostCode ?? ZString.Empty;

			if (postCode.Length > 2)
			{
				department = postCode.Left(2);
			}

			return department.IsEmpty || !department.IsNumbersOnlyOrEmpty;
		}

		internal static bool DeliveryDepartmentCantBeDeterminateJobDocAddressPostCode(FRJobDocAddress address, ref ZString department)
		{
			var postCode = address?.E2_Postcode ?? ZString.Empty;

			if (postCode.Length > 2)
			{
				department = postCode.Left(2);
			}

			return department.IsEmpty || !department.IsNumbersOnlyOrEmpty;
		}

		#endregion

		public static string DeclarantEoriNotConfigured => Res.GetString("78ACEA79-89C0-4356-89AF-36A413D93A18", "The registration code number (EORI) is not configured. Config it in Registration Numbers / Codes tab");

		public static string CBRNotConfigured => Res.GetString("486E1D87-6341-43DD-8583-FCC67D8A9675", "The brokerage code number (CBR) is not configured. Config it in Registration Numbers / Codes tab");

		public static string DeclarantIsRequired => Res.GetString("8624D400-A6D2-426C-AF1C-FAECD754D652", "Declarant is required when representation mode is indirect.");

		public static string ImporterIsRequired => Res.GetString("538BBE8C-AE8A-4BB0-B62D-25100C079E36", "Importer is required when representation mode is self or direct.");

		public static string SupplierIsRequired => Res.GetString("EAC56847-D86B-4037-AB04-18C88754F2BF", "Supplier is required when representation mode is self or direct.");

		public static string SupplierSrtNotConfigured => Res.GetString("C9812D1C-C729-4DE0-8B07-CD25D4E07470", "Supplier SRT code not found");

		public static string ImporterSrtNotConfigured => Res.GetString("F6C2E3C2-6D1B-47F4-BCB1-364218E37B25", "Importer SRT code not found");
	}
}
