using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.Native.Business.Update
{
	public class DangerousGoodsCountryReferenceMappingInterceptor : BaseInterceptor
	{
		public DangerousGoodsCountryReferenceMappingInterceptor(IInterceptorSetting setting, AncillaryImportServices sessionServices) : base(setting, sessionServices)
		{
		}

		public override void Invoke(IEntitySet entitySet)
		{
			if (!Env.Security.UNDGSubstanceCountryReferenceAttachDetach.IsAllowed)
			{
				var securityPath = Env.Security.UNDGSubstanceCountryReferenceAttachDetach.DisplayTextPathToSecurityRight.ToString();
				throw new NativeXMLUserVisibleException($"You do not have security right to import UNDGCountryReference XMLs. Please enable security: {securityPath}");
			}

			var undgCountryReferencePivot = entitySet.Root;

			if (!IsUNDGCountryReferenceFranceICPE(undgCountryReferencePivot))
			{
				throw new NativeXMLUserVisibleException("Importing UNDGCountryReference XMLs is only supported for FR ICPE");
			}

			if (undgCountryReferencePivot.HasProperty("StorageInstruction"))
			{
				var storageInstruction = undgCountryReferencePivot["StorageInstruction"].ToString();
				var storageInstructionList = new StorageInstructionList();
				if (!string.IsNullOrEmpty(storageInstruction) && !storageInstructionList.ContainsCode(storageInstruction))
				{
					throw new NativeXMLUserVisibleException($"StorageInstruction value {storageInstruction} is invalid. Only one of the following are allowed: {storageInstructionList.GetHumanReadableListOfElements(", ")}");
				}
			}

			Function(entitySet);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		bool IsUNDGCountryReferenceFranceICPE(IEntity undgCountryReferencePivot)
		{
			var undgCountryReference = undgCountryReferencePivot.GetParentEntity("UNDGCountryReference");
			var country = undgCountryReference.GetParentEntity("Country");
			if (undgCountryReference.HasProperty("Type") && country != null && country.HasProperty("Code"))
			{
				var type = undgCountryReference["Type"].ToString();
				var countryCode = country["Code"].ToString();

				if (type == Core.Constants.UNDGCountryReference.Type.ICPE && countryCode == Core.Constants.CountryCodes.France)
				{
					return true;
				}
			}

			return false;
		}
	}
}
