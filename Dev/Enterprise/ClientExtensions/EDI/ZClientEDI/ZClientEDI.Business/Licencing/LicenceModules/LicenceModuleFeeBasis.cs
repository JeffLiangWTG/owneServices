using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Progress;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Licencing.Business
{
	public class LicenceModuleFeeBasis : NonPersistentBusinessObject, IObsoleteValidation
	{
		public LicenceModuleFeeBasis()
			: base(null)
		{
		}

		#region ModuleCode

		[List("ModuleCodes")]
		[MaxLength(LicenceModules.Schema.LM_GroupModuleCodeMaxLength)]
		public ZString ModuleCode
		{
			get { return moduleCode; }
			set
			{
				SetNonPersistentPropertyValue(ModuleCodeInfo, ref moduleCode, value);

				if (!IsValidationSuspended)
				{
					ValidateModuleCode();
				}
			}
		}
		public ZPropertyInfo ModuleCodeInfo { get { return GetZPropertyInfo(nameof(ModuleCode)); } }
		ZString moduleCode;

		public void ValidateModuleCode()
		{
			ModuleCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ModuleCodeInfo);
			ListValidation.ErrorIfInvalidCode(ModuleCodeInfo);
		}

		public CodeDescriptionPairList ModuleCodes
		{
			get { return moduleCodes ?? (moduleCodes = LicenceModuleList.Instance.Names); }
		}
		CodeDescriptionPairList moduleCodes;

		#endregion

		#region FeeBasis

		[List("FeeBasisList")]
		[MaxLength(LicenceModules.Schema.LM_LicenceTypeMaxLength)]
		public ZString FeeBasis
		{
			get { return feeBasis; }
			set
			{
				SetNonPersistentPropertyValue(FeeBasisInfo, ref feeBasis, value);

				if (!IsValidationSuspended)
				{
					ValidateFeeBasis();
				}
			}
		}
		public ZPropertyInfo FeeBasisInfo { get { return GetZPropertyInfo(nameof(FeeBasis)); } }
		ZString feeBasis;

		public void ValidateFeeBasis()
		{
			FeeBasisInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(FeeBasisInfo);
			ListValidation.ErrorIfInvalidCode(FeeBasisInfo);
		}

		public CodeDescriptionPairList FeeBasisList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(LicenceTypes.Codes.NON, LicenceTypes.Descriptions.NON);
				result.AddPair(LicenceTypes.Codes.CPT, LicenceTypes.Descriptions.CPT);
				result.AddPair(LicenceTypes.Codes.ODM, LicenceTypes.Descriptions.ODM);
				result.AddPair(LicenceTypes.Codes.OPN, LicenceTypes.Descriptions.OPN);
				result.AddPair(LicenceTypes.Codes.OTM, LicenceTypes.Descriptions.OTM);
				result.AddPair(LicenceTypes.Codes.PUR, LicenceTypes.Descriptions.PUR);
				result.AddPair(LicenceTypes.Codes.REN, LicenceTypes.Descriptions.REN);
				result.AddPair(LicenceTypes.Codes.SRU, LicenceTypes.Descriptions.SRU);
				result.AddPair(LicenceTypes.Codes.TRI, LicenceTypes.Descriptions.TRI);
				return result;
			}
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			ValidateModuleCode();
			ValidateFeeBasis();
			base.RunPreSaveValidationCore();
		}

		public int Update(BusinessObject[] selection, IEdiProgress progress, StringBuilder errors)
		{
			int successCount = 0;
			int progressCount = 0;
			int totalLicence = selection.Length;

			foreach (LicenceHeader licence in selection)
			{
				if (progress.SafeIsCancelled())
				{
					break;
				}
				progress.SafeSetStatusAndPercentComplete("Updating " + licence.LicenceCode + "(Org " + licence.Company.Header.OH_Code + ")", progressCount++ * 100 / totalLicence);
				var module = licence.Modules.FindByCode(ModuleCode);
				if (module.Lookups.LicenceTypesList.ContainsCode(FeeBasis))
				{
					module.LM_LicenceType = FeeBasis;
					successCount++;
				}
				else if (errors != null)
				{
					errors.Append("Fee basis is invalid for " + licence.LicenceCode + "(Org " + licence.Company.Header.OH_Code + ")\r\n");
				}
			}

			return successCount;
		}
	}
}
